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
                modeler.CreateWall(180, 220, 3);
                modeler.CreateWindowGrid(
                    4,      // floors
                    4,      // windows per floor
                    20,     // window width
                    30,     // window height
                    10,     // horizontal clear gap
                    15,     // vertical clear gap
                    0,      // grid center X
                    0,      // grid center Y
                    true,   // create sills
                    1,      // sill thickness
                    1);     // sill outward projection
                Console.WriteLine("Created an unsaved 180 x 220 x 3 mm wall with 4 floors, 16 windows, 10 mm horizontal and 15 mm vertical clear gaps, and 1 mm exterior sills.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
        }
    }
}
