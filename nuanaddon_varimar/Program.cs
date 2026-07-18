using OfficeOpenXml;
using nuanaddon_varimar.LinkSAP;
using nuanaddon_varimar.Shared.Messages;
using System;
using B1Application = SAPbouiCOM.Framework.Application;

namespace nuanaddon_varimar {
    class Program {
        public static SAPbouiCOM.Application SBOApplication;
        public static SAPbobsCOM.Company SBOCompany;

        [STAThread]
        static void Main(string[] args) {
            try {
                ExcelPackage.License.SetNonCommercialOrganization("Nuanintech");

                B1Application oApp;
                if (args.Length < 1)
                    oApp = new B1Application();
                else
                    oApp = new B1Application(args[0]);

                SBOApplication = B1Application.SBO_Application;
                SBOCompany = (SAPbobsCOM.Company)SBOApplication.Company.GetDICompany();
                SBOCompany.GetContextCookie();

                Menu myMenu = new Menu();
                myMenu.AddMenuItems();
                oApp.RegisterMenuEventHandler(myMenu.SBO_Application_MenuEvent);
                B1Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);
                B1Application.SBO_Application.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent);
                B1Application.SBO_Application.FormDataEvent += new SAPbouiCOM._IApplicationEvents_FormDataEventEventHandler(SBO_Application_FormDataEvent);
                oApp.Run();
            }
            catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes eventType) {
            switch (eventType) {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    System.Windows.Forms.Application.Exit();
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    break;
                default:
                    break;
            }
        }

        static void SBO_Application_ItemEvent(string formUid, ref SAPbouiCOM.ItemEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;

            try {
                if ("0".Equals(formUid) ||
                    pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD ||
                    pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_ACTIVATE ||
                    pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_DEACTIVATE)
                    return;

                LnkSAP handler;
                if (LinkSapHandlerRegistry.TryCreateForItemEvent(pVal.FormTypeEx, out handler))
                    handler.HandleItemEvent(formUid, ref pVal, ref bubbleEvent);
            }
            catch (Exception ex) {
                SapMessages.Error("Program.cs -> SBO_Application_ItemEvent", ex);
            }
        }

        static void SBO_Application_FormDataEvent(ref SAPbouiCOM.BusinessObjectInfo businessObjectInfo, out bool bubbleEvent) {
            bubbleEvent = true;

            try {
                LnkSAP handler;
                if (LinkSapHandlerRegistry.TryCreateForFormDataEvent(businessObjectInfo.FormTypeEx, out handler))
                    handler.HandleFormDataEvent(ref businessObjectInfo, ref bubbleEvent);
            }
            catch (Exception ex) {
                SapMessages.Error("Program.cs -> SBO_Application_FormDataEvent", ex);
            }
        }
    }
}
