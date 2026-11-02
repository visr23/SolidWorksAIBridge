using System;

namespace SolidWorksAIBridge
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                var connector = new SolidWorksConnector();
                connector.Connect();
                var modeler = new SolidWorksModeler(connector.Application);
                modeler.CreateWall(160, 100, 5);
                modeler.CreateWindow(40, 50, 0, 10);
                Console.WriteLine("Created an unsaved 160 x 100 x 5 mm wall with a 40 x 50 mm window centered at (0, 10) mm.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
        }
    }
}
