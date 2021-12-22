using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ORMAPCancelledNumbers
{

    //  https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Digitally-signed-add-ins-and-configurations#applying-digital-signatures-to-an-add-in
    // Registered using the *.deschutes.org cert.  Copied the cert to my local drive (see .csproj)
    // Building this project without the cert will not register it.

    internal class CancelledNumbersModule : Module
    {
        private static CancelledNumbersModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static CancelledNumbersModule Current
        {
            get
            {
                return _this ?? (_this = (CancelledNumbersModule)FrameworkApplication.FindModule("ORMAPCancelledNumbers_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

        //#region Toggle State
        ///// <summary>
        ///// Activate or Deactivate the specified state. State is identified via
        ///// its name. Listen for state changes via the DAML <b>condition</b> attribute
        ///// </summary>
        ///// <param name="stateID"></param>
        //public static void ToggleState(string stateID)
        //{

        //    if (FrameworkApplication.State.Contains(stateID))
        //    {
        //        FrameworkApplication.State.Deactivate(stateID);
        //    }
        //    else
        //    {
        //        FrameworkApplication.State.Activate(stateID);
        //    }
        //}

        //#endregion Toggle State


    }
}
