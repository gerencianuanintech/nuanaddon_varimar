using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using System;

namespace nuanaddon_varimar.Application.Services {
    public class SalesOrderBatchAssignmentService {
        public BatchAssignmentResult Assign(SAPbouiCOM.Form batchSelectionForm, SalesOrderBatchAssignmentRequest request) {
            try {
                IBatchAssignmentRule rule = BatchAssignmentCustomerService.CreateRule(request.CardCode);
                if (rule == null)
                    return BatchAssignmentResult.Ok();

                SAPbouiCOM.Matrix lineMatrix = (SAPbouiCOM.Matrix)batchSelectionForm.Items.Item(SapBatchSelectionUiIds.LinesMatrix).Specific;

                for (int row = 1; row <= lineMatrix.RowCount; row++) {
                    lineMatrix.Columns.Item(SapBatchSelectionUiIds.LineItemCodeColumn).Cells.Item(row).Click();

                    SalesOrderLineBatchContext lineContext = new SalesOrderLineBatchContext {
                        Row = row,
                        ItemCode = SapUiMatrixAccessor.GetEditTextValue(lineMatrix, SapBatchSelectionUiIds.LineItemCodeColumn, row),
                        SelectedQuantity = SapUiMatrixAccessor.GetDecimalValue(lineMatrix, SapBatchSelectionUiIds.LineSelectedQuantityColumn, row),
                        RequiredQuantity = SapUiMatrixAccessor.GetDecimalValue(lineMatrix, SapBatchSelectionUiIds.LineRequiredQuantityColumn, row)
                    };

                    if (lineContext.MissingQuantity <= 0)
                        continue;

                    decimal remaining = rule.AssignLine(batchSelectionForm, lineContext, request.DeliveryDate);
                    if (remaining > 0)
                        return BatchAssignmentResult.Fail(SapMessages.NoAvailableBatchesForCustomerRulesDetail(lineContext.ItemCode, remaining));
                }

                return BatchAssignmentResult.Ok();
            }
            catch (Exception ex) {
                SapMessages.Error("Application.Services.SalesOrderBatchAssignmentService.cs -> Assign", ex);
                return BatchAssignmentResult.Fail(ex.Message);
            }
        }
    }
}
