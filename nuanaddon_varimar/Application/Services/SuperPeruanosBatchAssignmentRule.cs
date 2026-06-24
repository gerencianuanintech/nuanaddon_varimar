using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Parsing;
using System;

namespace nuanaddon_varimar.Application.Services {
    public class SuperPeruanosBatchAssignmentRule : ExpirationMonthsBatchAssignmentRule {
        public SuperPeruanosBatchAssignmentRule() : base(0) {
        }

        public override decimal AssignLine(SAPbouiCOM.Form batchSelectionForm, SalesOrderLineBatchContext lineContext, DateTime deliveryDate) {
            decimal missingQuantity = lineContext.MissingQuantity;
            SAPbouiCOM.Matrix batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);

            SortByExpirationDate(batchMatrix);

            int lastDeliveryDocEntry = SalesOrderSapQueries.ObtainLastDeliveryDocEntryByItem(lineContext.ItemCode);
            DateTime? lastDeliveredExpirationDate = lastDeliveryDocEntry > 0
                ? SalesOrderSapQueries.ObtainMaxExpirationDateFromDelivery(lastDeliveryDocEntry, lineContext.ItemCode)
                : null;

            int row = 1;
            while (missingQuantity > 0) {
                batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
                if (batchMatrix == null || batchMatrix.RowCount <= 0)
                    break;

                int lastUsefulRow = GetLastUsefulBatchRow(batchMatrix);
                if (row > lastUsefulRow)
                    break;

                bool assignedInThisPass = false;

                for (; row <= lastUsefulRow; row++) {
                    AvailableBatchRow batch;
                    if (!TryReadValidAvailableBatchRow(batchMatrix, row, out batch))
                        continue;

                    if (lastDeliveredExpirationDate.HasValue && batch.ExpirationDate.Date < lastDeliveredExpirationDate.Value.Date)
                        continue;

                    int usefulLifeDays = (int)(batch.ExpirationDate.Date - batch.ProductionDate.Date).TotalDays;
                    if (usefulLifeDays <= 0)
                        continue;

                    int firstThird = usefulLifeDays / 3;
                    DateTime limitDate = batch.ExpirationDate.Date.AddDays(firstThird);

                    if (deliveryDate.Date >= limitDate)
                        continue;

                    decimal quantityToAssign = missingQuantity <= batch.BalanceQuantity ? missingQuantity : batch.BalanceQuantity;
                    SapUiMatrixAccessor.SetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchQuantityToAssignColumn, row, quantityToAssign);
                    ClickAssign(batchSelectionForm);
                    ClickOk(batchSelectionForm);

                    missingQuantity -= quantityToAssign;
                    if (batch.AssignedQuantity > 0)
                        row++;

                    assignedInThisPass = true;
                    break;
                }

                if (!assignedInThisPass)
                    break;
            }

            return missingQuantity;
        }

        private int GetLastUsefulBatchRow(SAPbouiCOM.Matrix batchMatrix) {
            int lastUsefulRow = 0;

            if (batchMatrix == null)
                return lastUsefulRow;

            for (int row = 1; row <= batchMatrix.RowCount; row++) {
                AvailableBatchRow batch;
                if (TryReadValidAvailableBatchRow(batchMatrix, row, out batch))
                    lastUsefulRow = row;
            }

            return lastUsefulRow;
        }

        private bool TryReadValidAvailableBatchRow(SAPbouiCOM.Matrix batchMatrix, int row, out AvailableBatchRow batch) {
            batch = null;

            if (!SapUiMatrixAccessor.IsValidRow(batchMatrix, row))
                return false;

            try {
                DateTime productionDate;
                DateTime expirationDate;
                if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchProductionDateColumn, row), out productionDate))
                    return false;
                if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row), out expirationDate))
                    return false;

                decimal availableQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAvailableQuantityColumn, row);
                decimal assignedQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAssignedQuantityColumn, row);
                decimal balanceQuantity = availableQuantity - assignedQuantity;

                if (availableQuantity <= 0 || balanceQuantity <= 0)
                    return false;

                batch = new AvailableBatchRow {
                    ProductionDate = productionDate,
                    ExpirationDate = expirationDate,
                    AvailableQuantity = availableQuantity,
                    AssignedQuantity = assignedQuantity,
                    BalanceQuantity = balanceQuantity
                };

                return true;
            }
            catch (Exception ex) {
                if (ex.Message != null && ex.Message.IndexOf("invalid row number", StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;

                throw;
            }
        }

        private class AvailableBatchRow {
            public DateTime ProductionDate { get; set; }
            public DateTime ExpirationDate { get; set; }
            public decimal AvailableQuantity { get; set; }
            public decimal AssignedQuantity { get; set; }
            public decimal BalanceQuantity { get; set; }
        }
    }
}
