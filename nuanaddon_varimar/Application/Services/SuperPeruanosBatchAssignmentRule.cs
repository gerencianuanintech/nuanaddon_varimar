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
                    AvailableBatchRowData batchRow;
                    if (!TryReadUsefulBatchRow(batchMatrix, row, out batchRow))
                        continue;

                    if (lastDeliveredExpirationDate.HasValue && batchRow.ExpirationDate.Date < lastDeliveredExpirationDate.Value.Date)
                        continue;

                    int usefulLifeDays = (int)(batchRow.ExpirationDate.Date - batchRow.ProductionDate.Date).TotalDays;
                    if (usefulLifeDays <= 0)
                        continue;

                    int firstThird = usefulLifeDays / 3;
                    DateTime limitDate = batchRow.ExpirationDate.Date.AddDays(firstThird);

                    if (deliveryDate.Date >= limitDate)
                        continue;

                    BatchUiAssignmentResult assignmentResult = AssignAvailableBalance(batchSelectionForm, batchMatrix, batchRow, missingQuantity);
                    missingQuantity = assignmentResult.MissingQuantity;

                    if (!assignmentResult.StayOnSameRow)
                        row++;

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
