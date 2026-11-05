using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SldWorks;

namespace SolidWorksAIBridge.Pilot001
{
    internal static class PilotProgram
    {
        [STAThread]
        private static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture=CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture=CultureInfo.InvariantCulture;
            bool cad=args.Length==1&&args[0]=="--confirm-cad-r2";
            if(!cad && !(args.Length==1&&args[0]=="--preflight"))
            {
                Console.WriteLine("Usage: AuePilot001.R2.exe --preflight | --confirm-cad-r2; latter creates ONE new unsaved experimental part, stops on first failure.");
                return args.Length==1&&args[0]=="--help"?0:2;
            }
            PilotBuilder builder=null;
            var previous=new List<Tuple<ModelDoc2,bool,string,string>>();
            SldWorks.SldWorks application=null;
            bool success=false;
            try
            {
                var plan=new R2Plan();plan.Validate();
                if(!cad)
                {
                    foreach(var invalid in new[]{"ArchitecturalHeight","RecessDepth","LargeLeft"})
                    {
                        var bad=new R2Plan();
                        bad.Parameters[invalid]=invalid=="ArchitecturalHeight"?181:invalid=="RecessDepth"?12:0;
                        bool rejected=false;
                        try{bad.Validate();}catch(InvalidOperationException){rejected=true;}
                        if(!rejected)throw new InvalidOperationException("Negative fixture accepted: "+invalid);
                        Console.WriteLine("NEGATIVE_PREFLIGHT PASS "+invalid);
                    }
                    return 0;
                }
                var connector=new SolidWorksConnector();connector.Connect();application=connector.Application;
                foreach(ModelDoc2 doc in application.GetDocuments() as object[]??new object[0])previous.Add(Tuple.Create(doc,doc.GetSaveFlag(),doc.GetTitle(),doc.GetPathName()));
                Console.WriteLine("EXISTING_DOCUMENTS {0}; SOLIDWORKS_REVISION {1}",previous.Count,application.RevisionNumber());
                // Persistent one-attempt guard; do not remove/retry after a CAD failure.
                string marker=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"R2_CAD_ATTEMPT.txt");
                using(var stream=new FileStream(marker,FileMode.CreateNew,FileAccess.Write))
                using(var writer=new StreamWriter(stream))writer.WriteLine(DateTime.UtcNow.ToString("O")+"; one isolated R2 test authorized; do not rerun");
                builder=new PilotBuilder(application,plan);builder.Run();success=true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("STOP_FIRST_FAILURE; Stage={0}; {1}",builder==null?"Preflight/Connection":builder.Stage,ex);
            }
            finally
            {
                if(builder!=null)
                {
                    try{builder.Report(success);}catch(Exception ex){Console.WriteLine("REPORT_FAILURE "+ex.Message);success=false;}
                    try{builder.Present();}catch(Exception ex){Console.WriteLine("PRESENT_FAILURE "+ex.Message);success=false;}
                }
                if(application!=null)
                {
                    bool preserved=true;
                    foreach(var item in previous)
                    {
                        try{preserved &= item.Item1.GetSaveFlag()==item.Item2 && item.Item1.GetTitle()==item.Item3 && item.Item1.GetPathName()==item.Item4;}
                        catch{preserved=false;}
                    }
                    int count=(application.GetDocuments() as object[]??new object[0]).Length;
                    Console.WriteLine("EXISTING_DOCUMENT_METADATA_PRESERVED {0}; Before={1}; After={2}; NoSaveOrCloseCalls=true",preserved,previous.Count,count);
                    if(!preserved||count!=previous.Count+1)success=false;
                }
            }
            return success?0:1;
        }
    }
}
