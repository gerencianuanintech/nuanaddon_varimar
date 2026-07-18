using nuanaddon_varimar.DTO;
using System;

namespace nuanaddon_varimar.Application.Services {
    public interface IBatchAssignmentRule {
        decimal AssignLine(SAPbouiCOM.Form batchSelectionForm, SalesOrderLineBatchContext lineContext, DateTime deliveryDate);
    }
}
