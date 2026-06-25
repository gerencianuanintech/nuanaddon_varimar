using nuanaddon_varimar.DTO;
using System.Collections.Generic;

namespace nuanaddon_varimar.Application.Services {
    public interface ILotSelectionService {
        IList<BatchAssignmentDto> BuildAssignments(
            string itemCode,
            decimal requiredQuantity,
            IEnumerable<AvailableBatchDto> availableBatches,
            LotSelectionConfigDto config);
    }
}
