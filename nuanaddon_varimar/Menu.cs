using System;
using B1Application = SAPbouiCOM.Framework.Application;

namespace nuanaddon_varimar {
    class Menu {
        public void AddMenuItems() {
            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;

            oMenus = B1Application.SBO_Application.Menus;

            SAPbouiCOM.MenuCreationParams oCreationPackage = null;
            oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(B1Application.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
            oMenuItem = B1Application.SBO_Application.Menus.Item("43520");

            oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
            oCreationPackage.UniqueID = "nuanaddon_varimar";
            oCreationPackage.String = "Addon Hoja Almacenamiento";
            oCreationPackage.Enabled = true;
            oCreationPackage.Position = -1;

            oMenus = oMenuItem.SubMenus;

            try {
                if (!oMenus.Exists("nuanaddon_varimar"))
                    oMenus.AddEx(oCreationPackage);
            }
            catch (Exception) {
            }

            try {
                oMenuItem = B1Application.SBO_Application.Menus.Item("nuanaddon_varimar");
                oMenus = oMenuItem.SubMenus;

                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "nuanaddon_varimar.Form.FrmDocumento";
                oCreationPackage.String = "Almacenamiento";

                if (!oMenus.Exists("nuanaddon_varimar.Form.FrmDocumento"))
                    oMenus.AddEx(oCreationPackage);
            }
            catch (Exception) {
                B1Application.SBO_Application.SetStatusBarMessage("Menu Already Exists", SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        public void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;

            try {
                if (pVal.BeforeAction && pVal.MenuUID == "nuanaddon_varimar.Form.FrmDocumento") {
                    Form.FrmDocumento activeForm = new Form.FrmDocumento();
                    activeForm.Show();
                }
            }
            catch (Exception ex) {
                B1Application.SBO_Application.MessageBox(ex.ToString(), 1, "Ok", "", "");
            }
        }
    }
}
