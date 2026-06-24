using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nuanaddon_varimar.Application.Services {
    public class LotSelectionService : ILotSelectionService {
        public IReadOnlyList<BatchAssignmentDto> BuildAssignments(
            string itemCode,
            decimal requiredQuantity,
            IEnumerable<AvailableBatchDto> availableBatches,
            LotSelectionConfigDto config) {
            if (requiredQuantity <= 0 || availableBatches == null)
                return new List<BatchAssignmentDto>();

            LotSelectionConfigDto safeConfig = config ?? new LotSelectionConfigDto();
            List<AvailableBatchDto> batches = availableBatches
                .Where(batch => batch != null)
                .Where(batch => batch.RemainingQuantity > 0)
                .Where(batch => !string.IsNullOrWhiteSpace(batch.BatchNumber))
                .ToList();

            string criterion = NormalizeCriterion(safeConfig.SelectionCriterion);
            if (LotSelectionCriteria.MenorCantidadLoteUnico.Equals(criterion, StringComparison.OrdinalIgnoreCase))
                return BuildSingleBatchAssignment(itemCode, requiredQuantity, batches);

            IEnumerable<AvailableBatchDto> orderedBatches = OrderBatches(batches, criterion);
            return BuildMultiBatchAssignments(itemCode, requiredQuantity, orderedBatches);
        }

        private IReadOnlyList<BatchAssignmentDto> BuildSingleBatchAssignment(string itemCode, decimal requiredQuantity, IEnumerable<AvailableBatchDto> batches) {
            AvailableBatchDto selectedBatch = batches
                .Where(batch => batch.RemainingQuantity >= requiredQuantity)
                .OrderBy(batch => batch.RemainingQuantity)
                .ThenBy(batch => batch.BatchNumber, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (selectedBatch == null)
                return new List<BatchAssignmentDto>();

            return new List<BatchAssignmentDto> {
                CreateAssignment(itemCode, selectedBatch, requiredQuantity)
            };
        }

        private IReadOnlyList<BatchAssignmentDto> BuildMultiBatchAssignments(string itemCode, decimal requiredQuantity, IEnumerable<AvailableBatchDto> orderedBatches) {
            List<BatchAssignmentDto> assignments = new List<BatchAssignmentDto>();
            decimal missingQuantity = requiredQuantity;

            foreach (AvailableBatchDto batch in orderedBatches) {
                if (missingQuantity <= 0)
                    break;

                decimal quantityToAssign = missingQuantity <= batch.RemainingQuantity
                    ? missingQuantity
                    : batch.RemainingQuantity;

                assignments.Add(CreateAssignment(itemCode, batch, quantityToAssign));
                missingQuantity -= quantityToAssign;
            }

            return assignments;
        }

        private IEnumerable<AvailableBatchDto> OrderBatches(IEnumerable<AvailableBatchDto> batches, string criterion) {
            if (LotSelectionCriteria.MayorCantidad.Equals(criterion, StringComparison.OrdinalIgnoreCase)) {
                return batches
                    .OrderByDescending(batch => batch.RemainingQuantity)
                    .ThenBy(batch => batch.BatchNumber, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return batches
                .OrderBy(batch => batch.RemainingQuantity)
                .ThenBy(batch => batch.BatchNumber, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private BatchAssignmentDto CreateAssignment(string itemCode, AvailableBatchDto batch, decimal quantityToAssign) {
            return new BatchAssignmentDto {
                SapRow = batch.SapRow,
                ItemCode = string.IsNullOrWhiteSpace(batch.ItemCode) ? itemCode ?? string.Empty : batch.ItemCode,
                BatchNumber = batch.BatchNumber,
                QuantityToAssign = quantityToAssign
            };
        }

        private string NormalizeCriterion(string criterion) {
            string normalized = string.IsNullOrWhiteSpace(criterion)
                ? LotSelectionCriteria.MenorCantidad
                : criterion.Trim().ToUpperInvariant();

            return LotSelectionCriteria.ValidCriteria.Contains(normalized)
                ? normalized
                : LotSelectionCriteria.MenorCantidad;
        }
    }
}
