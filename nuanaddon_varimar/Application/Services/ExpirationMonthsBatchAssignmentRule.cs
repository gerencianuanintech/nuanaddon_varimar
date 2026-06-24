using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Parsing;
using System;

namespace nuanaddon_varimar.Application.Services {
    public abstract class ExpirationMonthsBatchAssignmentRule : IBatchAssignmentRule {
        private readonly int monthsToAdd;

        protected ExpirationMonthsBatchAssignmentRule(int monthsToAdd) {
            this.monthsToAdd = monthsToAdd;
        }

        public virtual decimal AssignLine(SAPbouiCOM.Form batchSelectionForm, SalesOrderLineBatchContext lineContext, DateTime deliveryDate) {
            decimal missingQuantity = lineContext.MissingQuantity;
            SAPbouiCOM.Matrix batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);

            SortByExpirationDate(batchMatrix);

            int row = 1;
            while (missingQuantity > 0) {
                batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
                int lastUsefulRow = GetLastUsefulBatchRow(batchMatrix);
                if (lastUsefulRow <= 0 || row > lastUsefulRow)
                    break;

                AvailableBatchRowData batchRow;
                if (!TryReadUsefulBatchRow(batchMatrix, row, out batchRow)) {
                    row++;
                    continue;
                }

                if (!IsValidForExpirationMonthsRule(batchRow, deliveryDate)) {
                    row++;
                    continue;
                }

                BatchUiAssignmentResult assignmentResult = AssignAvailableBalance(batchSelectionForm, batchMatrix, batchRow, missingQuantity);
                missingQuantity = assignmentResult.MissingQuantity;

                if (!assignmentResult.StayOnSameRow)
                    row++;
            }

            return missingQuantity;
        }

        protected BatchUiAssignmentResult AssignAvailableBalance(SAPbouiCOM.Form batchSelectionForm, SAPbouiCOM.Matrix batchMatrix, AvailableBatchRowData batchRow, decimal missingQuantity) {
            decimal quantityToAssign = missingQuantity <= batchRow.BalanceQuantity ? missingQuantity : batchRow.BalanceQuantity;
            SapUiMatrixAccessor.SetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchQuantityToAssignColumn, batchRow.Row, quantityToAssign);
            ClickAssign(batchSelectionForm);

            bool stayOnSameRow = batchRow.AssignedQuantity == 0;
            return new BatchUiAssignmentResult(missingQuantity - quantityToAssign, stayOnSameRow);
        }

        protected virtual bool IsValidForExpirationMonthsRule(AvailableBatchRowData batchRow, DateTime deliveryDate) {
            return batchRow.ExpirationDate.Date > deliveryDate.Date.AddMonths(monthsToAdd);
        }

        protected int GetLastUsefulBatchRow(SAPbouiCOM.Matrix batchMatrix) {
            int lastUsefulRow = 0;

            if (batchMatrix == null)
                return lastUsefulRow;

            for (int row = 1; row <= batchMatrix.RowCount; row++) {
                AvailableBatchRowData batchRow;
                if (TryReadUsefulBatchRow(batchMatrix, row, out batchRow))
                    lastUsefulRow = row;
            }

            return lastUsefulRow;
        }

        protected bool TryReadUsefulBatchRow(SAPbouiCOM.Matrix batchMatrix, int row, out AvailableBatchRowData rowData) {
            rowData = null;

            if (!SapUiMatrixAccessor.IsValidRow(batchMatrix, row))
                return false;

            string productionDateValue;
            string expirationDateValue;
            if (!SapUiMatrixAccessor.TryGetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchProductionDateColumn, row, out productionDateValue))
                return false;
            if (!SapUiMatrixAccessor.TryGetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row, out expirationDateValue))
                return false;

            DateTime productionDate;
            DateTime expirationDate;
            if (!SapValueParser.TryParseDate(productionDateValue, out productionDate))
                return false;
            if (!SapValueParser.TryParseDate(expirationDateValue, out expirationDate))
                return false;

            decimal availableQuantity;
            decimal assignedQuantity;
            if (!SapUiMatrixAccessor.TryGetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAvailableQuantityColumn, row, out availableQuantity))
                return false;
            if (!SapUiMatrixAccessor.TryGetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAssignedQuantityColumn, row, out assignedQuantity))
                return false;

            decimal balanceQuantity = availableQuantity - assignedQuantity;
            if (availableQuantity <= 0 || balanceQuantity <= 0)
                return false;

            rowData = new AvailableBatchRowData {
                Row = row,
                ProductionDate = productionDate,
                ExpirationDate = expirationDate,
                AvailableQuantity = availableQuantity,
                AssignedQuantity = assignedQuantity,
                BalanceQuantity = balanceQuantity
            };

            return true;
        }

        protected void SortByExpirationDate(SAPbouiCOM.Matrix batchMatrix) {
            try {
                batchMatrix.Columns.Item(SapBatchSelectionUiIds.BatchExpirationDateColumn).TitleObject.Click(SAPbouiCOM.BoCellClickType.ct_Double);
            }
            catch {
                // If SAP UI does not allow sorting in a specific patch level, keep current visual order like B1UP would.
            }
        }

        protected void ClickAssign(SAPbouiCOM.Form batchSelectionForm) {
            batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AssignButton).Click();
        }

        protected SAPbouiCOM.Matrix GetAvailableBatchMatrix(SAPbouiCOM.Form batchSelectionForm) {
            return (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AvailableBatchesMatrix).Specific;
        }

        protected void ClickOk(SAPbouiCOM.Form batchSelectionForm) {
            batchSelectionForm.Items.Item(SapCommonUiIds.OkButton).Click();
        }

        protected class BatchUiAssignmentResult {
            public BatchUiAssignmentResult(decimal missingQuantity, bool stayOnSameRow) {
                MissingQuantity = missingQuantity;
                StayOnSameRow = stayOnSameRow;
            }

            public decimal MissingQuantity { get; private set; }
            public bool StayOnSameRow { get; private set; }
        }

        protected class AvailableBatchRowData {
            public int Row { get; set; }
            public DateTime ProductionDate { get; set; }
            public DateTime ExpirationDate { get; set; }
            public decimal AvailableQuantity { get; set; }
            public decimal AssignedQuantity { get; set; }
            public decimal BalanceQuantity { get; set; }
        }
    }
}
