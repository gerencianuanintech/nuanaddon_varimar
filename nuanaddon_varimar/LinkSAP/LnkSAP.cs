namespace nuanaddon_varimar.LinkSAP {
    public class LnkSAP {
        protected SAPbouiCOM.Form oForm;
        protected SAPbouiCOM.Item oItem;
        protected SAPbouiCOM.Item oItemReference;
        protected SAPbouiCOM.Button oButton;
        protected SAPbouiCOM.EditText oEditText;
        protected SAPbouiCOM.Matrix oMatrix;

        protected void InitializeForm(string formUid) {
            oForm = Program.SBOApplication.Forms.Item(formUid);
        }

        public virtual void HandleItemEvent(string formUid, ref SAPbouiCOM.ItemEvent pVal, ref bool bubbleEvent) {
            InitializeForm(formUid);

            if (pVal.BeforeAction)
                HandleItemEventBefore(ref pVal, ref bubbleEvent);
            else
                HandleItemEventAfter(ref pVal);
        }

        public virtual void HandleFormDataEvent(ref SAPbouiCOM.BusinessObjectInfo pVal, ref bool bubbleEvent) {
            InitializeForm(pVal.FormUID);

            if (pVal.BeforeAction)
                HandleFormDataEventBefore(ref pVal, ref bubbleEvent);
            else
                HandleFormDataEventAfter(ref pVal);
        }

        protected virtual void HandleItemEventBefore(ref SAPbouiCOM.ItemEvent pVal, ref bool bubbleEvent) { }
        protected virtual void HandleItemEventAfter(ref SAPbouiCOM.ItemEvent pVal) { }
        protected virtual void HandleFormDataEventBefore(ref SAPbouiCOM.BusinessObjectInfo pVal, ref bool bubbleEvent) { }
        protected virtual void HandleFormDataEventAfter(ref SAPbouiCOM.BusinessObjectInfo pVal) { }
    }
}
