using nuanaddon_varimar.Shared.Parsing;

namespace nuanaddon_varimar.Infrastructure.Sap {
    public static class SapUiMatrixAccessor {
        public static bool IsValidRow(SAPbouiCOM.Matrix matrix, int row) {
            return matrix != null && row > 0 && row <= matrix.RowCount;
        }

        public static string GetEditTextValue(SAPbouiCOM.Matrix matrix, string columnId, int row) {
            return ((SAPbouiCOM.EditText)matrix.Columns.Item(columnId).Cells.Item(row).Specific).Value;
        }

        public static decimal GetDecimalValue(SAPbouiCOM.Matrix matrix, string columnId, int row) {
            return SapValueParser.ParseDecimal(GetEditTextValue(matrix, columnId, row));
        }

        public static void SetEditTextValue(SAPbouiCOM.Matrix matrix, string columnId, int row, decimal value) {
            ((SAPbouiCOM.EditText)matrix.Columns.Item(columnId).Cells.Item(row).Specific).Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
