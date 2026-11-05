using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SolidWorksAIBridge.Pilot001
{
    internal sealed class Expression
    {
        public readonly double Value;
        public readonly string Text;
        public Expression(double value, string text) { Value = value; Text = text; }
        public static implicit operator Expression(double value) { return new Expression(value, value.ToString("R", CultureInfo.InvariantCulture) + "mm"); }
        public static Expression operator +(Expression a, Expression b) { return new Expression(a.Value + b.Value, "(" + a.Text + "+" + b.Text + ")"); }
        public static Expression operator -(Expression a, Expression b) { return new Expression(a.Value - b.Value, "(" + a.Text + "-" + b.Text + ")"); }
        public static Expression Half(Expression a) { return new Expression(a.Value / 2, "(" + a.Text + "/2)"); }
    }

    internal sealed class Point
    {
        public readonly Expression X, Y;
        public Point(Expression x, Expression y) { X = x; Y = y; }
    }

    internal sealed class Operation
    {
        public string Name;
        public Point[][] Loops;
        public Expression StartZ, Depth;
        public string Target;
        public bool Cut;
        public int Direction, ExpectedBodies;
        public double ExpectedVolume;
    }

    internal sealed class R2Plan
    {
        public readonly Dictionary<string, double> Parameters = new Dictionary<string, double>
        {
            {"FacadeWidth",224.5},{"WallHeight",99.8},{"BackingThickness",12},
            {"ArchitecturalHeight",180},{"BayWidth",99.7},{"BaseHeight",27.7},{"BayProjection",8},
            {"LargeLeft",8},{"LargeRight",90.9},{"LargeHead",76.4},
            {"EntranceLeft",104.9},{"EntranceRight",135.9},{"EntranceHead",81.7},{"RecessDepth",6},
            {"DoorLeft",110.2},{"DoorRight",131.1},{"DoorBottom",13.7},{"DoorHead",61.8},
            {"RightLeft",155.4},{"RightRight",178.8},{"RightBottom",31.6},{"RightHead",62},
            {"UpperLeft",108.6},{"UpperRight",157.4},{"UpperBottom",104.3},{"UpperHead",139.4},
            {"BorderLeft",3.5},{"BorderRight",3.5},{"BorderTop",3.5},{"BorderBottom",3.5},{"SurroundProjection",3},
            {"MainRoofRight",232.5},{"MainRidgeX",118.8},{"RoofJunctionX",48.4},{"LowerRidge",131.1},
            {"RoofJunctionY",104},{"LeftRoofLeft",-10.5},{"LeftRoofRight",105.7},{"LeftRidgeX",49.5},{"CutTolerance",0.2}
        };
        public Expression P(string name) { return new Expression(Parameters[name], "\"" + name + "\""); }
        public static Point[] Rectangle(Expression left, Expression bottom, Expression right, Expression top)
        {
            return new[] {new Point(left,bottom),new Point(right,bottom),new Point(right,top),new Point(left,top)};
        }
        public static double Area(Point[] polygon)
        {
            double area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                Point a = polygon[i], b = polygon[(i + 1) % polygon.Length];
                area += a.X.Value * b.Y.Value - b.X.Value * a.Y.Value;
            }
            return Math.Abs(area) / 2;
        }
        public List<Operation> Operations()
        {
            var list = new List<Operation>();
            double support = (224.5 * 99.8 - 23.4 * 30.4) * 12;
            Action<string,Point[],Expression,Expression,double> cut = (name,poly,z,depth,removed) =>
            {
                support -= removed;
                list.Add(new Operation {Name=name,Loops=new[]{poly},StartZ=z,Depth=depth,Cut=true,Direction=-1,Target="MainWall",ExpectedBodies=1,ExpectedVolume=support});
            };
            cut("EntranceRecess",Rectangle(P("EntranceLeft"),0,P("EntranceRight"),P("EntranceHead")),0,P("RecessDepth"),31*81.7*6);
            cut("DoorOpening",Rectangle(P("DoorLeft"),P("DoorBottom"),P("DoorRight"),P("DoorHead")),0-P("RecessDepth"),P("BackingThickness")-P("RecessDepth")+P("CutTolerance"),20.9*48.1*6);
            cut("LargeWindowOpening",Rectangle(P("LargeLeft"),P("BaseHeight"),P("LargeRight"),P("LargeHead")),0,P("BackingThickness")+P("CutTolerance"),82.9*48.7*12);
            var basePoly=Rectangle(0,0,P("BayWidth"),P("BaseHeight"));
            list.Add(new Operation {Name="LeftBayBase",Loops=new[]{basePoly},StartZ=0,Depth=P("BayProjection"),Direction=1,ExpectedBodies=2,ExpectedVolume=Area(basePoly)*8});
            var upper=new[]{new Point(0,P("BaseHeight")),new Point(P("LargeLeft"),P("BaseHeight")),new Point(P("LargeLeft"),P("LargeHead")),new Point(P("LargeRight"),P("LargeHead")),new Point(P("LargeRight"),P("BaseHeight")),new Point(P("BayWidth"),P("BaseHeight")),new Point(P("BayWidth"),P("WallHeight")),new Point(0,P("WallHeight"))};
            list.Add(new Operation {Name="LeftBayUpperSupport",Loops=new[]{upper},StartZ=0,Depth=P("BayProjection"),Direction=1,Target="LeftBayBase",ExpectedBodies=2,ExpectedVolume=(Area(basePoly)+Area(upper))*8});
            var main=new[]{new Point(P("BayWidth"),P("WallHeight")),new Point(P("MainRoofRight"),P("WallHeight")),new Point(P("MainRidgeX"),P("ArchitecturalHeight")),new Point(P("RoofJunctionX"),P("LowerRidge")),new Point(P("BayWidth"),P("RoofJunctionY"))};
            list.Add(new Operation {Name="MainGableProfile",Loops=new[]{main},StartZ=0,Depth=P("BackingThickness"),Direction=-1,ExpectedBodies=3,ExpectedVolume=Area(main)*12});
            var left=new[]{new Point(P("LeftRoofLeft"),P("WallHeight")),new Point(P("LeftRoofRight"),P("WallHeight")),new Point(P("LeftRidgeX"),P("LowerRidge"))};
            list.Add(new Operation {Name="LeftGableProfile",Loops=new[]{left},StartZ=0,Depth=P("BayProjection"),Direction=1,ExpectedBodies=4,ExpectedVolume=Area(left)*8});
            list.Add(new Operation {Name="UpperGableWindowOpening",Loops=new[]{Rectangle(P("UpperLeft"),P("UpperBottom"),P("UpperRight"),P("UpperHead"))},StartZ=0,Depth=P("BackingThickness")+P("CutTolerance"),Cut=true,Direction=-1,Target="MainGableProfile",ExpectedBodies=4,ExpectedVolume=Area(main)*12-48.8*35.1*12});
            var outer=Rectangle(P("RightLeft")-P("BorderLeft"),P("RightBottom")-P("BorderBottom"),P("RightRight")+P("BorderRight"),P("RightHead")+P("BorderTop"));
            var inner=Rectangle(P("RightLeft"),P("RightBottom"),P("RightRight"),P("RightHead"));
            list.Add(new Operation {Name="OpeningSurround",Loops=new[]{outer,inner},StartZ=0,Depth=P("SurroundProjection"),Direction=1,ExpectedBodies=5,ExpectedVolume=(Area(outer)-Area(inner))*3});
            return list;
        }
        public void Validate()
        {
            foreach(var item in Parameters)
                if(double.IsNaN(item.Value)||double.IsInfinity(item.Value)) throw new InvalidOperationException("Nonfinite "+item.Key);
            if(P("ArchitecturalHeight").Value!=180) throw new InvalidOperationException("Height lock changed.");
            var operations=Operations();
            if(operations.Count!=9 || operations.Last().ExpectedBodies!=5) throw new InvalidOperationException("Unexpected R2 topology.");
            foreach(var operation in operations)
            {
                if(operation.Depth.Value<=0 || operation.ExpectedVolume<=0) throw new InvalidOperationException("Invalid depth/volume: "+operation.Name);
                foreach(var polygon in operation.Loops)
                {
                    if(Area(polygon)<=0) throw new InvalidOperationException("Zero profile area.");
                    foreach(var point in polygon)
                        if(point.Y.Value<0 || point.Y.Value>180) throw new InvalidOperationException("Height envelope violation.");
                }
            }
            if(P("RecessDepth").Value>=P("BackingThickness").Value) throw new InvalidOperationException("Recess cuts through support.");
            if(P("LargeLeft").Value<=0 || P("LargeRight").Value>=P("BayWidth").Value || P("LargeHead").Value>=P("WallHeight").Value) throw new InvalidOperationException("Bay loses supporting piers/header.");
            Console.WriteLine("PREFLIGHT PASS; Operations=9; ExpectedFinalBodies=5; HeightTargetMm=180; Parameters=ESTIMATED");
            foreach(var operation in operations) Console.WriteLine("PLAN {0}; DepthMm={1:R}; ExpectedTargetVolumeMm3={2:R}",operation.Name,operation.Depth.Value,operation.ExpectedVolume);
        }
    }
}
