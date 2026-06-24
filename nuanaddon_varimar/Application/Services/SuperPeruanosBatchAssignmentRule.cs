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

            while (missingQuantity > 0) {
                batchMatrix = GetAvailableBatchMatrix(batchSelectionForm);
                if (batchMatrix == null || batchMatrix.RowCount <= 0)
                    break;

                bool assignedInThisPass = false;

                for (int row = 1; row <= batchMatrix.RowCount; row++) {
                    if (!SapUiMatrixAccessor.IsValidRow(batchMatrix, row))
                        break;

                    DateTime productionDate;
                    DateTime expirationDate;
                    if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchProductionDateColumn, row), out productionDate))
                        continue;
                    if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row), out expirationDate))
                        continue;

                    if (lastDeliveredExpirationDate.HasValue && expirationDate.Date < lastDeliveredExpirationDate.Value.Date)
                        continue;

                    decimal availableQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAvailableQuantityColumn, row);
                    decimal assignedQuantity = SapUiMatrixAccessor.GetDecimalValue(batchMatrix, SapBatchSelectionUiIds.BatchAssignedQuantityColumn, row);
                    decimal balanceQuantity = availableQuantity - assignedQuantity;
                    if (balanceQuantity <= 0)
                        continue;

                    int usefulLifeDays = (int)(expirationDate.Date - productionDate.Date).TotalDays;
                    if (usefulLifeDays <= 0)
                        continue;

                    int firstThird = usefulLifeDays / 3;
                    DateTime limitDate = expirationDate.Date.AddDays(firstThird);

                    if (deliveryDate.Date >= limitDate)
                        continue;

                    decimal quantityToAssign = missingQuantity <= balanceQuantity ? missingQuantity : balanceQuantity;
                    SapUiMatrixAccessor.SetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchQuantityToAssignColumn, row, quantityToAssign);
                    ClickAssign(batchSelectionForm);
                    ClickOk(batchSelectionForm);

                    missingQuantity -= quantityToAssign;
                    assignedInThisPass = true;
                    break;
                }

                if (!assignedInThisPass)
                    break;
            }

            return missingQuantity;
        }
    }
}
