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
                if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
                {
                    Console.WriteLine("Usage: SolidWorksAIBridge.exe [--photo-blockout | --help]");
                    Console.WriteLine("No arguments: legacy wall/door demo. --photo-blockout: NEW UNSAVED four-body concept with ASSUMED miniature dimensions; no original CAD inspection.");
                    return;
                }
                bool photoBlockout = args.Length == 1 && args[0] == "--photo-blockout";
                if (args.Length != 0 && !photoBlockout)
                    throw new ArgumentException("Unknown arguments. Use --help. No CAD connection attempted.");

                var connector = new SolidWorksConnector();
                connector.Connect();
                var modeler = new SolidWorksModeler(connector.Application);
                if (photoBlockout)
                {
                    Console.WriteLine(modeler.CreatePhotoBlockout());
                    return;
                }
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
