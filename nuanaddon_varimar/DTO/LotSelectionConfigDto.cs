using nuanaddon_varimar.Shared.Constants;

namespace nuanaddon_varimar.DTO {
    public class LotSelectionConfigDto {
        public LotSelectionConfigDto() {
            BufferDays = 7;
            SelectionCriterion = LotSelectionCriteria.MenorCantidad;
        }

        public int BufferDays { get; set; }
        public string SelectionCriterion { get; set; }
    }
}
