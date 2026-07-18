namespace nuanaddon_varimar.DTO {
    public class BatchAssignmentDto {
        public int SapRow { get; set; }
        public string ItemCode { get; set; }
        public string BatchNumber { get; set; }
        public decimal QuantityToAssign { get; set; }
    }
}
