namespace nuanaddon_varimar.DTO {
    public class SalesOrderLineBatchContext {
        public int Row { get; set; }
        public string ItemCode { get; set; }
        public string WarehouseCode { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal SelectedQuantity { get; set; }
        public decimal MissingQuantity {
            get { return RequiredQuantity - SelectedQuantity; }
        }
    }
}
