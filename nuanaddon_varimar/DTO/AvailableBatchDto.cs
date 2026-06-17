using System;

namespace nuanaddon_varimar.DTO {
    public class AvailableBatchDto {
        public int Row { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal AssignedQuantity { get; set; }
        public decimal BalanceQuantity {
            get { return AvailableQuantity - AssignedQuantity; }
        }
    }
}
