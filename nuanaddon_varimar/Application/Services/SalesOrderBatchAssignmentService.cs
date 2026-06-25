using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using nuanaddon_varimar.Shared.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

                LotSelectionConfigDto config = lotConfigurationService.GetConfig();
                SAPbouiCOM.Matrix lineMatrix = (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.LinesMatrix).Specific;
                bool assignedAnyBatch = false;

                for (int row = 1; row <= lineMatrix.RowCount; row++) {
                    lineMatrix.Columns.Item(SapBatchSelectionUiIds.LineItemCodeColumn).Cells.Item(row).Click();

                    SalesOrderLineBatchContext lineContext = new SalesOrderLineBatchContext {
                        Row = row,
                        ItemCode = SapUiMatrixAccessor.GetEditTextValue(lineMatrix, SapBatchSelectionUiIds.LineItemCodeColumn, row),
                        SelectedQuantity = SapUiMatrixAccessor.GetDecimalValue(lineMatrix, SapBatchSelectionUiIds.LineSelectedQuantityColumn, row),
                        RequiredQuantity = SapUiMatrixAccessor.GetDecimalValue(lineMatrix, SapBatchSelectionUiIds.LineRequiredQuantityColumn, row)
                    };

                    if (lineContext.MissingQuantity <= 0)
                        continue;

                    decimal remaining = AssignLineWithConfig(batchSelectionForm, request, lineContext, config, ref assignedAnyBatch);

                    if (remaining > 0) {
                        if (assignedAnyBatch)
                            batchSelectionForm.Items.Item(SapCommonUiIds.OkButton).Click();

                        return BatchAssignmentResult.Fail(SapMessages.NoAvailableBatchesForCustomerRulesDetail(lineContext.ItemCode, remaining));
                    }
                }

                if (assignedAnyBatch)
                    batchSelectionForm.Items.Item(SapCommonUiIds.OkButton).Click();

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
            ref bool assignedAnyBatch) {
            decimal missingQuantity = lineContext.MissingQuantity;

            while (missingQuantity > 0) {
                SAPbouiCOM.Matrix batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
                IList<AvailableBatchDto> availableBatches = BuildAvailableBatches(batchMatrix, lineContext.ItemCode);
                if (availableBatches.Count == 0)
                    break;

                IList<BatchAssignmentDto> assignments = lotAssignmentPlannerService.PlanAssignments(
                    request.CardCode,
                    lineContext.ItemCode,
                    missingQuantity,
                    request.DeliveryDate,
                    availableBatches,
                    config);

                BatchAssignmentDto assignment = assignments.FirstOrDefault();
                if (assignment == null)
                    break;

                AvailableBatchDto sourceBatch = availableBatches.FirstOrDefault(batch => batch.SapRow == assignment.SapRow);
                if (!IsValidAssignment(batchMatrix, sourceBatch, assignment, missingQuantity))
                    break;

                if (!SapUiMatrixAccessor.TrySetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchQuantityToAssignColumn, assignment.SapRow, assignment.QuantityToAssign)) {
                    SapMessages.Error(LotAssignmentMessages.InvalidSapRowDetail(assignment.SapRow));
                    break;
                }

                batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AssignButton).Click();
                assignedAnyBatch = true;
                missingQuantity -= assignment.QuantityToAssign;
            }

            return missingQuantity;
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
    }
}
