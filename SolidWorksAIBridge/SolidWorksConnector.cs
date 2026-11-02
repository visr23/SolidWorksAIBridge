using System;
using System.Runtime.InteropServices;

namespace SolidWorksAIBridge
{
    public sealed class SolidWorksConnector
    {
        private SldWorks.SldWorks application;

        public SldWorks.SldWorks Application
        {
            get
            {
                if (application == null)
                    throw new InvalidOperationException("Call Connect before accessing SOLIDWORKS.");
                return application;
            }
        }

        public void Connect()
        {
            application = null;
            try
            {
                // Preserve the existing connection: attach, never launch SOLIDWORKS.
                application = (SldWorks.SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException(
                    "Could not connect to a running SOLIDWORKS instance. Start SOLIDWORKS 2024 " +
                    "and run the bridge at the same Windows privilege level. " +
                    "COM error: 0x" + ex.ErrorCode.ToString("X8"), ex);
            }

            if (application == null)
                throw new InvalidOperationException("No running SOLIDWORKS application was found.");
        }
    }
}
