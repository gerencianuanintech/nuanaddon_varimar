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

                string varSQL = $"CALL SBO_SP_IZ_GET_WAREHOUSES()";
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
        public static void ObtainItemCode(out string itemCode, string itemName) {
            itemCode = "";
            try {
                string varSQL = $"CALL SBO_SP_IZ_GET_ITEMCODE('{itemName}')";
                Recordset oRs = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRs.DoQuery(varSQL);

                while (!oRs.EoF) {
                    itemCode = Convert.ToString(oRs.Fields.Item("ItemCode").Value);
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
        public static void ObtainQuantityDistNumber(out decimal quantity, string itemCode, string whsCode, string DistNumber) {
            quantity = 0;
            try
            {
                string varSQL = $"CALL SBO_SP_IZ_GET_QUANTITYDISTNUMBER('{itemCode}','{whsCode}','{DistNumber}')";
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
                string varSQL = $"CALL SBO_SP_IZ_GET_BINABSENTRY('{whsCode}','{binCode}')";
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
    }
}
