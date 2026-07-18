using System;

namespace nuanaddon_varimar.DTO {
    public class AvailableBatchDto {
        private decimal availableQuantity;
        private decimal alreadyAssignedQuantity;
        private decimal remainingQuantity;

        public int SapRow { get; set; }
        public string ItemCode { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal AvailableQuantity {
            get { return availableQuantity; }
            set {
                availableQuantity = value;
                remainingQuantity = availableQuantity - alreadyAssignedQuantity;
            }
        }

        public decimal AlreadyAssignedQuantity {
            get { return alreadyAssignedQuantity; }
            set {
                alreadyAssignedQuantity = value;
                remainingQuantity = availableQuantity - alreadyAssignedQuantity;
            }
        }

        public decimal RemainingQuantity {
            get { return remainingQuantity; }
            set { remainingQuantity = value; }
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
            set { RemainingQuantity = value; }
        }
    }
}
