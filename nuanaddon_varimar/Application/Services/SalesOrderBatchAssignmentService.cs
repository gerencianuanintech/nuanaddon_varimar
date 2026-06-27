using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using nuanaddon_varimar.Shared.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace nuanaddon_varimar.Application.Services {
    public class SalesOrderBatchAssignmentService {
        private readonly ILotConfigurationService lotConfigurationService;
        private readonly ILotAssignmentPlannerService lotAssignmentPlannerService;

        public SalesOrderBatchAssignmentService()
            : this(new LotConfigurationService(), new LotAssignmentPlannerService()) {
        }

        public SalesOrderBatchAssignmentService(
            ILotConfigurationService lotConfigurationService,
            ILotAssignmentPlannerService lotAssignmentPlannerService) {
            this.lotConfigurationService = lotConfigurationService;
            this.lotAssignmentPlannerService = lotAssignmentPlannerService;
        }

        public BatchAssignmentResult Assign(SAPbouiCOM.Form batchSelectionForm, SalesOrderBatchAssignmentRequest request) {
            try {
                if (!BatchAssignmentCustomerService.IsSupportedCustomer(request.CardCode))
                    return BatchAssignmentResult.Ok();

                SAPbouiCOM.Matrix lineMatrix = (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.LinesMatrix).Specific;
                bool assignedAnyBatch = false;

                using (BatchAssignmentProgress progress = BatchAssignmentProgress.Create("Asignando lotes...", CalculateProgressSteps(lineMatrix))) {
                    progress.Step("Leyendo configuracion de lotes...");
                    LotSelectionConfigDto config = lotConfigurationService.GetConfig();

                    for (int row = 1; row <= lineMatrix.RowCount; row++) {
                        progress.Step("Leyendo linea de articulo " + row.ToString(CultureInfo.InvariantCulture) + "...");
                        lineMatrix.Columns.Item(SapBatchSelectionUiIds.LineItemCodeColumn).Cells.Item(row).Click();

                        SalesOrderLineBatchContext lineContext = new SalesOrderLineBatchContext {
                            Row = row,
                            ItemCode = SapUiMatrixAccessor.GetEditTextValue(lineMatrix, SapBatchSelectionUiIds.LineItemCodeColumn, row),
                            SelectedQuantity = SapUiMatrixAccessor.GetDecimalValue(lineMatrix, SapBatchSelectionUiIds.LineSelectedQuantityColumn, row),
                            RequiredQuantity = SapUiMatrixAccessor.GetDecimalValue(lineMatrix, SapBatchSelectionUiIds.LineRequiredQuantityColumn, row)
                        };

                        if (lineContext.MissingQuantity <= 0)
                            continue;

                        progress.Step("Procesando lotes del articulo " + lineContext.ItemCode + "...");
                        decimal remaining = AssignLineWithConfig(batchSelectionForm, request, lineContext, config, ref assignedAnyBatch, progress);

                        if (remaining > 0) {
                            if (assignedAnyBatch) {
                                progress.Step("Confirmando asignaciones parciales...");
                                batchSelectionForm.Items.Item(SapCommonUiIds.OkButton).Click();
                            }

                            return BatchAssignmentResult.Fail(SapMessages.NoAvailableBatchesForCustomerRulesDetail(lineContext.ItemCode, remaining));
                        }
                    }

                    if (assignedAnyBatch) {
                        progress.Step("Confirmando asignacion de lotes...");
                        batchSelectionForm.Items.Item(SapCommonUiIds.OkButton).Click();
                    }
                }

                return BatchAssignmentResult.Ok();
            }
            catch (Exception ex) {
                SapMessages.Error("Application.Services.SalesOrderBatchAssignmentService.cs -> Assign", ex);
                return BatchAssignmentResult.Fail(ex.Message);
            }
        }

        private decimal AssignLineWithConfig(
            SAPbouiCOM.Form batchSelectionForm,
            SalesOrderBatchAssignmentRequest request,
            SalesOrderLineBatchContext lineContext,
            LotSelectionConfigDto config,
            ref bool assignedAnyBatch,
            BatchAssignmentProgress progress) {
            decimal missingQuantity = lineContext.MissingQuantity;

            progress.Step("Leyendo lotes disponibles...");
            SAPbouiCOM.Matrix batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
            IList<AvailableBatchDto> availableBatches = BuildAvailableBatches(batchMatrix, lineContext.ItemCode);
            if (availableBatches.Count == 0)
                return missingQuantity;

            progress.Step("Calculando plan de asignacion...");
            IList<BatchAssignmentDto> assignments = lotAssignmentPlannerService.PlanAssignments(
                request.CardCode,
                lineContext.ItemCode,
                missingQuantity,
                request.DeliveryDate,
                availableBatches,
                config);

            progress.Step("Preparando asignacion en SAP...");
            if (TryAssignPlannedBatchesTogether(batchSelectionForm, lineContext, assignments, missingQuantity, progress)) {
                assignedAnyBatch = assignments.Count > 0;
                return 0m;
            }

            foreach (BatchAssignmentDto assignment in assignments) {
                if (missingQuantity <= 0)
                    break;

                batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);

                AvailableBatchDto currentBatch;
                if (!TryResolveCurrentBatch(batchMatrix, assignment, lineContext.ItemCode, out currentBatch))
                    continue;

                assignment.SapRow = currentBatch.SapRow;

                if (!IsValidAssignment(batchMatrix, currentBatch, assignment, missingQuantity))
                    continue;

                progress.Step("Asignando lote " + assignment.BatchNumber + "...");
                if (!SapUiMatrixAccessor.TrySetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchQuantityToAssignColumn, assignment.SapRow, assignment.QuantityToAssign)) {
                    SapMessages.Error(LotAssignmentMessages.InvalidSapRowDetail(assignment.SapRow));
                    continue;
                }

                batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AssignButton).Click();
                assignedAnyBatch = true;
                missingQuantity -= assignment.QuantityToAssign;
            }

            return missingQuantity;
        }

        private bool TryAssignPlannedBatchesTogether(
            SAPbouiCOM.Form batchSelectionForm,
            SalesOrderLineBatchContext lineContext,
            IList<BatchAssignmentDto> assignments,
            decimal missingQuantity,
            BatchAssignmentProgress progress) {
            if (assignments == null || assignments.Count == 0)
                return false;

            SAPbouiCOM.Matrix batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
            IList<ResolvedBatchAssignment> resolvedAssignments = new List<ResolvedBatchAssignment>();
            decimal plannedQuantity = 0m;

            foreach (BatchAssignmentDto assignment in assignments) {
                AvailableBatchDto currentBatch;
                if (!TryResolveCurrentBatch(batchMatrix, assignment, lineContext.ItemCode, out currentBatch))
                    return false;

                assignment.SapRow = currentBatch.SapRow;

                if (!IsValidAssignment(batchMatrix, currentBatch, assignment, missingQuantity - plannedQuantity))
                    return false;

                resolvedAssignments.Add(new ResolvedBatchAssignment(assignment, currentBatch));
                plannedQuantity += assignment.QuantityToAssign;
            }

            if (resolvedAssignments.Count == 0 || plannedQuantity <= 0 || plannedQuantity != missingQuantity)
                return false;

            progress.Step("Escribiendo cantidades en lotes seleccionados...");
            foreach (ResolvedBatchAssignment resolvedAssignment in resolvedAssignments) {
                if (!SapUiMatrixAccessor.TrySetEditTextValue(
                    batchMatrix,
                    SapBatchSelectionUiIds.BatchQuantityToAssignColumn,
                    resolvedAssignment.Assignment.SapRow,
                    resolvedAssignment.Assignment.QuantityToAssign)) {
                    SapMessages.Error(LotAssignmentMessages.InvalidSapRowDetail(resolvedAssignment.Assignment.SapRow));
                    ClearPlannedQuantities(batchMatrix, resolvedAssignments);
                    return false;
                }
            }

            progress.Step("Seleccionando lotes en SAP...");
            if (!TrySelectPlannedRows(batchMatrix, resolvedAssignments)) {
                ClearPlannedQuantities(batchMatrix, resolvedAssignments);
                return false;
            }

            try {
                progress.Step("Pasando lotes seleccionados...");
                batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AssignButton).Click();
                return true;
            }
            catch (Exception ex) {
                SapMessages.Warning("No se pudo asignar multiples lotes con un solo boton. Se usara asignacion uno por uno. " + ex.Message);
                ClearPlannedQuantities(batchMatrix, resolvedAssignments);
                return false;
            }
        }

        private bool TrySelectPlannedRows(SAPbouiCOM.Matrix batchMatrix, IList<ResolvedBatchAssignment> resolvedAssignments) {
            if (batchMatrix == null || resolvedAssignments == null || resolvedAssignments.Count == 0)
                return false;

            try {
                for (int index = 0; index < resolvedAssignments.Count; index++) {
                    int sapRow = resolvedAssignments[index].Assignment.SapRow;
                    bool preservePreviousSelection = index > 0;
                    batchMatrix.SelectRow(sapRow, true, preservePreviousSelection);
                }

                return true;
            }
            catch (Exception ex) {
                SapMessages.Warning("No se pudo seleccionar multiples lotes en SAP UI API. Se usara asignacion uno por uno. " + ex.Message);
                return false;
            }
        }

        private void ClearPlannedQuantities(SAPbouiCOM.Matrix batchMatrix, IList<ResolvedBatchAssignment> resolvedAssignments) {
            if (batchMatrix == null || resolvedAssignments == null)
                return;

            foreach (ResolvedBatchAssignment resolvedAssignment in resolvedAssignments) {
                SapUiMatrixAccessor.TrySetEditTextValue(
                    batchMatrix,
                    SapBatchSelectionUiIds.BatchQuantityToAssignColumn,
                    resolvedAssignment.Assignment.SapRow,
                    0m);
            }
        }

        private bool TryResolveCurrentBatch(
            SAPbouiCOM.Matrix batchMatrix,
            BatchAssignmentDto assignment,
            string itemCode,
            out AvailableBatchDto currentBatch) {
            currentBatch = null;

            if (assignment == null || string.IsNullOrWhiteSpace(assignment.BatchNumber))
                return false;

            if (TryReadAvailableBatch(batchMatrix, assignment.SapRow, itemCode, out currentBatch) &&
                BatchNumbersMatch(currentBatch.BatchNumber, assignment.BatchNumber))
                return true;

            if (batchMatrix == null)
                return false;

            for (int row = 1; row <= batchMatrix.RowCount; row++) {
                if (!TryReadAvailableBatch(batchMatrix, row, itemCode, out currentBatch))
                    continue;

                if (BatchNumbersMatch(currentBatch.BatchNumber, assignment.BatchNumber))
                    return true;
            }

            currentBatch = null;
            return false;
        }

        private bool BatchNumbersMatch(string currentBatchNumber, string plannedBatchNumber) {
            return string.Equals(
                (currentBatchNumber ?? string.Empty).Trim(),
                (plannedBatchNumber ?? string.Empty).Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private IList<AvailableBatchDto> BuildAvailableBatches(SAPbouiCOM.Matrix batchMatrix, string itemCode) {
            IList<AvailableBatchDto> batches = new List<AvailableBatchDto>();

            if (batchMatrix == null)
                return batches;

            for (int row = 1; row <= batchMatrix.RowCount; row++) {
                AvailableBatchDto batch;
                if (TryReadAvailableBatch(batchMatrix, row, itemCode, out batch))
                    batches.Add(batch);
            }

            return batches;
        }

        private bool TryReadAvailableBatch(SAPbouiCOM.Matrix batchMatrix, int row, string itemCode, out AvailableBatchDto batch) {
            batch = null;

            if (!SapUiMatrixAccessor.IsValidRow(batchMatrix, row))
                return false;

            string expirationDateValue;
            if (!SapUiMatrixAccessor.TryGetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row, out expirationDateValue))
                return false;

            DateTime expirationDate;
            if (!SapValueParser.TryParseDate(expirationDateValue, out expirationDate))
                return false;

            decimal availableQuantity;
            decimal assignedQuantity;
            if (!SapUiMatrixAccessor.TryGetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAvailableQuantityColumn, row, out availableQuantity))
                return false;
            if (!SapUiMatrixAccessor.TryGetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAssignedQuantityColumn, row, out assignedQuantity))
                return false;

            decimal remainingQuantity = availableQuantity - assignedQuantity;
            if (availableQuantity <= 0 || remainingQuantity <= 0)
                return false;

            DateTime productionDate;
            DateTime? parsedProductionDate = null;
            string productionDateValue;
            if (SapUiMatrixAccessor.TryGetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchProductionDateColumn, row, out productionDateValue) &&
                SapValueParser.TryParseDate(productionDateValue, out productionDate))
                parsedProductionDate = productionDate;

            batch = new AvailableBatchDto {
                SapRow = row,
                ItemCode = itemCode,
                BatchNumber = ReadBatchNumberOrFallback(batchMatrix, row),
                ProductionDate = parsedProductionDate,
                ExpirationDate = expirationDate,
                AvailableQuantity = availableQuantity,
                AlreadyAssignedQuantity = assignedQuantity,
                RemainingQuantity = remainingQuantity
            };

            return true;
        }

        private string ReadBatchNumberOrFallback(SAPbouiCOM.Matrix batchMatrix, int sapRow) {
            if (SapBatchSelectionUiIds.IsBatchNumberColumnConfirmed()) {
                string batchNumber;
                if (SapUiMatrixAccessor.TryGetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchNumberColumn, sapRow, out batchNumber) &&
                    !string.IsNullOrWhiteSpace(batchNumber))
                    return batchNumber.Trim();
            }

            // Stable temporary tie-break until the real SAP batch-number column id is confirmed.
            return "SAPROW_" + sapRow.ToString("D8", CultureInfo.InvariantCulture);
        }

        private bool IsValidAssignment(
            SAPbouiCOM.Matrix batchMatrix,
            AvailableBatchDto sourceBatch,
            BatchAssignmentDto assignment,
            decimal missingQuantity) {
            if (sourceBatch == null || assignment == null)
                return false;

            if (!SapUiMatrixAccessor.IsValidRow(batchMatrix, assignment.SapRow)) {
                SapMessages.Error(LotAssignmentMessages.InvalidSapRowDetail(assignment.SapRow));
                return false;
            }

            if (assignment.QuantityToAssign <= 0) {
                SapMessages.Error(LotAssignmentMessages.CannotAssignZeroOrNegativeQuantity);
                return false;
            }

            if (assignment.QuantityToAssign > missingQuantity)
                return false;

            if (assignment.QuantityToAssign > sourceBatch.RemainingQuantity)
                return false;

            return true;
        }

        private SAPbouiCOM.Matrix GetAvailableBatchMatrix(SAPbouiCOM.Form batchSelectionForm) {
            return (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AvailableBatchesMatrix).Specific;
        }

        private int CalculateProgressSteps(SAPbouiCOM.Matrix lineMatrix) {
            int rowCount = lineMatrix == null ? 1 : lineMatrix.RowCount;
            return Math.Max(1, 4 + (rowCount * 8));
        }

        private class ResolvedBatchAssignment {
            public ResolvedBatchAssignment(BatchAssignmentDto assignment, AvailableBatchDto batch) {
                Assignment = assignment;
                Batch = batch;
            }

            public BatchAssignmentDto Assignment { get; private set; }
            public AvailableBatchDto Batch { get; private set; }
        }

        private class BatchAssignmentProgress : IDisposable {
            private readonly SAPbouiCOM.ProgressBar progressBar;
            private readonly int totalSteps;
            private int currentStep;

            private BatchAssignmentProgress(SAPbouiCOM.ProgressBar progressBar, int totalSteps) {
                this.progressBar = progressBar;
                this.totalSteps = totalSteps <= 0 ? 1 : totalSteps;
            }

            public static BatchAssignmentProgress Create(string text, int totalSteps) {
                SAPbouiCOM.ProgressBar progressBar = null;

                try {
                    if (Program.SBOApplication != null)
                        progressBar = Program.SBOApplication.StatusBar.CreateProgressBar(text, totalSteps <= 0 ? 1 : totalSteps, false);
                }
                catch {
                    progressBar = null;
                }

                return new BatchAssignmentProgress(progressBar, totalSteps);
            }

            public void Step(string text) {
                if (progressBar == null)
                    return;

                try {
                    progressBar.Text = text;

                    if (currentStep < totalSteps) {
                        currentStep++;
                        progressBar.Value = currentStep;
                    }
                }
                catch {
                    // Progress feedback must never interrupt batch assignment.
                }
            }

            public void Dispose() {
                if (progressBar == null)
                    return;

                try {
                    progressBar.Stop();
                }
                catch {
                }

                try {
                    if (Marshal.IsComObject(progressBar))
                        Marshal.ReleaseComObject(progressBar);
                }
                catch {
                }
            }
        }
    }
}
