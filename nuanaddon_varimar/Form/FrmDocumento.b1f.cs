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
        }

        private void OnCustomInitialize() {

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

                            // Validación suma
                            if (cant != (can1 + can2 + can3)) {
                                var obsNueva = $"Suma Cantidad 1 + Cantidad 2 + Cantidad 3 ({can1 + can2 + can3}) ≠ Cant. Total ({cant})";
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

                        string distNumber = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_LOT").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion1 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI1").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion2 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI2").Cells.Item(i).Specific).Value?.Trim();
                        string ubicacion3 = ((SAPbouiCOM.EditText)mtxDetalle.Columns.Item("Col_UBI3").Cells.Item(i).Specific).Value?.Trim();

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

                string newKey = Program.SBOCompany.GetNewObjectKey();
                int docEntry = int.Parse(newKey);

                Program.SBOApplication.StatusBar.SetText($"Transferencia creada. DocEntry: {newKey}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Form.FrmDocumento.cs -> btnTransfer_PressedAfter: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally { UIAPIRawForm.Freeze(false); }
        }
        #endregion

        #region Eventos
        private void InitializeData() {
            this.edtFecha.Value = DateTime.Now.ToString("yyyyMMdd");
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
