using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nuanaddon_varimar.Application.Services {
    public class LotSelectionService : ILotSelectionService {
        public IList<BatchAssignmentDto> BuildAssignments(
            string itemCode,
            decimal requiredQuantity,
            IEnumerable<AvailableBatchDto> availableBatches,
            LotSelectionConfigDto config) {
            IList<BatchAssignmentDto> assignments = new List<BatchAssignmentDto>();

            if (requiredQuantity <= 0 || availableBatches == null)
                return assignments;

            string criterion = config == null
                ? LotSelectionCriteria.MenorCantidad
                : LotSelectionCriteria.NormalizeOrDefault(config.SelectionCriterion);

            IList<AvailableBatchDto> candidates = availableBatches
                .Where(batch => batch != null)
                .Where(batch => batch.SapRow > 0)
                .Where(batch => batch.RemainingQuantity > 0)
                .ToList();

            if (criterion == LotSelectionCriteria.MenorCantidadLoteUnico)
                return BuildSingleBatchAssignment(itemCode, requiredQuantity, candidates);

            IOrderedEnumerable<AvailableBatchDto> ordered = criterion == LotSelectionCriteria.MayorCantidad
                ? candidates.OrderByDescending(batch => batch.RemainingQuantity).ThenBy(batch => batch.BatchNumber).ThenBy(batch => batch.SapRow)
                : candidates.OrderBy(batch => batch.RemainingQuantity).ThenBy(batch => batch.BatchNumber).ThenBy(batch => batch.SapRow);

            decimal remaining = requiredQuantity;
            foreach (AvailableBatchDto batch in ordered) {
                if (remaining <= 0)
                    break;

                decimal quantityToAssign = Math.Min(remaining, batch.RemainingQuantity);
                if (quantityToAssign <= 0)
                    continue;

                assignments.Add(CreateAssignment(itemCode, batch, quantityToAssign));
                remaining -= quantityToAssign;
            }

            return assignments;
        }

        private IList<BatchAssignmentDto> BuildSingleBatchAssignment(string itemCode, decimal requiredQuantity, IList<AvailableBatchDto> candidates) {
            IList<BatchAssignmentDto> assignments = new List<BatchAssignmentDto>();

            AvailableBatchDto batch = candidates
                .Where(candidate => candidate.RemainingQuantity >= requiredQuantity)
                .OrderBy(candidate => candidate.RemainingQuantity)
                .ThenBy(candidate => candidate.BatchNumber)
                .ThenBy(candidate => candidate.SapRow)
                .FirstOrDefault();

            if (batch == null)
                return assignments;

            assignments.Add(CreateAssignment(itemCode, batch, requiredQuantity));
            return assignments;
        }

        private BatchAssignmentDto CreateAssignment(string itemCode, AvailableBatchDto batch, decimal quantityToAssign) {
            return new BatchAssignmentDto {
                SapRow = batch.SapRow,
                ItemCode = itemCode,
                BatchNumber = batch.BatchNumber,
                QuantityToAssign = quantityToAssign
            };
        }
    }
}
