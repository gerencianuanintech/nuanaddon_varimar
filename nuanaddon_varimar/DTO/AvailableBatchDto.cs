using System;

namespace nuanaddon_varimar.DTO {
    public class AvailableBatchDto {
        public int SapRow { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal AlreadyAssignedQuantity { get; set; }

        public decimal RemainingQuantity {
            get { return AvailableQuantity - AlreadyAssignedQuantity; }
        }

        public int Row {
            get { return SapRow; }
            set { SapRow = value; }
        }

        public decimal AssignedQuantity {
            get { return AlreadyAssignedQuantity; }
            set { AlreadyAssignedQuantity = value; }
        }

        public decimal BalanceQuantity {
            get { return RemainingQuantity; }
        }
    }
}
