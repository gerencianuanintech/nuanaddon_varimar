using nuanaddon_varimar.DTO;
using System;
using System.Collections.Generic;

namespace nuanaddon_varimar.Application.Services {
    public interface ILotAssignmentPlannerService {
        IList<BatchAssignmentDto> PlanAssignments(
            string cardCode,
            string itemCode,
            decimal requiredQuantity,
            DateTime deliveryDate,
            IEnumerable<AvailableBatchDto> availableBatches,
            LotSelectionConfigDto config);
    }
}
