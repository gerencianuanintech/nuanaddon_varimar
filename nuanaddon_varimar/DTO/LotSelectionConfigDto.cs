using nuanaddon_varimar.Shared.Constants;

namespace nuanaddon_varimar.DTO {
    public class LotSelectionConfigDto {
        public int BufferDays { get; set; } = 7;
        public string SelectionCriterion { get; set; } = LotSelectionCriteria.MenorCantidad;
    }
}
