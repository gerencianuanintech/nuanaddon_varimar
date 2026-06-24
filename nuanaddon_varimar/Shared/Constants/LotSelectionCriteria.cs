using System.Collections.Generic;

namespace nuanaddon_varimar.Shared.Constants {
    public static class LotSelectionCriteria {
        public const string MenorCantidad = "MENOR_CANTIDAD";
        public const string MayorCantidad = "MAYOR_CANTIDAD";
        public const string MenorCantidadLoteUnico = "MENOR_CANTIDAD_LOTE_UNICO";

        public static readonly HashSet<string> ValidCriteria = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) {
            MenorCantidad,
            MayorCantidad,
            MenorCantidadLoteUnico
        };
    }
}
