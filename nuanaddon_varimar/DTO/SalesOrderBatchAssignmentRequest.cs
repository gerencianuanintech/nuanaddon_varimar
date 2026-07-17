using System;
using System.Collections.Generic;

namespace nuanaddon_varimar.DTO {
    public class SalesOrderBatchAssignmentRequest {
        public string FormUid { get; set; }
        public string CardCode { get; set; }
        public DateTime DeliveryDate { get; set; }
        public IDictionary<string, IList<string>> WarehouseCodesByItem { get; set; }
    }
}
