namespace nuanaddon_varimar.Shared.Constants {
    public static class SapBatchSelectionUiIds {
        public const string LinesMatrix = "3";
        public const string AvailableBatchesMatrix = "4";
        public const string AssignButton = "48";

        public const string LineItemCodeColumn = "1";
        public const string LineRequiredQuantityColumn = "17";
        public const string LineSelectedQuantityColumn = "18";

        // TODO: Confirmar en SAP Business One el ID real de la columna "Numero de lote" en la matriz 4.
        public static readonly string BatchNumberColumn = "CONFIRMAR_ID";
        public const string BatchProductionDateColumn = "14";
        public const string BatchExpirationDateColumn = "15";
        public const string BatchAvailableQuantityColumn = "234000058";
        public const string BatchQuantityToAssignColumn = "234000059";
        public const string BatchAssignedQuantityColumn = "234000061";
    }
}
