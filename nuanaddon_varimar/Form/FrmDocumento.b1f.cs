using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Utilities;
using OfficeOpenXml;
using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace nuanaddon_varimar.Form
{
    [FormAttribute("FrmDocumento", "Form/FrmDocumento.b1f")]
    class FrmDocumento : UserFormBase {
        #region Atributos
        private SAPbouiCOM.StaticText sttNro;
        private SAPbouiCOM.EditText edtNro;
        private SAPbouiCOM.StaticText sttRuta;
        private SAPbouiCOM.EditText edtRuta;
        private SAPbouiCOM.Button btnFind;
        private SAPbouiCOM.Button btnLoad;
        private SAPbouiCOM.StaticText sttFecha;
        private SAPbouiCOM.EditText edtFecha;
        private SAPbouiCOM.Button btnOk;
        private SAPbouiCOM.Button btnCancel;
        private SAPbouiCOM.Matrix mtxDetalle;
        private SAPbouiCOM.StaticText sttOri;
        private SAPbouiCOM.ComboBox cmbOri;
        private SAPbouiCOM.StaticText sttDes;
        private SAPbouiCOM.ComboBox cmbDes;
        private SAPbouiCOM.StaticText sttTrf;
        private SAPbouiCOM.EditText edtTrf;
        private SAPbouiCOM.Button btnTransfer;
        private SAPbouiCOM.EditText edtDocEntry;
        #endregion

        #region Constructores
        public FrmDocumento() { }
        #endregion

        #region Eventos
        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent() {
            this.sttNro = ((SAPbouiCOM.StaticText)(this.GetItem("sttNro").Specific));
            this.edtNro = ((SAPbouiCOM.EditText)(this.GetItem("edtNro").Specific));
            this.sttFecha = ((SAPbouiCOM.StaticText)(this.GetItem("sttFecha").Specific));
            this.edtFecha = ((SAPbouiCOM.EditText)(this.GetItem("edtFecha").Specific));
            this.sttRuta = ((SAPbouiCOM.StaticText)(this.GetItem("sttRuta").Specific));
            this.edtRuta = ((SAPbouiCOM.EditText)(this.GetItem("edtRuta").Specific));
            this.sttTrf = ((SAPbouiCOM.StaticText)(this.GetItem("sttTrf").Specific));
            this.edtTrf = ((SAPbouiCOM.EditText)(this.GetItem("edtTrf").Specific));
            this.sttOri = ((SAPbouiCOM.StaticText)(this.GetItem("sttOri").Specific));
            this.cmbOri = ((SAPbouiCOM.ComboBox)(this.GetItem("cmbOri").Specific));
            this.sttDes = ((SAPbouiCOM.StaticText)(this.GetItem("sttDes").Specific));
            this.cmbDes = ((SAPbouiCOM.ComboBox)(this.GetItem("cmbDes").Specific));
            this.btnFind = ((SAPbouiCOM.Button)(this.GetItem("btnFind").Specific));
            this.btnFind.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.btnFind_PressedAfter);
            this.btnLoad = ((SAPbouiCOM.Button)(this.GetItem("btnLoad").Specific));
            this.btnLoad.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.btnLoad_PressedAfter);
            this.btnOk = ((SAPbouiCOM.Button)(this.GetItem("1").Specific));
            this.btnOk.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.btnOk_PressedAfter);
            this.btnCancel = ((SAPbouiCOM.Button)(this.GetItem("2").Specific));
            this.btnTransfer = ((SAPbouiCOM.Button)(this.GetItem("btnTrf").Specific));
            this.btnTransfer.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.btnTransfer_PressedAfter);
            this.mtxDetalle = ((SAPbouiCOM.Matrix)(this.GetItem("mtxDet").Specific));
            this.edtDocEntry = ((SAPbouiCOM.EditText)(this.GetItem("DocEntry").Specific));
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents() {
            try {
                Program.SBOApplication.MenuEvent += this.MenuEvent;
                this.DataLoadAfter += new DataLoadAfterHandler(this.Form_DataLoadAfter);
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> OnInitializeFormEvents: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OnCustomInitialize() {
            this.edtDocEntry.Item.Visible = false;

            InitializeComboWarehousesOrigin();
            InitializeComboWarehousesDestination();

            InitializeData();
        }
        private void btnFind_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal) {
            try {
                Process[] procesos = Process.GetProcessesByName("SAP Business One");
                // Obtiene el identificador de sesión del usuario actual
                int currentSessionId = Process.GetCurrentProcess().SessionId;

                foreach (var item in procesos) {
                    if (currentSessionId.Equals(item.SessionId) && item.MainWindowTitle.Contains(Program.SBOCompany.CompanyName)) {
                        WindowWrapper windowWrapper = new WindowWrapper(item.MainWindowHandle);

                        Thread objThread = new Thread(() => {
                            using (var openFileDialog = new OpenFileDialog()) {
                                openFileDialog.Filter = "Todos los archivos (*.*)|*.*";
                                var dialogResult = openFileDialog.ShowDialog(windowWrapper);
                                if (dialogResult == DialogResult.OK)
                                {
                                    string filePath = openFileDialog.FileName;
                                    this.edtRuta.Value = filePath;
                                }
                            }
                        });
                        // Kick off a new thread
                        objThread.SetApartmentState(ApartmentState.STA);
                        objThread.Start();
                        objThread.Join(); // Espera a que el hilo STA finalice
                    }
                }
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> btnFind_PressedAfter: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        //Modificar aqui
        private void btnLoad_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal) {
            UIAPIRawForm.Freeze(true);
            try {
                if (this.cmbOri.Selected == null && this.cmbDes.Selected == null) {
                    Program.SBOApplication.MessageBox("La bodega de origen y destino son requeridas");
                    return;
                }

                // Ruta del archivo Excel
                string filePath = edtRuta.Value;

                mtxDetalle.Clear();

                // Leer el archivo
                using (var package = new ExcelPackage(new FileInfo(filePath))) {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Primera hoja
                    if (worksheet?.Dimension == null) return;

                    int rows = worksheet.Dimension.Rows;
                    int i = 1;
                    decimal cant = 0;
                    decimal can1 = 0;
                    decimal can2 = 0;
                    decimal can3 = 0;
                    decimal can4 = 0;
                    decimal can5 = 0;
                    decimal can6 = 0;
                    decimal can7 = 0;
                    decimal can8 = 0;
                    decimal can9 = 0;
                    decimal can10 = 0;
                    decimal can11 = 0;
                    decimal can12 = 0;
                    decimal can13 = 0;
                    decimal can14 = 0;
                    decimal can15 = 0;

                    for (int row = 6; row <= rows - 1; row++) {
                        if (!string.IsNullOrEmpty(worksheet.Cells[row, 2].Text)) {
                            mtxDetalle.AddRow();
                            
                            // Limpiamos la fila para evitar datos "fantasma"
                            mtxDetalle.ClearRowData(i);

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("#").Cells.Item(i).Specific).Value = i.ToString();

                            string itemName = worksheet.Cells[row, 1].Text.Trim();
                            string distNumber = worksheet.Cells[row, 3].Text;
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_DES").Cells.Item(i).Specific).Value = itemName;
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_LOT").Cells.Item(i).Specific).Value = distNumber;
                            var fechaTexto = worksheet.Cells[row, 4].Text?.Trim();

                            if (string.IsNullOrEmpty(fechaTexto)) {
                                // Celda vacía: no ponemos nada
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_VEN").Cells.Item(i).Specific).Value = string.Empty;
                            }
                            else if (DateTime.TryParse(fechaTexto, out DateTime fechaValida)) {
                                // Fecha válida
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_VEN").Cells.Item(i).Specific).Value = fechaValida.ToString("yyyyMMdd");
                            }
                            else {
                                // Texto no vacío pero inválido → observación
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_VEN").Cells.Item(i).Specific).Value = string.Empty;
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Fecha vencimiento no válida";
                            }
                            var valCANT = ParseNumericCell(worksheet.Cells[row, 5], out bool invCANT);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CANT").Cells.Item(i).Specific).Value = valCANT;
                            if (invCANT) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cant. Total no valido";
                            else cant = decimal.TryParse(valCANT, out var tmpCANT) ? tmpCANT : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI1").Cells.Item(i).Specific).Value = worksheet.Cells[row, 6].Text;
                            var valCAN1 = ParseNumericCell(worksheet.Cells[row, 7], out bool invCAN1);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN1").Cells.Item(i).Specific).Value = valCAN1;
                            if (invCAN1) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 1 no valido";
                            else can1 = decimal.TryParse(valCAN1, out var tmpCAN1) ? tmpCAN1 : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI2").Cells.Item(i).Specific).Value = worksheet.Cells[row, 8].Text;
                            var valCAN2 = ParseNumericCell(worksheet.Cells[row, 9], out bool invCAN2);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN2").Cells.Item(i).Specific).Value = valCAN2;
                            if (invCAN2) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 2 no valido";
                            else can2 = decimal.TryParse(valCAN2, out var tmpCAN2) ? tmpCAN2 : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI3").Cells.Item(i).Specific).Value = worksheet.Cells[row, 10].Text;
                            var valCAN3 = ParseNumericCell(worksheet.Cells[row, 11], out bool invCAN3);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN3").Cells.Item(i).Specific).Value = valCAN3;
                            if (invCAN3) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 2 no valido";
                            else can3 = decimal.TryParse(valCAN3, out var tmpCAN3) ? tmpCAN3 : 0;

                            // FRRC se aumento 12 cantidades y ubicaciones
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI4").Cells.Item(i).Specific).Value = worksheet.Cells[row, 12].Text;
                            var valCAN4 = ParseNumericCell(worksheet.Cells[row, 13], out bool invCAN4);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN4").Cells.Item(i).Specific).Value = valCAN4;
                            if (invCAN4) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 4 no valido";
                            else can4 = decimal.TryParse(valCAN4, out var tmpCAN4) ? tmpCAN4 : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI5").Cells.Item(i).Specific).Value = worksheet.Cells[row, 14].Text;
                            var valCAN5 = ParseNumericCell(worksheet.Cells[row, 15], out bool invCAN5);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN5").Cells.Item(i).Specific).Value = valCAN5;
                            if (invCAN5) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 5 no valido";
                            else can5 = decimal.TryParse(valCAN5, out var tmpCAN5) ? tmpCAN5 : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI6").Cells.Item(i).Specific).Value = worksheet.Cells[row, 16].Text;
                            var valCAN6 = ParseNumericCell(worksheet.Cells[row, 17], out bool invCAN6);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN6").Cells.Item(i).Specific).Value = valCAN6;
                            if (invCAN6) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 6 no valido";
                            else can6 = decimal.TryParse(valCAN6, out var tmpCAN6) ? tmpCAN6 : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI7").Cells.Item(i).Specific).Value = worksheet.Cells[row, 18].Text;
                            var valCAN7 = ParseNumericCell(worksheet.Cells[row, 19], out bool invCAN7);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN7").Cells.Item(i).Specific).Value = valCAN7;
                            if (invCAN7) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 7 no valido";
                            else can7 = decimal.TryParse(valCAN7, out var tmpCAN7) ? tmpCAN7 : 0;

                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI8").Cells.Item(i).Specific).Value = worksheet.Cells[row, 20].Text;
                            var valCAN8 = ParseNumericCell(worksheet.Cells[row, 21], out bool invCAN8);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN8").Cells.Item(i).Specific).Value = valCAN8;
                            if (invCAN8) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 8 no valido";
                            else can8 = decimal.TryParse(valCAN8, out var tmpCAN8) ? tmpCAN8 : 0;


                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI9").Cells.Item(i).Specific).Value = worksheet.Cells[row, 22].Text;
                            var valCAN9 = ParseNumericCell(worksheet.Cells[row, 23], out bool invCAN9);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN9").Cells.Item(i).Specific).Value = valCAN9;
                            if (invCAN9) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 9 no valido";
                            else can9 = decimal.TryParse(valCAN9, out var tmpCAN9) ? tmpCAN9 : 0;


                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB10").Cells.Item(i).Specific).Value = worksheet.Cells[row, 24].Text;
                            var valCAN10 = ParseNumericCell(worksheet.Cells[row, 25], out bool invCAN10);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA10").Cells.Item(i).Specific).Value = valCAN10;
                            if (invCAN10) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 10 no valido";
                            else can10 = decimal.TryParse(valCAN10, out var tmpCAN10) ? tmpCAN10 : 0;


                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB11").Cells.Item(i).Specific).Value = worksheet.Cells[row, 26].Text;
                            var valCAN11 = ParseNumericCell(worksheet.Cells[row, 27], out bool invCAN11);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA11").Cells.Item(i).Specific).Value = valCAN11;
                            if (invCAN11) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 11 no valido";
                            else can11 = decimal.TryParse(valCAN11, out var tmpCAN11) ? tmpCAN11 : 0;


                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB12").Cells.Item(i).Specific).Value = worksheet.Cells[row, 28].Text;
                            var valCAN12 = ParseNumericCell(worksheet.Cells[row, 29], out bool invCAN12);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA12").Cells.Item(i).Specific).Value = valCAN12;
                            if (invCAN12) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 12 no valido";
                            else can12 = decimal.TryParse(valCAN12, out var tmpCAN12) ? tmpCAN12 : 0;


                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB13").Cells.Item(i).Specific).Value = worksheet.Cells[row, 30].Text;
                            var valCAN13 = ParseNumericCell(worksheet.Cells[row, 31], out bool invCAN13);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA13").Cells.Item(i).Specific).Value = valCAN13;
                            if (invCAN13) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 13 no valido";
                            else can13 = decimal.TryParse(valCAN13, out var tmpCAN13) ? tmpCAN13 : 0;



                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB14").Cells.Item(i).Specific).Value = worksheet.Cells[row, 32].Text;
                            var valCAN14 = ParseNumericCell(worksheet.Cells[row, 33], out bool invCAN14);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA14").Cells.Item(i).Specific).Value = valCAN14;
                            if (invCAN14) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 14 no valido";
                            else can14 = decimal.TryParse(valCAN14, out var tmpCAN14) ? tmpCAN14 : 0;



                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB15").Cells.Item(i).Specific).Value = worksheet.Cells[row, 34].Text;
                            var valCAN15 = ParseNumericCell(worksheet.Cells[row, 35], out bool invCAN15);
                            ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA15").Cells.Item(i).Specific).Value = valCAN15;
                            if (invCAN15) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "Valor en Cantidad 15 no valido";
                            else can15 = decimal.TryParse(valCAN15, out var tmpCAN15) ? tmpCAN15 : 0;

                            // Validación suma
                            if (cant != (can1 + can2 + can3 + can4 + can5 + can6 + can7 + can8 + can9 + can10 + can11 + can12 + can13 + can14 + can15)) {
                                var obsNueva = $"Suma de las cantidades ({can1 + can2 + can3 + can4 + can5 +can6 + can7 + can8 + can9 + can10 + can11 + can12 + can13 + can14 + can15}) ≠ Cant. Total ({cant})";
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = obsNueva;
                            }

                            // Validación de que la descripción tenga un código de item válido
                            Functions.ObtainItemCode(out string itemCode, itemName);
                            if (string.IsNullOrEmpty(itemCode))
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "No se encontró un código de item para la descripción";
                            else
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_COD").Cells.Item(i).Specific).Value = itemCode;

                            // Validación para revisar cantidad disponible del lote
                            string whsCode = cmbOri.Selected.Value;
                            Functions.ObtainQuantityDistNumber(out decimal quantity, itemCode, whsCode, distNumber);
                            if (cant > quantity)
                                ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = $"Cantidad no disponible para el lote {distNumber}";
                            
                            var observacion = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value;
                            if (string.IsNullOrEmpty(observacion)) ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value = "OK";

                            mtxDetalle.FlushToDataSource();
                            i++;
                        }
                    }
                    mtxDetalle.LoadFromDataSource();
                }

            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> btnLoad_PressedAfter: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally { UIAPIRawForm.Freeze(false); }
        }
        private void btnTransfer_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal) {
            UIAPIRawForm.Freeze(true);
            try {
                int nroAlmacenamiento = int.Parse(this.edtNro.Value);
                Functions.ObtainDocNumTransfer(out int docNum, nroAlmacenamiento);
                if (!docNum.Equals(0)) {
                    Program.SBOApplication.StatusBar.SetText($"Transferencia existente con Numero: {docNum}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    Functions.UpdateDocNumTransfer(docNum, nroAlmacenamiento);
                    return;
                }

                // --- Crear objeto transferencia ---
                SAPbobsCOM.StockTransfer oStockTransfer = (SAPbobsCOM.StockTransfer)Program.SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                // DocDate desde EditText (aceptamos "yyyyMMdd" o parse general)
                string fechaTexto = this.edtFecha.Value?.Trim();
                if (!string.IsNullOrEmpty(fechaTexto) && DateTime.TryParseExact(fechaTexto, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fExact))
                    oStockTransfer.DocDate = fExact;
                else if (DateTime.TryParse(fechaTexto, out DateTime fAny))
                    oStockTransfer.DocDate = fAny;
                else
                    // Si viene vacío o inválido, ponemos hoy
                    oStockTransfer.DocDate = DateTime.Today;

                oStockTransfer.FromWarehouse = cmbOri.Selected.Value;
                oStockTransfer.ToWarehouse = cmbDes.Selected.Value;
                oStockTransfer.UserFields.Fields.Item("U_IZ_NRO_ALMACENAMIENTO").Value = int.Parse(edtNro.Value.ToString());

                // Número de filas
                int rowCount = mtxDetalle.RowCount;

                for (int i = 1; i <= rowCount; i++) {
                    string observacion = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_OBS").Cells.Item(i).Specific).Value;
                    if (observacion.Equals("OK")) {
                        // Cantidades
                        double cantidadTotal = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CANT").Cells.Item(i).Specific).Value);
                        double cantidad1 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN1").Cells.Item(i).Specific).Value);
                        double cantidad2 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN2").Cells.Item(i).Specific).Value);
                        double cantidad3 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN3").Cells.Item(i).Specific).Value);
                        //frrc 02-09-2025 aumento de cantidades hasta la 15
                        double cantidad4 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN4").Cells.Item(i).Specific).Value);
                        double cantidad5 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN5").Cells.Item(i).Specific).Value);
                        double cantidad6 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN6").Cells.Item(i).Specific).Value);
                        double cantidad7 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN7").Cells.Item(i).Specific).Value);
                        double cantidad8 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN8").Cells.Item(i).Specific).Value);
                        double cantidad9 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CAN9").Cells.Item(i).Specific).Value);
                        double cantidad10 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA10").Cells.Item(i).Specific).Value);
                        double cantidad11 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA11").Cells.Item(i).Specific).Value);
                        double cantidad12 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA12").Cells.Item(i).Specific).Value);
                        double cantidad13 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA13").Cells.Item(i).Specific).Value);
                        double cantidad14 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA14").Cells.Item(i).Specific).Value);
                        double cantidad15 = ParseDouble(((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_CA15").Cells.Item(i).Specific).Value);

                        string distNumber = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_LOT").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion1 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI1").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion2 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI2").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion3 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI3").Cells.Item(i).Specific).Value?.Trim();
                        //frrc 02-09-2025 se aumento ubicaciones hasta la 15
                        string ubicacion4 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI4").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion5 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI5").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion6 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI6").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion7 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI7").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion8 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI8").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion9 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI9").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion10 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB10").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion11 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB11").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion12 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB12").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion13 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB13").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion14 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB14").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion15 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UB15").Cells.Item(i).Specific).Value?.Trim();

                        oStockTransfer.Lines.ItemCode = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_COD").Cells.Item(i).Specific).Value;
                        oStockTransfer.Lines.FromWarehouseCode = cmbOri.Selected.Value;
                        oStockTransfer.Lines.WarehouseCode = cmbDes.Selected.Value;
                        oStockTransfer.Lines.Quantity = cantidadTotal;

                        // ----- LOTE -----
                        oStockTransfer.Lines.BatchNumbers.BatchNumber = distNumber;
                        oStockTransfer.Lines.BatchNumbers.Quantity = cantidadTotal;
                        oStockTransfer.Lines.BatchNumbers.Add();

                        int loteBaseIndex = 0; // Es el primer (y único) lote de la línea

                        // ----- UBICACIONES DESTINO (solo destino usa bins) -----
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion1, cantidad1, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion2, cantidad2, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion3, cantidad3, loteBaseIndex);
                        // FRRC 02-09-2025 aumento ubicaciones destino 
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion4, cantidad4, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion5, cantidad5, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion6, cantidad6, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion7, cantidad7, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion8, cantidad8, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion9, cantidad9, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion10, cantidad10, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion11, cantidad11, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion12, cantidad12, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion13, cantidad13, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion14, cantidad14, loteBaseIndex);
                        AddBinToAllocationIfAny(oStockTransfer, cmbDes.Selected.Value, ubicacion15, cantidad15, loteBaseIndex);
                        // Cerrar línea
                        oStockTransfer.Lines.Add();
                    }
                }
                // ====== GRABAR ======
                int ret = oStockTransfer.Add();
                if (ret != 0) {
                    Program.SBOCompany.GetLastError(out int errCode, out string errMsg);
                    Program.SBOApplication.StatusBar.SetText($"DIAPI Error: {errCode} - {errMsg}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
                else {
                    Functions.ObtainDocNumTransfer(out int docNumNuevo, nroAlmacenamiento);
                    Functions.UpdateDocNumTransfer(docNumNuevo, nroAlmacenamiento);
                    this.edtTrf.Value = docNumNuevo.ToString();
                    Program.SBOApplication.StatusBar.SetText($"Transferencia creada. Numero: {docNumNuevo}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> btnTransfer_PressedAfter: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally { UIAPIRawForm.Freeze(false); }
        }
        private void btnOk_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal) {
            UIAPIRawForm.Freeze(true);
            try {
                if (pVal.ActionSuccess) {
                    if (pVal.FormMode.Equals(3)) {
                        InitializeData();
                    }
                }
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> btnOk_PressedAfter: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally { UIAPIRawForm.Freeze(false); }
        }
        private void MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent) {
            BubbleEvent = true;
            try {
                if (!pVal.BeforeAction){
                    switch (pVal.MenuUID) {
                        case "1281": //Buscar
                            edtNro.Item.Enabled = true;
                            this.btnTransfer.Item.Enabled = false;
                            break;
                        case "1282": //Crear
                            InitializeData();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> MenuEvent: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void Form_DataLoadAfter(ref SAPbouiCOM.BusinessObjectInfo pVal) {
            try {
                if (UIAPIRawForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                    this.btnTransfer.Item.Enabled = true;
                else
                    this.btnTransfer.Item.Enabled = false;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> MenuEvent: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        #endregion

        #region Eventos
        private void InitializeData() {
            try {
                this.edtFecha.Value = DateTime.Now.ToString("yyyyMMdd");
                Functions.ObtainSequence(out int sequence);
                this.edtNro.Value = sequence.ToString();
                this.edtFecha.Item.Click();
                this.btnTransfer.Item.Enabled = false;

            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> InitializeData: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void InitializeComboWarehousesOrigin() {
            try {
                var varValores = cmbOri.ValidValues;
                List<WarehousesDTO> warehousesDTOs = null;
                Functions.ObtainWarehouses(out warehousesDTOs);

                if (warehousesDTOs.Count > 0) {
                    foreach (var warehousesDTO in warehousesDTOs)
                        varValores.Add(warehousesDTO.WhsCode, warehousesDTO.WhsName);
                }
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> InitializeComboWarehousesOrigin: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void InitializeComboWarehousesDestination() {
            try {
                var varValores = cmbDes.ValidValues;
                List<WarehousesDTO> warehousesDTOs = null;
                Functions.ObtainWarehouses(out warehousesDTOs);

                if (warehousesDTOs.Count > 0) {
                    foreach (var warehousesDTO in warehousesDTOs)
                        varValores.Add(warehousesDTO.WhsCode, warehousesDTO.WhsName);
                }
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> InitializeComboWarehousesDestination: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private string ParseNumericCell(ExcelRange cell, out bool invalido) {
            invalido = false;
            try {
                var texto = cell.Text?.Trim();

                // Vacío o guion: trata como 0, sin marcar inválido
                if (string.IsNullOrEmpty(texto) || texto == "-")
                    return "0";

                // Intento con Invariant y con cultura actual (por si hay coma decimal)
                if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var num) ||
                    decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out num)) {
                    // Normaliza a Invariant o al formato que use tu SAP (si requiere punto o coma)
                    return num.ToString(CultureInfo.InvariantCulture);
                }

                // No numérico -> retorna 0 y marca inválido
                invalido = true;
                return "0";
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> ParseNumericCell: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                invalido = true;
                return "0";
            }
        }
        // Parse seguro para double, tratando vacío y "-" como 0
        private double ParseDouble(string txt) {
            try {
                txt = (txt ?? "").Trim();
                if (txt == "" || txt == "-") return 0d;
                if (double.TryParse(txt, NumberStyles.Number, CultureInfo.InvariantCulture, out double d)) return d;
                if (double.TryParse(txt, NumberStyles.Number, CultureInfo.CurrentCulture, out d)) return d;
                return 0d;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> ParseDouble: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return 0d;
            }
        }
        private void AddBinToAllocationIfAny(SAPbobsCOM.StockTransfer oTrans, string whsTo, string binCode, double qty, int loteBaseIndex) {
            try {
                if (string.IsNullOrWhiteSpace(binCode) || binCode == "-" || qty <= 0) return;

                Functions.ObtainBinAbsEntry(out int binAbs, whsTo, binCode);
                oTrans.Lines.BinAllocations.BinAbsEntry = binAbs;
                oTrans.Lines.BinAllocations.BinActionType = SAPbobsCOM.BinActionTypeEnum.batToWarehouse;
                oTrans.Lines.BinAllocations.Quantity = qty;
                oTrans.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = loteBaseIndex; // enlaza con el lote 0
                oTrans.Lines.BinAllocations.Add();
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> AddBinToAllocationIfAny: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        #endregion

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        private class WindowWrapper : IWin32Window {
            private readonly IntPtr _handle;
            public WindowWrapper(IntPtr handle) { _handle = handle; }
            public IntPtr Handle { get { return _handle; } }
        }

       
    }
}
