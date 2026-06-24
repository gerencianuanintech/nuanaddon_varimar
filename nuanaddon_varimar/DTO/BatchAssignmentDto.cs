namespace nuanaddon_varimar.DTO {
    public class BatchAssignmentDto {
        public int SapRow { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public decimal QuantityToAssign { get; set; }
    }
}
