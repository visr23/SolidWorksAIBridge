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
                modeler.CreateDoorRow(
                    new double[] { 25, 35, 45 },  // widths
                    new double[] { 55, 65, 60 },  // heights
                    new double[] { -55, 0, 55 }); // centers X
                Console.WriteLine("Created an unsaved 180 x 220 x 3 mm wall with three independently sized ground-door openings in one shared sketch and Blind cut.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
        }
    }
}
