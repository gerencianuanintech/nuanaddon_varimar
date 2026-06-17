using nuanaddon_varimar.Shared.Constants;

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
    }
}
