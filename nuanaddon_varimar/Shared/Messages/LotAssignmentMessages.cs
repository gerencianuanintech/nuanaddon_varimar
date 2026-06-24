namespace nuanaddon_varimar.Shared.Messages {
    public static class LotAssignmentMessages {
        public const string InvalidBufferDaysParameter = "Parametro DiasBufferLotes invalido. Se usara el valor por defecto 7.";
        public const string InvalidSelectionCriterionParameter = "Parametro CriterioSeleccionLotes invalido. Se usara MENOR_CANTIDAD.";
        public const string DeliveryDateRequired = "Debe seleccionar una fecha de entrega.";
        public const string ErrorReadingBatchMatrix = "Error al leer matriz de lotes.";
        public const string ErrorWritingBatchAssignments = "Error al escribir cantidades en SAP.";
        public const string InvalidBatchAssignment = "La asignacion de lotes contiene cantidades invalidas.";
        public const string BatchNumberColumnIdRequiresConfirmation = "El ID de columna del numero de lote requiere confirmacion tecnica.";

        public static string NoAvailableBatches(string itemCode) {
            return "No existen lotes disponibles para el articulo " + itemCode + ".";
        }

        public static string NoBatchesPassCustomerRule(string itemCode) {
            return "No existen lotes que cumplan la regla comercial para el articulo " + itemCode + ".";
        }

        public static string SingleBatchCriterionNotSatisfied(string itemCode) {
            return "No se pudo cumplir el criterio de lote unico para el articulo " + itemCode + ". No existe un lote disponible con cantidad suficiente para cubrir la cantidad requerida.";
        }
    }
}
