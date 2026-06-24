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
            SAPbouiCOM.Matrix batchMatrix = (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.AvailableBatchesMatrix).Specific;

            SortByExpirationDate(batchMatrix);

            int lastDeliveryDocEntry = SalesOrderSapQueries.ObtainLastDeliveryDocEntryByItem(lineContext.ItemCode);
            DateTime? lastDeliveredExpirationDate = lastDeliveryDocEntry > 0
                ? SalesOrderSapQueries.ObtainMaxExpirationDateFromDelivery(lastDeliveryDocEntry, lineContext.ItemCode)
                : null;

            int row = 1;
            while (row <= batchMatrix.RowCount && missingQuantity > 0) {
                DateTime productionDate;
                DateTime expirationDate;
                if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchProductionDateColumn, row), out productionDate)) {
                    row++;
                    continue;
                }
                if (!SapValueParser.TryParseDate(SapUiMatrixAccessor.GetEditTextValue(batchMatrix, SapBatchSelectionUiIds.BatchExpirationDateColumn, row), out expirationDate)) {
                    row++;
                    continue;
                }

                if (lastDeliveredExpirationDate.HasValue && expirationDate.Date < lastDeliveredExpirationDate.Value.Date) {
                    row++;
                    continue;
                }

                int usefulLifeDays = (int)(expirationDate.Date - productionDate.Date).TotalDays;
                int firstThird = usefulLifeDays / 3;
                DateTime limitDate = expirationDate.Date.AddDays(firstThird);

                if (deliveryDate.Date >= limitDate) {
                    row++;
                    continue;
                }

                decimal previousMissingQuantity = missingQuantity;
                missingQuantity = AssignAvailableBalance(batchSelectionForm, batchMatrix, row, missingQuantity);
                if (missingQuantity == previousMissingQuantity)
                    row++;
            }

            return missingQuantity;
        }
    }
}
