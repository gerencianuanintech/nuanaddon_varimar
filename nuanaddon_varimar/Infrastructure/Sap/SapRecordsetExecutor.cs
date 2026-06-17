using nuanaddon_varimar.Shared.Messages;
using SAPbobsCOM;
using System;
using System.Runtime.InteropServices;

namespace nuanaddon_varimar.Infrastructure.Sap {
    public static class SapRecordsetExecutor {
        public static void Execute(string query, string context, Action<Recordset> readRecordset) {
            Recordset recordset = null;

            try {
                recordset = (Recordset)Program.SBOCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                recordset.DoQuery(query);
                readRecordset(recordset);
            }
            catch (Exception ex) {
                SapMessages.Error(context, ex);
            }
            finally {
                if (recordset != null)
                    Marshal.ReleaseComObject(recordset);
            }
        }

        public static string EscapeSqlValue(string value) {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
