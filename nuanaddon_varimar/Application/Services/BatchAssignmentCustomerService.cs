using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using System;

namespace nuanaddon_varimar.Application.Services {
    public static class BatchAssignmentCustomerService {
        public static bool IsSupportedCustomer(string cardCode) {
            return CustomerCodes.Cencosud.Equals(cardCode) ||
                   CustomerCodes.SuperPeruanos.Equals(cardCode) ||
                   CustomerCodes.Tottus.Equals(cardCode);
        }

        public static IBatchAssignmentRule CreateRule(string cardCode) {
            switch (cardCode) {
                case CustomerCodes.Cencosud:
                    return new CencosudBatchAssignmentRule();
                case CustomerCodes.SuperPeruanos:
                    return new SuperPeruanosBatchAssignmentRule();
                case CustomerCodes.Tottus:
                    return new TottusBatchAssignmentRule();
                default:
                    return null;
            }
        }

        public static DateTime CalculateBaseDate(DateTime deliveryDate, LotSelectionConfigDto config) {
            int bufferDays = config == null ? 7 : config.BufferDays;
            if (bufferDays < 0)
                bufferDays = 7;

            return deliveryDate.Date.AddDays(bufferDays);
        }

        public static bool BatchPassesCustomerRule(string cardCode, AvailableBatchDto batch, DateTime baseDate) {
            if (batch == null)
                return false;

            switch (cardCode) {
                case CustomerCodes.Cencosud:
                    return PassesExpirationMonthsRule(batch, baseDate, 4);
                case CustomerCodes.Tottus:
                    return PassesExpirationMonthsRule(batch, baseDate, 5);
                case CustomerCodes.SuperPeruanos:
                    return PassesSuperPeruanosRule(batch, baseDate);
                default:
                    return true;
            }
        }

        private static bool PassesExpirationMonthsRule(AvailableBatchDto batch, DateTime baseDate, int monthsToAdd) {
            return batch.ExpirationDate.HasValue &&
                   DateTime.Compare(batch.ExpirationDate.Value.Date, baseDate.Date.AddMonths(monthsToAdd)) > 0;
        }

        private static bool PassesSuperPeruanosRule(AvailableBatchDto batch, DateTime baseDate) {
            if (!batch.ProductionDate.HasValue || !batch.ExpirationDate.HasValue)
                return false;

            int lastDeliveryDocEntry = SalesOrderSapQueries.ObtainLastDeliveryDocEntryByItem(batch.ItemCode);
            DateTime? lastDeliveredExpirationDate = lastDeliveryDocEntry > 0
                ? SalesOrderSapQueries.ObtainMaxExpirationDateFromDelivery(lastDeliveryDocEntry, batch.ItemCode)
                : null;

            if (lastDeliveredExpirationDate.HasValue && batch.ExpirationDate.Value.Date < lastDeliveredExpirationDate.Value.Date)
                return false;

            int usefulLifeDays = (int)(batch.ExpirationDate.Value.Date - batch.ProductionDate.Value.Date).TotalDays;
            int firstThird = usefulLifeDays / 3;
            DateTime limitDate = batch.ExpirationDate.Value.Date.AddDays(firstThird);

            return baseDate.Date < limitDate;
        }
    }
}
