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
                if (row > batchMatrix.RowCount)
                    break;

                DateTime expirationDate;
                if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row), out expirationDate)) {
                    row++;
                    continue;
                }

                if (DateTime.Compare(expirationDate.Date, deliveryDate.Date.AddMonths(monthsToAdd)) <= 0) {
                    row++;
                    continue;
                }

                BatchUiAssignmentResult assignmentResult = AssignAvailableBalance(batchSelectionForm, batchMatrix, row, missingQuantity);
                missingQuantity = assignmentResult.MissingQuantity;

                if (!assignmentResult.StayOnSameRow)
                    row++;

                batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
                if (row > batchMatrix.RowCount)
                    break;
            }

            return missingQuantity;
        }

        protected BatchUiAssignmentResult AssignAvailableBalance(SAPbouiCOM.Form batchSelectionForm, SAPbouiCOM.Matrix batchMatrix, int row, decimal missingQuantity) {
            decimal availableQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAvailableQuantityColumn, row);
            decimal assignedQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAssignedQuantityColumn, row);
            decimal balanceQuantity = availableQuantity - assignedQuantity;

            if (balanceQuantity <= 0)
                return new BatchUiAssignmentResult(missingQuantity, false);

            decimal quantityToAssign = missingQuantity <= balanceQuantity ? missingQuantity : balanceQuantity;
            SapUiMatrixAccessor.SetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchQuantityToAssignColumn, row, quantityToAssign);
            ClickAssign(batchSelectionForm);
            ClickOk(batchSelectionForm);

            bool stayOnSameRow = missingQuantity > balanceQuantity && assignedQuantity == 0;
            return new BatchUiAssignmentResult(missingQuantity - quantityToAssign, stayOnSameRow);
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
    }
}
