using System.Globalization;

namespace nuanaddon_varimar.Shared.Messages {
    public static class LotAssignmentMessages {
        public const string InvalidBufferDaysParameter = "Parametro DiasBufferLotes invalido. Se usara valor por defecto.";
        public const string InvalidSelectionCriterionParameter = "Parametro CriterioSeleccionLotes invalido. Se usara valor por defecto.";
        public const string NoAvailableBatchesForItem = "No existen lotes disponibles para el articulo.";
        public const string NoBatchesMatchingCustomerRule = "No existen lotes que cumplan la regla comercial.";
        public const string NoSingleBatchEnough = "No existe lote unico suficiente.";
        public const string CannotAssignZeroOrNegativeQuantity = "No se puede asignar cantidad cero o negativa.";
        public const string InvalidSapRow = "No se puede escribir en una fila SAP invalida.";
        public const string BatchNumberColumnNotConfirmed = "No se confirmo el ID real de la columna numero de lote.";
        public const string CannotValidateEligibleBatchQuantity = "No se pudo validar el estado y las cantidades de los lotes en ubicaciones permitidas. No se realizara la asignacion.";
        public const string CannotDetermineWarehouseForItem = "No se pudo determinar el almacen de la linea del articulo.";

        public static string InvalidSapRowDetail(int sapRow) {
            return InvalidSapRow + " SapRow: " + sapRow.ToString(CultureInfo.InvariantCulture);
        }

        public static string CannotDetermineWarehouseForItemDetail(string itemCode) {
            return CannotDetermineWarehouseForItem + " Articulo: " + (itemCode ?? string.Empty).Trim();
        }
    }
}
