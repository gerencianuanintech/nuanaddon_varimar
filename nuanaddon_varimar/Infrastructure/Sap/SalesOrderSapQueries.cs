using System;
using System.Globalization;

namespace nuanaddon_varimar.Infrastructure.Sap {
    public static class SalesOrderSapQueries {
        public static int ObtainLastDeliveryDocEntryByItem(string itemCode) {
            int docEntry = 0;
            string safeItemCode = SapRecordsetExecutor.EscapeSqlValue(itemCode);
            string query =
                "SELECT TOP 1 T1.\"DocEntry\" " +
                "FROM \"ODLN\" T0 " +
                "INNER JOIN \"DLN1\" T1 ON T0.\"DocEntry\" = T1.\"DocEntry\" " +
                $"WHERE T1.\"ItemCode\" = '{safeItemCode}' AND T0.\"CANCELED\" = 'N' " +
                "ORDER BY T0.\"DocDate\" DESC, T0.\"DocEntry\" DESC";

            SapRecordsetExecutor.Execute(query, "Infrastructure.Sap.SalesOrderSapQueries.cs -> ObtainLastDeliveryDocEntryByItem", recordset => {
                if (!recordset.EoF)
                    docEntry = Convert.ToInt32(recordset.Fields.Item("DocEntry").Value);
            });

            return docEntry;
        }

        public static DateTime? ObtainMaxExpirationDateFromDelivery(int docEntry, string itemCode) {
            DateTime? expirationDate = null;
            string safeItemCode = SapRecordsetExecutor.EscapeSqlValue(itemCode);
            string query =
                "SELECT MAX(T3.\"ExpDate\") AS \"ExpDate\" " +
                "FROM \"ODLN\" T0 " +
                "INNER JOIN \"DLN1\" T1 ON T0.\"DocEntry\" = T1.\"DocEntry\" " +
                "INNER JOIN \"IBT1\" T2 ON T1.\"ItemCode\" = T2.\"ItemCode\" AND T1.\"DocEntry\" = T2.\"BaseEntry\" AND T2.\"Direction\" = 2 " +
                "INNER JOIN \"OBTN\" T3 ON T3.\"DistNumber\" = T2.\"BatchNum\" AND T3.\"ItemCode\" = T2.\"ItemCode\" " +
                $"WHERE T0.\"DocEntry\" = {docEntry} AND T1.\"ItemCode\" = '{safeItemCode}'";

            SapRecordsetExecutor.Execute(query, "Infrastructure.Sap.SalesOrderSapQueries.cs -> ObtainMaxExpirationDateFromDelivery", recordset => {
                if (!recordset.EoF && recordset.Fields.Item("ExpDate").Value != null && recordset.Fields.Item("ExpDate").Value != DBNull.Value) {
                    object value = recordset.Fields.Item("ExpDate").Value;
                    if (value is DateTime)
                        expirationDate = (DateTime)value;
                    else {
                        DateTime parsedDate;
                        if (DateTime.TryParse(Convert.ToString(value), CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDate) ||
                            DateTime.TryParse(Convert.ToString(value), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) {
                            expirationDate = parsedDate;
                        }
                    }
                }
            });

            return expirationDate;
        }
    }
}
