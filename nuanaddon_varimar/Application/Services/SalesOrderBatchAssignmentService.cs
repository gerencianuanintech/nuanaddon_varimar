using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using nuanaddon_varimar.Shared.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nuanaddon_varimar.Application.Services {
    public class SalesOrderBatchAssignmentService {
        private readonly ILotConfigurationService lotConfigurationService;
        private readonly ILotSelectionService lotSelectionService;

        public SalesOrderBatchAssignmentService()
            : this(new LotConfigurationService(), new LotSelectionService()) {
        }

        public SalesOrderBatchAssignmentService(ILotConfigurationService lotConfigurationService, ILotSelectionService lotSelectionService) {
            this.lotConfigurationService = lotConfigurationService;
            this.lotSelectionService = lotSelectionService;
        }

        public BatchAssignmentResult Assign(SAPbouiCOM.Form batchSelectionForm, SalesOrderBatchAssignmentRequest request) {
            try {
                LotSelectionConfigDto config = lotConfigurationService.GetConfiguration();
                SAPbouiCOM.Matrix lineMatrix = (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.LinesMatrix).Specific;
                SAPbouiCOM.Matrix batchMatrix = (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AvailableBatchesMatrix).Specific;

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

                    IReadOnlyList<AvailableBatchDto> availableBatches;
                    try {
                        availableBatches = ReadAvailableBatches(batchMatrix, lineContext.ItemCode);
                    }
                    catch (InvalidOperationException ex) {
                        SapMessages.Error("Application.Services.SalesOrderBatchAssignmentService.cs -> ReadAvailableBatches", ex);
                        return BatchAssignmentResult.Fail(ex.Message);
                    }
                    catch (Exception ex) {
                        SapMessages.Error("Application.Services.SalesOrderBatchAssignmentService.cs -> ReadAvailableBatches", ex);
                        return BatchAssignmentResult.Fail(LotAssignmentMessages.ErrorReadingBatchMatrix);
                    }

                    if (availableBatches.Count == 0)
                        return BatchAssignmentResult.Fail(LotAssignmentMessages.NoAvailableBatches(lineContext.ItemCode));

                    List<AvailableBatchDto> batchesWithBalance = availableBatches
                        .Where(batch => batch.RemainingQuantity > 0)
                        .Where(batch => !string.IsNullOrWhiteSpace(batch.BatchNumber))
                        .ToList();

                    if (batchesWithBalance.Count == 0)
                        return BatchAssignmentResult.Fail(LotAssignmentMessages.NoAvailableBatches(lineContext.ItemCode));

                    DateTime baseDate = BatchAssignmentCustomerService.CalculateBaseDate(request.DeliveryDate, config);
                    List<AvailableBatchDto> customerRuleBatches = batchesWithBalance
                        .Where(batch => BatchAssignmentCustomerService.BatchPassesCustomerRule(request.CardCode, batch, baseDate))
                        .ToList();

                    if (customerRuleBatches.Count == 0)
                        return BatchAssignmentResult.Fail(LotAssignmentMessages.NoBatchesPassCustomerRule(lineContext.ItemCode));

                    IReadOnlyList<BatchAssignmentDto> assignments = lotSelectionService.BuildAssignments(
                        lineContext.ItemCode,
                        lineContext.MissingQuantity,
                        customerRuleBatches,
                        config);

                    if (assignments.Count == 0) {
                        if (LotSelectionCriteria.MenorCantidadLoteUnico.Equals(config.SelectionCriterion, StringComparison.OrdinalIgnoreCase))
                            return BatchAssignmentResult.Fail(LotAssignmentMessages.SingleBatchCriterionNotSatisfied(lineContext.ItemCode));

                        return BatchAssignmentResult.Fail(LotAssignmentMessages.NoAvailableBatches(lineContext.ItemCode));
                    }

                    if (!AssignmentsAreValid(assignments, customerRuleBatches, lineContext.MissingQuantity))
                        return BatchAssignmentResult.Fail(LotAssignmentMessages.InvalidBatchAssignment);

                    WriteAssignments(batchSelectionForm, batchMatrix, assignments);
                }

                return BatchAssignmentResult.Ok();
            }
            catch (Exception ex) {
                SapMessages.Error("Application.Services.SalesOrderBatchAssignmentService.cs -> Assign", ex);
                return BatchAssignmentResult.Fail(LotAssignmentMessages.ErrorWritingBatchAssignments);
            }
        }

        private IReadOnlyList<AvailableBatchDto> ReadAvailableBatches(SAPbouiCOM.Matrix batchMatrix, string itemCode) {
            List<AvailableBatchDto> batches = new List<AvailableBatchDto>();

            if (SapBatchSelectionUiIds.BatchNumberColumn == "CONFIRMAR_ID")
                throw new InvalidOperationException(LotAssignmentMessages.BatchNumberColumnIdRequiresConfirmation);

            for (int row = 1; row <= batchMatrix.RowCount; row++) {
                DateTime productionDate;
                DateTime expirationDate;

                AvailableBatchDto batch = new AvailableBatchDto {
                    SapRow = row,
                    ItemCode = itemCode ?? string.Empty,
                    BatchNumber = SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchNumberColumn, row).Trim(),
                    ProductionDate = SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchProductionDateColumn, row), out productionDate)
                        ? (DateTime?)productionDate
                        : null,
                    ExpirationDate = SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row), out expirationDate)
                        ? (DateTime?)expirationDate
                        : null,
                    AvailableQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAvailableQuantityColumn, row),
                    AlreadyAssignedQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAssignedQuantityColumn, row)
                };

                batches.Add(batch);
            }

            return batches;
        }

        private bool AssignmentsAreValid(IReadOnlyList<BatchAssignmentDto> assignments, IReadOnlyList<AvailableBatchDto> availableBatches, decimal requiredQuantity) {
            if (assignments == null || assignments.Count == 0)
                return false;

            decimal totalQuantity = assignments.Sum(assignment => assignment.QuantityToAssign);
            if (totalQuantity <= 0 || totalQuantity > requiredQuantity)
                return false;

            foreach (BatchAssignmentDto assignment in assignments) {
                if (assignment.SapRow <= 0 || assignment.QuantityToAssign <= 0)
                    return false;

                AvailableBatchDto batch = availableBatches.FirstOrDefault(item => item.SapRow == assignment.SapRow);
                if (batch == null || assignment.QuantityToAssign > batch.RemainingQuantity)
                    return false;
            }

            return true;
        }

        private void WriteAssignments(SAPbouiCOM.Form batchSelectionForm, SAPbouiCOM.Matrix batchMatrix, IReadOnlyList<BatchAssignmentDto> assignments) {
            try {
                foreach (BatchAssignmentDto assignment in assignments) {
                    if (assignment.SapRow <= 0 || assignment.SapRow > batchMatrix.RowCount)
                        throw new InvalidOperationException(LotAssignmentMessages.InvalidBatchAssignment);

                    SapUiMatrixAccessor.SetEditTextValue(
                        batchMatrix,
                        SapBatchSelectionUiIds.BatchQuantityToAssignColumn,
                        assignment.SapRow,
                        assignment.QuantityToAssign);
                }

                ClickAssign(batchSelectionForm);
            }
            catch (Exception ex) {
                SapMessages.Error("Application.Services.SalesOrderBatchAssignmentService.cs -> WriteAssignments", ex);
                throw;
            }
        }

        private void ClickAssign(SAPbouiCOM.Form batchSelectionForm) {
            batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AssignButton).Click();
            batchSelectionForm.Items.Item(SapCommonUiIds.OkButton).Click();
        }
    }
}
