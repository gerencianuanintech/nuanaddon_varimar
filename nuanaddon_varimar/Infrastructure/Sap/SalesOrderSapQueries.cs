using System;
using System.Collections.Generic;
using nuanaddon_varimar.Shared.Parsing;

namespace nuanaddon_varimar.Infrastructure.Sap {
    public static class SalesOrderSapQueries {
        public static bool TryObtainEligibleBatchQuantities(
            string itemCode,
            string warehouseCode,
            out IDictionary<string, decimal> batchQuantities) {
            IDictionary<string, decimal> eligibleBatchQuantities =
                new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            bool querySucceeded = false;
            string safeItemCode = SapRecordsetExecutor.EscapeSqlValue(itemCode);
            string safeWarehouseCode = SapRecordsetExecutor.EscapeSqlValue(warehouseCode);
            string query =
                "SELECT T0.\"DistNumber\" AS \"Lote\", SUM(T1.\"OnHandQty\") AS \"Cantidad\" " +
                "FROM \"OBTN\" T0 " +
                "INNER JOIN \"OBBQ\" T1 ON T1.\"SnBMDAbs\" = T0.\"AbsEntry\" AND T1.\"ItemCode\" = T0.\"ItemCode\" " +
                "INNER JOIN \"OBIN\" T2 ON T2.\"AbsEntry\" = T1.\"BinAbs\" " +
                $"WHERE T0.\"ItemCode\" = '{safeItemCode}' " +
                "AND T0.\"Status\" = '0' " +
                $"AND T1.\"WhsCode\" = '{safeWarehouseCode}' " +
                $"AND T2.\"WhsCode\" = '{safeWarehouseCode}' " +
                "AND T1.\"OnHandQty\" > 0 " +
                "AND T2.\"RtrictType\" IN (0, 2) " +
                "GROUP BY T0.\"DistNumber\"";

            SapRecordsetExecutor.Execute(query, "Infrastructure.Sap.SalesOrderSapQueries.cs -> TryObtainEligibleBatchQuantities", recordset => {
                while (!recordset.EoF) {
                    string batchNumber = Convert.ToString(recordset.Fields.Item("Lote").Value);
                    decimal quantity = Convert.ToDecimal(recordset.Fields.Item("Cantidad").Value);

                    if (!string.IsNullOrWhiteSpace(batchNumber) && quantity > 0)
                        eligibleBatchQuantities[batchNumber.Trim()] = quantity;

                    recordset.MoveNext();
                }

                querySucceeded = true;
            });

            batchQuantities = eligibleBatchQuantities;
            return querySucceeded;
        }

        public static int ObtainLastDeliveryDocEntryByItem(string itemCode, string cardCode) {
            int docEntry = 0;
            string safeItemCode = SapRecordsetExecutor.EscapeSqlValue(itemCode);
            string safeCardCode = SapRecordsetExecutor.EscapeSqlValue(cardCode);
            string query =
                "SELECT TOP 1 T1.\"DocEntry\" " +
                "FROM \"ODLN\" T0 " +
                "INNER JOIN \"DLN1\" T1 ON T0.\"DocEntry\" = T1.\"DocEntry\" " +
                $"WHERE T1.\"ItemCode\" = '{safeItemCode}' AND T0.\"CardCode\" = '{safeCardCode}' AND T0.\"CANCELED\" = 'N' " +
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
                        if (SapValueParser.TryParseDate(Convert.ToString(value), out parsedDate))
                            expirationDate = parsedDate;
                    }
                }
            });

            return expirationDate;
        }
    }
}
