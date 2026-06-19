using nuanaddon_varimar.Application.Services;
using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using nuanaddon_varimar.Shared.Parsing;
using SAPbouiCOM;
using System;
using System.Threading;

namespace nuanaddon_varimar.LinkSAP {
    public class LnkSalesOrder_0000000139 : LnkSAP {
        protected override void HandleItemEventAfter(ref ItemEvent pVal) {
            try {
                switch (pVal.EventType) {
                    case BoEventTypes.et_FORM_DRAW:
                    case BoEventTypes.et_FORM_LOAD:
                        AddAssignBatchButton();
                        break;
                    case BoEventTypes.et_ITEM_PRESSED:
                        HandleItemPressedAfter(ref pVal);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) {
                SapMessages.Error("LinkSAP.LnkSalesOrder_0000000139.cs -> HandleItemEventAfter", ex);
            }
        }

        private void AddAssignBatchButton() {
            if (ItemExists(SapSalesOrderUiIds.AssignBatchButton))
                return;

            oItemReference = oForm.Items.Item(SapSalesOrderUiIds.DocumentDateItem);
            oItem = oForm.Items.Add(SapSalesOrderUiIds.AssignBatchButton, BoFormItemTypes.it_BUTTON);
            oItem.Left = oItemReference.Left;
            oItem.Top = oItemReference.Top + oItemReference.Height + 6;
            oItem.Width = 95;
            oItem.Height = oItemReference.Height + 6;
            oItem.FromPane = oItemReference.FromPane;
            oItem.ToPane = oItemReference.ToPane;

            oButton = (Button)oItem.Specific;
            oButton.Caption = SapSalesOrderUiIds.AssignBatchButtonCaption;
        }

        private void HandleItemPressedAfter(ref ItemEvent pVal) {
            if (!SapSalesOrderUiIds.AssignBatchButton.Equals(pVal.ItemUID))
                return;

            string deliveryDateValue = GetEditTextValue(SapSalesOrderUiIds.DeliveryDateItem);
            DateTime deliveryDate;
            if (!SapValueParser.TryParseDate(deliveryDateValue, out deliveryDate)) {
                SapMessages.MessageBox(SapMessages.DeliveryDateRequired);
                return;
            }

            string cardCode = GetEditTextValue(SapSalesOrderUiIds.CardCodeItem);
            if (!BatchAssignmentCustomerService.IsSupportedCustomer(cardCode))
                return;

            ClickFirstSalesOrderLineBatchColumn();
            Program.SBOApplication.ActivateMenuItem("5896");

            SAPbouiCOM.Form batchSelectionForm = WaitForBatchSelectionForm();
            if (batchSelectionForm == null) {
                SapMessages.Error("No se pudo abrir la ventana de seleccion de lotes");
                return;
            }

            BatchAssignmentResult result = new SalesOrderBatchAssignmentService().Assign(
                batchSelectionForm,
                new SalesOrderBatchAssignmentRequest {
                    FormUid = oForm.UniqueID,
                    CardCode = cardCode,
                    DeliveryDate = deliveryDate
                });

            if (!result.Success && !string.IsNullOrWhiteSpace(result.Message))
                SapMessages.MessageBox(result.Message);
        }

        private string GetEditTextValue(string itemId) {
            return ((EditText)oForm.Items.Item(itemId).Specific).Value;
        }

        private bool ItemExists(string itemId) {
            try {
                oForm.Items.Item(itemId);
                return true;
            }
            catch {
                return false;
            }
        }

        private void ClickFirstSalesOrderLineBatchColumn() {
            oMatrix = (Matrix)oForm.Items.Item(SapSalesOrderUiIds.LinesMatrix).Specific;
            if (oMatrix.RowCount > 0)
                oMatrix.Columns.Item(SapSalesOrderUiIds.BatchSelectionColumn).Cells.Item(1).Click();
        }

        private SAPbouiCOM.Form WaitForBatchSelectionForm() {
            for (int attempt = 0; attempt < 20; attempt++) {
                for (int index = 0; index < Program.SBOApplication.Forms.Count; index++) {
                    SAPbouiCOM.Form form = Program.SBOApplication.Forms.Item(index);
                    if (SapFormTypeEx.BatchNumberSelection.Equals(form.TypeEx))
                        return form;
                }

                Thread.Sleep(100);
            }

            return null;
        }
    }
}
