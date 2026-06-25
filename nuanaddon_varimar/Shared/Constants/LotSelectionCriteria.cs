namespace nuanaddon_varimar.Shared.Constants {
    public static class LotSelectionCriteria {
        public const string MenorCantidad = "MENOR_CANTIDAD";
        public const string MayorCantidad = "MAYOR_CANTIDAD";
        public const string MenorCantidadLoteUnico = "MENOR_CANTIDAD_LOTE_UNICO";

        public static bool IsValid(string value) {
            string normalized = Normalize(value);
            return normalized == MenorCantidad ||
                   normalized == MayorCantidad ||
                   normalized == MenorCantidadLoteUnico;
        }

        public static string NormalizeOrDefault(string value) {
            string normalized = Normalize(value);

            if (normalized == MayorCantidad)
                return MayorCantidad;

            if (normalized == MenorCantidadLoteUnico)
                return MenorCantidadLoteUnico;

            return MenorCantidad;
        }

        private static string Normalize(string value) {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
