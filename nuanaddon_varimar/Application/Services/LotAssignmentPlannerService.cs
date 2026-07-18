using nuanaddon_varimar.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nuanaddon_varimar.Application.Services {
    public class LotAssignmentPlannerService : ILotAssignmentPlannerService {
        private readonly ILotSelectionService lotSelectionService;

        public LotAssignmentPlannerService() : this(new LotSelectionService()) {
        }

        public LotAssignmentPlannerService(ILotSelectionService lotSelectionService) {
            this.lotSelectionService = lotSelectionService;
        }

        public IList<BatchAssignmentDto> PlanAssignments(
            string cardCode,
            string itemCode,
            decimal requiredQuantity,
            DateTime deliveryDate,
            IEnumerable<AvailableBatchDto> availableBatches,
            LotSelectionConfigDto config) {
            if (requiredQuantity <= 0 || availableBatches == null)
                return new List<BatchAssignmentDto>();

            DateTime baseDate = BatchAssignmentCustomerService.CalculateBaseDate(deliveryDate, config);
            IList<AvailableBatchDto> validBatches = availableBatches
                .Where(batch => BatchAssignmentCustomerService.BatchPassesCustomerRule(cardCode, batch, baseDate))
                .ToList();

            return lotSelectionService.BuildAssignments(itemCode, requiredQuantity, validBatches, config);
        }
    }
}
