using nuanaddon_varimar.DTO;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nuanaddon_varimar.Utilities {
    public static class Functions {
        public static void ObtainWarehouses(out List<WarehousesDTO> warehousesDTOs) {
            warehousesDTOs = new List<WarehousesDTO>();

            try {
                WarehousesDTO warehousesDTO;

                string varSQL = $"EXEC SBO_SP_IZ_GET_WAREHOUSES";
                //string varSQL = $"CALL SBO_SP_IZ_GET_WAREHOUSES()";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    warehousesDTO = new WarehousesDTO();
                    warehousesDTO.WhsCode = Convert.ToString(oRs.Fields.Item("WhsCode").Value);
                    warehousesDTO.WhsName = Convert.ToString(oRs.Fields.Item("WhsName").Value);
                    warehousesDTOs.Add(warehousesDTO);
                    oRs.MoveNext();
                }

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> ObtainWarehouses: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public static void ObtainMandatoryPiece(out string isMandatory, string itemCode) {
            isMandatory = "N";
            try {
                string varSQL = $"EXEC SBO_SP_IZ_GET_MANDATORYPIECE '{itemCode}' ";
                //string varSQL = $"CALL SBO_SP_IZ_GET_ITEMCODE ('{itemName}') ";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    isMandatory = Convert.ToString(oRs.Fields.Item("IsMandatory").Value);
                    oRs.MoveNext();
                }

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> ObtainMandatoryPiece: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public static void ObtainQuantityDistNumber(out decimal quantity, string itemCode, string whsCode, string DistNumber) {
            quantity = 0;
            try {
                string varSQL = $"EXEC SBO_SP_IZ_GET_QUANTITYDISTNUMBER '{itemCode}','{whsCode}','{DistNumber}' ";
                //string varSQL = $"CALL SBO_SP_IZ_GET_QUANTITYDISTNUMBER ('{itemCode}','{whsCode}','{DistNumber}') ";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    quantity = Convert.ToDecimal(oRs.Fields.Item("Stock").Value);
                    oRs.MoveNext();
                }

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> ObtainQuantityDistNumber: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public static void ObtainBinAbsEntry(out int binAbsEntry, string whsCode, string binCode) {
            binAbsEntry = 0;
            try {
                string varSQL = $"EXEC SBO_SP_IZ_GET_BINABSENTRY '{whsCode}','{binCode}' ";
                //string varSQL = $"CALL SBO_SP_IZ_GET_BINABSENTRY ('{whsCode}','{binCode}') ";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    binAbsEntry = Convert.ToInt32(oRs.Fields.Item("AbsEntry").Value);
                    oRs.MoveNext();
                }

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> ObtainBinAbsEntry: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public static void ObtainSequence(out int sequence) {
            sequence = 0;
            try {
                string varSQL = $"EXEC SBO_SP_IZ_GET_SEQUENCE";
                //string varSQL = $"CALL SBO_SP_IZ_GET_SEQUENCE()";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    sequence = Convert.ToInt32(oRs.Fields.Item("Code").Value);
                    oRs.MoveNext();
                }

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> ObtainSequence: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public static void ObtainDocNumTransfer(out int docNum, int nroAlmacenamiento) {
            docNum = 0;
            try {
                string varSQL = $"EXEC SBO_SP_IZ_GET_DOCNUMTRANSFER {nroAlmacenamiento} ";
                //string varSQL = $"CALL SBO_SP_IZ_GET_DOCNUMTRANSFER ({nroAlmacenamiento}) ";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    docNum = Convert.ToInt32(oRs.Fields.Item("DocNum").Value);
                    oRs.MoveNext();
                }

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> ObtainDocNum: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public static void UpdateDocNumTransfer(int docNum, int nroAlmacenamiento) {
            try {
                string varSQL = $"Update \"@IZ_DTMA_CABALM\" set \"U_IZ_NRO_TRANSFERENCIA\" = {docNum} where \"Code\" = '{nroAlmacenamiento}'";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

                // Ejecuta la consulta
                oRs.DoQuery(varSQL);

                // Libera el objeto Recordset
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRs);
                oRs = null;
            }
            catch (Exception ex) {
                Program.SBOApplication.StatusBar.SetText($"Utilities.Functions.cs -> UpdateDocNumTransfer: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }
}
