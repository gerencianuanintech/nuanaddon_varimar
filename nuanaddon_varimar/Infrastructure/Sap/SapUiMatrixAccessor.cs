using nuanaddon_varimar.Shared.Parsing;

namespace nuanaddon_varimar.Infrastructure.Sap {
    public static class SapUiMatrixAccessor {
        public static bool IsValidRow(SAPbouiCOM.Matrix matrix, int row) {
            return matrix != null && row > 0 && row <= matrix.RowCount;
        }

        public static bool TryGetEditTextValue(SAPbouiCOM.Matrix matrix, string columnId, int row, out string value) {
            value = string.Empty;

            if (!IsValidRow(matrix, row))
                return false;

            try {
                object specific = matrix.Columns.Item(columnId).Cells.Item(row).Specific;
                SAPbouiCOM.EditText editText = specific as SAPbouiCOM.EditText;
                if (editText == null)
                    return false;

                value = editText.Value;
                return true;
            }
            catch {
                value = string.Empty;
                return false;
            }
        }

        public static bool TryGetDecimalValue(SAPbouiCOM.Matrix matrix, string columnId, int row, out decimal value) {
            value = 0m;

            string text;
            if (!TryGetEditTextValue(matrix, columnId, row, out text))
                return false;

            value = SapValueParser.ParseDecimal(text);
            return true;
        }

        public static string GetEditTextValue(SAPbouiCOM.Matrix matrix, string columnId, int row) {
            return ((SAPbouiCOM.EditText)matrix.Columns.Item(columnId).Cells.Item(row).Specific).Value;
        }

        public static decimal GetDecimalValue(SAPbouiCOM.Matrix matrix, string columnId, int row) {
            return SapValueParser.ParseDecimal(GetEditTextValue(matrix, columnId, row));
        }

        public static bool TrySetEditTextValue(SAPbouiCOM.Matrix matrix, string columnId, int row, decimal value) {
            if (!IsValidRow(matrix, row))
                return false;

            try {
                object specific = matrix.Columns.Item(columnId).Cells.Item(row).Specific;
                SAPbouiCOM.EditText editText = specific as SAPbouiCOM.EditText;
                if (editText == null)
                    return false;

                editText.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch {
                return false;
            }
        }

        public static void SetEditTextValue(SAPbouiCOM.Matrix matrix, string columnId, int row, decimal value) {
            ((SAPbouiCOM.EditText)matrix.Columns.Item(columnId).Cells.Item(row).Specific).Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
