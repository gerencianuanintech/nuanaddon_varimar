using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nuanaddon_varimar.Form
{
    [FormAttribute("nuanaddon_varimar.Form.FrmDocumento", "Form/FrmDocumento.b1f")]
    class FrmDocumento : UserFormBase
    {
        public FrmDocumento()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("sttNro").Specific));
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("edtNro").Specific));
            this.StaticText1 = ((SAPbouiCOM.StaticText)(this.GetItem("sttRuta").Specific));
            this.EditText1 = ((SAPbouiCOM.EditText)(this.GetItem("edtRuta").Specific));
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("btnFind").Specific));
            this.Button1 = ((SAPbouiCOM.Button)(this.GetItem("btnLoad").Specific));
            this.StaticText2 = ((SAPbouiCOM.StaticText)(this.GetItem("sttFecha").Specific));
            this.EditText2 = ((SAPbouiCOM.EditText)(this.GetItem("edtFecha").Specific));
            this.Button2 = ((SAPbouiCOM.Button)(this.GetItem("1").Specific));
            this.Button3 = ((SAPbouiCOM.Button)(this.GetItem("2").Specific));
            this.Matrix1 = ((SAPbouiCOM.Matrix)(this.GetItem("mtxDet").Specific));
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
        }

        private void OnCustomInitialize()
        {

        }

        private SAPbouiCOM.StaticText StaticText0;
        private SAPbouiCOM.EditText EditText0;
        private SAPbouiCOM.StaticText StaticText1;
        private SAPbouiCOM.EditText EditText1;
        private SAPbouiCOM.Button Button0;
        private SAPbouiCOM.Button Button1;
        private SAPbouiCOM.StaticText StaticText2;
        private SAPbouiCOM.EditText EditText2;
        private SAPbouiCOM.Button Button2;
        private SAPbouiCOM.Button Button3;
        private SAPbouiCOM.Matrix Matrix1;
    }
}
