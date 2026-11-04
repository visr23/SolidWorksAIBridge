using System;
using SldWorks;
using SwConst;
namespace SolidWorksAIBridge
{
    public sealed partial class SolidWorksModeler
    {
        private static void CreatePhotoBlockoutPrism(ModelDoc2 doc, string name, double[,] polygon, double depthMm)
        {
            SketchManager sm = doc.SketchManager;
            bool oldAdd = sm.AddToDB;
            bool open = false;
            try
            {
                doc.ClearSelection2(true);
                if (!doc.Extension.SelectByID2("Front Plane", "PLANE", 0,0,0,false,0,null,0))
                    throw new Exception("Front Plane unavailable in new document.");
                sm.InsertSketch(true);
                open = sm.ActiveSketch != null;
                if (!open) throw new Exception("Cannot start sketch: " + name);
                sm.AddToDB = true;
                for (int i=0; i<polygon.GetLength(0); i++)
                {
                    int j=(i+1)%polygon.GetLength(0);
                    if (sm.CreateLine(polygon[i,0]/1000,polygon[i,1]/1000,0,
                        polygon[j,0]/1000,polygon[j,1]/1000,0)==null)
                        throw new Exception("Cannot create profile edge: " + name);
                }
                Feature sketch = (Feature)doc.FeatureByPositionReverse(0);
                if (sketch==null || sketch.GetTypeName2()!="ProfileFeature") throw new Exception("Sketch not found.");
                sketch.Name=name+"_ASSUMED_Profile";
                sm.InsertSketch(true); open=false;
                doc.ClearSelection2(true);
                if (!sketch.Select2(false,0)) throw new Exception("Cannot select profile.");
                Feature feature=doc.FeatureManager.FeatureExtrusion2(true,false,false,
                    (int)swEndConditions_e.swEndCondMidPlane,(int)swEndConditions_e.swEndCondBlind,
                    depthMm/1000,depthMm/1000,false,false,false,false,0,0,
                    false,false,false,false,false,false,false,
                    (int)swStartConditions_e.swStartSketchPlane,0,false);
                if(feature==null) throw new Exception("Extrusion failed: "+name);
                feature.Name=name+"_ASSUMED";
                if(!doc.EditRebuild3()) throw new Exception("Rebuild failed: "+name);
                Console.WriteLine("CREATED "+feature.Name);
            }
            finally
            {
                if(open) sm.InsertSketch(true);
                sm.AddToDB=oldAdd;
                doc.ClearSelection2(true);
            }
        }
        public string CreatePhotoBlockout()
        {
            // Creates only a new document. Never inspect or modify the active user part.
            {
                ModelDoc2 doc = CreateNewPart();
                Console.WriteLine("NEW_UNSAVED_PART "+doc.GetTitle());
                // Explicit concept assumptions in miniature mm, chosen from photo proportions.
                // These are not extracted from Part1 or any mesh/native feature tree.
                CreatePhotoBlockoutPrism(doc,"MainMass",new double[,]{{-20,0},{90,0},{90,80},{-20,80}},140);
                CreatePhotoBlockoutPrism(doc,"FrontLeftMass",new double[,]{{-90,0},{-20,0},{-20,65},{-90,65}},170);
                CreatePhotoBlockoutPrism(doc,"MainGableEnvelope",new double[,]{{-28,80},{98,80},{35,125}},150);
                CreatePhotoBlockoutPrism(doc,"SecondaryGableEnvelope",new double[,]{{-98,65},{-12,65},{-55,90}},180);
                PartDoc part=(PartDoc)doc;
                object[] bodies=(object[])part.GetBodies2((int)swBodyType_e.swSolidBody,false);
                int count=bodies==null?0:bodies.Length;
                double volume=0;
                foreach(object body in bodies??new object[0])
                {
                    double[] mass=(double[])((Body2)body).GetMassProperties(1.0);
                    if(mass==null || mass.Length<5) throw new Exception("Body mass properties unavailable.");
                    volume+=mass[3]*1e9;
                }
                double expected=110*80*140+70*65*170+126*45*0.5*150+86*25*0.5*180;
                if(count!=4) throw new Exception("Expected four separate concept bodies; actual="+count);
                if(Math.Abs(volume-expected)>expected*1e-6) throw new Exception("Volume mismatch; actual="+volume+" expected="+expected);
                doc.ShowNamedView2("*Isometric",(int)swStandardViews_e.swIsometricView);
                doc.ViewZoomtofit2();
                doc.GraphicsRedraw2();
                return "SUCCESS; NewPart="+doc.GetTitle()+"; Bodies="+count+"; VolumeMm3="+volume.ToString(System.Globalization.CultureInfo.InvariantCulture)+"; ExpectedMm3="+expected+"; Saved=false; Dimensions=ASSUMED_CONCEPT_ONLY; Roofs=SOLID_ENVELOPES_NOT_TILE_RECIPES";
            }
        }
    }
}
