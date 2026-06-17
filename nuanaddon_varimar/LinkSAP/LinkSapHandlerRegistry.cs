using nuanaddon_varimar.Shared.Constants;
using System;
using System.Collections.Generic;

namespace nuanaddon_varimar.LinkSAP {
    public static class LinkSapHandlerRegistry {
        private static readonly Dictionary<string, Func<LnkSAP>> ItemEventHandlers = new Dictionary<string, Func<LnkSAP>> {
            { SapFormTypeEx.SalesOrder, () => new LnkSalesOrder_0000000139() },
            { SapFormTypeEx.BatchNumberSelection, () => new LnkBatchNumberSelection_0000000042() }
        };

        private static readonly Dictionary<string, Func<LnkSAP>> FormDataEventHandlers = new Dictionary<string, Func<LnkSAP>> {
            { SapFormTypeEx.SalesOrder, () => new LnkSalesOrder_0000000139() }
        };

        public static bool TryCreateForItemEvent(string formTypeEx, out LnkSAP handler) {
            return TryCreate(ItemEventHandlers, formTypeEx, out handler);
        }

        public static bool TryCreateForFormDataEvent(string formTypeEx, out LnkSAP handler) {
            return TryCreate(FormDataEventHandlers, formTypeEx, out handler);
        }

        private static bool TryCreate(Dictionary<string, Func<LnkSAP>> handlers, string formTypeEx, out LnkSAP handler) {
            Func<LnkSAP> factory;
            if (handlers.TryGetValue(formTypeEx, out factory)) {
                handler = factory();
                return true;
            }

            handler = null;
            return false;
        }
    }
}
