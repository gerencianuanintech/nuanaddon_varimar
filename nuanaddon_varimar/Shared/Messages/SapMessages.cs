using System;

namespace nuanaddon_varimar.Shared.Messages {
    public static class SapMessages {
        public const string DeliveryDateRequired = "Debe seleccionar una fecha de entrega";
        public const string NoAvailableBatchesForCustomerRules = "No hay lotes disponibles que cumplan con las reglas del cliente";

        public static void Error(string context, Exception exception) {
            Error($"{context}: {exception.Message}");
        }

        public static void Error(string message) {
            SetStatusBarText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        }

        public static void Warning(string message) {
            SetStatusBarText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        }

        public static void Success(string message) {
            SetStatusBarText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
        }

        public static void MessageBox(string message) {
            if (Program.SBOApplication != null) {
                Program.SBOApplication.MessageBox(message, 1, "Ok", "", "");
                return;
            }

            System.Windows.Forms.MessageBox.Show(message);
        }

        public static void CompanyError(string context) {
            if (Program.SBOCompany == null) {
                Error($"{context}: SAP company error not available");
                return;
            }

            Error($"{context}: {Program.SBOCompany.GetLastErrorCode()} - {Program.SBOCompany.GetLastErrorDescription()}");
        }

        private static void SetStatusBarText(string message, SAPbouiCOM.BoMessageTime messageTime, SAPbouiCOM.BoStatusBarMessageType messageType) {
            if (Program.SBOApplication != null) {
                Program.SBOApplication.StatusBar.SetText(message, messageTime, messageType);
                return;
            }

            System.Windows.Forms.MessageBox.Show(message);
        }
    }
}
