
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;

using PaintBooth.Database;
using PaintBooth.Model;
using PaintBooth.Models;
using System.Diagnostics;

//using SharpDX.DXGI;
using System.Drawing;
using System.Reflection;
using static devDept.Eyeshot.Entities.Brep;
using devregion = devDept.Eyeshot.Entities.Region;
namespace PaintBooth.Business
{
    public class PaintBoothDesignBusiness(AppDbContext _DbContext, IConfiguration _configuration)
    {

        #region Variables
        public DesignDocument drawing { get; set; }
        public DesignDocument developmentdrawing { get; set; }
        public int PaintBoothID { get; set; }
        public double ScaleFactor { get; set; }
        public int EnquiryID { get; set; }

        public double D1 { get; set; }

        public double JobSize { get; set; }

        public double D2 { get; set; }

        public double D3 { get; set; }

        public double W1 { get; set; }

        public double W2 { get; set; }

        public double W3 { get; set; }

        public double D { get; set; }


        public double H1 { get; set; }

        public double H2 { get; set; }

        public double W { get; set; }
        public double H { get; set; }
        public string material = string.Empty;
        public double PanelWidth { get; set; }
        public double PHForAboveECPanels { get; set; }
        public double PanelLength { get; set; }
        public double PanelLengthForSideDoors { get; set; }
        public double PanelWidthForSideDoors { get; set; }
        public double PanelHeightForSideDoors { get; set; }
        public double PanelWidthForFrontDoors { get; set; }
        public double PanelHeightForFrontDoors { get; set; }
        public double PanelLengthForTopPanels { get; set; }
        public double BackPanelHeightBeforeEC { get; set; }
        public double panelHeightforAboveCompEntryPanels { get; set; }
        public double panelWidthforAboveCompEntryPanels { get; set; }

        public double ExtractionChamberHeight { get; set; }
        public double PanelHeight { get; set; }
        public double SheetThickness { get; set; }
        public double SettingStandardBend1 { get; set; }
        public double SettingStandardBend2 { get; set; }
        public double SettingH { get; set; }
        public double SettingW { get; set; }
        public double SettingT { get; set; }
        public string Materials { get; set; }
        public string SlotDimention { get; set; }
        public double StandardBend1 { get; set; }
        public double StandardBend2 { get; set; }
        public string LightTypes { get; set; }
        public decimal LuxLevel { get; set; }
        public decimal Lumens { get; set; }
        public string Section { get; set; }
        public double PitchDistance { get; set; }
        public int NoofPanels { get; set; }
        public double BackPanelLength { get; set; }
        public double FrontPanelLength { get; set; }
        public double FrontPanelHeight { get; set; }
        static double totalPanelsforD; static double totalPanelsforW; static double totalPanelsforH;
        static double smallPanelWidthforD; 

        static double noOfFramesW;
        static int noOfPanelsforD; static int noOfPanelsforW; static int noOfPanelsforH;
        public string Weight = string.Empty;
        #region Extraction chamber 
        public double ECSheetThickness { get; set; }
        public string ECMaterials { get; set; }
        public string ECSlotDimention { get; set; }
        public double ECStandardBend1 { get; set; }
        public double ECStandardBend2 { get; set; }
        public string ECLightTypes { get; set; }
        public decimal ECLuxLevel { get; set; }
        public decimal ECLumens { get; set; }
        public string ECSection { get; set; }
        public double ECPitchDistance { get; set; }
        public int plenumHeight { get; set; }
        public string paintboothType { get; set; }
        #endregion


        public static double RightPanels_Weight = 0;
        public static double LeftPanels_Weight = 0;
        public static double D3Panels_weight = 0;
        public static double TopPanel_Weight = 0;
        public static double BackPanel_Weight = 0;
        public static double TopStructureFrame_Weight = 0;
        public static double BaseFrame_Weight = 0;
        public static decimal TubeLightCalculations = 0;
        public int[] panels;
        public int[] stiff;
        DrawingDocument drawingdoc = new DrawingDocument();
        public static int selectedBaffleHeight = 1140;
        public static int bafflePanelCount;
        public double XStartOffsetForSideDoors { get; set; } = 0;

        public static int roundScalefactor = 0; public static double totalWeightOfbaffle = 0; public static double totalWeight = 0; // Variable to store the total weight
        #endregion

        #region Right and Panels drawing
        public PanelDrawResult PanelsInPaintBooth(int j, PaintBoothModel model, int k)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;

            var rectangle1 = devregion.CreatePolygon(new Point3D[]
            {
                    new Point3D(0, PanelLength, 0),
                    new Point3D(0, PanelLength, PanelHeight),
                    new Point3D(PanelWidth, PanelLength, PanelHeight),
                    new Point3D(PanelWidth, PanelLength, 0)
            });
            Brep brep = rectangle1.ExtrudeAsBrep(SheetThickness);

            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.Green);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                    new Point3D(0, PanelLength, 0),
                    new Point3D(0, PanelLength, PanelHeight),
                    new Point3D(PanelWidth, PanelLength, PanelHeight),
                    new Point3D(PanelWidth, PanelLength, 0),
                    new Point3D(0, PanelLength, 0)
            });
            devregion section = CreatePolygon(k);
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, PanelLength, SheetThickness);
            // Generate holes on YZ and XY planes
            frame = GenerateHoles(frame);
            Material mat1 = Material.StructuralSteel;
            mat1 = new Material(Materials);

            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            drawing.Entities.Add(frame, Color.Yellow);

            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = "";

            if (k == 0)
            {
                dwgFilePath = $"{path}/PaintBooth drawing/RightPanel {j} {DateTime.Now:hh - mm}.dwg";
            }
            else
            {
                dwgFilePath = $"{path}/PaintBooth drawing/LeftPanel {j} {DateTime.Now:hh - mm}.dwg";
            }
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            string pdfFilePath = $"{path}/PaintBooth drawing/RightPanel {j} {DateTime.Now:hh - mm}.pdf";
            //pdf file

            Write3DPdfParams pdf = new Write3DPdfParams(drawing);
            Write3DPDF pdf1 = new Write3DPDF(pdf, pdfFilePath);
            pdf1.DoWork();
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult DoorsOnBothSide(PaintBoothModel model, int k, string paithboothType)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            double x = 0;

            if (paithboothType == "8" || paithboothType == "9")
            {
                x = model.D3;
            }

            var rectangle1 = devregion.CreatePolygon(new Point3D[]
            {
                    new Point3D(0, PanelLengthForSideDoors, 0),
                    new Point3D(0, PanelLengthForSideDoors, PanelHeightForSideDoors),
                    new Point3D(PanelWidthForSideDoors, PanelLengthForSideDoors, PanelHeightForSideDoors),
                    new Point3D(PanelWidthForSideDoors, PanelLengthForSideDoors, 0)
            });
            Brep brep = rectangle1.ExtrudeAsBrep(SheetThickness);

            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.Green);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                    new Point3D(0, PanelLengthForSideDoors, 0),
                    new Point3D(0, PanelLengthForSideDoors, PanelHeightForSideDoors),
                    new Point3D(PanelWidthForSideDoors, PanelLengthForSideDoors,PanelHeightForSideDoors),
                    new Point3D(PanelWidthForSideDoors, PanelLengthForSideDoors, 0),
                    new Point3D(0, PanelLengthForSideDoors, 0)
            });
            devregion section = CreatePolygon(k);
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, PanelLengthForSideDoors, SheetThickness);
            // ✅ Use XStartOffsetForSideDoors as base X position


            Material mat1 = Material.StructuralSteel;
            mat1 = new Material(Materials);

            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);

            drawing.Entities.Add(frame, Color.Yellow);

            

            #region Write file 
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = "";
            if (k == 0)
            {
                dwgFilePath = $"{path}/PaintBooth drawing/RightSidePanelDoors {k} {DateTime.Now:hh - mm}.dwg";
            }
            else
            {
                dwgFilePath = $"{path}/PaintBooth drawing/LeftSidePanelDoors {k} {DateTime.Now:hh - mm}.dwg";
            }
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };

        }
        public PanelDrawResult PanelsInPaintBoothForPlenum(int j, PaintBoothModel model, int k, int PlenumHeight, EnquiryMaster FASDoor)
        {
            drawing = new();

            drawing.Units = linearUnitsType.Millimeters;
            double z = plenumHeight = PlenumHeight;
            var rectangle1 = devregion.CreatePolygon(new Point3D[]
            {
                    new Point3D(0, PanelLength, 0),
                    new Point3D(0, PanelLength, z),
                    new Point3D(PanelWidth, PanelLength, z),
                    new Point3D(PanelWidth, PanelLength, 0)
            });
            Brep brep = rectangle1.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.AntiqueWhite);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                    new Point3D(0, PanelLength, 0),
                    new Point3D(0, PanelLength, z),
                    new Point3D(PanelWidth, PanelLength, z),
                    new Point3D(PanelWidth, PanelLength, 0),
                    new Point3D(0, PanelLength, 0)
            });
            devregion section = CreatePolygon(k);
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, PanelLength, SheetThickness);
            // Generate holes on YZ and XY planes
            frame = GenerateHoles(frame);
            Material mat1 = Material.StructuralSteel;
            mat1 = new Material(Materials);

            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            if (FASDoor.IsMaintenanceDoorAdded)
            {
                double doorPositionX = (PanelWidth - (double)FASDoor.MaintainanceDoorWidth) / 2;
                double doorWidthFAS = (double)FASDoor.MaintainanceDoorWidth;
                double doorHeightFAS = (z - (double)FASDoor.MaintainanceDoorHeight) / 2;
                if (j == 1)
                {
                    if (FASDoor.SideLocationDoorFAS == "Right" && k == 0)
                    {
                        Point3D[] cutout = new Point3D[]
                       // LinearPath cutout = new LinearPath(new Point3D[]

                       {
                        new Point3D(doorPositionX,                PanelLength, doorHeightFAS),
                        new Point3D(doorPositionX,                PanelLength,  doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(doorPositionX + doorWidthFAS, PanelLength, doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(doorPositionX + doorWidthFAS, PanelLength, doorHeightFAS)
                       };
                        devregion cutout1 = devregion.CreatePolygon(cutout);
                        brep.ExtrudeRemove(cutout1, SheetThickness);
                        drawing.Entities.Add(brep);
                        //Solid s = cutout1.SweepAsSolid(cutout,0);
                    }
                    else if (FASDoor.SideLocationDoorFAS == "Left" && k == 1)
                    {
                        Point3D[] cutout = new Point3D[]
                        {
                        new Point3D(doorPositionX,                PanelLength, doorHeightFAS),
                        new Point3D(doorPositionX,                PanelLength,  doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(doorPositionX + doorWidthFAS, PanelLength, doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(doorPositionX + doorWidthFAS, PanelLength, doorHeightFAS)
                        };
                        var cutout1 = devregion.CreatePolygon(cutout);
                        brep.ExtrudeRemove(cutout1, SheetThickness);
                        drawing.Entities.Add(brep);
                    }
                }
            }
            drawing.Entities.Add(frame, Color.Yellow);
            #region Write files
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = "";
            if (k == 0)
            {
                dwgFilePath = $"{path}/PaintBooth drawing/RightPanelForFAS {j} {DateTime.Now:hh - mm}.dwg";
            }
            else
            {
                dwgFilePath = $"{path}/PaintBooth drawing/LeftPanelForFAS {j} {DateTime.Now:hh - mm}.dwg";
            }
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };

        }
        public PanelDrawResult PanelsAboveEntryDoor(int k, DoorDimensionsModel DoorModel)
        {

            drawing = new();
            //double z = DoorModel.doorHeight;
            double z = DoorModel.doorHeight + DoorModel.panelZOffsetAboveDoor;  // ✅ Z accumulates per row
            //double panelHeight = panelHeightforAboveCompEntryPanels = DoorModel.panelHeightforAboveCompEntryPanels;
            double panelHeight = panelHeightforAboveCompEntryPanels = DoorModel.PHforAboveCompEntryPanels;
            double panelWidth = panelWidthforAboveCompEntryPanels = DoorModel.panelWidthforAboveCompEntryPanels;
            drawing.Units = linearUnitsType.Millimeters;



            var rectangle1 = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(DoorModel.xOffeset, PanelLengthForSideDoors, z),
                new Point3D(DoorModel.xOffeset, PanelLengthForSideDoors, panelHeight+z),
                new Point3D(DoorModel.xOffeset+panelWidth, PanelLengthForSideDoors, panelHeight+z),
                new Point3D(DoorModel.xOffeset+panelWidth, PanelLengthForSideDoors, z)
            });
            Brep brep = rectangle1.ExtrudeAsBrep(SheetThickness);

            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.Green);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0, PanelLengthForSideDoors, 0),
                new Point3D(0, PanelLengthForSideDoors, panelHeight),
                new Point3D(panelWidth, PanelLengthForSideDoors,panelHeight),
                new Point3D(panelWidth, PanelLengthForSideDoors, 0),
                new Point3D(0, PanelLengthForSideDoors, 0)
                
            });
            double sectionY = (k == 0) ? 0 : PanelLengthForSideDoors;
            devregion section = CreatePolygon(k);
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(DoorModel.xOffeset, PanelLengthForSideDoors, SheetThickness + z);
            // Generate holes on YZ and XY planes
            frame = GenerateHoles(frame);
            Material mat1 = Material.StructuralSteel;
            mat1 = new Material(Materials);

            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            drawing.Entities.Add(frame, Color.Yellow);

            #region Write file 
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = "";
            if (k == 0)
            {
                dwgFilePath = $"{path}/PaintBooth drawing/RightPanelsAboveEntryDoor {k} {DateTime.Now:hh - mm}.dwg";
            }
            else
            {
                dwgFilePath = $"{path}/PaintBooth drawing/LeftPanelPanelsAboveEntryDoor {k} {DateTime.Now:hh - mm}.dwg";
            }
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };

        }
        #endregion
        public PanelDrawResult PanelsAboveFrontEntryDoor(DoorDimensionsModel DoorModel)
        {

            drawing = new();
            double z = DoorModel.doorHeight;
            double panelHeight = panelHeightforAboveCompEntryPanels = DoorModel.panelHeightforAboveCompEntryPanels;
            double panelWidth = panelWidthforAboveCompEntryPanels = DoorModel.panelWidthforAboveCompEntryPanels;
            drawing.Units = linearUnitsType.Millimeters;
            var rectangle1 = devregion.CreatePolygon(new Point3D[]
            {

                new Point3D(0,  0,              0),
                new Point3D(0,  panelWidth,     0),
                new Point3D(0,  panelWidth,     panelHeight),
                new Point3D(0,  0,              panelHeight)

                //new Point3D(0,  DoorModel.xOffeset,              z),
                //new Point3D(0,  DoorModel.xOffeset+panelWidth,     z),
                //new Point3D(0,  DoorModel.xOffeset+panelWidth,     panelHeight+z),
                //new Point3D(0,  DoorModel.xOffeset,              panelHeight+z)
            });

            Brep brep = rectangle1.ExtrudeAsBrep(SheetThickness);
            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.Green);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,  0,          0),
                new Point3D(0,  panelWidth, 0),
                new Point3D(0,  panelWidth, panelHeight),
                new Point3D(0,  0,          panelHeight),
                new Point3D(0,  0,          0)
                
                //new Point3D(0,  DoorModel.xOffeset,              z),
                //new Point3D(0,  DoorModel.xOffeset+panelWidth,     z),
                //new Point3D(0,  DoorModel.xOffeset+panelWidth,     panelHeight+z),
                //new Point3D(0,  DoorModel.xOffeset,              panelHeight+z),
                //new Point3D(0,  DoorModel.xOffeset,              z)
            });

            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,              0,      0),
                new Point3D(-36.8,           0,      0),
                new Point3D(-36.8,           0,      SheetThickness),
                new Point3D(-SheetThickness, 0,      SheetThickness),
                new Point3D(-SheetThickness, 0,      36.8),
                new Point3D(0,              0,      36.8)

            });
            // Move section to rail start
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);
            frame = GenerateHoles(frame);
            Material mat1 = Material.StructuralSteel;
            mat1 = new Material(Materials);
            frame.Regen(0.1);

            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            drawing.Entities.Add(frame, Color.Yellow);

            #region Write file 
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");


            string dwgFilePath = $"{path}/PaintBooth drawing/PanelsAboveFrontEntryDoor {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }

        #region 3D D3 Panels Drawings
        //public PanelDrawResult D3Panels(int i, PaintBoothModel model, double ExtractionChemberHeight)
        //{
        //    //string panelPosition;
        //    drawing = new();
        //    drawing.Units = linearUnitsType.Millimeters;
        //    const string Layer = "OuterDoorFrame";
        //    drawing.Layers.Add(new Layer(Layer, Color.Gray));
        //    ExtractionChamberHeight = ExtractionChemberHeight;
        //    double x = model.D;
        //    PanelLength = (i == 0) ? 0 : model.W;
        //    var rectangle = devregion.CreatePolygon(new Point3D[]
        //    {
        //            new Point3D(0, PanelLength, 0),
        //            new Point3D(0, PanelLength, ExtractionChemberHeight),
        //            new Point3D(D3, PanelLength, ExtractionChemberHeight),
        //            new Point3D(D3, PanelLength, 0)
        //    });
        //    Brep brep = rectangle.ExtrudeAsBrep(ECSheetThickness);
        //    //calculate Weight of panels 
        //    Material mat = Material.StructuralSteel;
        //    mat = new Material(ECMaterials);
        //    brep.Regen(0.1);
        //    double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
        //    double Rectangle_Weight = Math.Round(massofRectangle, 3);
        //    drawing.Entities.Add(brep, Color.Green);

        //    LinearPath rail = new LinearPath(new Point3D[]
        //    {
        //            new Point3D(0, PanelLength, 0),
        //            new Point3D(0, PanelLength, ExtractionChemberHeight),
        //            new Point3D(D3, PanelLength, ExtractionChemberHeight),
        //            new Point3D(D3, PanelLength, 0),
        //            new Point3D(0, PanelLength, 0)
        //    });
        //    devregion section = CreatePolygonForEC(i);

        //    Solid frame = section.SweepAsSolid(rail, 0);
        //    frame.Translate(0, PanelLength, ECSheetThickness);

        //    // Generate holes on YZ and XY planes
        //    GenerateHolesForEC(frame);
        //    //Calculate weight of panels
        //    Material mat1 = Material.StructuralSteel;
        //    mat1 = new Material(ECMaterials);
        //    frame.Regen(0.1);
        //    double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
        //    double Frame_Weight = Math.Round(massofFrame, 3);
        //    D3Panels_weight = Rectangle_Weight + Frame_Weight;
        //    drawing.Entities.Add(frame, Color.Yellow);

        //    //Added serviceDoor code here 
        //    double doorWidth = model.DoorWidth; // Width of the service door
        //    double doorHeight = model.DoorHeight; // Height of the service door
        //    double doorPositionX = (D3 - doorWidth) / 2;
        //    double doorStartHeight = 100; // Starting height above the base
        //    double sectionDoorTHK = ECSheetThickness;
        //    double sectionDoorHeight = 76;
        //    double sectionDoorWidth = 38;


        //    double frameY = 0;     // same Y you used for door cutout
        //    double z0 = doorStartHeight;
        //    double z1 = doorStartHeight + doorHeight;
        //    double x0 = doorPositionX;
        //    double T = sectionDoorTHK;
        //    double L = sectionDoorWidth - (2 * T);
        //    double outerLineDistance = 22.5;
        //    #region old service door code
        //    //if (model.chkServiceDoor)
        //    //{
        //    //    if (model.ServiceDoorLocation == "RightSide" && i == 0)
        //    //    {
        //    //        {

        //    //            Point3D[] cutout = new Point3D[]
        //    //            {
        //    //            new Point3D(doorPositionX, PanelLength, doorStartHeight),
        //    //            new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
        //    //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
        //    //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
        //    //            };
        //    //            var cutout1 = devregion.CreatePolygon(cutout);

        //    //            brep.ExtrudeRemove(cutout1, ECSheetThickness);
        //    //            drawing.Entities.Add(brep);

        //    //            var Door = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(doorPositionX, PanelLength, doorStartHeight),
        //    //            new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
        //    //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
        //    //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
        //    //            });
        //    //            Brep DoorBrep = Door.ExtrudeAsBrep(-ECSheetThickness);//1.6 mm Thickness
        //    //            drawing.Entities.Add(DoorBrep, Color.Aquamarine);

        //    //            #region outer frame
        //    //            #region top frame section
        //    //            //YZ Plane
        //    //            var topsectionframe = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(x0-sectionDoorWidth,  -frameY-T, z1),                     // 1
        //    //            new Point3D(x0-sectionDoorWidth,  -frameY-T, z1-sectionDoorHeight+T), // 2
        //    //            new Point3D(x0-sectionDoorWidth,   0,        z1-sectionDoorHeight+T), // 3
        //    //            new Point3D(x0-sectionDoorWidth,   0,        z1+T),      // 4

        //    //            new Point3D(x0 - sectionDoorWidth,  -L-T-T,   z1+T), // 5
        //    //            new Point3D(x0 - sectionDoorWidth,  -L-T-T,   z1-sectionDoorHeight+T), // 6
        //    //            new Point3D(x0 - sectionDoorWidth,  -L-T,     z1-sectionDoorHeight+T),  // 7
        //    //            new Point3D(x0 - sectionDoorWidth,  -L-T,     z1),        // 8


        //    //            });

        //    //            Brep topsectionframeBrep = topsectionframe.ExtrudeAsBrep(doorWidth + (2 * sectionDoorWidth));
        //    //            drawing.Entities.Add(topsectionframeBrep, Layer);
        //    //            #endregion

        //    //            #region Base Frame Section
        //    //            var basesectionframe = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(x0 - sectionDoorWidth ,    -frameY-T,  z0-L+T),        // 8
        //    //            new Point3D(x0 - sectionDoorWidth ,    -frameY-T,  z0),                     // 1
        //    //            new Point3D(x0 - sectionDoorWidth,     0,          z0), // 2
        //    //            new Point3D(x0 - sectionDoorWidth ,    0,          z0-L), // 3

        //    //            new Point3D(x0 - sectionDoorWidth ,    -L-T-T,     z0-L),      // 4
        //    //            new Point3D(x0 - sectionDoorWidth ,   -L-T-T,     z0), // 5
        //    //            new Point3D(x0 - sectionDoorWidth ,   -L-T,       z0), // 6
        //    //            new Point3D(x0 - sectionDoorWidth ,    -L-T,       z0-L+T), // 7
        //    //            });
        //    //            Brep basesectionframeBrep = basesectionframe.ExtrudeAsBrep(-doorWidth - (2 * sectionDoorWidth));
        //    //            drawing.Entities.Add(basesectionframeBrep, Layer);
        //    //            #endregion

        //    //            #region left L section
        //    //            var leftLFrameH = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //             new Point3D(x0- sectionDoorWidth ,    0,      z1+T),
        //    //             new Point3D(x0- sectionDoorWidth ,    -L-T-T,  z1+T),
        //    //             new Point3D(x0-T- sectionDoorWidth ,  -L-T-T, z1+T),
        //    //             new Point3D(x0-T- sectionDoorWidth ,   0,     z1+T),
        //    //            });

        //    //            Brep leftLFrameHBrep = leftLFrameH.ExtrudeAsBrep(doorHeight + L + T);
        //    //            drawing.Entities.Add(leftLFrameHBrep, Layer);


        //    //            var leftLFrameW = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //             new Point3D(x0 - sectionDoorWidth+ECSheetThickness,     T-T,    z1-sectionDoorHeight),
        //    //             new Point3D(x0+L- sectionDoorWidth+ECSheetThickness,    T-T,    z1-sectionDoorHeight),
        //    //             new Point3D(x0 + L - sectionDoorWidth+ECSheetThickness, 0-T,    z1-sectionDoorHeight),
        //    //             new Point3D(x0 - sectionDoorWidth+ECSheetThickness ,    0-T,    z1-sectionDoorHeight),
        //    //            });
        //    //            Brep leftLFrameWBrep = leftLFrameW.ExtrudeAsBrep(doorHeight - sectionDoorHeight - ECSheetThickness);
        //    //            drawing.Entities.Add(leftLFrameWBrep, Layer);

        //    //            #endregion

        //    //            #region Right L section
        //    //            Brep rightLFrameHBrep = (Brep)leftLFrameHBrep.Clone();
        //    //            rightLFrameHBrep.Translate(doorWidth + (2 * sectionDoorWidth), 0, 0);
        //    //            drawing.Entities.Add(rightLFrameHBrep, Layer);

        //    //            Brep rightleftLFrameWBrep = (Brep)leftLFrameWBrep.Clone();
        //    //            rightleftLFrameWBrep.Translate(doorWidth + sectionDoorWidth, 0, 0);
        //    //            drawing.Entities.Add(rightleftLFrameWBrep, Layer);

        //    //            #endregion
        //    //            #endregion

        //    //            #region Outer Rectangle
        //    //            const string OuterRectangleLayer = "OuterRectangleLayer";
        //    //            drawing.Layers.Add(new Layer(OuterRectangleLayer, Color.BlueViolet));
        //    //            #region Left Outer Rectangle
        //    //            var LeftOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(x0-22.5,            -frameY-T+ ECSheetThickness, z1+22.5),//Left Line
        //    //            new Point3D(x0-22.5,            -frameY-T+ ECSheetThickness, z0-22.5),
        //    //            new Point3D(x0+27.5,  -frameY-T+ ECSheetThickness, z0-22.5),
        //    //            new Point3D(x0+27.5,  -frameY-T+ ECSheetThickness, z1+22.5),//Right line

        //    //            });

        //    //            Brep LeftOuterRectanglebrep = LeftOuterRectangle.ExtrudeAsBrep(-3);/*3 is Thickness*/
        //    //            drawing.Entities.Add(LeftOuterRectanglebrep, OuterRectangleLayer);




        //    //            #endregion

        //    //            #region Right Outer Rectangle
        //    //            var RightOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(x0+doorWidth-22.5,            -frameY - T + 1.6, z1+22.5),//Left Line
        //    //            new Point3D(x0+doorWidth-22.5,            -frameY - T + 1.6, z0-22.5),
        //    //            new Point3D(x0+doorWidth+27.5,  -frameY - T + 1.6, z0-22.5),
        //    //            new Point3D(x0+doorWidth+27.5,  -frameY-T+1.6, z1+22.5),//Right line

        //    //            });

        //    //            Brep RightOuterRectanglebrep = RightOuterRectangle.ExtrudeAsBrep(-3);/*3 is Thickness*/
        //    //            drawing.Entities.Add(RightOuterRectanglebrep, OuterRectangleLayer);
        //    //            #endregion

        //    //            #region Top Outer Rectangle
        //    //            var TopOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(x0+27.5,            -frameY-T+1.6, z1+22.5),//Left Line 1.6 is Thickness
        //    //            new Point3D(x0+27.5,            -frameY-T+1.6, z1-22.5),
        //    //            new Point3D(x0+doorWidth-22.5,  -frameY-T+1.6, z1-22.5),
        //    //            new Point3D(x0+doorWidth-22.5,  -frameY-T+1.6, z1+22.5),//Right line

        //    //            });
        //    //            Brep TopOuterRectanglebrep = TopOuterRectangle.ExtrudeAsBrep(-3);/*3 is Thickness*/
        //    //            drawing.Entities.Add(TopOuterRectanglebrep, OuterRectangleLayer);
        //    //            #endregion

        //    //            #region Bottom Outer Rectangle
        //    //            var BottomOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //    //            {
        //    //            new Point3D(x0+27.5,            -frameY-T+1.6, z0+22.5),//Left Line
        //    //            new Point3D(x0+27.5,            -frameY-T+1.6, z0-22.5),
        //    //            new Point3D(x0+doorWidth-22.5,  -frameY-T+1.6, z0-22.5),
        //    //            new Point3D(x0+doorWidth-22.5,  -frameY-T+1.6, z0+22.5),//Right line

        //    //            });
        //    //            Brep BottomOuterRectanglebrep = BottomOuterRectangle.ExtrudeAsBrep(-3);/*3 is Thickness*/
        //    //            drawing.Entities.Add(BottomOuterRectanglebrep, OuterRectangleLayer);
        //    //            #endregion
        //    //            #endregion
        //    //        }

        //    //    }
        //    //    else if (model.ServiceDoorLocation == "LeftSide" && i == 1)
        //    //    {
        //    //        {
        //    //            Point3D[] cutout = new Point3D[]
        //    //        {
        //    //        new Point3D(doorPositionX, PanelLength, doorStartHeight),
        //    //        new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
        //    //        new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
        //    //        new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
        //    //        };
        //    //            var cutout1 = devregion.CreatePolygon(cutout);
        //    //            brep.ExtrudeRemove(cutout1, ECSheetThickness);
        //    //            drawing.Entities.Add(brep);

        //    //        }
        //    //    }
        //    //    else if (model.ServiceDoorLocation == "Both Side")
        //    //    {
        //    //        {
        //    //            Point3D[] cutout = new Point3D[]
        //    //            {
        //    //            new Point3D(doorPositionX, PanelLength, doorStartHeight),
        //    //            new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
        //    //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
        //    //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
        //    //            };
        //    //            var cutoutPolygon = devregion.CreatePolygon(cutout);
        //    //            brep.ExtrudeRemove(cutoutPolygon, ECSheetThickness);
        //    //            drawing.Entities.Add(brep);
        //    //        }
        //    //    }
        //    //} 
        //    #endregion
        //    bool doorHere = model.chkServiceDoor &&
        //       ((model.ServiceDoorLocation == "RightSide" && i == 0) ||
        //        (model.ServiceDoorLocation == "LeftSide" && i == 1) ||
        //         model.ServiceDoorLocation == "Both Side");
        //    if (doorHere)
        //    {
        //        // 1) cut the opening in THIS panel (PanelLength is already 0 or W)
        //        var cutout1 = devregion.CreatePolygon(new Point3D[]
        //        {
        //new Point3D(doorPositionX,             PanelLength, doorStartHeight),
        //new Point3D(doorPositionX,             PanelLength, doorStartHeight + doorHeight),
        //new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
        //new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
        //        });
        //        brep.ExtrudeRemove(cutout1, ECSheetThickness);

        //        // 2) build door + frames on the y = 0 side
        //        int firstDoorEntity = drawing.Entities.Count;

        //        var Door = devregion.CreatePolygon(new Point3D[]
        //        {
        //new Point3D(doorPositionX,             0, doorStartHeight),
        //new Point3D(doorPositionX,             0, doorStartHeight + doorHeight),
        //new Point3D(doorPositionX + doorWidth, 0, doorStartHeight + doorHeight),
        //new Point3D(doorPositionX + doorWidth, 0, doorStartHeight)
        //        });
        //        Brep DoorBrep = Door.ExtrudeAsBrep(-ECSheetThickness);
        //        drawing.Entities.Add(DoorBrep, Color.Aquamarine);

        //        // ↓↓↓ paste your existing code here UNCHANGED:
        //        //     everything from  #region outer frame  to the end of  #endregion (Outer Rectangle)
        //        //     (top/base frame sections, left/right L sections, the 4 outer rectangles)

        //        // 3) left panel: turn the door parts 180° about Z onto the y = W wall (keeps solids valid, unlike a mirror)
        //        if (i == 1)
        //        {
        //            var turn = new Rotation(Math.PI, Vector3D.AxisZ, new Point3D(D3 / 2, model.W / 2, 0));
        //            for (int n = firstDoorEntity; n < drawing.Entities.Count; n++)
        //                drawing.Entities[n].TransformBy(turn);
        //        }
        //    }
        //    #region WriteFile
        //    var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

        //    if (!Directory.Exists(path + "/PaintBooth drawing"))
        //        Directory.CreateDirectory(path + "/PaintBooth drawing");
        //    string dwgFilePath;
        //    if (i == 0)
        //    {
        //        dwgFilePath = $"{path}/PaintBooth drawing/D3Panels Right side  {i} {DateTime.Now:hh - mm}.dwg";
        //    }
        //    else
        //    {
        //        dwgFilePath = $"{path}/PaintBooth drawing/D3Panels Left side {i} {DateTime.Now:hh - mm}.dwg";
        //    }
        //    WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
        //    WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
        //    dwgg1.DoWork();

        //    #endregion
        //    return new PanelDrawResult
        //    {
        //        drawing = drawing,
        //        Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
        //        lstpath = dwgFilePath,
        //    };

        //}

        //public PanelDrawResult D3Panels(int i, PaintBoothModel model, double ExtractionChemberHeight)
        //{
        //    drawing = new();
        //    drawing.Units = linearUnitsType.Millimeters;
        //    const string Layer = "OuterDoorFrame";
        //    drawing.Layers.Add(new Layer(Layer, Color.Gray));
        //    ExtractionChamberHeight = ExtractionChemberHeight;
        //    double x = model.D;
        //    PanelLength = (i == 0) ? 0 : model.W;

        //    #region Panel sheet
        //    var rectangle = devregion.CreatePolygon(new Point3D[]
        //    {
        //new Point3D(0,  PanelLength, 0),
        //new Point3D(0,  PanelLength, ExtractionChemberHeight),
        //new Point3D(D3, PanelLength, ExtractionChemberHeight),
        //new Point3D(D3, PanelLength, 0)
        //    });
        //    Brep brep = rectangle.ExtrudeAsBrep(ECSheetThickness);
        //    Material mat = new Material(ECMaterials);
        //    brep.Regen(0.1);
        //    double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
        //    double Rectangle_Weight = Math.Round(massofRectangle, 3);
        //    drawing.Entities.Add(brep, Color.Green);
        //    #endregion

        //    #region Panel frame
        //    LinearPath rail = new LinearPath(new Point3D[]
        //    {
        //new Point3D(0,  PanelLength, 0),
        //new Point3D(0,  PanelLength, ExtractionChemberHeight),
        //new Point3D(D3, PanelLength, ExtractionChemberHeight),
        //new Point3D(D3, PanelLength, 0),
        //new Point3D(0,  PanelLength, 0)
        //    });
        //    devregion section = CreatePolygonForEC(i);
        //    Solid frame = section.SweepAsSolid(rail, 0);
        //    frame.Translate(0, PanelLength, ECSheetThickness);
        //    GenerateHolesForEC(frame);

        //    Material mat1 = new Material(ECMaterials);
        //    frame.Regen(0.1);
        //    double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
        //    double Frame_Weight = Math.Round(massofFrame, 3);
        //    D3Panels_weight = Rectangle_Weight + Frame_Weight;
        //    drawing.Entities.Add(frame, Color.Yellow);
        //    #endregion

        //    #region Service door variables
        //    double doorWidth = model.DoorWidth;
        //    double doorHeight = model.DoorHeight;
        //    double doorPositionX = (D3 - doorWidth) / 2;
        //    double doorStartHeight = 100;               // keep same value as the GA label
        //    double sectionDoorTHK = ECSheetThickness;
        //    double sectionDoorHeight = 76;
        //    double sectionDoorWidth = 38;

        //    double frameY = 0;                          // door is always built on the y = 0 side, turned for left panel
        //    double z0 = doorStartHeight;
        //    double z1 = doorStartHeight + doorHeight;
        //    double x0 = doorPositionX;
        //    double T = sectionDoorTHK;
        //    double L = sectionDoorWidth - (2 * T);
        //    #endregion

        //    #region Service door (RightSide / LeftSide / Both Side)
        //    bool doorHere = model.chkServiceDoor &&
        //                    ((model.ServiceDoorLocation == "RightSide" && i == 0) ||
        //                     (model.ServiceDoorLocation == "LeftSide" && i == 1) ||
        //                      model.ServiceDoorLocation == "Both Side");
        //    if (doorHere)
        //    {
        //        // 1) cut the opening in THIS panel (PanelLength is already 0 or W)
        //        var cutout1 = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(doorPositionX,             PanelLength, doorStartHeight),
        //    new Point3D(doorPositionX,             PanelLength, doorStartHeight + doorHeight),
        //    new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
        //    new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
        //        });
        //        brep.ExtrudeRemove(cutout1, ECSheetThickness);

        //        // 2) everything added from here is a door part → turned later for the left panel
        //        int firstDoorEntity = drawing.Entities.Count;

        //        #region Door sheet
        //        var Door = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(doorPositionX,             0, doorStartHeight),
        //    new Point3D(doorPositionX,             0, doorStartHeight + doorHeight),
        //    new Point3D(doorPositionX + doorWidth, 0, doorStartHeight + doorHeight),
        //    new Point3D(doorPositionX + doorWidth, 0, doorStartHeight)
        //        });
        //        Brep DoorBrep = Door.ExtrudeAsBrep(-ECSheetThickness);
        //        drawing.Entities.Add(DoorBrep, Color.Aquamarine);
        //        #endregion

        //        #region outer frame
        //        #region top frame section
        //        var topsectionframe = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 - sectionDoorWidth, -frameY - T, z1),
        //    new Point3D(x0 - sectionDoorWidth, -frameY - T, z1 - sectionDoorHeight + T),
        //    new Point3D(x0 - sectionDoorWidth,  0,          z1 - sectionDoorHeight + T),
        //    new Point3D(x0 - sectionDoorWidth,  0,          z1 + T),
        //    new Point3D(x0 - sectionDoorWidth, -L - T - T,  z1 + T),
        //    new Point3D(x0 - sectionDoorWidth, -L - T - T,  z1 - sectionDoorHeight + T),
        //    new Point3D(x0 - sectionDoorWidth, -L - T,      z1 - sectionDoorHeight + T),
        //    new Point3D(x0 - sectionDoorWidth, -L - T,      z1),
        //        });
        //        Brep topsectionframeBrep = topsectionframe.ExtrudeAsBrep(doorWidth + (2 * sectionDoorWidth));
        //        drawing.Entities.Add(topsectionframeBrep, Layer);
        //        #endregion

        //        #region Base Frame Section
        //        var basesectionframe = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 - sectionDoorWidth, -frameY - T, z0 - L + T),
        //    new Point3D(x0 - sectionDoorWidth, -frameY - T, z0),
        //    new Point3D(x0 - sectionDoorWidth,  0,          z0),
        //    new Point3D(x0 - sectionDoorWidth,  0,          z0 - L),
        //    new Point3D(x0 - sectionDoorWidth, -L - T - T,  z0 - L),
        //    new Point3D(x0 - sectionDoorWidth, -L - T - T,  z0),
        //    new Point3D(x0 - sectionDoorWidth, -L - T,      z0),
        //    new Point3D(x0 - sectionDoorWidth, -L - T,      z0 - L + T),
        //        });
        //        Brep basesectionframeBrep = basesectionframe.ExtrudeAsBrep(-doorWidth - (2 * sectionDoorWidth));
        //        drawing.Entities.Add(basesectionframeBrep, Layer);
        //        #endregion

        //        #region left L section
        //        var leftLFrameH = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 - sectionDoorWidth,     0,         z1 + T),
        //    new Point3D(x0 - sectionDoorWidth,     -L - T - T, z1 + T),
        //    new Point3D(x0 - T - sectionDoorWidth, -L - T - T, z1 + T),
        //    new Point3D(x0 - T - sectionDoorWidth, 0,          z1 + T),
        //        });
        //        Brep leftLFrameHBrep = leftLFrameH.ExtrudeAsBrep(doorHeight + L + T);
        //        drawing.Entities.Add(leftLFrameHBrep, Layer);

        //        var leftLFrameW = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 - sectionDoorWidth + ECSheetThickness,     0,  z1 - sectionDoorHeight),
        //    new Point3D(x0 + L - sectionDoorWidth + ECSheetThickness, 0,  z1 - sectionDoorHeight),
        //    new Point3D(x0 + L - sectionDoorWidth + ECSheetThickness, -T, z1 - sectionDoorHeight),
        //    new Point3D(x0 - sectionDoorWidth + ECSheetThickness,     -T, z1 - sectionDoorHeight),
        //        });
        //        Brep leftLFrameWBrep = leftLFrameW.ExtrudeAsBrep(doorHeight - sectionDoorHeight - ECSheetThickness);
        //        drawing.Entities.Add(leftLFrameWBrep, Layer);
        //        #endregion

        //        #region Right L section
        //        Brep rightLFrameHBrep = (Brep)leftLFrameHBrep.Clone();
        //        rightLFrameHBrep.Translate(doorWidth + (2 * sectionDoorWidth), 0, 0);
        //        drawing.Entities.Add(rightLFrameHBrep, Layer);

        //        Brep rightLFrameWBrep = (Brep)leftLFrameWBrep.Clone();
        //        rightLFrameWBrep.Translate(doorWidth + sectionDoorWidth, 0, 0);
        //        drawing.Entities.Add(rightLFrameWBrep, Layer);
        //        #endregion
        //        #endregion

        //        #region Outer Rectangle
        //        const string OuterRectangleLayer = "OuterRectangleLayer";
        //        drawing.Layers.Add(new Layer(OuterRectangleLayer, Color.BlueViolet));
        //        double yOuter = -frameY - T + ECSheetThickness;

        //        var LeftOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 - 22.5, yOuter, z1 + 22.5),
        //    new Point3D(x0 - 22.5, yOuter, z0 - 22.5),
        //    new Point3D(x0 + 27.5, yOuter, z0 - 22.5),
        //    new Point3D(x0 + 27.5, yOuter, z1 + 22.5),
        //        });
        //        drawing.Entities.Add(LeftOuterRectangle.ExtrudeAsBrep(-3), OuterRectangleLayer);

        //        var RightOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 + doorWidth - 22.5, yOuter, z1 + 22.5),
        //    new Point3D(x0 + doorWidth - 22.5, yOuter, z0 - 22.5),
        //    new Point3D(x0 + doorWidth + 27.5, yOuter, z0 - 22.5),
        //    new Point3D(x0 + doorWidth + 27.5, yOuter, z1 + 22.5),
        //        });
        //        drawing.Entities.Add(RightOuterRectangle.ExtrudeAsBrep(-3), OuterRectangleLayer);

        //        var TopOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 + 27.5,             yOuter, z1 + 22.5),
        //    new Point3D(x0 + 27.5,             yOuter, z1 - 22.5),
        //    new Point3D(x0 + doorWidth - 22.5, yOuter, z1 - 22.5),
        //    new Point3D(x0 + doorWidth - 22.5, yOuter, z1 + 22.5),
        //        });
        //        drawing.Entities.Add(TopOuterRectangle.ExtrudeAsBrep(-3), OuterRectangleLayer);

        //        var BottomOuterRectangle = devregion.CreatePolygon(new Point3D[]
        //        {
        //    new Point3D(x0 + 27.5,             yOuter, z0 + 22.5),
        //    new Point3D(x0 + 27.5,             yOuter, z0 - 22.5),
        //    new Point3D(x0 + doorWidth - 22.5, yOuter, z0 - 22.5),
        //    new Point3D(x0 + doorWidth - 22.5, yOuter, z0 + 22.5),
        //        });
        //        drawing.Entities.Add(BottomOuterRectangle.ExtrudeAsBrep(-3), OuterRectangleLayer);
        //        #endregion

        //        // 3) left panel: turn all door parts 180° about Z onto the y = W wall (keeps solids valid, unlike a mirror)
        //        if (i == 1)
        //        {
        //            var turn = new Rotation(Math.PI, Vector3D.AxisZ, new Point3D(D3 / 2, model.W / 2, 0));
        //            for (int n = firstDoorEntity; n < drawing.Entities.Count; n++)
        //                drawing.Entities[n].TransformBy(turn);
        //        }
        //    }
        //    #endregion

        //    #region WriteFile
        //    var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
        //    if (!Directory.Exists(path + "/PaintBooth drawing"))
        //        Directory.CreateDirectory(path + "/PaintBooth drawing");

        //    string dwgFilePath = i == 0
        //        ? $"{path}/PaintBooth drawing/D3Panels Right side  {i} {DateTime.Now:hh - mm}.dwg"
        //        : $"{path}/PaintBooth drawing/D3Panels Left side {i} {DateTime.Now:hh - mm}.dwg";

        //    new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();
        //    #endregion

        //    return new PanelDrawResult
        //    {
        //        drawing = drawing,
        //        Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
        //        lstpath = dwgFilePath,
        //    };
        //}

        public PanelDrawResult D3Panels(int i, PaintBoothModel model, double ExtractionChemberHeight,bool waterWash)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            const string Layer = "OuterDoorFrame";
            drawing.Layers.Add(new Layer(Layer, Color.Gray));
            ExtractionChamberHeight = ExtractionChemberHeight;
            double x = model.D;
            PanelLength = (i == 0) ? 0 : model.W;

            #region Panel sheet
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
        new Point3D(0,  PanelLength, 0),
        new Point3D(0,  PanelLength, ExtractionChemberHeight),
        new Point3D(D3, PanelLength, ExtractionChemberHeight),
        new Point3D(D3, PanelLength, 0)
            });
            Brep brep = rectangle.ExtrudeAsBrep(ECSheetThickness);
            Material mat = new Material(ECMaterials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.Green);
            #endregion

            //    #region Panel frame
            //    LinearPath rail = new LinearPath(new Point3D[]
            //    {
            //new Point3D(0,  PanelLength, 0),
            //new Point3D(0,  PanelLength, ExtractionChemberHeight),
            //new Point3D(D3, PanelLength, ExtractionChemberHeight),
            //new Point3D(D3, PanelLength, 0),
            //new Point3D(0,  PanelLength, 0)
            //    });
            //    devregion section = CreatePolygonForEC(i);
            //    Solid frame = section.SweepAsSolid(rail, 0);
            //    frame.Translate(0, PanelLength, ECSheetThickness);
            //    GenerateHolesForEC(frame);

            //    Material mat1 = new Material(ECMaterials);
            //    frame.Regen(0.1);
            //    double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            //    double Frame_Weight = Math.Round(massofFrame, 3);
            //    D3Panels_weight = Rectangle_Weight + Frame_Weight;
            //    drawing.Entities.Add(frame, Color.Yellow);
            //    #endregion
            #region Panel frame
            double Frame_Weight;
            if (!waterWash)
            {
                // ---- dry booth: swept EC frame (unchanged) ----
                LinearPath rail = new LinearPath(new Point3D[]
                {
              new Point3D(0,  PanelLength, 0),
              new Point3D(0,  PanelLength, ExtractionChemberHeight),
              new Point3D(D3, PanelLength, ExtractionChemberHeight),
              new Point3D(D3, PanelLength, 0),
              new Point3D(0,  PanelLength, 0)
                });
                devregion section = CreatePolygonForEC(i);
                Solid frame = section.SweepAsSolid(rail, 0);
                frame.Translate(0, PanelLength, ECSheetThickness);
                GenerateHolesForEC(frame);

                Material mat1 = new Material(ECMaterials);
                frame.Regen(0.1);
                double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
                Frame_Weight = Math.Round(massofFrame, 3);
                drawing.Entities.Add(frame, Color.Yellow);
            }
            else
            {
                // ---- water wash: WC panel tray, 38 flanges toward chamber inside, 10x12 slots at 19 from face ----
                double F = 38, T = ECSheetThickness, slotC = 19, sL = 12, sW = 10;
                double H = ExtractionChemberHeight;
                double dir = (i == 0) ? +1 : -1;                       // inside of chamber
                double y0 = PanelLength;
                Material matF = new Material(ECMaterials);
                double fw = 0;

                LinearPath Loop(params Point3D[] p) => new LinearPath(p.Append(p[0]).ToArray());

                void Flange(LinearPath outer, IEnumerable<LinearPath> holes, Vector3D ext)
                {
                    var curves = new List<ICurve> { outer };
                    curves.AddRange(holes);
                    var b = new devregion(curves.ToArray()).ExtrudeAsBrep(ext);
                    b.Regen(0.1);
                    fw += b.GetMass(matF, linearUnitsType.Millimeters, massUnitsType.Kilograms, out _);
                    drawing.Entities.Add(b, Color.Yellow);
                }

                // side flanges (x = 0 and x = D3): slots @200 starting 67 below top
                var zs = new List<double>();
                for (double z = H - 67; z >= 150; z -= 200) zs.Add(z);
                foreach (double xf in new[] { 0.0, D3 - T })
                {
                    var holes = zs.Select(z => Loop(
                        new Point3D(xf, y0 + dir * (slotC - sW / 2), z - sL / 2),
                        new Point3D(xf, y0 + dir * (slotC + sW / 2), z - sL / 2),
                        new Point3D(xf, y0 + dir * (slotC + sW / 2), z + sL / 2),
                        new Point3D(xf, y0 + dir * (slotC - sW / 2), z + sL / 2)));
                    Flange(Loop(new Point3D(xf, y0, 0), new Point3D(xf, y0 + dir * F, 0),
                                new Point3D(xf, y0 + dir * F, H), new Point3D(xf, y0, H)),
                           holes, new Vector3D(T, 0, 0));
                }

                // top / bottom flanges: slots @200 starting 80 from ends
                var xs = new List<double>();
                for (double xx = 80; xx <= D3 - 80 + 0.5; xx += 200) xs.Add(xx);
                foreach (double zf in new[] { 0.0, H - T })
                {
                    var holes = xs.Select(xc => Loop(
                        new Point3D(xc - sL / 2, y0 + dir * (slotC - sW / 2), zf),
                        new Point3D(xc + sL / 2, y0 + dir * (slotC - sW / 2), zf),
                        new Point3D(xc + sL / 2, y0 + dir * (slotC + sW / 2), zf),
                        new Point3D(xc - sL / 2, y0 + dir * (slotC + sW / 2), zf)));
                    Flange(Loop(new Point3D(0, y0, zf), new Point3D(D3, y0, zf),
                                new Point3D(D3, y0 + dir * F, zf), new Point3D(0, y0 + dir * F, zf)),
                           holes, new Vector3D(0, 0, T));
                }
                Frame_Weight = Math.Round(fw, 3);
            }
            D3Panels_weight = Rectangle_Weight + Frame_Weight;
            #endregion

            #region Service door variables (client: WC panel cutout, WC door frame 1215 005 05 105, door)
            double cutW = model.DoorWidth;              // client 572
            double cutH = model.DoorHeight;             // client 1120
            double cutFromEdge = 237;                   // cutout edge -> panel edge at x = D3          (CHECK side)
            double doorStartHeight = 233;               // cutout bottom from panel bottom (keep same in GA label)

            double cx0 = D3 - cutFromEdge - cutW, cx1 = cx0 + cutW;
            double cz0 = doorStartHeight, cz1 = cz0 + cutH;

            double frD = 37, frTop = 70, frBot = 37, frSide = 33.5;   // frame 639 x 1228 x 37 (for 572 x 1120 cutout)
            double dW = cutW + 53, dH = cutH + 60, dD = 25, dT = 1.6; // door tray 625 x 1180 x 25, 1.6 thk
            double dx0 = cx0 - 26.5, dz0 = cz0 - 30;                   // door centred on cutout
            double dx1 = dx0 + dW, dz1 = dz0 + dH;
            double yd0 = -frD, yd1 = -frD - dD;                        // door sits on frame, outward = -Y
            double ox0 = (i == 1) ? D3 - cx1 : cx0;      // door opening on THIS panel (left panel is turned)
            double ox1 = (i == 1) ? D3 - cx0 : cx1;
            #endregion

            #region Service door (RightSide / LeftSide / Both Side)
            bool doorHere = model.chkServiceDoor &&
                            ((model.ServiceDoorLocation == "RightSide" && i == 0) ||
                             (model.ServiceDoorLocation == "LeftSide" && i == 1) ||
                              model.ServiceDoorLocation == "Both Side");
            if (doorHere)
            {
                // 1) cut the opening in THIS panel (PanelLength is already 0 or W)
                //    var cutout1 = devregion.CreatePolygon(new Point3D[]
                //    {
                //new Point3D(cx0, PanelLength, cz0),
                //new Point3D(cx0, PanelLength, cz1),
                //new Point3D(cx1, PanelLength, cz1),
                //new Point3D(cx1, PanelLength, cz0)
                //    });
                var cutout1 = devregion.CreatePolygon(new Point3D[]
            {
              new Point3D(ox0, PanelLength, cz0),
              new Point3D(ox0, PanelLength, cz1),
              new Point3D(ox1, PanelLength, cz1),
              new Point3D(ox1, PanelLength, cz0)
            });
                brep.ExtrudeRemove(cutout1, ECSheetThickness);

                // 2) everything added from here is a door part → turned later for the left panel
                int firstDoorEntity = drawing.Entities.Count;

                // axis-aligned box, built on the y = 0 side
                void Box(double bx0, double bx1, double by0, double by1, double bz0, double bz1, Color col, string layer = null)
                {
                    var r = devregion.CreatePolygon(new Point3D[]
                    {
                new Point3D(Math.Min(bx0, bx1), Math.Min(by0, by1), bz0),
                new Point3D(Math.Max(bx0, bx1), Math.Min(by0, by1), bz0),
                new Point3D(Math.Max(bx0, bx1), Math.Max(by0, by1), bz0),
                new Point3D(Math.Min(bx0, bx1), Math.Max(by0, by1), bz0)
                    });
                    var b = r.ExtrudeAsBrep(new Vector3D(0, 0, bz1 - bz0));
                    if (layer != null) drawing.Entities.Add(b, layer);
                    else drawing.Entities.Add(b, col);
                }

                #region Door frame (1215 005 05 105) - 37 deep, around the cutout
                Box(cx0 - frSide, cx0, 0, -frD, cz0 - frBot, cz1 + frTop, Color.Gray, Layer);   // left side
                Box(cx1, cx1 + frSide, 0, -frD, cz0 - frBot, cz1 + frTop, Color.Gray, Layer);   // right side
                Box(cx0, cx1, 0, -frD, cz0 - frBot, cz0, Color.Gray, Layer);                     // bottom 37
                Box(cx0, cx1, 0, -frD, cz1, cz1 + frTop, Color.Gray, Layer);                     // top 70
                #endregion

                #region Door tray - face outward, 25 flanges toward frame
                Box(dx0, dx1, yd1, yd1 + dT, dz0, dz1, Color.Aquamarine);         // face sheet
                Box(dx0, dx0 + dT, yd1, yd0, dz0, dz1, Color.Aquamarine);         // left flange
                Box(dx1 - dT, dx1, yd1, yd0, dz0, dz1, Color.Aquamarine);         // right flange
                Box(dx0, dx1, yd1, yd0, dz0, dz0 + dT, Color.Aquamarine);         // bottom flange
                Box(dx0, dx1, yd1, yd0, dz1 - dT, dz1, Color.Aquamarine);         // top flange
                #endregion

                #region Handles - bent strip: 15 lips on door, 42 legs out, 100 grip   (client HANDLE detail)
                double hz = dz0 + 271.5 + 639 / 2.0;            // middle zone of door
                double hW = 20, hH = 100, hOut = 42, hLip = 15, hT = 1.6;
                foreach (double hx in new[] { dx0 + 150, dx0 + 150 + 325 })
                {
                    double xa = hx - hW / 2, xb = hx + hW / 2;
                    double zt = hz + hH / 2, zb = hz - hH / 2;
                    double yo = yd1 - hOut;                     // grip face, 42 out from door

                    Box(xa, xb, yo, yo + hT, zb, zt, Color.Silver);   // grip 100
                    Box(xa, xb, yd1, yo, zt - hT, zt, Color.Silver);   // top leg 42
                    Box(xa, xb, yd1, yo, zb, zb + hT, Color.Silver);   // bottom leg 42
                    Box(xa, xb, yd1, yd1 - hT, zt, zt + hLip, Color.Silver);   // top lip on door
                    Box(xa, xb, yd1, yd1 - hT, zb - hLip, zb, Color.Silver);   // bottom lip on door
                }
                #endregion

                // 3) left panel: turn all door parts 180° about Z onto the y = W wall (keeps solids valid, unlike a mirror)
                if (i == 1)
                {
                    var turn = new Rotation(Math.PI, Vector3D.AxisZ, new Point3D(D3 / 2, model.W / 2, 0));
                    for (int n = firstDoorEntity; n < drawing.Entities.Count; n++)
                        drawing.Entities[n].TransformBy(turn);
                }
            }
            #endregion
            #region Punched angles (water wash) - 38x38x3, tack weld inside
            if (waterWash)
            {
                double angA = 38, angT = 3;
                double dirIn = (i == 0) ? +1 : -1;           // into the chamber, same side as flanges
                double yF = PanelLength;                     // inside face of sheet
                double ax0 = 5, ax1 = D3 - 5;                // 1150 on 1160 → 5 clear each end
                var matA = new Material(ECMaterials);

                void Angle(double xa, double xb, double zTop)
                {
                    if (xb - xa < 20) return;                // too short beside the door - skip
                    var p = new[]
                    {
                  new Point3D(xa, yF,                    zTop - angA),   // leg on sheet (vertical)
                  new Point3D(xa, yF + dirIn * angT,     zTop - angA),
                  new Point3D(xa, yF + dirIn * angT,     zTop - angT),
                  new Point3D(xa, yF + dirIn * angA,     zTop - angT),   // leg into chamber (top)
                  new Point3D(xa, yF + dirIn * angA,     zTop),
                  new Point3D(xa, yF,                    zTop),
                  new Point3D(xa, yF,                    zTop - angA)
              };
                    var b = new devregion(new LinearPath(p)).ExtrudeAsBrep(new Vector3D(xb - xa, 0, 0));
                    b.Regen(0.1);
                    Frame_Weight += Math.Round(b.GetMass(matA, linearUnitsType.Millimeters, massUnitsType.Kilograms, out _), 3);
                    drawing.Entities.Add(b, Color.SteelBlue);
                }

                foreach (double zTop in new[] { 190.0, 701, 906 })      // top edges from panel bottom   (CHECK)
                {
                    bool hitsDoor = doorHere && zTop > cz0 && zTop - angA < cz1;
                    if (hitsDoor)
                    {
                        Angle(ax0, ox0, zTop);                         // left of door opening
                        Angle(ox1, ax1, zTop);                         // right of door opening
                    }
                    else Angle(ax0, ax1, zTop);
                }
                D3Panels_weight = Rectangle_Weight + Frame_Weight;
            }
            #endregion
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = i == 0
                ? $"{path}/PaintBooth drawing/D3Panels Right side  {i} {DateTime.Now:hh - mm}.dwg"
                : $"{path}/PaintBooth drawing/D3Panels Left side {i} {DateTime.Now:hh - mm}.dwg";

            new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult D3PanelsBeforeD(/*int k,*/ int i/*, string Doorlocation*/, PaintBoothModel model, double ExtractionChemberHeight)
        {
            string panelPosition;
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                    new Point3D(0, PanelLength, 0),
                    new Point3D(0, PanelLength, ExtractionChemberHeight),
                    new Point3D(D3, PanelLength, ExtractionChemberHeight),
                    new Point3D(D3, PanelLength, 0)
            });
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            drawing.Entities.Add(brep, Color.Green);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                    new Point3D(0, PanelLength, 0),
                    new Point3D(0, PanelLength, ExtractionChemberHeight),
                    new Point3D(D3, PanelLength, ExtractionChemberHeight),
                    new Point3D(D3, PanelLength, 0),
                    new Point3D(0, PanelLength, 0)
            });
            devregion section = CreatePolygon(i);

            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, PanelLength, SheetThickness);

            // Generate holes on YZ and XY planes
            GenerateHoles(frame);
            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new Material(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            D3Panels_weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);
            //if (i == 0)
            //{
            //    SavePanelDetails(model, "D3Panels Right side", i, D3Panels_weight);
            //}
            //else
            //{
            //    SavePanelDetails(model, "D3Panels Left Side", i, D3Panels_weight);
            //}

            ////Added serviceDoor code here 
            //double doorWidth = model.DoorWidth; // Width of the service door
            //double doorHeight = model.DoorHeight; // Height of the service door
            //double doorPositionX = (D3 - doorWidth) / 2;
            //double doorStartHeight = 100; // Starting height above the base

            //if (model.ServiceDoorLocation == "RightSide" && k == 1 && i == 0)
            //{
            //    // if (k == 1 && i==0 && i != 1) // RightAssembly
            //    {
            //        //GenerateDoor(model);
            //        Point3D[] cutout = new Point3D[]
            //        {
            //        new Point3D(doorPositionX, PanelLength, doorStartHeight),
            //        new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
            //        new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
            //        new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
            //        };
            //        var cutout1 = devregion.CreatePolygon(cutout);

            //        brep.ExtrudeRemove(cutout1, SheetThickness);
            //        drawing.Entities.Add(brep);

            //    }

            //}
            //else if (model.ServiceDoorLocation == "LeftSide" && k == 1 && i == 1)
            //{
            //    // if (k == 1 && i == 1 && i != 0) // LeftAssembly
            //    {
            //        Point3D[] cutout = new Point3D[]
            //    {
            //        new Point3D(doorPositionX, PanelLength, doorStartHeight),
            //        new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
            //        new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
            //        new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
            //    };
            //        var cutout1 = devregion.CreatePolygon(cutout);
            //        brep.ExtrudeRemove(cutout1, SheetThickness);
            //        drawing.Entities.Add(brep);
            //    }
            //}
            //else if (model.ServiceDoorLocation == "Both Side" && k == 1)
            //{
            //    // if(k==1) // 0 for Right, 1 for Left
            //    {
            //        Point3D[] cutout = new Point3D[]
            //        {
            //            new Point3D(doorPositionX, PanelLength, doorStartHeight),
            //            new Point3D(doorPositionX, PanelLength, doorStartHeight + doorHeight),
            //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight + doorHeight),
            //            new Point3D(doorPositionX + doorWidth, PanelLength, doorStartHeight)
            //        };
            //        var cutoutPolygon = devregion.CreatePolygon(cutout);
            //        brep.ExtrudeRemove(cutoutPolygon, SheetThickness);
            //        drawing.Entities.Add(brep);
            //    }
            //}
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath;
            if (i == 0)
            {
                dwgFilePath = $"{path}/PaintBooth drawing/D3PanelsBeforeD Right side  {i} {DateTime.Now:hh - mm}.dwg";
            }
            else
            {
                dwgFilePath = $"{path}/PaintBooth drawing/D3PanelsBeforeD Left side {i} {DateTime.Now:hh - mm}.dwg";
            }


            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion           
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        #endregion

        #region 3D top panels Drawings
        public PanelDrawResult TopSidePanels(int j, PaintBoothModel model, int m, string painBoothType)
        {
            #region old code

            string panelPosition;
            string dwgFilePath = "";
            int k = 0;
            //double PaintboothHeight = model.H;
            double ExtraHeightforTopPanels = 70;//Extra Height for Top Panels

            //double PaintboothHeight = PaintboothHeight + ExtraHeightforTopPanels;
            double PaintboothHeight = painBoothType == "8" ? model.H + ExtraHeightforTopPanels + plenumHeight : model.H + ExtraHeightforTopPanels;
            double StandardBend1 = SettingStandardBend1;
            double StandardBend2 = SettingStandardBend2;

            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            W = PanelLengthForTopPanels;

            double ComponentInDSize = model.D1 + (model.Depth / 2);
            double TubelightLocation = Math.Ceiling(ComponentInDSize / PanelWidth);
            double ComponentInWSize = model.W1 + (model.Width / 2);
            double TubelightLocationinW = Math.Ceiling(ComponentInWSize / PanelLength);
           
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,PaintboothHeight),
                new Point3D(PanelWidth,0,PaintboothHeight),
                new Point3D(PanelWidth,W,PaintboothHeight),
                new Point3D(0,W,PaintboothHeight),
            });
            drawing.Entities.Add(rectangle, Color.AntiqueWhite);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,PaintboothHeight),
                new Point3D(PanelWidth,0,PaintboothHeight),
                new Point3D(PanelWidth,W,PaintboothHeight),
                new Point3D(0,W,PaintboothHeight),
                new Point3D(0,0,PaintboothHeight),
            });

            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, PaintboothHeight),
                new Point3D(0,SheetThickness,PaintboothHeight),
                new Point3D(0,SheetThickness,(PaintboothHeight +(StandardBend2 - SheetThickness))),
                new Point3D(0,StandardBend1,(PaintboothHeight +(StandardBend2 - SheetThickness))),
                new Point3D(0,StandardBend1, (PaintboothHeight+StandardBend2)),
                new Point3D(0,0, (PaintboothHeight+StandardBend2))
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);
            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            TopPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.AntiqueWhite);

            
            #endregion


            #endregion

            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            dwgFilePath = $"{path}/PaintBooth drawing/TopPanel {j} {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            string pdfFilePath = $"{path}/PaintBooth drawing/TopPanel {j} {DateTime.Now:hh - mm}.pdf";

            Write3DPdfParams pdfauto = new Write3DPdfParams(drawing);
            Write3DPDF pdf12 = new Write3DPDF(pdfauto, pdfFilePath);
            pdf12.DoWork();
            #endregion

            // TubelightCutout(model);
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult TopSidePanelsForFAS(int j, PaintBoothModel model, int m, int PlenumHeight)
        {
            #region old code

            string panelPosition;
            string dwgFilePath = "";
            int k = 0;
            double PaintboothHeight = model.H + PlenumHeight;
            double StandardBend1 = SettingStandardBend1;
            double StandardBend2 = SettingStandardBend2;

            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            W = PanelLengthForTopPanels;
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,PaintboothHeight),
                new Point3D(PanelWidth,0,PaintboothHeight),
                new Point3D(PanelWidth,W,PaintboothHeight),
                new Point3D(0,W,PaintboothHeight),
            });
            drawing.Entities.Add(rectangle, Color.AntiqueWhite);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,PaintboothHeight),
                new Point3D(PanelWidth,0,PaintboothHeight),
                new Point3D(PanelWidth,W,PaintboothHeight),
                new Point3D(0,W,PaintboothHeight),
                new Point3D(0,0,PaintboothHeight),
            });

            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, PaintboothHeight),
                new Point3D(0,SheetThickness,PaintboothHeight),
                new Point3D(0,SheetThickness,(PaintboothHeight +(StandardBend2 - SheetThickness))),
                new Point3D(0,StandardBend1,(PaintboothHeight +(StandardBend2 - SheetThickness))),
                new Point3D(0,StandardBend1, (PaintboothHeight+StandardBend2)),
                new Point3D(0,0, (PaintboothHeight+StandardBend2))
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);
            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            TopPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.AntiqueWhite);

            #region TubeLight
            int TubeLightQuantity = 1;
            //double PaintBoothArea = (model.W / 1000) * (model.D / 1000);
            decimal PaintBoothArea = ((decimal)(model.W * model.D)) / 1000000m;//Area is in SquareMeters
            TubeLightCalculations = Math.Ceiling(((decimal)PaintBoothArea * LuxLevel) / (Lumens * TubeLightQuantity * 0.7m));//0.7m is scaleing factor

            model.Lights = (int)TubeLightCalculations;

            #region fetch Tublight Table and save Quantity of TB in database
            // Step 2: Retrieve the saved record based on a unique identifier
            var savedRecord = _DbContext.TubeLightDetails
                                        .FirstOrDefault(x => x.EnquiryID == model.EnquiryId || x.SalesNo == model.SalesNO);

            // Step 3: Update the Quantity value
            if (savedRecord != null)
            {
                savedRecord.Quantity = (int)TubeLightCalculations;

                // Step 4: Save the changes for the updated Quantity
                _DbContext.SaveChanges();  // Only the Quantity column will be updated
            }
            #endregion
            #endregion
            #endregion

            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            dwgFilePath = $"{path}/PaintBooth drawing/TopPanelForFAS {j} {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            string pdfFilePath = $"{path}/PaintBooth drawing/TopPanelForFAS {j} {DateTime.Now:hh - mm}.pdf";

            Write3DPdfParams pdfauto = new Write3DPdfParams(drawing);
            Write3DPDF pdf12 = new Write3DPDF(pdfauto, pdfFilePath);
            pdf12.DoWork();
            #endregion


            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult TopFilterFramesDrawingWithindualframe(PaintBoothModel model)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            string dwgFilePath = "";
            double boothHeight = model.H;

            double angleframe = 50.0;
            double totalW = model.W - (angleframe * 2);
            double totalD = model.D - (angleframe * 2);

            // Find how many filters fit near 710mm
            int filtersAlongW = (int)Math.Round(totalW / 710.0);
            int filtersAlongD = (int)Math.Round(totalD / 710.0);

            // Ensure at least 1 filter
            if (filtersAlongW < 1) filtersAlongW = 1;
            if (filtersAlongD < 1) filtersAlongD = 1;

            // Actual filter size — divide space equally
            double actualFilterW = totalW / filtersAlongW;
            double actualFilterD = totalD / filtersAlongD;

            // ── Filter panel rectangles (LightGray fill) ──────────────
            for (int i = 0; i < filtersAlongW; i++)
            {
                for (int k = 0; k < filtersAlongD; k++)
                {
                    double offsetX = angleframe + (k * actualFilterD);
                    double offsetY = angleframe + (i * actualFilterW);

                    var rectangle = devregion.CreatePolygon(new Point3D[]
                    {
                        new Point3D(offsetX,                 offsetY,                 boothHeight),
                        new Point3D(offsetX + actualFilterD, offsetY,                 boothHeight),
                        new Point3D(offsetX + actualFilterD, offsetY + actualFilterW, boothHeight),
                        new Point3D(offsetX,                 offsetY + actualFilterW, boothHeight),
                    });

                    drawing.Entities.Add(rectangle, Color.LightGray);
                    Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
                    drawing.Entities.Add(brep, Color.LightGray);
                }
            }

            // ── Horizontal members (along X/D axis) ──────────────────
            // i goes 0 to filtersAlongW (inclusive) to draw all borders
            HashSet<double> drawnY = new HashSet<double>();

            for (int i = 0; i <= filtersAlongW; i++)
            {
                double yPos = angleframe + (i * actualFilterW);
                if (drawnY.Contains(yPos)) continue;
                drawnY.Add(yPos);

                LinearPath rail = new LinearPath(new Point3D[]
                {
                    new Point3D(angleframe,           yPos, boothHeight),
                    new Point3D(model.D - angleframe, yPos, boothHeight),
                });

                var sec = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(angleframe, yPos,           boothHeight),
                    new Point3D(angleframe, yPos,           boothHeight + SettingH),
                    new Point3D(angleframe, yPos - SettingW, boothHeight + SettingH),
                    new Point3D(angleframe, yPos - SettingW, boothHeight + (SettingH - SettingT)),
                    new Point3D(angleframe, yPos - SettingT, boothHeight + (SettingH - SettingT)),
                    new Point3D(angleframe, yPos - SettingT, boothHeight + SettingT),
                    new Point3D(angleframe, yPos - SettingW, boothHeight + SettingT),
                    new Point3D(angleframe, yPos - SettingW, boothHeight),
                });

                Solid member = sec.SweepAsSolid(rail, 0);
                member.Translate(0, 0, SheetThickness);
                drawing.Entities.Add(member, Color.BurlyWood);
            }

            // ── Vertical members (along Y/W axis) ────────────────────
            // k goes 0 to filtersAlongD (inclusive) to draw all borders
            HashSet<double> drawnX = new HashSet<double>();

            for (int k = 0; k <= filtersAlongD; k++)
            {
                double xPos = angleframe + (k * actualFilterD);
                if (drawnX.Contains(xPos)) continue;
                drawnX.Add(xPos);

                LinearPath rail = new LinearPath(new Point3D[]
                {
            new Point3D(xPos, angleframe,           boothHeight),
            new Point3D(xPos, model.W - angleframe, boothHeight),
                });

                var sec = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(xPos,            angleframe, boothHeight),
                    new Point3D(xPos,            angleframe, boothHeight + SettingH),
                    new Point3D(xPos - SettingW, angleframe, boothHeight + SettingH),
                    new Point3D(xPos - SettingW, angleframe, boothHeight + (SettingH - SettingT)),
                    new Point3D(xPos - SettingT, angleframe, boothHeight + (SettingH - SettingT)),
                    new Point3D(xPos - SettingT, angleframe, boothHeight + SettingT),
                    new Point3D(xPos - SettingW, angleframe, boothHeight + SettingT),
                    new Point3D(xPos - SettingW, angleframe, boothHeight),
                });

                Solid member = sec.SweepAsSolid(rail, 0);
                member.Translate(0, 0, SheetThickness);
                drawing.Entities.Add(member, Color.BurlyWood);
            }

            #region Weight calculation
            Material mat = new Material(Materials);
            double totalWeight = 0;
            foreach (var entity in drawing.Entities)
            {
                if (entity is Brep b)
                {
                    b.Regen(0.1);
                    double mass = b.GetMass(mat, linearUnitsType.Millimeters,
                                            massUnitsType.Kilograms, out _);
                    totalWeight += mass;
                }
                else if (entity is Solid s)
                {
                    s.Regen(0.1);
                    double mass = s.GetMass(mat, linearUnitsType.Millimeters,
                                            massUnitsType.Kilograms, out _);
                    totalWeight += mass;
                }
            }
            #endregion

            //drawing.Regen(0.1);

            #region Write file
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            dwgFilePath = $"{path}/PaintBooth drawing/FilterFrames_Top_{DateTime.Now:hh-mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();

            string pdfFilePath = $"{path}/PaintBooth drawing/FilterFrames_Top_{DateTime.Now:hh-mm}.pdf";
            new Write3DPDF(new Write3DPdfParams(drawing), pdfFilePath).DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(totalWeight, 3),
                lstpath = dwgFilePath,
            };
        }


        public PanelDrawResult TopFilterFramesDrawing(PaintBoothModel model)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            string dwgFilePath = "";
            double boothHeight = model.H;

            double angleframe = 50.0;
            double totalW = model.W - (angleframe * 2);
            double totalD = model.D - (angleframe * 2);

            // Find how many filters fit near 710mm
            int filtersAlongW = (int)Math.Round(totalW / 710.0);
            int filtersAlongD = (int)Math.Round(totalD / 710.0);

            if (filtersAlongW < 1) filtersAlongW = 1;
            if (filtersAlongD < 1) filtersAlongD = 1;

            // ✅ Force even count along D for symmetric split
            if (filtersAlongD % 2 != 0) filtersAlongD += 1;

            // Actual filter size — divide space equally
            double actualFilterW = totalW / filtersAlongW;
            double actualFilterD = totalD / filtersAlongD;

            // ── Flat bar 40x3mm between filter cells ──────────────────
            double flatBar = 40.0;

            // ── Flat bars along D direction (between filter columns) ──
            for (int k = 1; k < filtersAlongD; k++)
            {
                double xPos = angleframe + (k * actualFilterD);

                var flatBarRectD = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(xPos,           angleframe,                                   boothHeight),
                    new Point3D(xPos + flatBar, angleframe,                                   boothHeight),
                    new Point3D(xPos + flatBar, angleframe + (filtersAlongW * actualFilterW), boothHeight),
                    new Point3D(xPos,           angleframe + (filtersAlongW * actualFilterW), boothHeight),
                });
                drawing.Entities.Add(flatBarRectD, Color.BurlyWood);
                Brep flatBarBrepD = flatBarRectD.ExtrudeAsBrep(3);
                drawing.Entities.Add(flatBarBrepD, Color.BurlyWood);
            }

            // ── Flat bars along W direction (between filter rows) ─────
            for (int i = 1; i < filtersAlongW; i++)
            {
                double yPos = angleframe + (i * actualFilterW);

                var flatBarRectW = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(angleframe,                                   yPos,           boothHeight),
                    new Point3D(angleframe + (filtersAlongD * actualFilterD), yPos,           boothHeight),
                    new Point3D(angleframe + (filtersAlongD * actualFilterD), yPos + flatBar, boothHeight),
                    new Point3D(angleframe,                                   yPos + flatBar, boothHeight),
                });
                drawing.Entities.Add(flatBarRectW, Color.BurlyWood);
                Brep flatBarBrepW = flatBarRectW.ExtrudeAsBrep(3);
                drawing.Entities.Add(flatBarBrepW, Color.BurlyWood);
            }

            // ── 2 big frame boundaries — exact center split ────────────
            double centerD = model.D / 2.0;  // ✅ exact center of booth

            double frame1_x1 = 0;
            double frame1_x2 = centerD;      // ✅ exact center
            double frame2_x1 = centerD;      // ✅ exact center
            double frame2_x2 = model.D;
            double frameY1 = 0;
            double frameY2 = model.W;

            // ── Draw outer border for each big frame (4 sides each) ───
            var frames = new[]
            {
                (x1: frame1_x1, x2: frame1_x2, y1: frameY1, y2: frameY2),
                (x1: frame2_x1, x2: frame2_x2, y1: frameY1, y2: frameY2),
            };

            foreach (var f in frames)
            {
                // Bottom — sweeps inward (toward +Y)
                DrawSweepMember(
                    new Point3D(f.x1, f.y1, boothHeight),
                    new Point3D(f.x2, f.y1, boothHeight),
                    isAlongX: true, inward: true, boothHeight);

                // Top — sweeps inward (toward -Y)
                DrawSweepMember(
                    new Point3D(f.x1, f.y2, boothHeight),
                    new Point3D(f.x2, f.y2, boothHeight),
                    isAlongX: true, inward: false, boothHeight);

                // Left — sweeps inward (toward +X)
                DrawSweepMember(
                    new Point3D(f.x1, f.y1, boothHeight),
                    new Point3D(f.x1, f.y2, boothHeight),
                    isAlongX: false, inward: true, boothHeight);

                // Right — sweeps inward (toward -X)
                DrawSweepMember(
                    new Point3D(f.x2, f.y1, boothHeight),
                    new Point3D(f.x2, f.y2, boothHeight),
                    isAlongX: false, inward: false, boothHeight);
            }

            #region Weight calculation
            Material mat = new Material(Materials);
            double totalWeight = 0;
            foreach (var entity in drawing.Entities)
            {
                if (entity is Brep b)
                {
                    b.Regen(0.1);
                    totalWeight += b.GetMass(mat, linearUnitsType.Millimeters,
                                             massUnitsType.Kilograms, out _);
                }
                else if (entity is Solid s)
                {
                    s.Regen(0.1);
                    totalWeight += s.GetMass(mat, linearUnitsType.Millimeters,
                                             massUnitsType.Kilograms, out _);
                }
            }
            #endregion

            // drawing.Regen(0.1);

            #region Write file
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            dwgFilePath = $"{path}/PaintBooth drawing/FilterFrames_Top_{DateTime.Now:hh-mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();

            string pdfFilePath = $"{path}/PaintBooth drawing/FilterFrames_Top_{DateTime.Now:hh-mm}.pdf";
            new Write3DPDF(new Write3DPdfParams(drawing), pdfFilePath).DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(totalWeight, 3),
                lstpath = dwgFilePath,
            };
        }
        private void DrawSweepMember(Point3D start, Point3D end,
                              bool isAlongX, bool inward, double boothHeight)
        {
            LinearPath rail = new LinearPath(new Point3D[] { start, end });
            double sign = inward ? 1.0 : -1.0;  // +1 = inward, -1 = outward

            devDept.Eyeshot.Entities.Region sec;

            if (isAlongX)
            {
                sec = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(start.X, start.Y,                     boothHeight),
                    new Point3D(start.X, start.Y,                     boothHeight + SettingH),
                    new Point3D(start.X, start.Y + (sign * SettingW),  boothHeight + SettingH),
                    new Point3D(start.X, start.Y + (sign * SettingW),  boothHeight + (SettingH - SettingT)),
                    new Point3D(start.X, start.Y + (sign * SettingT),  boothHeight + (SettingH - SettingT)),
                    new Point3D(start.X, start.Y + (sign * SettingT),  boothHeight + SettingT),
                    new Point3D(start.X, start.Y + (sign * SettingW),  boothHeight + SettingT),
                    new Point3D(start.X, start.Y + (sign * SettingW),  boothHeight),
                });
            }
            else
            {
                sec = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(start.X,                     start.Y, boothHeight),
                    new Point3D(start.X,                     start.Y, boothHeight + SettingH),
                    new Point3D(start.X + (sign * SettingW),  start.Y, boothHeight + SettingH),
                    new Point3D(start.X + (sign * SettingW),  start.Y, boothHeight + (SettingH - SettingT)),
                    new Point3D(start.X + (sign * SettingT),  start.Y, boothHeight + (SettingH - SettingT)),
                    new Point3D(start.X + (sign * SettingT),  start.Y, boothHeight + SettingT),
                    new Point3D(start.X + (sign * SettingW),  start.Y, boothHeight + SettingT),
                    new Point3D(start.X + (sign * SettingW),  start.Y, boothHeight),
                });
            }

            Solid member = sec.SweepAsSolid(rail, 0);
            member.Translate(0, 0, SheetThickness);
            drawing.Entities.Add(member, Color.BurlyWood);
        }

        /// <summary>
        /// Tube light qty for ANY booth type: area × lux / (lumens × qty per fixture × factor).
        /// Sets model.Lights + the static TubeLightCalculations and saves the qty to TubeLightDetails.
        /// </summary>
        public int CalculateTubeLights(PaintBoothModel model, decimal factor = 0.7m)
        {
            int TubeLightQuantity = 1;                                            // tubes per fixture
            decimal PaintBoothArea = (decimal)(model.W * model.D) / 1000000m;    // m²

            if (Lumens <= 0) return 0;                                            // avoid divide-by-zero
            TubeLightCalculations = Math.Ceiling((PaintBoothArea * LuxLevel) / (Lumens * TubeLightQuantity * factor));
            model.Lights = (int)TubeLightCalculations;

            var savedRecord = _DbContext.TubeLightDetails
                                        .FirstOrDefault(x => x.EnquiryID == EnquiryID || x.SalesNo == model.SalesNO);
            if (savedRecord != null)
            {
                savedRecord.Quantity = model.Lights;
                _DbContext.SaveChanges();
            }
            return model.Lights;
        }
        #region 3D Back panels 
        public PanelDrawResult BackPanelsAfterExtractionChamber(PaintBoothModel model, double EXtractionChemberHeight, string PaintBoothTypefromEnquiry, bool waterWash)
        {

            model.D = model.D;
            ExtractionChamberHeight = EXtractionChemberHeight;
            int k = 0;
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            double x = 0;
            // Types 4, 5, 6, 7 have D3 on BOTH front and back so back wall is at D + D3 + D3
            if (PaintBoothTypefromEnquiry == "4" || PaintBoothTypefromEnquiry == "5" ||
                PaintBoothTypefromEnquiry == "6" || PaintBoothTypefromEnquiry == "7")
                x = model.D + D3 + D3;
            else
                x = model.D + D3;

            if (waterWash)
            {
                // ===== WC REAR PANELS (1215 005 05 101): 1160 trays along width, last = remainder =====
                double H = EXtractionChemberHeight;               // 2134
                double T = SheetThickness, F = 38, slotC = 19, sL = 12, sW = 10;
                double stdW = 1160, total = BackPanelLength;
                double dOut = +1;                                 // flanges outward (+X), clear of parts inside   (CHECK)
                var matW = new Material(Materials);
                double wTotal = 0;

                // panel-local (u along Y, v up, d out of wall) → world
                Point3D P(double y0, double u, double v, double d) => new Point3D(x + dOut * d, y0 + u, v);
                LinearPath Loop(params Point3D[] p) => new LinearPath(p.Append(p[0]).ToArray());

                void Add(Brep b, Color c)
                {
                    b.Regen(0.1);
                    wTotal += b.GetMass(matW, linearUnitsType.Millimeters, massUnitsType.Kilograms, out _);
                    drawing.Entities.Add(b, c);
                }

                void WCRearPanel(double y0, double w)
                {
                    // ---- face sheet with 6 x Ø10 holes (210 from sides; 643 / 1151 / 1348 from top) ----
                    var curves = new List<ICurve> { Loop(P(y0, 0, 0, 0), P(y0, w, 0, 0), P(y0, w, H, 0), P(y0, 0, H, 0)) };
                    foreach (double fromTop in new[] { 643.0, 643 + 508, 643 + 508 + 197 })
                        foreach (double u in new[] { 210.0, w - 210 })
                            if (u > 60 && u < w - 60)
                                curves.Add(new Circle(new Plane(P(y0, u, H - fromTop, 0), Vector3D.AxisX), 5));
                    Add(new devregion(curves.ToArray()).ExtrudeAsBrep(new Vector3D(dOut * T, 0, 0)), Color.Green);

                    // ---- side flanges: slots 67 from top, @200 ----
                    var vs = new List<double>();
                    for (double v = H - 67; v >= 60; v -= 200) vs.Add(v);
                    foreach (double uf in new[] { 0.0, w - T })
                    {
                        var c = new List<ICurve> { Loop(P(y0, uf, 0, 0), P(y0, uf, 0, F), P(y0, uf, H, F), P(y0, uf, H, 0)) };
                        foreach (double v in vs)
                            c.Add(Loop(P(y0, uf, v - sL / 2, slotC - sW / 2), P(y0, uf, v - sL / 2, slotC + sW / 2),
                                       P(y0, uf, v + sL / 2, slotC + sW / 2), P(y0, uf, v + sL / 2, slotC - sW / 2)));
                        Add(new devregion(c.ToArray()).ExtrudeAsBrep(new Vector3D(0, T, 0)), Color.Yellow);
                    }

                    // ---- top / bottom flanges: slots 80 from ends, @200 ----
                    var us = new List<double>();
                    for (double u = 80; u <= w - 80 + 0.5; u += 200) us.Add(u);
                    foreach (double vf in new[] { 0.0, H - T })
                    {
                        var c = new List<ICurve> { Loop(P(y0, 0, vf, 0), P(y0, w, vf, 0), P(y0, w, vf, F), P(y0, 0, vf, F)) };
                        foreach (double u in us)
                            c.Add(Loop(P(y0, u - sL / 2, vf, slotC - sW / 2), P(y0, u + sL / 2, vf, slotC - sW / 2),
                                       P(y0, u + sL / 2, vf, slotC + sW / 2), P(y0, u - sL / 2, vf, slotC + sW / 2)));
                        Add(new devregion(c.ToArray()).ExtrudeAsBrep(new Vector3D(0, 0, T)), Color.Yellow);
                    }
                }

                // ---- lay out along the width ----
                for (double y = 0; y < total - 1; y += stdW)
                    WCRearPanel(y, Math.Min(stdW, total - y));

                BackPanel_Weight = Math.Round(wTotal, 3);

                var pathW = _configuration["FolderPathConfig:DrawingPath"].ToString();
                if (!Directory.Exists(pathW + "/PaintBooth drawing"))
                    Directory.CreateDirectory(pathW + "/PaintBooth drawing");
                string dwgW = $"{pathW}/PaintBooth drawing/WC RearPanels {DateTime.Now:hh - mm}.dwg";
                new WriteAutodesk(new WriteAutodeskParams(drawing), dwgW).DoWork();

                return new PanelDrawResult { drawing = drawing, Weight = BackPanel_Weight, lstpath = dwgW };
            }


            var rectangle = devregion.CreatePolygon(new Point3D[]
                {
                new Point3D(x,0,0),
                new Point3D(x,BackPanelLength,0),
                new Point3D(x,BackPanelLength,EXtractionChemberHeight),//Changed PanelHeight to EXtractionChemberHeight 
                new Point3D(x,0,EXtractionChemberHeight)
                });
            drawing.Entities.Add(rectangle, Color.Green);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                //new Point3D(x,0,0),
                //new Point3D(x,BackPanelLength,0),
                //new Point3D(x,BackPanelLength,EXtractionChemberHeight),
                //new Point3D(x,0,EXtractionChemberHeight),
                //new Point3D(x,0,0)
                new Point3D(0,0,0),
                new Point3D(0,BackPanelLength,0),
                new Point3D(0,BackPanelLength,EXtractionChemberHeight),
                new Point3D(0,0,EXtractionChemberHeight),
                new Point3D(0,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                //new Point3D(x, 0, 0),
                //new Point3D(((x)+36.8),0,0),
                //new Point3D(((x)+36.8),0,1.2),
                //new Point3D(((x)+SheetThickness),0,SheetThickness),
                //new Point3D(((x)+SheetThickness),0,36.8),
                //new Point3D(x,0,36.8)
                new Point3D(0, 0, 0),
                new Point3D((36.8),0,0),
                new Point3D((36.8),0,1.2),
                new Point3D(SheetThickness,0,SheetThickness),
                new Point3D((SheetThickness),0,36.8),
                new Point3D(0,0,36.8)
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(x, 0, SheetThickness);

            frame = GenerateHoles(frame);

            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/RearPanel {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(BackPanel_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult BackPanelsBeforeEC(PaintBoothModel model, double backPanelHeightBeforeEC, string PaintBoothTypefromEnquiry)

        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            //double z = BackPanelHeightBeforeEC = backPanelHeightBeforeEC;
            double z = BackPanelHeightBeforeEC = PHForAboveECPanels;
            // Types 4, 5, 6, 7 have D3 on BOTH front and back so back wall is at D + D3 + D3
            double x = (PaintBoothTypefromEnquiry == "4" || PaintBoothTypefromEnquiry == "5" ||
                        PaintBoothTypefromEnquiry == "6" || PaintBoothTypefromEnquiry == "7")
                       ? model.D + D3 + D3
                       : model.D + D3;

            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(x,0,0),
                new Point3D(x,BackPanelLength,0),
                new Point3D(x,BackPanelLength,z),
                new Point3D(x,0,z)
            });
            drawing.Entities.Add(rectangle, Color.Green);

            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                //new Point3D(x,0,0),
                //new Point3D(x,BackPanelLength,0),
                //new Point3D(x,BackPanelLength,z),
                //new Point3D(x,0,z),
                //new Point3D(x,0,0)
                new Point3D(0,0,0),
                new Point3D(0,BackPanelLength,0),
                new Point3D(0,BackPanelLength,z),
                new Point3D(0,0,z),
                new Point3D(0,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                //new Point3D(x, 0, 0),
                //new Point3D(((x)+36.8),0,0),
                //new Point3D(((x)+36.8),0,SheetThickness),
                //new Point3D(((x)+SheetThickness),0,SheetThickness),
                //new Point3D(((x)+SheetThickness),0,36.8),
                //new Point3D(x,0,36.8)
                new Point3D(0, 0, 0),
                new Point3D((ECStandardBend2-SheetThickness),0,0),
                new Point3D((ECStandardBend2-SheetThickness),0,SheetThickness),
                new Point3D(SheetThickness,0,SheetThickness),
                new Point3D(SheetThickness,0,(ECStandardBend2-SheetThickness)),
                new Point3D(0,0,(ECStandardBend2-SheetThickness))
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(x, 0, SheetThickness);
            frame = GenerateHoles(frame);
            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);

            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);


            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/RearPanelBeforeExtractionC {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(BackPanel_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult BackPanelsForFAS(PaintBoothModel model, int PlenumHeight, EnquiryMaster FASDoor, int j)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;

            //double x = model.D;
            double x = model.D;

            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(x,0,0),
                new Point3D(x,BackPanelLength,0),
                new Point3D(x,BackPanelLength,PlenumHeight),
                new Point3D(x,0,PlenumHeight)
            });
            drawing.Entities.Add(rectangle, Color.AntiqueWhite);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            mat = new(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(x,0,0),
                new Point3D(x,BackPanelLength,0),
                new Point3D(x,BackPanelLength,PlenumHeight),
                new Point3D(x,0,PlenumHeight),
                new Point3D(x,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                //new Point3D(x, 0, 0),
                //new Point3D(((x)+36.8),0,0),
                //new Point3D(((x)+36.8),0,SheetThickness),
                //new Point3D(((x)+SheetThickness),0,SheetThickness),
                //new Point3D(((x)+SheetThickness),0,36.8),
                //new Point3D(x,0,36.8)
                 new Point3D(0, 0, 0),
                new Point3D(((0)+36.8),0,0),
                new Point3D(((0)+36.8),0,SheetThickness),
                new Point3D(((0)+SheetThickness),0,SheetThickness),
                new Point3D(((0)+SheetThickness),0,36.8),
                new Point3D(0,0,36.8)
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(model.D, 0, SheetThickness);

            frame = GenerateHoles(frame);

            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.AntiqueWhite);

            double doorPositionX = (BackPanelLength - (double)FASDoor.MaintainanceDoorWidth) / 2;
            double doorWidthFAS = (double)FASDoor.MaintainanceDoorWidth;
            double doorHeightFAS = (PlenumHeight - (double)FASDoor.MaintainanceDoorHeight) / 2;

            if (j == 0)
            {
                if (FASDoor.SideLocationDoorFAS == "Rear")
                {
                    Point3D[] cutout = new Point3D[]
                    {
                        new Point3D(x, doorPositionX,                doorHeightFAS),
                        new Point3D(x, doorPositionX,                 doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(x, doorPositionX + doorWidthFAS,  doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(x, doorPositionX + doorWidthFAS,  doorHeightFAS)
                    };
                    var cutout1 = devregion.CreatePolygon(cutout);
                    brep.ExtrudeRemove(cutout1, -SheetThickness);
                    drawing.Entities.Add(brep);
                }


            }

            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/RearPanel {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PanelDrawResult FrontPanels(PaintBoothModel model)
        {
            //model.D = model.D;
            int k = 0;
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,FrontPanelLength,0),
                new Point3D(0,FrontPanelLength,FrontPanelHeight),
                new Point3D(0,0,FrontPanelHeight)
            });
            drawing.Entities.Add(rectangle, Color.Green);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,FrontPanelLength,0),
                new Point3D(0,FrontPanelLength,FrontPanelHeight),
                new Point3D(0,0,FrontPanelHeight),
                new Point3D(0,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(-36.8,0,0),
                new Point3D(-36.8,0,1.2),
                new Point3D(-SheetThickness,0,SheetThickness),
                new Point3D(-SheetThickness,0,36.8),
                new Point3D(0,0,36.8)
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);

            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            //BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);


            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/FrontPanel {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PaintBoothclass FrontPanelsWithDoorsOld(double y, PaintBoothModel model)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            double z = (double)model.HingedDoorHeight;
            //double z = model.Width + 700 + 250;//Component Width+700+250=total panel Height 
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,y,0),
                new Point3D(0,y,z),
                new Point3D(0,0,z)
            });
            drawing.Entities.Add(rectangle, Color.BurlyWood);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,y,0),
                new Point3D(0,y,z),
                new Point3D(0,0,z),
                new Point3D(0,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D((0+36.8),0,0),
                new Point3D((0+36.8),0,1.2),
                new Point3D((0+SheetThickness),0,SheetThickness),
                new Point3D((0+SheetThickness),0,36.8),
                new Point3D(0,0,36.8)
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);

            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/FrontPanelWithDoors {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion

            //var existingRecord = _DbContext.PanelDetails
            //        .FirstOrDefault(p =>
            //            p.EnquiryId == EnquiryID &&
            //            p.PanelPosition == "BackPanels" &&
            //            p.StandardPanelDepth == PanelWidth &&
            //            p.StandardPanelHeight == PanelHeight);

            //// If the record does not exist, save it
            //if (existingRecord == null || existingRecord.NoOfPanels == 0)
            //{
            //    SavePanelDetails(model, pmodel, "BackPanels", k, BackPanel_Weight);
            //}
            //else
            //{
            //    // Increment NoOfPanels
            //    existingRecord.NoOfPanels += 1;
            //    _DbContext.SaveChanges(); // Only the NoOfPanels column will be updated
            //}
            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath,

            };
        }
        public PanelDrawResult FrontPanelsWithDoors(double y, PaintBoothModel model)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;

            double z = (double)model.HingedDoorHeight;
            //double z = model.Width + 700 + 250;//Component Width+700+250=total panel Height 
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,PanelWidthForFrontDoors,0),
                new Point3D(0,PanelWidthForFrontDoors,PanelHeightForFrontDoors),
                new Point3D(0,0,PanelHeightForFrontDoors)
            });
            drawing.Entities.Add(rectangle, Color.Green);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,PanelWidthForFrontDoors,0),
                new Point3D(0,PanelWidthForFrontDoors,PanelHeightForFrontDoors),
                new Point3D(0,0,PanelHeightForFrontDoors),
                new Point3D(0,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(-36.8,0,0),
                new Point3D(-36.8,0,SheetThickness),
                new Point3D(-SheetThickness,0,SheetThickness),
                new Point3D(-SheetThickness,0,36.8),
                new Point3D(0,0,36.8)
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);

            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            // BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/FrontPanelWithDoors {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };

        }
        public PanelDrawResult FrontPanelsForFAS(PaintBoothModel model, int PlenumHeight, EnquiryMaster FASDoor, int j)
        {
            //double BackPanel_Weight = 0;
            //PanelInputModel pmodel = new PanelInputModel();

            int k = 0;
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,BackPanelLength,0),
                new Point3D(0,BackPanelLength,PlenumHeight),
                new Point3D(0,0,PlenumHeight)
            });
            drawing.Entities.Add(rectangle, Color.AntiqueWhite);
            Brep brep = rectangle.ExtrudeAsBrep(SheetThickness);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,0),
                new Point3D(0,BackPanelLength,0),
                new Point3D(0,BackPanelLength,PlenumHeight),
                new Point3D(0,0,PlenumHeight),
                new Point3D(0,0,0)
            });
            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(-36.8,0,0),
                new Point3D(-36.8,0,SheetThickness),
                new Point3D(-SheetThickness,0,SheetThickness),
                new Point3D(-SheetThickness,0,36.8),
                new Point3D(0,0,36.8)
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);

            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            BackPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.AntiqueWhite);
            double doorPositionX = (BackPanelLength - (double)FASDoor.MaintainanceDoorWidth) / 2;
            double doorWidthFAS = (double)FASDoor.MaintainanceDoorWidth;
            double doorHeightFAS = (PlenumHeight - (double)FASDoor.MaintainanceDoorHeight) / 2;

            if (j == 0)
            {
                if (FASDoor.SideLocationDoorFAS == "Front")
                {
                    Point3D[] cutout = new Point3D[]
                    {
                        new Point3D(0, doorPositionX,                doorHeightFAS),
                        new Point3D(0, doorPositionX,                 doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(0, doorPositionX + doorWidthFAS,  doorHeightFAS+(double)FASDoor.MaintainanceDoorHeight),
                        new Point3D(0, doorPositionX + doorWidthFAS,  doorHeightFAS)
                    };
                    var cutout1 = devregion.CreatePolygon(cutout);
                    brep.ExtrudeRemove(cutout1, -SheetThickness);
                    drawing.Entities.Add(brep, Color.DarkOrange);
                }
            }

            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/FrontPanelInTop {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(Rectangle_Weight + Frame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        #endregion

        #region Structure frame
        public PanelDrawResult TopStructureFrame(PaintBoothModel model)
        {
            //if (model.PanelTypesforH == "1")
            //    PanelHeight = model.PanelHeightforH;
            //else
            double PanelHeight = model.H;

            drawing = new();
            int k = 0;
            drawing.Units = linearUnitsType.Millimeters;


            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,-SettingStandardBend2,PanelHeight),
                new Point3D(model.D,-SettingStandardBend2,PanelHeight),
                new Point3D(model.D,(model.W-SettingStandardBend2),PanelHeight),
                new Point3D(0,(model.W-SettingStandardBend2),PanelHeight),
            });

            LinearPath rail = new LinearPath(new Point3D[]
           {
               new Point3D(0,0,PanelHeight+75),
                new Point3D(model.D,0,PanelHeight+75),
                new Point3D(model.D,model.W,PanelHeight+75),
                new Point3D(0,model.W,PanelHeight+75),
                 new Point3D(0,0,PanelHeight+75),
           });

            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, PanelHeight),
                new Point3D(0,0,(SettingH+PanelHeight)),
                new Point3D(0,-SettingW,(SettingH+PanelHeight)),
                new Point3D(0,-SettingW,((SettingH-SettingT)+PanelHeight)),
                new Point3D(0,-SettingT,((SettingH-SettingT)+PanelHeight)),
                new Point3D(0,-SettingT,(SettingT+PanelHeight)),
                new Point3D(0,-SettingW,(SettingT+PanelHeight)),
                new Point3D(0,-SettingW,PanelHeight)
            });

            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);
            //Calculate weight of TopStructureFrame
            Material mt = Material.StructuralSteel;
            mt = new(Materials);
            frame = GenerateHoles(frame);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mt, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            TopStructureFrame_Weight = Frame_Weight;
            //SavePanelDetails(model, "TopStructureFrame", k, TopStructureFrame_Weight);

            drawing.Entities.Add(frame, Color.Yellow);
            model.CChannelHeight = SettingH;
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/TopStructure {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = Math.Round(TopStructureFrame_Weight, 3),
                lstpath = dwgFilePath,
            };
        }
        public PaintBoothclass BaseStructure(PaintBoothModel model, string PaintBoothTypefromEnquiry)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            int k = 0;
            LinearPath rail = new();
            double x = 0;
            if (PaintBoothTypefromEnquiry == "4" || PaintBoothTypefromEnquiry == "5" || PaintBoothTypefromEnquiry == "6" || PaintBoothTypefromEnquiry == "7")
                x = model.D;
            else
                x = model.D + D3;
            devregion rectangle = new();
            if (PaintBoothTypefromEnquiry == "8" || PaintBoothTypefromEnquiry == "9")
            {
                rectangle = devregion.CreatePolygon(new Point3D[]
               {
                    new Point3D(-model.D3,0,0),
                    new Point3D(x,0,0),
                    new Point3D(x,model.W,0),
                    new Point3D(-model.D3,model.W,0),
               });
            }
            else
                rectangle = devregion.CreatePolygon(new Point3D[]
               {
                new Point3D(0,0,0),
                new Point3D(x,0,0),
                new Point3D(x,model.W,0),
                new Point3D(0,model.W,0),
               });
            // drawing.Entities.Add(rectangle, Color.Pink);
            #region Frame Calculations
            if (PaintBoothTypefromEnquiry == "1")
            {
                rail = new LinearPath(new Point3D[]
                {
                    new Point3D(0,0,0),
                    new Point3D(x,0,0),
                    new Point3D(x,model.W,0),
                    new Point3D(0,model.W,0),
                    //new Point3D(0,0,0),
                });
            }
            else if (PaintBoothTypefromEnquiry == "8" || PaintBoothTypefromEnquiry == "9")
            {
                rail = new LinearPath(new Point3D[]
               {
                    new Point3D(-model.D3,0,0),
                    new Point3D(x,0,0),
                    new Point3D(x,model.W,0),
                    new Point3D(-model.D3,model.W,0),
                     new Point3D(-model.D3,0,0),
              });
            }
            else
            {
                rail = new LinearPath(new Point3D[]
               {
                    new Point3D(0,0,0),
                    new Point3D(x,0,0),
                    new Point3D(x,model.W,0),
                    new Point3D(0,model.W,0),
                    new Point3D(0,0,0),
              });
            }
            devregion section;
            if (Section == "Bend Section")
            {
                if (PaintBoothTypefromEnquiry == "8" || PaintBoothTypefromEnquiry == "9")
                {
                    section = devregion.CreatePolygon(new Point3D[]
                    {

                        new Point3D(-model.D3, 0, 0),
                        new Point3D(-model.D3,0,-SettingH),
                        new Point3D(-model.D3,-SettingW,-SettingH),
                        new Point3D(-model.D3,-SettingW,-(SettingH-SettingT)),
                        new Point3D(-model.D3,-SettingT,-(SettingH-SettingT)),
                        new Point3D(-model.D3,-SettingT,-SettingT),
                        new Point3D(-model.D3,-SettingW,-SettingT),
                        new Point3D(-model.D3,-SettingW,0)
                    });
                }
                else
                {
                    section = devregion.CreatePolygon(new Point3D[]
                    {

                        new Point3D(0, 0, 0),
                        new Point3D(0,0,-SettingH),
                        new Point3D(0,-SettingW,-SettingH),
                        new Point3D(0,-SettingW,-(SettingH-SettingT)),
                        new Point3D(0,-SettingT,-(SettingH-SettingT)),
                        new Point3D(0,-SettingT,-SettingT),
                        new Point3D(0,-SettingW,-SettingT),
                        new Point3D(0,-SettingW,0)
                    });
                }
                
            }
            else
            {
                section = devregion.CreatePolygon(new Point3D[]
               {

                new Point3D(0, 0, 0),
                new Point3D(0,0,-75),
                new Point3D(0,-40,-75),
                new Point3D(0,-40,-72),
                new Point3D(0,-3,-72),
                new Point3D(0,-3,-3),
                new Point3D(0,-40,-3),
                new Point3D(0,-40,0)
               });
            }

            Solid frame = section.SweepAsSolid(rail, 0);
            
            frame.Translate(0, 0, SheetThickness);
            //Calculate Weight of BaseStructureFrame
            Material mt = Material.StructuralSteel;
            frame.Regen(0.1);
            double MassofBaseFrame = frame.GetMass(mt, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            BaseFrame_Weight = Math.Round(MassofBaseFrame, 3);
            #endregion
            SavePanelDetails(model, "BaseStructureFrame", k, BaseFrame_Weight);

            GenerateHoles(frame);
            drawing.Entities.Add(frame, Color.Yellow);
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/BaseStructure {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };
        }
        #endregion
        private Solid GenerateHoles(Solid frame)
        {
            double StandardBend1 = SettingStandardBend1;
            double StandardBend2 = SettingStandardBend2;
            double divisionresult = PanelHeight / PitchDistance;
            int wholenumberpart = (int)Math.Floor(divisionresult);
            double multipliedresult = wholenumberpart * PitchDistance;
            double samespacedivide = PanelHeight - multipliedresult;

            for (int i = 0; i < wholenumberpart; i++)
            {
                double centerz = i == 0 ? samespacedivide / 2 : PanelHeight - (i + 1) * PitchDistance + (samespacedivide / 2);
                devregion ssr2 = devregion.CreateSlot(Plane.YZ, (StandardBend2 / 2), centerz, 5, 2, 1.5708);
                ssr2.Translate(0, 0, samespacedivide / 2);

                frame.ExtrudeRemove(ssr2, PanelWidth, 0);
                drawing.Entities.Add(frame);
            }
            //for generating holes on xy Plane
            double divisionresult1 = PanelWidth / PitchDistance;
            int wholenumberpart1 = (int)Math.Floor(divisionresult1);
            double multipliedresult1 = wholenumberpart1 * PitchDistance;
            double samespacedivide1 = PanelWidth - multipliedresult1;

            for (int i = 0; i <= wholenumberpart1; i++)
            {
                double centerx = i == 0 ? samespacedivide1 / 2 : PanelWidth - (i + 1) * PitchDistance + (samespacedivide / 2);
                devregion circle1 = devregion.CreateSlot(Plane.XY, centerx, (StandardBend2 / 2), 5, 2);

                frame.ExtrudeRemove(circle1, PanelHeight, 0);
            }
            return frame;
        }
        private devregion CreatePolygon(int k)
        {
            double StandardBend1 = SettingStandardBend1;
            double StandardBend2 = SettingStandardBend2;
            if (k == 1)
            {
                return devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(0, 0, 0),
                    new Point3D(SheetThickness, 0, 0),
                    new Point3D(SheetThickness, StandardBend2 - SheetThickness, 0),
                    new Point3D(StandardBend1, StandardBend2 - SheetThickness, 0),
                    new Point3D(StandardBend1, StandardBend2, 0),
                    new Point3D(0, StandardBend2, 0)
                });
            }
            else
            {
                return devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(0, 0, 0),
                    new Point3D(SheetThickness, 0, 0),
                    new Point3D(SheetThickness, -(StandardBend2 - SheetThickness), 0),
                    new Point3D(StandardBend1, -(StandardBend2 - SheetThickness), 0),
                    new Point3D(StandardBend1, -StandardBend2, 0),
                    new Point3D(0, -StandardBend2, 0)
                });
            }
        }
        private devregion CreatePolygonForEC(int k)
        {
            double StandardBend1 = ECStandardBend1;
            double StandardBend2 = ECStandardBend2;
            if (k == 1)
            {
                return devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(0, 0, 0),
                    new Point3D(ECSheetThickness, 0, 0),
                    new Point3D(ECSheetThickness, StandardBend2 - ECSheetThickness, 0),
                    new Point3D(StandardBend1, StandardBend2 - ECSheetThickness, 0),
                    new Point3D(StandardBend1, StandardBend2, 0),
                    new Point3D(0, StandardBend2, 0)
                });
            }
            else
            {
                return devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(0, 0, 0),
                    new Point3D(ECSheetThickness, 0, 0),
                    new Point3D(ECSheetThickness, -(StandardBend2 - ECSheetThickness), 0),
                    new Point3D(StandardBend1, -(StandardBend2 - ECSheetThickness), 0),
                    new Point3D(StandardBend1, -StandardBend2, 0),
                    new Point3D(0, -StandardBend2, 0)
                });
            }
        }
        private Solid GenerateHolesForEC(Solid frame)
        {
            double StandardBend1 = ECStandardBend1;
            double StandardBend2 = ECStandardBend2;
            double divisionresult = PanelHeight / ECPitchDistance;
            int wholenumberpart = (int)Math.Floor(divisionresult);
            double multipliedresult = wholenumberpart * PitchDistance;
            double samespacedivide = PanelHeight - multipliedresult;

            for (int i = 0; i < wholenumberpart; i++)
            {
                double centerz = i == 0 ? samespacedivide / 2 : PanelHeight - (i + 1) * ECPitchDistance + (samespacedivide / 2);
                devregion ssr2 = devregion.CreateSlot(Plane.YZ, (StandardBend2 / 2), centerz, 5, 2, 1.5708);
                ssr2.Translate(0, 0, samespacedivide / 2);

                frame.ExtrudeRemove(ssr2, PanelWidth, 0);
                drawing.Entities.Add(frame);
            }
            //for generating holes on xy Plane
            double divisionresult1 = PanelWidth / ECPitchDistance;
            int wholenumberpart1 = (int)Math.Floor(divisionresult1);
            double multipliedresult1 = wholenumberpart1 * ECPitchDistance;
            double samespacedivide1 = PanelWidth - multipliedresult1;

            for (int i = 0; i <= wholenumberpart1; i++)
            {
                double centerx = i == 0 ? samespacedivide1 / 2 : PanelWidth - (i + 1) * ECPitchDistance + (samespacedivide / 2);
                devregion circle1 = devregion.CreateSlot(Plane.XY, centerx, (StandardBend2 / 2), 5, 2);

                frame.ExtrudeRemove(circle1, PanelHeight, 0);
            }
            return frame;
        }
        public List<PaintBoothclass> OuterFilterFrame(PaintBoothModel model, double extractionChamberHeight, string paintBoothType)
        {
            List<PaintBoothclass> metalBaffleDrawings = new List<PaintBoothclass>();

            #region Setup Parameters
            model.W = model.PanelTypesforW == "1" ? model.PanelHeightforW : model.W;
            model.H = model.PanelTypesforH == "1" ? model.PanelHeightforH : model.H;
            model.D = model.PanelTypes == "1" ? model.StandardPanelWidthForD : model.D;


            double frameWidth = 600; // Width of each baffle
                                     // double frameHeight = 1000; // Height of each baffle
            model.FrameWidth = frameWidth;
            noOfFramesW = Math.Floor(model.W / frameWidth);
            bafflePanelCount = (int)noOfFramesW;
            double offsetX = 0;

            offsetX = model.D;
            double equalFilterSpace = (model.W - (noOfFramesW * frameWidth)) / 2;
            int BaffleHeight = 1140;

            model.FilterHeight = BaffleHeight;
            int totalBafflesinHeight = (int)extractionChamberHeight / BaffleHeight;
            #endregion

            #region Generate and Save Separate Drawings for Each Metal Baffle
            for (int j = 0; j < totalBafflesinHeight; j++)
            {

                for (int i = 0; i < noOfFramesW; i++)
                {
                    // Create a new drawing document for each baffle instance
                    DesignDocument drawing = new DesignDocument();
                    drawing.Units = linearUnitsType.Millimeters;

                    // Calculate position for the current baffle
                    double posX = offsetX;

                    // Create an instance of MetalBaffle for the current position
                    PaintBoothclass metalBaffleInstance = MetalBaffle(BaffleHeight);

                    foreach (Entity entity in metalBaffleInstance.drawing.Entities)
                    {
                        Entity clonedEntity = (Entity)entity.Clone();

                        // Rotate and translate to position each metal baffle
                        clonedEntity.Rotate(Math.PI / 2, Vector3D.AxisZ, new Point3D(0, 0, 0));
                        clonedEntity.Translate(posX, equalFilterSpace, 0);

                        drawing.Entities.Add(clonedEntity, entity.Color);
                    }
                    // Call the FilterFrame1 method to generate the current frame
                    PaintBoothclass filterFrameInstance = FilterFrame1(BaffleHeight, model);

                    // Position each frame at an offset
                    posX = offsetX + 100; // Add 100 units to X for each iteration               

                    foreach (Entity entity in filterFrameInstance.drawing.Entities)
                    {
                        Entity clonedEntity = (Entity)entity.Clone();
                        clonedEntity.Translate(posX, equalFilterSpace, 0); // Translate to new X position                   
                        drawing.Entities.Add(clonedEntity, entity.Color);
                    }
                    // Save the individual drawing
                    var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

                    if (!Directory.Exists(path + "/PaintBooth drawing/OuterFilterFrame"))
                        Directory.CreateDirectory(path + "/PaintBooth drawing/OuterFilterFrame");

                    string dwgFilePath = $"{path}/PaintBooth drawing/OuterFilterFrame/FilterFrame_{i + 1}_{DateTime.Now:hh-mm}.dwg";

                    WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
                    WriteAutodesk dwgWriter = new WriteAutodesk(auto, dwgFilePath);
                    dwgWriter.DoWork();

                    // Add the individual drawing info to the list
                    metalBaffleDrawings.Add(new PaintBoothclass
                    {
                        drawing = drawing,
                        lstpath = dwgFilePath
                    });
                }

            }
            #endregion
            SaveMetalBaffleDetails(model, paintBoothType);
            SaveFilterDetails(model, paintBoothType);
            return metalBaffleDrawings;
        }

        //this method for when down draft With FAS without Civil paintbooth type=5 then call 
        public List<PaintBoothclass> OuterFilterFrameAtZeroX(PaintBoothModel model, string DraftSubType)
        {
            List<PaintBoothclass> metalBaffleDrawings = new List<PaintBoothclass>();

            #region Setup Parameters
            model.W = model.PanelTypesforW == "1" ? model.PanelHeightforW : model.W;
            model.H = model.PanelTypesforH == "1" ? model.PanelHeightforH : model.H;
            model.D = model.PanelTypes == "1" ? model.StandardPanelWidthForD : model.D;

            double frameWidth = 600;
            model.FrameWidth = frameWidth;
            noOfFramesW = Math.Floor(model.W / frameWidth);
            bafflePanelCount = (int)noOfFramesW;

            double equalFilterSpace = (model.W - (noOfFramesW * frameWidth)) / 2;
            int BaffleHeight = 1000;
            int baffleHeight1 = 1200;
            int paintBoothHeight = (int)model.H;

            int remainingHeight1 = paintBoothHeight % BaffleHeight;
            int remainingHeight2 = paintBoothHeight % baffleHeight1;

            if (remainingHeight1 < remainingHeight2)
            {
                selectedBaffleHeight = BaffleHeight;
            }
            else
            {
                selectedBaffleHeight = baffleHeight1;
            }

            model.FilterHeight = selectedBaffleHeight;
            int totalBafflesinHeight = paintBoothHeight / selectedBaffleHeight;
            #endregion

            #region Generate and Save Separate Drawings for Each Metal Baffle
            for (int j = 0; j < totalBafflesinHeight; j++)
            {
                for (int i = 0; i < noOfFramesW; i++)
                {
                    DesignDocument drawing = new DesignDocument();
                    drawing.Units = linearUnitsType.Millimeters;


                    double posX = D3; // Updated position instead of 0
                    double framePosX = D3;

                    // Add metal baffle
                    PaintBoothclass metalBaffleInstance = MetalBaffle(selectedBaffleHeight);
                    foreach (Entity entity in metalBaffleInstance.drawing.Entities)
                    {
                        Entity clonedEntity = (Entity)entity.Clone();
                        clonedEntity.Rotate(Math.PI / 2, Vector3D.AxisZ, new Point3D(0, 0, 0));
                        clonedEntity.Translate(posX, equalFilterSpace, 0);
                        drawing.Entities.Add(clonedEntity, entity.Color);
                    }

                    // Add filter frame
                    PaintBoothclass filterFrameInstance = FilterFrame1(selectedBaffleHeight, model);
                    foreach (Entity entity in filterFrameInstance.drawing.Entities)
                    {
                        Entity clonedEntity = (Entity)entity.Clone();
                        clonedEntity.Translate(framePosX, equalFilterSpace, 0);
                        drawing.Entities.Add(clonedEntity, entity.Color);
                    }

                    // Regenerate entity bounds once at the end
                    drawing.Entities.Regen();

                    // Validate entity presence
                    if (drawing.Entities.Count == 0)
                        throw new Exception("No entities were added to the drawing. Cannot export empty DWG.");

                    // Prepare path
                    var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

                    string folderPath = Path.Combine(path, "PaintBooth drawing", "OuterFilterFrameZeroX");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string dwgFilePath = Path.Combine(folderPath, $"FilterFrame_{i + 1}_{DateTime.Now:HH-mm-ss}.dwg");

                    // Write DWG file
                    try
                    {
                        WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
                        WriteAutodesk dwgWriter = new WriteAutodesk(auto, dwgFilePath);
                        dwgWriter.DoWork(); // <-- Main export
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"DWG export failed for frame {i + 1}: {ex.Message}", ex);
                    }

                    metalBaffleDrawings.Add(new PaintBoothclass
                    {
                        drawing = drawing,
                        lstpath = dwgFilePath
                    });
                }
            }
            #endregion

            //SaveMetalBaffleDetails(model);
            //SaveFilterDetails(model);
            return metalBaffleDrawings;
        }
        public PaintBoothclass FAS(/*int j,*/ PaintBoothModel model, int m)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;

            #region Setup Parameters
            model.W = model.PanelTypesforW == "1" ? model.PanelHeightforW : model.W;
            model.H = model.PanelTypesforH == "1" ? model.PanelHeightforH : model.H;
            model.D = model.PanelTypes == "1" ? model.StandardPanelWidthForD : model.D;
            PanelHeight = model.H;

            double totalFilterAreaSqM = model.FilterArea;
            double frameWidthMm = 1200;
            double frameHeightMm = 600;

            // Convert frame area from mm² to m²
            double frameAreaSqM = (frameWidthMm * frameHeightMm) / 1_000_000.0;

            // Calculate number of frames needed
            int numberOfFrames = (int)Math.Floor(totalFilterAreaSqM / frameAreaSqM);
            #endregion
            //double xOffset = m * frameWidthMm; // `m` is the index passed in

            //var rectangle = devregion.CreatePolygon(new Point3D[]
            //{
            //    new Point3D(xOffset, 0, PanelHeight),
            //    new Point3D(xOffset + frameWidthMm, 0, PanelHeight),
            //    new Point3D(xOffset + frameWidthMm, frameHeightMm, PanelHeight),
            //    new Point3D(xOffset, frameHeightMm, PanelHeight),
            //});
            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, PanelHeight),
                new Point3D(frameWidthMm, 0, PanelHeight),
                new Point3D(frameWidthMm, frameHeightMm, PanelHeight),
                new Point3D(0,frameHeightMm, PanelHeight),
            });
            drawing.Entities.Add(rectangle, Color.AntiqueWhite);

            // Save the individual drawing
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing/FAS"))
                Directory.CreateDirectory(path + "/PaintBooth drawing/FAS");

            string dwgFilePath = $"{path}/PaintBooth drawing/FAS/Filters_{DateTime.Now:hh-mm-ss}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgWriter = new WriteAutodesk(auto, dwgFilePath);
            dwgWriter.DoWork();

            // Add the individual drawing info to the list
            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };

        }
        public int SaveFilterDetails(PaintBoothModel model, string paintBoothType)
        {
            int multiplier = (paintBoothType == "8" || paintBoothType == "9") ? 2 : 1;
            DeleteOldPaintArresterFilterDetails(EnquiryID);
            FilterFrameDetails obj = new FilterFrameDetails();


            obj.IsActive = true;
            obj.EnquiryID = EnquiryID;
            obj.SalesNO = model.SalesNO;
            //obj.FrameWidth = (decimal)model.FrameWidth;
            //obj.FrameHeight = (decimal)model.FilterHeight;
            obj.FrameWidth = (decimal)model.W;
            obj.FrameHeight = (decimal)model.ExtractionChamberHeight;
            obj.Quantity = (int)(noOfFramesW * multiplier);
            obj.FrameWeight = (decimal)(totalWeight) * (int)(noOfFramesW * multiplier);
            _DbContext.FilterFrameDetails.Add(obj);
            _DbContext.SaveChanges();
            return 1;
        }
        public int SaveMetalBaffleDetails(PaintBoothModel model, string paintBoothType)
        {
            int multiplier = (paintBoothType == "8" || paintBoothType == "9") ? 2 : 1;
            DeleteOldMetalBaffelsDetails(EnquiryID);
            MetalBaffleDetails obj = new MetalBaffleDetails();
            obj.IsActive = true;
            obj.EnquiryID = EnquiryID;
            obj.SalesNo = model.SalesNO;
            obj.BaffleWidth = (decimal)model.FrameWidth;
            obj.BaffleHeight = (decimal)model.FilterHeight;
            obj.Quantity = (int)(noOfFramesW * multiplier);
            obj.BaffleWeight = (decimal)(totalWeightOfbaffle * (int)(noOfFramesW * multiplier));
            _DbContext.MetalBaffleDetails.Add(obj);
            _DbContext.SaveChanges();
            return 1;
        }

        public int SavePanelDetails(PaintBoothModel model, string panelPosition, int noOfPanel, double panelWeight)
        {
            double width = 0;
            double depth = 0;
            double height = 0;
            double frameHeight = 0;
            double frameWidth = 0;

            // ---------- SET DIMENSIONS BASED ON POSITION ----------
            switch (panelPosition)
            {
                case "PARight":
                case "PALeft":
                    width = PanelHeight;
                    depth = PanelWidth;
                    height = PanelHeight;
                    panelWeight = panelWeight * noOfPanel;
                    break;

                case "PARight_Panel_Doors":
                case "PALeft_Panel_Doors":
                case "PARight_Panel_Doors2":
                case "PALeft_Panel_Doors2":
                    width = PanelWidthForSideDoors;
                    depth = PanelWidthForSideDoors;
                    height = PanelHeightForSideDoors;
                    break;

                case "PARight_Panel_Above_CE_Doors":
                case "PALeft_Panel_Above_CE_Doors":
                    width = panelWidthforAboveCompEntryPanels;
                    depth = panelWidthforAboveCompEntryPanels;
                    height = panelHeightforAboveCompEntryPanels;
                    break;

                case "PARightFAS":
                case "PALeftFAS":
                    width = PanelWidth;
                    depth = PanelWidth;
                    height = plenumHeight;
                    break;

                case "PARearFAS":
                    width = BackPanelLength;
                    depth = BackPanelLength;
                    height = plenumHeight;
                    break;

                case "ECRight_Panels":
                case "ECLeft_Panels":
                case "ECRight_PanelsBeforeD":
                case "ECLeft_PanelsBeforeD":
                    width = PanelHeight;
                    depth = D3;
                    height = ExtractionChamberHeight;
                    break;

                case "PATop":
                case "PATopFAS":
                    width = PanelLength;
                    depth = PanelWidth;
                    height = W;
                    panelWeight = panelWeight * noOfPanel;
                    break;

                case "ECRear_Panels":
                    width = BackPanelLength;
                    depth = BackPanelLength;
                    height = ExtractionChamberHeight;
                    break;

                case "ECRear_Panels_Before_EC":
                    width = BackPanelLength;
                    depth = BackPanelLength;
                    height = BackPanelHeightBeforeEC;
                    break;
                case "ECFront_Panels":
                    width = BackPanelLength;
                    depth = BackPanelLength;
                    height = ExtractionChamberHeight;
                    break;

                case "ECFront_Panels_Before_EC":
                    width = BackPanelLength;
                    depth = BackPanelLength;
                    height = BackPanelHeightBeforeEC;
                    break;
                case "PAFront_Doors":
                    width = PanelWidthForFrontDoors;
                    depth = PanelWidthForFrontDoors;
                    height = PanelHeightForFrontDoors;
                    panelWeight = panelWeight * noOfPanel;
                    break;

                case "PAFront":
                    width = FrontPanelLength;
                    depth = FrontPanelLength;
                    height = FrontPanelHeight;
                    break;

                case "PAFront_Above_Comp_Entry_Doors":
                    width = panelWidthforAboveCompEntryPanels;
                    depth = panelWidthforAboveCompEntryPanels;
                    height = panelHeightforAboveCompEntryPanels;
                    break;

                case "TopStructureFrame":
                case "BaseStructureFrame":
                    frameHeight = model.W;
                    frameWidth = model.D + D3;
                    break;

                case "Transition_Piece":
                    width = 0;
                    depth = 0;
                    height = 0;
                    break;

                case "FilterTop":
                    width = 0;
                    depth = 0;
                    height = 0;
                    break;

                case "Civil_Tank":
                    frameHeight = model.W;
                    frameWidth = model.D + D3 + D3; // Civil tank spans full depth including both D3 panels
                    break;

               

            }
            // In isECPanel bool:
            bool isECPanel = panelPosition == "ECRight_Panels" ||
                             panelPosition == "ECLeft_Panels" ||
                             panelPosition == "ECRight_PanelsBeforeD" ||
                             panelPosition == "ECLeft_PanelsBeforeD" ||
                             panelPosition == "ECFront_Panels" ||
                             panelPosition == "ECFront_Panels_Before_EC";

            // ---------- CHECK IF SAME RECORD EXISTS ----------
            var existing = _DbContext.PanelDetails.FirstOrDefault(x =>
                x.EnquiryId == EnquiryID &&
                x.PanelPosition == panelPosition &&
                x.StandardPanelWidth == width &&
                x.StandardPanelDepth == depth &&
                x.StandardPanelHeight == height &&
                x.IsActive
            );

            if (existing != null)
            {
                // ✅ UPDATE EXISTING
                existing.NoOfPanels += noOfPanel;
                existing.PanelWeight += panelWeight;
                existing.ModifiedDate = DateTime.Now;

                _DbContext.SaveChanges();
                return 2; // updated
            }

            else
            {
                // ✅ INSERT NEW
                PanelDetails tblobj = new PanelDetails
                {
                    EnquiryId = EnquiryID,
                    SalesNo = model.SalesNO,
                    PanelPosition = panelPosition,
                    StandardPanelWidth = width,
                    StandardPanelDepth = depth,
                    StandardPanelHeight = height,
                    FrameHeight = frameHeight,
                    FrameWidth = frameWidth,
                    NoOfPanels = noOfPanel,
                    PanelWeight = panelWeight,
                    //SlotDimention = (panelPosition == "ECRight_Panels" || panelPosition == "ECLeft_Panels") ? ECSlotDimention : SlotDimention,
                    //StandardBend1 = (panelPosition == "ECRight_Panels" || panelPosition == "ECLeft_Panels") ? (decimal)ECStandardBend1 : (decimal)SettingStandardBend1,
                    //StandardBend2 = (panelPosition == "ECRight_Panels" || panelPosition == "ECLeft_Panels") ? (decimal)ECStandardBend2 : (decimal)SettingStandardBend2,
                    //SheetThickness = (panelPosition == "ECRight_Panels" || panelPosition == "ECLeft_Panels") ? (decimal)ECSheetThickness : (decimal)SheetThickness,
                    //PitchDistance = (panelPosition == "ECRight_Panels" || panelPosition == "ECLeft_Panels") ? (decimal)ECPitchDistance : (decimal)PitchDistance,
                    // REPLACE with (add ECRight_PanelsBeforeD and ECLeft_PanelsBeforeD):
                    SlotDimention = isECPanel ? ECSlotDimention : SlotDimention,
                    StandardBend1 = isECPanel ? (decimal)ECStandardBend1 : (decimal)SettingStandardBend1,
                    StandardBend2 = isECPanel ? (decimal)ECStandardBend2 : (decimal)SettingStandardBend2,
                    SheetThickness = isECPanel ? (decimal)ECSheetThickness : (decimal)SheetThickness,
                    PitchDistance = isECPanel ? (decimal)ECPitchDistance : (decimal)PitchDistance,
                    CostingStatus = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1,
                    IsActive = true
                };

                _DbContext.PanelDetails.Add(tblobj);
                _DbContext.SaveChanges();
                return 1; // inserted
            }
        }
        public PaintBoothclass detailsdrawing(DesignDocument model, PaintBoothModel pmodel,string paintBoothType)
        {

            model.Entities.Regen();
            model.Entities.UpdateBoundingBox();
            string EnquiryCode = pmodel.SalesNO;
            var path = _configuration["FolderPathConfig:DrawingPath"];
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string partname = "CombineAssembly";
            model.Units = linearUnitsType.Millimeters;
            AddSheet asfp = new AddSheet();

            drawingdoc = asfp.AddSheets(partname);

            #region INSERTION OF VIEWS
            // Get the model bounding box (Eyeshot axis: X = width, Z = depth, Y = height)
            Point3D box = model.Entities.BoxSize;
            double modelWidth = Math.Max(box.X, box.Z); // largest horizontal span (width or depth)
            double modelHeight = box.Y + 4900;                  // vertical height (front view height)

            #endregion

            #region SCALING SHEET

            // Define usable drawing area (A2 size minus margins)
            double usableWidth = 594 - 40;     // Adjust to match StandardFrame margins
            double usableHeight = 420 - 80;    // Adjust to match titlebox height

            // Safety check
            if (usableWidth <= 0) usableWidth = 1;
            if (usableHeight <= 0) usableHeight = 1;

            // Add margins
            double marginX = 40;
            double marginY = 40;

            // Calculate the required scale
            double scaleX = (modelWidth + marginX) / usableWidth;
            double scaleY = (modelHeight + marginY) / usableHeight;

            double requiredScale = Math.Max(scaleX, scaleY);

            int finalScaleFactor = (int)Math.Ceiling(requiredScale);
            if (finalScaleFactor < 1) finalScaleFactor = 1;

            #endregion

            #region addviews

            MySheet mysheet;
            pmodel.standardbend2 = (decimal)SettingStandardBend2;
            var panelDetails = GetAllPanels(EnquiryCode);
            mysheet = asfp.StandardFrame(finalScaleFactor, (MySheet)drawingdoc.ActiveSheet, new Point2D(0, 0), pmodel, panelDetails);
            const string Dim = "Dimension";

            drawingdoc.Layers.Add(new Layer(Dim, Color.Red));
            ViewBuilder vb = new ViewBuilder(model, drawingdoc);
            vb.DoWork();
            var vbType = vb.GetType();
            foreach (var prop in vbType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Debug.WriteLine($"Prop: {prop.Name} : {prop.PropertyType}");
            }

            foreach (var kvp in vb.SheetsViews)
            {
                Debug.WriteLine($"Sheet: {kvp.Key} -- ViewCount={kvp.Value.Count}");

                foreach (var view in kvp.Value)
                {
                    var vType = view.GetType();
                    Debug.WriteLine($"  -- View instance: {vType.FullName} --");

                    // Targeted dump of the fields we actually care about
                    Debug.WriteLine($"     BlockName = {view.BlockName}");
                    Debug.WriteLine($"     InsertionPoint = {view.InsertionPoint}");

                    var t = view.Transformation;
                    Debug.WriteLine($"     Transformation.ToString() = {t}");

                    var tType = t.GetType();
                    foreach (var p in tType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        object val;
                        try { val = p.GetValue(t); } catch { val = "<unreadable>"; }
                        Debug.WriteLine($"       Transformation.{p.Name} = {val}");
                    }

                    // Keep the full generic dump too, for anything we haven't targeted above
                    foreach (var p in vType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        object val;
                        try { val = p.GetValue(view); }
                        catch { val = "<unreadable>"; }
                        Debug.WriteLine($"     {p.Name} ({p.PropertyType.Name}) = {val}");
                    }
                }
            }
            // ---- END DEBUG DUMP ----

            vb.AddTo(drawingdoc);
            AddViewDimensions((MySheet)drawingdoc.ActiveSheet, vb, pmodel,paintBoothType);

            drawingdoc.ActiveSheet = mysheet;
            #endregion addviews          

            #region writefiles
            if (!Directory.Exists(path + "/MFG Drawing"))
                Directory.CreateDirectory(path + "/MFG Drawing");
            string dwgFilePath = $"{path}/MFG Drawing/GA {DateTime.Now:hh-mm}.dwg";
            WriteAutodeskParams auto = new WriteAutodeskParams(drawingdoc);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion writefiles
            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };
        }

        //private void AddViewDimensions(MySheet sheet, ViewBuilder vb, PaintBoothModel model, string paintBoothType)
        //{
        //    const string Dim = "Dimension";

        //    double d = (double)model.D;
        //    double d3 = (double)model.D3;
        //    double w = (double)model.W;
        //    double h = (double)model.H;
        //    double ECh = (double)model.ExtractionChamberHeight;

        //    foreach (var kvp in vb.SheetsViews)
        //    {
        //        foreach (var view in kvp.Value)
        //        {
        //            double vx = view.X, vy = view.Y;
        //            double scale = view.Scale;

        //            if (view.BlockName == "Front" || view.BlockName == "Top")
        //            {
        //                double crossDim = (view.BlockName == "Front") ? h : w;
        //                double tightW = (d + d3) * scale;
        //                double tightH = crossDim * scale;

        //                double dimOffset = tightW * 0.04;
        //                double extLineExt = tightW * 0.015;
        //                double textH = Math.Max(tightH * 0.025, tightW * 0.015);
        //                double arrowSize = extLineExt * 1.5;

        //                Point2D bl = new Point2D(vx - tightW / 2, vy - tightH / 2);
        //                Point2D br = new Point2D(vx + tightW / 2, vy - tightH / 2);
        //                Point2D tl = new Point2D(vx - tightW / 2, vy + tightH / 2);

        //                Point2D boundary = new Point2D(bl.X + d * scale, bl.Y);

        //                double hY = bl.Y - dimOffset;

        //                sheet.Entities.Add(new Line(bl.X, bl.Y, bl.X, hY - extLineExt) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(boundary.X, boundary.Y, boundary.X, hY - extLineExt) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(br.X, br.Y, br.X, hY - extLineExt) { LayerName = Dim });

        //                sheet.Entities.Add(new Line(bl.X, hY, boundary.X, hY) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(boundary.X, hY, br.X, hY) { LayerName = Dim });

        //                // Arrowheads for D segment
        //                AddArrowhead(sheet, new Point2D(bl.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
        //                AddArrowhead(sheet, new Point2D(boundary.X, hY), new Vector2D(1, 0), arrowSize, Dim);
        //                // Arrowheads for D3 segment
        //                AddArrowhead(sheet, new Point2D(boundary.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
        //                AddArrowhead(sheet, new Point2D(br.X, hY), new Vector2D(1, 0), arrowSize, Dim);

        //                sheet.Entities.Add(new Text(
        //                    new Point3D((bl.X + boundary.X) / 2, hY - textH * 1.2, 0),
        //                    $"{d:F0}  DEPTH", textH, Text.alignmentType.BottomCenter)
        //                { LayerName = Dim });

        //                sheet.Entities.Add(new Text(
        //                    new Point3D((boundary.X + br.X) / 2, hY - textH * 1.2, 0),
        //                    $"{d3:F0}  EC WIDTH", textH, Text.alignmentType.BottomCenter)
        //                { LayerName = Dim });

        //                double vX = bl.X - dimOffset;
        //                sheet.Entities.Add(new Line(bl.X, bl.Y, vX - extLineExt, bl.Y) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(tl.X, tl.Y, vX - extLineExt, tl.Y) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(vX, bl.Y, vX, tl.Y) { LayerName = Dim });

        //                AddArrowhead(sheet, new Point2D(vX, bl.Y), new Vector2D(0, -1), arrowSize, Dim);
        //                AddArrowhead(sheet, new Point2D(vX, tl.Y), new Vector2D(0, 1), arrowSize, Dim);

        //                sheet.Entities.Add(new Text(
        //                    new Point3D(vX - textH * 1.2, (bl.Y + tl.Y) / 2, 0),
        //                    $"{crossDim:F0}  HEIGHT", textH, Text.alignmentType.MiddleRight)
        //                { LayerName = Dim });

        //                // ---- NEW: Extraction Chamber Height (Front view only) ----
        //                //if (view.BlockName == "Front")
        //                //{
        //                //    double echSheetH = ECh * scale;
        //                //    double vX2 = br.X + dimOffset;

        //                //    Point2D echBottom = new Point2D(br.X, bl.Y);
        //                //    Point2D echTop = new Point2D(br.X, bl.Y + echSheetH);

        //                //    sheet.Entities.Add(new Line(echBottom.X, echBottom.Y, vX2 + extLineExt, echBottom.Y) { LayerName = Dim });
        //                //    sheet.Entities.Add(new Line(echTop.X, echTop.Y, vX2 + extLineExt, echTop.Y) { LayerName = Dim });
        //                //    sheet.Entities.Add(new Line(vX2, echBottom.Y, vX2, echTop.Y) { LayerName = Dim });

        //                //    AddArrowhead(sheet, new Point2D(vX2, echBottom.Y), new Vector2D(0, -1), arrowSize, Dim);
        //                //    AddArrowhead(sheet, new Point2D(vX2, echTop.Y), new Vector2D(0, 1), arrowSize, Dim);

        //                //    sheet.Entities.Add(new Text(
        //                //        new Point3D(vX2 + textH * 1.2, (echBottom.Y + echTop.Y) / 2, 0),
        //                //        $"{ECh:F0}  EC HEIGHT", textH, Text.alignmentType.MiddleLeft)
        //                //    { LayerName = Dim });
        //                //}
        //                // ---- Extraction Chamber Height annotation ----
        //                if (view.BlockName == "Front")
        //                {
        //                    double echSheetH = ECh * scale;
        //                    double vX2 = br.X + dimOffset;

        //                    Point2D echBottom = new Point2D(br.X, bl.Y);
        //                    Point2D echTop = new Point2D(br.X, bl.Y + echSheetH);

        //                    sheet.Entities.Add(new Line(echBottom.X, echBottom.Y, vX2 + extLineExt, echBottom.Y) { LayerName = Dim });
        //                    sheet.Entities.Add(new Line(echTop.X, echTop.Y, vX2 + extLineExt, echTop.Y) { LayerName = Dim });
        //                    sheet.Entities.Add(new Line(vX2, echBottom.Y, vX2, echTop.Y) { LayerName = Dim });

        //                    AddArrowhead(sheet, new Point2D(vX2, echBottom.Y), new Vector2D(0, -1), arrowSize, Dim);
        //                    AddArrowhead(sheet, new Point2D(vX2, echTop.Y), new Vector2D(0, 1), arrowSize, Dim);

        //                    sheet.Entities.Add(new Text(
        //                        new Point3D(vX2 + textH * 1.2, (echBottom.Y + echTop.Y) / 2, 0),
        //                        $"{ECh:F0}  EC HEIGHT", textH, Text.alignmentType.MiddleLeft)
        //                    { LayerName = Dim });
        //                }
        //                else if (view.BlockName == "Top")
        //                {
        //                    // No real vertical axis in plan view — just annotate the value
        //                    // near the EC chamber's location (over the D3 segment)
        //                    sheet.Entities.Add(new Text(
        //                       new Point3D((boundary.X + br.X) / 2, tl.Y + textH * 2.0, 0),
        //                       $"EC HEIGHT: {ECh:F0}", textH, Text.alignmentType.BottomCenter)
        //                                            { LayerName = Dim });
        //                }

        //            }
        //            else if (view.BlockName == "Left")
        //            {
        //                double tightW = w * scale;
        //                double tightH = h * scale;
        //                double dimOffset = tightW * 0.04;
        //                double extLineExt = tightW * 0.015;
        //                double textH = Math.Max(tightH * 0.025, tightW * 0.015);
        //                double arrowSize = extLineExt * 1.5;

        //                Point2D bl = new Point2D(vx - tightW / 2, vy - tightH / 2);
        //                Point2D br = new Point2D(vx + tightW / 2, vy - tightH / 2);
        //                Point2D tl = new Point2D(vx - tightW / 2, vy + tightH / 2);

        //                double hY = bl.Y - dimOffset;
        //                sheet.Entities.Add(new Line(bl.X, bl.Y, bl.X, hY - extLineExt) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(br.X, br.Y, br.X, hY - extLineExt) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(bl.X, hY, br.X, hY) { LayerName = Dim });

        //                AddArrowhead(sheet, new Point2D(bl.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
        //                AddArrowhead(sheet, new Point2D(br.X, hY), new Vector2D(1, 0), arrowSize, Dim);

        //                sheet.Entities.Add(new Text(
        //                    new Point3D((bl.X + br.X) / 2, hY - textH * 1.2, 0),
        //                    $"{w:F0}  WIDTH", textH, Text.alignmentType.BottomCenter)
        //                { LayerName = Dim });

        //                double vX = bl.X - dimOffset;
        //                sheet.Entities.Add(new Line(bl.X, bl.Y, vX - extLineExt, bl.Y) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(tl.X, tl.Y, vX - extLineExt, tl.Y) { LayerName = Dim });
        //                sheet.Entities.Add(new Line(vX, bl.Y, vX, tl.Y) { LayerName = Dim });

        //                AddArrowhead(sheet, new Point2D(vX, bl.Y), new Vector2D(0, -1), arrowSize, Dim);
        //                AddArrowhead(sheet, new Point2D(vX, tl.Y), new Vector2D(0, 1), arrowSize, Dim);

        //                sheet.Entities.Add(new Text(
        //                    new Point3D(vX - textH * 1.2, (bl.Y + tl.Y) / 2, 0),
        //                    $"{h:F0}  HEIGHT", textH, Text.alignmentType.MiddleRight)
        //                { LayerName = Dim });
        //            }
        //        }
        //    }
        //}
        
        private void AddViewDimensions(MySheet sheet, ViewBuilder vb, PaintBoothModel model, string paintBoothType)
         {
            const string Dim = "Dimension";

            double d = (double)model.D;
            double d3 = (double)model.D3;
            double w = (double)model.W;
            double h = (double)model.H;
            double ECh = (double)model.ExtractionChamberHeight;

            bool isSemiDownDraft = paintBoothType == "8" || paintBoothType == "9";
            bool hasPlenum = paintBoothType == "3" || paintBoothType == "4"
                  || paintBoothType == "5" || paintBoothType == "8";
            bool hasExtractionChamber = paintBoothType != "4" && paintBoothType != "5";
            bool hasCivil = paintBoothType == "4"; // "With FAS & Civil" — type 5 explicitly has none

            foreach (var kvp in vb.SheetsViews)
            {
                foreach (var view in kvp.Value)
                {
                    double vx = view.X, vy = view.Y;
                    double scale = view.Scale;
                    bool isFront = view.BlockName == "Front";

                    if (view.BlockName == "Front" || view.BlockName == "Top")
                    {
                        double crossDim = isFront ? h : w;
                        double totalDepth = isSemiDownDraft ? (d3 + d + d3)
                                           : hasExtractionChamber ? (d + d3)
                                           : d;
                        double tightW = totalDepth * scale;

                        // Front view geometry may be taller than just the main body (plenum on top, civil below)
                        double plenumHVal = (isFront && hasPlenum) ? (double)model.PlenumHeight : 0;
                        double civilHVal = (isFront && hasCivil) ? 2100 : 0;

                        double totalVerticalExtent = isFront ? (h + plenumHVal + civilHVal) : crossDim;
                        double tightH = totalVerticalExtent * scale;

                        double dimOffset = tightW * 0.04;
                        double extLineExt = tightW * 0.015;
                        double textH = Math.Max((crossDim * scale) * 0.025, tightW * 0.015);
                        double arrowSize = extLineExt * 1.5;

                        Point2D fullBL = new Point2D(vx - tightW / 2, vy - tightH / 2);
                        Point2D fullBR = new Point2D(vx + tightW / 2, vy - tightH / 2);

                        // Main body floor: shift up from the full geometry bottom by the civil block height
                        Point2D bl = new Point2D(fullBL.X, fullBL.Y + civilHVal * scale);
                        Point2D br = new Point2D(fullBR.X, fullBR.Y + civilHVal * scale);

                        // Main body top: for Front, always bl + h; for Top, unaffected by plenum/civil
                        Point2D tl = isFront
                            ? new Point2D(bl.X, bl.Y + h * scale)
                            : new Point2D(vx - tightW / 2, vy + tightH / 2);

                        double hY = bl.Y - dimOffset;

                        Debug.WriteLine($"[FRONT] view.X={vx}, view.Y={vy}, view.Height={view.Height}, view.Scale={scale}");
                        Debug.WriteLine($"[FRONT] model.H={h}, civilHVal={civilHVal}, totalVerticalExtent={totalVerticalExtent}, tightH={tightH}");
                        Debug.WriteLine($"[FRONT] fullBL.Y={fullBL.Y}, bl.Y={bl.Y}, tl.Y={tl.Y}");

                        double rightChamberStartX;

                        if (!hasExtractionChamber)
                        {
                            // No EC chamber at all — single segment, just D
                            sheet.Entities.Add(new Line(bl.X, bl.Y, bl.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(br.X, br.Y, br.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(bl.X, hY, br.X, hY) { LayerName = Dim });

                            AddArrowhead(sheet, new Point2D(bl.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(br.X, hY), new Vector2D(1, 0), arrowSize, Dim);

                            sheet.Entities.Add(new Text(
                                new Point3D((bl.X + br.X) / 2, hY - textH * 1.2, 0),
                                $"{d:F0}  DEPTH", textH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });

                            rightChamberStartX = br.X;
                        }
                        else if (isSemiDownDraft)
                        {
                            // Layout: [D3] + [D] + [D3]
                            Point2D boundary1 = new Point2D(bl.X + d3 * scale, bl.Y);
                            Point2D boundary2 = new Point2D(boundary1.X + d * scale, bl.Y);
                            rightChamberStartX = boundary2.X;

                            sheet.Entities.Add(new Line(bl.X, bl.Y, bl.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(boundary1.X, boundary1.Y, boundary1.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(boundary2.X, boundary2.Y, boundary2.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(br.X, br.Y, br.X, hY - extLineExt) { LayerName = Dim });

                            sheet.Entities.Add(new Line(bl.X, hY, boundary1.X, hY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(boundary1.X, hY, boundary2.X, hY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(boundary2.X, hY, br.X, hY) { LayerName = Dim });

                            AddArrowhead(sheet, new Point2D(bl.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(boundary1.X, hY), new Vector2D(1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(boundary1.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(boundary2.X, hY), new Vector2D(1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(boundary2.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(br.X, hY), new Vector2D(1, 0), arrowSize, Dim);

                            double ecLabelTextH = textH * 0.7;

                            sheet.Entities.Add(new Text(
                                new Point3D((bl.X + boundary1.X) / 2, hY - textH * 1.1, 0),
                                $"{d3:F0}", ecLabelTextH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });

                            sheet.Entities.Add(new Text(
                                new Point3D((bl.X + boundary1.X) / 2, hY - textH * 2.2, 0),
                                "EC WIDTH", ecLabelTextH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });

                            sheet.Entities.Add(new Text(
                                new Point3D((boundary1.X + boundary2.X) / 2, hY - textH * 1.1, 0),
                                $"{d:F0}", ecLabelTextH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });

                            sheet.Entities.Add(new Text(
                                new Point3D((boundary1.X + boundary2.X) / 2, hY - textH * 2.2, 0),
                                "DEPTH", ecLabelTextH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });

                            sheet.Entities.Add(new Text(
                                new Point3D((boundary2.X + br.X) / 2, hY - textH * 2.2, 0),
                                "EC WIDTH", ecLabelTextH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });
                        }
                        else
                        {
                            // Single-chamber layout: [D] + [D3]
                            Point2D boundary = new Point2D(bl.X + d * scale, bl.Y);
                            rightChamberStartX = boundary.X;

                            sheet.Entities.Add(new Line(bl.X, bl.Y, bl.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(boundary.X, boundary.Y, boundary.X, hY - extLineExt) { LayerName = Dim });
                            sheet.Entities.Add(new Line(br.X, br.Y, br.X, hY - extLineExt) { LayerName = Dim });

                            sheet.Entities.Add(new Line(bl.X, hY, boundary.X, hY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(boundary.X, hY, br.X, hY) { LayerName = Dim });

                            AddArrowhead(sheet, new Point2D(bl.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(boundary.X, hY), new Vector2D(1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(boundary.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(br.X, hY), new Vector2D(1, 0), arrowSize, Dim);

                            sheet.Entities.Add(new Text(
                                new Point3D((bl.X + boundary.X) / 2, hY - textH * 1.2, 0),
                                $"{d:F0}  DEPTH", textH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });

                            sheet.Entities.Add(new Text(
                                new Point3D((boundary.X + br.X) / 2, hY - textH * 1.2, 0),
                                $"{d3:F0}  EC WIDTH", textH, Text.alignmentType.BottomCenter)
                            { LayerName = Dim });
                        }

                        // ---- Height dimension (left side, vertical) ----
                        double vX = bl.X - dimOffset;
                        sheet.Entities.Add(new Line(bl.X, bl.Y, vX - extLineExt, bl.Y) { LayerName = Dim });
                        sheet.Entities.Add(new Line(tl.X, tl.Y, vX - extLineExt, tl.Y) { LayerName = Dim });
                        sheet.Entities.Add(new Line(vX, bl.Y, vX, tl.Y) { LayerName = Dim });

                        AddArrowhead(sheet, new Point2D(vX, bl.Y), new Vector2D(0, -1), arrowSize, Dim);
                        AddArrowhead(sheet, new Point2D(vX, tl.Y), new Vector2D(0, 1), arrowSize, Dim);

                        sheet.Entities.Add(new Text(
                            new Point3D(vX - textH * 1.2, (bl.Y + tl.Y) / 2, 0),
                            $"{crossDim:F0}  HEIGHT", textH, Text.alignmentType.MiddleRight)
                        { LayerName = Dim });

                        // ---- Extraction Chamber Height annotation ----
                        if (hasExtractionChamber)
                        {
                            if (isFront)
                            {
                                double echSheetH = ECh * scale;
                                double vX2 = br.X + dimOffset;

                                Point2D echBottom = new Point2D(br.X, bl.Y);
                                Point2D echTop = new Point2D(br.X, bl.Y + echSheetH);

                                sheet.Entities.Add(new Line(echBottom.X, echBottom.Y, vX2 + extLineExt, echBottom.Y) { LayerName = Dim });
                                sheet.Entities.Add(new Line(echTop.X, echTop.Y, vX2 + extLineExt, echTop.Y) { LayerName = Dim });
                                sheet.Entities.Add(new Line(vX2, echBottom.Y, vX2, echTop.Y) { LayerName = Dim });

                                AddArrowhead(sheet, new Point2D(vX2, echBottom.Y), new Vector2D(0, -1), arrowSize, Dim);
                                AddArrowhead(sheet, new Point2D(vX2, echTop.Y), new Vector2D(0, 1), arrowSize, Dim);

                                sheet.Entities.Add(new Text(
                                    new Point3D(vX2 + textH * 1.2, (echBottom.Y + echTop.Y) / 2, 0),
                                    $"{ECh:F0}  EC HEIGHT", textH, Text.alignmentType.MiddleLeft)
                                { LayerName = Dim });
                            }
                            else // Top
                            {
                                sheet.Entities.Add(new Text(
                                    new Point3D((rightChamberStartX + br.X) / 2, tl.Y + textH * 2.0, 0),
                                    $"EC HEIGHT: {ECh:F0}", textH, Text.alignmentType.BottomCenter)
                                { LayerName = Dim });
                            }
                        }

                        // ---- Plenum (FAS) height — Front view only ----
                        if (isFront && hasPlenum)
                        {
                            double plenumH = (double)model.PlenumHeight;
                            double plenumSheetH = plenumH * scale;

                            double vX3 = vX - dimOffset;

                            double plenumBottomY = tl.Y;
                            double plenumTopY = tl.Y + plenumSheetH;

                            sheet.Entities.Add(new Line(vX, plenumBottomY, vX3 - extLineExt, plenumBottomY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(vX, plenumTopY, vX3 - extLineExt, plenumTopY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(vX3, plenumBottomY, vX3, plenumTopY) { LayerName = Dim });

                            AddArrowhead(sheet, new Point2D(vX3, plenumBottomY), new Vector2D(0, -1), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(vX3, plenumTopY), new Vector2D(0, 1), arrowSize, Dim);

                            sheet.Entities.Add(new Text(
                                new Point3D(vX3 - textH * 1.2, (plenumBottomY + plenumTopY) / 2, 0),
                                $"{plenumH:F0}  PLENUM HEIGHT", textH * 0.8, Text.alignmentType.MiddleRight)
                            { LayerName = Dim });
                        }

                        // ---- Civil block height — Front view only, type "4" ----
                        if (isFront && hasCivil)
                        {
                            double civilH = 2100; // confirm exact property name
                            double civilSheetH = civilH * scale;

                            double vX4 = vX - dimOffset; // one step further left than main HEIGHT line

                            double civilTopY = bl.Y;               // civil top = main body floor
                            double civilBottomY = bl.Y - civilSheetH; // civil bottom = true ground

                            sheet.Entities.Add(new Line(bl.X, civilTopY, vX4 - extLineExt, civilTopY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(bl.X, civilBottomY, vX4 - extLineExt, civilBottomY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(vX4, civilTopY, vX4, civilBottomY) { LayerName = Dim });

                            AddArrowhead(sheet, new Point2D(vX4, civilTopY), new Vector2D(0, 1), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(vX4, civilBottomY), new Vector2D(0, -1), arrowSize, Dim);

                            sheet.Entities.Add(new Text(
                                new Point3D(vX4 - textH * 1.2, (civilTopY + civilBottomY) / 2, 0),
                                $"{civilH:F0}  CIVIL HEIGHT", textH * 0.8, Text.alignmentType.MiddleRight)
                            { LayerName = Dim });
                        }
                    }
                    else if (view.BlockName == "Left")
                    {

                        double plenumHVal = hasPlenum ? (double)model.PlenumHeight : 0;
                        double civilHVal = hasCivil ? 2100 : 0;

                        double totalVerticalExtent = h + plenumHVal + civilHVal;
                        double tightW = w * scale;
                        double tightH = totalVerticalExtent * scale;

                        double dimOffset = tightW * 0.04;
                        double extLineExt = tightW * 0.015;
                        double textH = Math.Max((h * scale) * 0.025, tightW * 0.015);
                        double arrowSize = extLineExt * 1.5;

                        Point2D fullBL = new Point2D(vx - tightW / 2, vy - tightH / 2);
                        Point2D fullBR = new Point2D(vx + tightW / 2, vy - tightH / 2);

                        Point2D bl = new Point2D(fullBL.X, fullBL.Y + civilHVal * scale);
                        Point2D br = new Point2D(fullBR.X, fullBR.Y + civilHVal * scale);
                        Point2D tl = new Point2D(bl.X, bl.Y + h * scale);


                        Debug.WriteLine($"[LEFT] view.X={vx}, view.Y={vy}, view.Height={view.Height}, view.Scale={scale}");
                        Debug.WriteLine($"[LEFT] model.H={h}, civilHVal={civilHVal}, totalVerticalExtent={totalVerticalExtent}, tightH={tightH}");
                        Debug.WriteLine($"[LEFT] fullBL.Y={fullBL.Y}, bl.Y={bl.Y}, tl.Y={tl.Y}");

                        double hY = bl.Y - dimOffset;
                        sheet.Entities.Add(new Line(bl.X, bl.Y, bl.X, hY - extLineExt) { LayerName = Dim });
                        sheet.Entities.Add(new Line(br.X, br.Y, br.X, hY - extLineExt) { LayerName = Dim });
                        sheet.Entities.Add(new Line(bl.X, hY, br.X, hY) { LayerName = Dim });

                        AddArrowhead(sheet, new Point2D(bl.X, hY), new Vector2D(-1, 0), arrowSize, Dim);
                        AddArrowhead(sheet, new Point2D(br.X, hY), new Vector2D(1, 0), arrowSize, Dim);

                        sheet.Entities.Add(new Text(
                            new Point3D((bl.X + br.X) / 2, hY - textH * 1.2, 0),
                            $"{w:F0}  WIDTH", textH, Text.alignmentType.BottomCenter)
                        { LayerName = Dim });

                        double vX = bl.X - dimOffset;
                        sheet.Entities.Add(new Line(bl.X, bl.Y, vX - extLineExt, bl.Y) { LayerName = Dim });
                        sheet.Entities.Add(new Line(tl.X, tl.Y, vX - extLineExt, tl.Y) { LayerName = Dim });
                        sheet.Entities.Add(new Line(vX, bl.Y, vX, tl.Y) { LayerName = Dim });

                        AddArrowhead(sheet, new Point2D(vX, bl.Y), new Vector2D(0, -1), arrowSize, Dim);
                        AddArrowhead(sheet, new Point2D(vX, tl.Y), new Vector2D(0, 1), arrowSize, Dim);

                        sheet.Entities.Add(new Text(
                            new Point3D(vX - textH * 1.2, (bl.Y + tl.Y) / 2, 0),
                            $"{h:F0}  HEIGHT", textH, Text.alignmentType.MiddleRight)
                        { LayerName = Dim });

                        // ---- Civil block height on Left view too ----
                        if (hasCivil)
                        {
                            double civilSheetH = civilHVal * scale;
                            double vX4 = vX - dimOffset;

                            double civilTopY = bl.Y;
                            double civilBottomY = bl.Y - civilSheetH;

                            sheet.Entities.Add(new Line(bl.X, civilTopY, vX4 - extLineExt, civilTopY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(bl.X, civilBottomY, vX4 - extLineExt, civilBottomY) { LayerName = Dim });
                            sheet.Entities.Add(new Line(vX4, civilTopY, vX4, civilBottomY) { LayerName = Dim });

                            AddArrowhead(sheet, new Point2D(vX4, civilTopY), new Vector2D(0, 1), arrowSize, Dim);
                            AddArrowhead(sheet, new Point2D(vX4, civilBottomY), new Vector2D(0, -1), arrowSize, Dim);

                            sheet.Entities.Add(new Text(
                                new Point3D(vX4 - textH * 1.2, (civilTopY + civilBottomY) / 2, 0),
                                $"{civilHVal:F0}  CIVIL HEIGHT", textH * 0.8, Text.alignmentType.MiddleRight)
                            { LayerName = Dim });
                        }
                    }
                }
            }
        }

        private void AddArrowhead(MySheet sheet, Point2D tip, Vector2D direction, double size, string layer)
        {
            const string Dim = "Dimension";
            Vector2D dir = direction;
            dir.Normalize();
            Vector2D perp = new Vector2D(-dir.Y, dir.X);

            Point2D back = tip - dir * size;
            Point2D p1 = back + perp * (size * 0.35);
            Point2D p2 = back - perp * (size * 0.35);

            var arrow = devregion.CreatePolygon(Plane.XY, new Point2D[] { tip, p1, p2 });
            arrow.ColorMethod = colorMethodType.byLayer;
            arrow.LayerName = Dim;
            sheet.Entities.Add(arrow);
        }

        public List<PanelDetails> GetAllPanels(string enquiryCode)
        {
            var panels = _DbContext.PanelDetails.Where(c => c.IsActive == true && c.SalesNo == enquiryCode)
                .Select(c => new PanelDetails
                {
                    PanelID = c.PanelID,
                    PanelPosition = c.PanelPosition,
                    PanelWeight = c.PanelWeight

                }).ToList();

            return panels;
        }
        public PaintBoothclass MetalBaffle(double FilterframeHeight)
        {
            DesignDocument drawing = new DesignDocument();
            drawing.Units = linearUnitsType.Millimeters;
            double W = 66;
            double H = 20;
            double ST = 1.2; //SheetThickness
            double offsetDistance = 36;
            double startPoint = 11.5;
            double ExtrudeHeight = FilterframeHeight;

            Material mat = new Material(Materials);
            List<double> partWeights = new List<double>();

            // Helper function to calculate weight
            double CalculateWeight(Brep brep, Material mat)
            {
                brep.Regen(0.1);
                double mass = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
                return Math.Abs(Math.Round(mass, 3));
            }

            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(startPoint,0),
                new Point3D(startPoint,H),
                new Point3D((W+startPoint),H),
                new Point3D((W+startPoint),0),
                new Point3D(((W+startPoint)-ST),0),
                new Point3D(((W+startPoint)-ST),H-ST),
                new Point3D(ST+startPoint,H-ST),
                new Point3D(ST+startPoint,0),
                new Point3D(startPoint,0),
            });
            Brep brep = rail.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0);

            double railWeight = CalculateWeight(brep, mat);
            partWeights.Add(railWeight);


            drawing.Entities.Add(brep, Color.Yellow);
            int i = 1;
            LinearPath railClone = rail;
            LinearPath oppositeRail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0),
                new Point3D(0,H),
                new Point3D(W,H),
                new Point3D(W,0),
                new Point3D(W-ST,0),
                new Point3D(W-ST,H-ST),
                new Point3D(ST,H-ST),
                new Point3D(ST,0),
                new Point3D(0,0),
            });
            while (i < 6)
            {
                railClone = (LinearPath)railClone.Clone();
                railClone.Translate(W + offsetDistance, 0, 0);
                Brep brepClone = railClone.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0); // Extrude each clone
                drawing.Entities.Add(brepClone, Color.White);
                // drawing.Entities.Add(railClone, Color.Yellow);
                i++;
            }
            i = 1;
            Point3D centerPoint = new Point3D(W / 2, 0); // Adjusted to be in the middle of the shape
            oppositeRail.Rotate(Math.PI, Vector3D.AxisZ, centerPoint);
            oppositeRail.Translate(54.5, 0);
            Brep oppositeBrepClone = oppositeRail.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0); // Extrude each opposite clone
            double oppositeBreplWeight = CalculateWeight(oppositeBrepClone, mat);
            partWeights.Add(oppositeBreplWeight);

            drawing.Entities.Add(oppositeBrepClone, Color.White);
            while (i < 5)
            {
                oppositeRail = (LinearPath)oppositeRail.Clone();
                oppositeRail.Translate(W + offsetDistance, 0);
                oppositeBrepClone = oppositeRail.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0); // Extrude each opposite clone
                drawing.Entities.Add(oppositeBrepClone, Color.White);

                i++;
            }

            // Define the Bottom Plate with dimensions 572.6 x 40 and position it 150 units from the bottom
            double bottomPlateWidth = 600;
            double bottomPlateHeight = 40;
            double bottomPlateYPosition = 150;

            LinearPath bottomPlate = new LinearPath(new Point3D[]
            {
                new Point3D(0, 0),
                new Point3D(bottomPlateWidth, 0),
                new Point3D(bottomPlateWidth, bottomPlateHeight),
                new Point3D(0, bottomPlateHeight),
                new Point3D(0, 0)
            });

            bottomPlate.Translate(0, 20, bottomPlateYPosition);
            // Define the hole radius and positions (20 units from each side)
            double holeRadius = 5; // Adjust hole radius as needed
            double holeOffsetFromEdge = 20;

            // Left Hole position
            Circle leftHole = new Circle(new Point3D(holeOffsetFromEdge, bottomPlateHeight, bottomPlateYPosition), holeRadius);

            // Right Hole position
            Circle rightHole = new Circle(new Point3D(bottomPlateWidth - holeOffsetFromEdge, bottomPlateHeight, bottomPlateYPosition), holeRadius);
            //// Extrude the Bottom Plate
            Brep bottomPlateBrep = bottomPlate.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0); // Extrude by 5 units
            double bottomPlateBrepWeight = CalculateWeight(bottomPlateBrep, mat);
            partWeights.Add(bottomPlateBrepWeight);

            Brep leftHoleBrep = leftHole.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
            double leftHoleBrepWeight = CalculateWeight(leftHoleBrep, mat);
            partWeights.Add(leftHoleBrepWeight);

            Brep rightHoleBrep = rightHole.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
            double rightHoleBreplWeight = CalculateWeight(rightHoleBrep, mat);
            partWeights.Add(rightHoleBreplWeight);

            // Add the Bottom Plate with holes to the drawing

            drawing.Entities.Add(leftHoleBrep, Color.Green);
            drawing.Entities.Add(rightHoleBrep, Color.Green);
            drawing.Entities.Add(bottomPlateBrep, Color.Green);

            LinearPath bottomPlateClone = (LinearPath)bottomPlate.Clone();
            // Adjust the Y translation for the cloned Bottom Plate
            bottomPlateClone.Translate(0, 0, 612);


            // Clone and translate the hole circles to the second Bottom Plate position
            Circle leftHoleClone = (Circle)leftHole.Clone();
            leftHoleClone.Translate(0, 0, 612);
            Circle rightHoleClone = (Circle)rightHole.Clone();
            rightHoleClone.Translate(0, 0, 612);

            // Extrude and subtract holes from the second Bottom Plate
            Brep leftHoleBrep2 = leftHoleClone.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
            Brep rightHoleBrep2 = rightHoleClone.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
            // Extrude and add the second Bottom Plate
            Brep bottomPlateBrep2 = bottomPlateClone.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0); // Extrude by 5 units
            double bottomPlateBrep2Weight = CalculateWeight(bottomPlateBrep2, mat);
            partWeights.Add(bottomPlateBrep2Weight);
            double totalWeight = 0;
            foreach (var weight in partWeights)
            {
                totalWeight += weight;

            }
            totalWeightOfbaffle = totalWeight;

            drawing.Entities.Add(leftHoleClone, Color.Green);
            drawing.Entities.Add(rightHoleClone, Color.Green);

            drawing.Entities.Add(bottomPlateBrep2, Color.Green);



            #region Write file         
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

            string dwgFilePath = path + "/Bullows Panel Drawing/" + "MetalBaffle" + DateTime.Now.ToString("hh-mm") + ".dwg";

            // Save as DWG
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            //ExportAllViewsWithSection(drawing, path);

            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };
            #endregion
        }
        public PaintBoothclass FilterFrame1(double filterHeight, PaintBoothModel model)
        {
            // Initialize drawing and dimensions
            drawing = new DesignDocument();
            drawing.Units = linearUnitsType.Millimeters;

            double frameWidth = 600;
            double frameHeight = filterHeight;
            double railWidth = 30;
            double railHeight = 40;
            double sheetThickness = 1.2;
            double holeRadius = 7 / 2.0;

            Material mat = new Material(Materials); // Define material properties


            // Helper function to calculate weight
            double CalculateWeight(Solid frame, Material mat)
            {
                frame.Regen(0.1);
                double mass = frame.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
                return Math.Abs(Math.Round(mass, 3));
            }

            // Frame Outline
            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(0, frameWidth, 0),
                new Point3D(0, frameWidth, frameHeight),
                new Point3D(0, 0, frameHeight),
                new Point3D(0, 0, 0)
            });
            drawing.Entities.Add(rail);

            #region Frame Section and Weight Calculation
            // Frame Section for Extrusion
            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(railWidth, 0, 0),
                new Point3D(railWidth, 0, sheetThickness),
                new Point3D(sheetThickness, 0, sheetThickness),
                new Point3D(sheetThickness, 0, railHeight),
                new Point3D(0, 0, railHeight),
            });

            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, sheetThickness);

            // Calculate weight for the frame rail
            double railWeight = CalculateWeight(frame, mat);
            totalWeight += railWeight; // Add rail weight to total weight

            // Add holes and calculate weight reduction for each hole
            double totalHoleWeight = 0;

            List<devregion> holes = new List<devregion>
    {
        devregion.CreateCircle(Plane.XY, new Point3D(frameWidth / 2, railHeight / 2), holeRadius),
        devregion.CreateCircle(Plane.XY, new Point3D((frameWidth / 2) - 285, railHeight / 2), holeRadius),
        devregion.CreateCircle(Plane.XY, new Point3D((frameWidth / 2) + 285, railHeight / 2), holeRadius),
        devregion.CreateCircle(Plane.XY, new Point3D(railWidth / 2, frameHeight / 3), holeRadius),
        devregion.CreateCircle(Plane.XY, new Point3D(railWidth / 2, frameHeight - frameHeight / 3), holeRadius)
    };

            foreach (var hole in holes)
            {
                Solid tempFrame = (Solid)frame.Clone();
                tempFrame.ExtrudeRemove(hole, 50, 0);
                double holeWeight = CalculateWeight(tempFrame, mat) - railWeight; // Calculate weight reduction
                totalHoleWeight += holeWeight;
            }

            totalWeight -= totalHoleWeight; // Subtract hole weights from total weight
            #endregion

            #region Save Drawing and Return
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

            string dwgFilePath = path + "/Bullows Panel Drawing/" + "FilterFrame" + DateTime.Now.ToString("hh-mm") + ".dwg";

            // Save as DWG
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };
            #endregion
        }
        //Enclosed with FAS
        public PaintBoothclass EnclosedWithFAS(PaintBoothModel model)
        {
            // Initialize drawing and dimensions
            drawing = new DesignDocument();
            drawing.Units = linearUnitsType.Millimeters;

            double frameWidth = 600;
            double frameHeight = 1000;

            var rectangle = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,PanelHeight),
                new Point3D(frameWidth,0,PanelHeight),
                new Point3D(frameWidth,frameHeight,PanelHeight),
                new Point3D(0,frameHeight,PanelHeight),
            });

            drawing.Entities.Add(rectangle, Color.Blue);


            #region Save Drawing and Return
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

            string dwgFilePath = path + "/Bullows Panel Drawing/" + "FilterFrame" + DateTime.Now.ToString("hh-mm") + ".dwg";

            // Save as DWG
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };
            #endregion
        }
        public void StandardFrame(int scaleFator, DesignDocument drawing)
        {

            double UntrimmedWidth = 625 * scaleFator;
            double UntrimmedHeight = 450 * scaleFator;
            double trimmedWidth = 594 * scaleFator;
            double trimmedHeight = 420 * scaleFator;

            double outerX0 = 0, outerX1 = UntrimmedWidth, outerY0 = -300, outerY1 = UntrimmedHeight - 300;

            double iX0 = (UntrimmedWidth - trimmedWidth) / 2;
            double iY0 = (UntrimmedHeight - trimmedHeight) / 2;

            double innerX0 = iX0, innerX1 = trimmedWidth + iX0, innerY0 = iY0 - 300, innerY1 = trimmedHeight + iY0 - 300;
            var outerRectangle = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                new Point2D(outerX0,outerY0),
                new Point2D(outerX1,outerY0),
                new Point2D(outerX1,outerY1),
                new Point2D(outerX0,outerY1),

            });
            drawing.Entities.Add(outerRectangle, Color.Yellow);

            var innerRectangle = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                new Point2D(innerX0,innerY0),
                new Point2D(innerX1,innerY0),
                new Point2D(innerX1,innerY1),
                new Point2D(innerX0,innerY1),

            });
            drawing.Entities.Add(innerRectangle, Color.Yellow);

            #region TitleBox
            double titleBoxWidth = 185 * scaleFator;
            double titleBoxHeight = 65 * scaleFator;
            double titleBoxX = innerX1 - titleBoxWidth, titleBoxY = innerY0 + titleBoxHeight;

            var titleBox = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                new Point2D(titleBoxX,titleBoxY),
                new Point2D(innerX1,titleBoxY),
                new Point2D(innerX1,innerY0),
                new Point2D(titleBoxX,innerY0),

            });
            drawing.Entities.Add(titleBox, Color.Yellow);
            drawing.Entities.AddRange(new Entity[]
            {
                  //left
                new Line(titleBoxX,innerY0+(10*scaleFator),titleBoxX+(25*scaleFator),innerY0+(10*scaleFator)),
                new Line(titleBoxX,innerY0+(20 * scaleFator),innerX1,innerY0+(20*scaleFator)),
                new Line(titleBoxX+(130 * scaleFator),titleBoxY,titleBoxX+(130 * scaleFator),innerY0),

                new Line(titleBoxX+(25 * scaleFator),innerY0+(20 * scaleFator),titleBoxX+(25 * scaleFator),innerY0),

                new Text(titleBoxX+((25/2)*scaleFator),innerY0+((20-5)*scaleFator),0,"SCALE",(3*scaleFator),Text.alignmentType.MiddleCenter),
                new Text(titleBoxX+((25+(25/2))*scaleFator),innerY0+((20-5)*scaleFator),0,"TITLE",(3*scaleFator),Text.alignmentType.MiddleCenter),
                //Right
                //new Line(titleBoxX+130+10,innerY0+20,titleBoxX+130+10,titleBoxY),
                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7)*scaleFator,innerX1,innerY0+(20+7)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+(7/2))*scaleFator),0,"APPROVED BY",(2*scaleFator),Text.alignmentType.MiddleCenter),

                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*2)*scaleFator,innerX1,innerY0+(20+7*2)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7+(7/2))*scaleFator),0,"STANDARD",(2*scaleFator),Text.alignmentType.MiddleCenter),



                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*3)*scaleFator,innerX1,innerY0+(20+7*3)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*2+(7/2))*scaleFator),0,"CHECKED BY",(2*scaleFator),Text.alignmentType.MiddleCenter),


                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*4)*scaleFator,innerX1,innerY0+(20+7*4)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*3+(7/2))*scaleFator),0,"DRAWN",(2*scaleFator),Text.alignmentType.MiddleCenter),

                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*5)*scaleFator,innerX1,innerY0+(20+7*5)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*4+(7/2))*scaleFator),0,"DESIGNED",(2*scaleFator),Text.alignmentType.MiddleCenter),

                new Text(titleBoxX+(130+20+12.5)*scaleFator,titleBoxY-(5*scaleFator),0,"NAME",(2*scaleFator),Text.alignmentType.MiddleCenter),
                new Text(titleBoxX+(130+20+25+5)*scaleFator,titleBoxY-(5*scaleFator),0,"DATE",(2*scaleFator),Text.alignmentType.MiddleCenter),

              //  new Line(titleBoxX+(130 * scaleFator),innerY0+20+7*5,innerX1,innerY0+20+7*5),
                new Line(innerX1-(10*scaleFator),innerY0+(20*scaleFator),innerX1-(10 * scaleFator),titleBoxY),
                new Line(innerX1-((10+25)*scaleFator),innerY0+(20 * scaleFator),innerX1-((10+25)*scaleFator),titleBoxY),


                new Text(titleBoxX+(130+5)*scaleFator,innerY0 + (20 - 5)*scaleFator,0,"DRAWING NO.",(2*scaleFator),Text.alignmentType.MiddleLeft),


            }, Color.Yellow);
            drawing.Entities.Add(new Text(titleBoxX + (10 * scaleFator), titleBoxY - (10 * scaleFator), "Bullows Paint Equipment Pvt.Ltd", (5 * scaleFator)), Color.Red);
            string ApprovedBy = "ABCD";
            string CreatedBy = "ABCD";
            string checkedBy = "ABCD";
            string drawnBy = "ABCD";


            drawing.Entities.AddRange(new Entity[]
            {
               new Text(titleBoxX+(130+20+12.5)*scaleFator,innerY0+((20+(7/2))*scaleFator),0,ApprovedBy,(2*scaleFator),Text.alignmentType.MiddleCenter),

                new Text(titleBoxX+(130+20+12.5)*scaleFator,innerY0+((20+7*2+(7/2))*scaleFator),0,checkedBy,(2*scaleFator),Text.alignmentType.MiddleCenter),
                  new Text(titleBoxX+(130+20+12.5)*scaleFator,innerY0+((20+7*3+(7/2))*scaleFator),0,drawnBy,(2*scaleFator),Text.alignmentType.MiddleCenter),
            }, Color.Green);

            #region BOM         
            //var BOMBox = devregion.CreatePolygon(Plane.XY, new Point2D[]
            //{
            //    new Point2D(titleBoxX,titleBoxY),
            //    new Point2D(titleBoxX,titleBoxY+15),
            //    new Point2D(innerX1,titleBoxY+15),
            //    new Point2D(innerX1,titleBoxY),

            //});
            //drawing.Entities.Add(BOMBox, Color.White);
            //List<double> XCoordinate = new List<double>()
            //{
            //    titleBoxX+20+10,
            //    titleBoxX+20+40+10,
            //    titleBoxX+20+40+80+10,
            //    titleBoxX+20+40+80+40+10,
            //    titleBoxX+20+40+80+40+60+10,
            //    titleBoxX+20+40+80+60+40+40+10,
            //    titleBoxX+20+40+80+60+40+40+40+10,
            //};

            //drawing.Entities.AddRange(new Entity[]
            //{
            //      new Text(titleBoxX+10,(titleBoxY+15-(15/2)),0,"SR.NO",3,Text.alignmentType.MiddleCenter),
            //      new Text(XCoordinate[0],(titleBoxY+15-(15/2)),0,"PART NO",3,Text.alignmentType.MiddleCenter),
            //       new Text(XCoordinate[1],(titleBoxY+15-(15/2)),0,"PART NAME",3,Text.alignmentType.MiddleLeft),
            //        new Text(XCoordinate[2],(titleBoxY+15-(15/2)),0,"MATERIAL",3,Text.alignmentType.MiddleLeft),
            //       new Text(XCoordinate[3],(titleBoxY+15-(15/2)),0,"T.SPECIFICATION",3,Text.alignmentType.MiddleLeft),
            //       new Text(XCoordinate[4], (titleBoxY+15-(15/2)),0,"QUANTITY",3,Text.alignmentType.MiddleLeft),
            //       new Text(XCoordinate[5], (titleBoxY+15-(15/2)),0,"UMO",3,Text.alignmentType.MiddleCenter),
            //      new Text(XCoordinate[6], (titleBoxY+15-(15/2)),0,"WEIGHT",3,Text.alignmentType.MiddleCenter),
            //}, Color.Pink);
            //double y = 0;
            //for (int i = 0; i < 10; i++)
            //{
            //    y = titleBoxY + (15 * (i + 2));
            //    Line l = new Line(titleBoxX, titleBoxY + (15 * (i + 2)), innerX1, titleBoxY + (15 * (i + 2)));

            //    drawing.Entities.Add(l, Color.White);

            //}
            //drawing.Entities.AddRange(new Entity[]
            //{    new Line(titleBoxX,titleBoxY,titleBoxX,y),
            //     new Line(XCoordinate[0] - 10,y,XCoordinate[0]-10,titleBoxY),  //sr.no           
            //     new Line(XCoordinate[1]-10,y,XCoordinate[1]-10,titleBoxY),//part no
            //     new Line(XCoordinate[2] - 10,y,XCoordinate[2] - 10,titleBoxY),//part name
            //     new Line(XCoordinate[3] - 10,y,XCoordinate[3] - 10,titleBoxY),//material
            //     new Line(XCoordinate[4] - 10,y,XCoordinate[4] - 10,titleBoxY),//SPECIFICATION
            //     new Line(XCoordinate[5] - 10,y,XCoordinate[5]-10,titleBoxY),//QUANTITY
            //     new Line(XCoordinate[6] - 10,y,XCoordinate[6] - 10,titleBoxY),
            //}, Color.White); 
            #endregion
            #endregion

            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/Standard Frame"))
                Directory.CreateDirectory(path + "/Standard Frame");

            string dwgFilePath = path + "/Standard Frame/" + "Frame" + DateTime.Now.ToString("hh-mm") + ".dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
        }
        public PaintBoothclass StandardFrameWithoutDrawing(int scaleFator, string enquirycode)
        {
            drawing = new();
            var panels = GetAllPanels(enquirycode);
            double UntrimmedWidth = 625 * scaleFator;
            double UntrimmedHeight = 450 * scaleFator;
            double trimmedWidth = 594 * scaleFator;
            double trimmedHeight = 420 * scaleFator;

            double outerX0 = 0, outerX1 = UntrimmedWidth, outerY0 = 0, outerY1 = UntrimmedHeight;

            double iX0 = (UntrimmedWidth - trimmedWidth) / 2;
            double iY0 = (UntrimmedHeight - trimmedHeight) / 2;

            double innerX0 = iX0, innerX1 = trimmedWidth + iX0, innerY0 = iY0, innerY1 = trimmedHeight + iY0;
            var outerRectangle = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                new Point2D(outerX0,outerY0),
                new Point2D(outerX1,outerY0),
                new Point2D(outerX1,outerY1),
                new Point2D(outerY0,outerY1),

            });
            drawing.Entities.Add(outerRectangle, Color.Yellow);

            var innerRectangle = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                new Point2D(innerX0,innerY0),
                new Point2D(innerX1,innerY0),
                new Point2D(innerX1,innerY1),
                new Point2D(innerX0,innerY1),

            });
            drawing.Entities.Add(innerRectangle, Color.Yellow);

            #region TitleBox
            double titleBoxWidth = 185 * scaleFator;
            double titleBoxHeight = 65 * scaleFator;
            double titleBoxX = innerX1 - titleBoxWidth, titleBoxY = innerY0 + titleBoxHeight;
            double nameOfFirmX = titleBoxX + (130 * scaleFator);
            double scaleX = titleBoxX + 25 * scaleFator;
            double scaleY = titleBoxY + 20 * scaleFator;
            var titleBox = devregion.CreatePolygon(Plane.XY, new Point2D[]//titleBoxX-left top corner of title box,innerX1-right bottom corner of titlebox
            {
                new Point2D(titleBoxX,titleBoxY),
                new Point2D(innerX1,titleBoxY),
                new Point2D(innerX1,innerY0),
                new Point2D(titleBoxX,innerY0),

            });

            drawing.Entities.Add(titleBox, Color.Yellow);
            drawing.Entities.AddRange(new Entity[]
            {
                  //left
                new Line(titleBoxX,innerY0+(10*scaleFator),titleBoxX+(25*scaleFator),innerY0+(10*scaleFator)),
                new Line(titleBoxX,innerY0+(20 * scaleFator),innerX1,innerY0+(20*scaleFator)),
                new Line(titleBoxX+(130 * scaleFator),titleBoxY,titleBoxX+(130 * scaleFator),innerY0),

                new Line(titleBoxX+(25 * scaleFator),innerY0+(20 * scaleFator),titleBoxX+(25 * scaleFator),innerY0),
                new Text(titleBoxX+(10*scaleFator),titleBoxY-(10*scaleFator),"NAME OF THE FIRM",4),
                new Text(titleBoxX+((25/2)*scaleFator),innerY0+((20-5)*scaleFator),0,"SCALE",4,Text.alignmentType.MiddleCenter),
                new Text(titleBoxX+((25+(25/2))*scaleFator),innerY0+((20-5)*scaleFator),0,"TITLE",4,Text.alignmentType.MiddleCenter),
                //Right
                //new Line(titleBoxX+130+10,innerY0+20,titleBoxX+130+10,titleBoxY),
                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7)*scaleFator,innerX1,innerY0+(20+7)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+(7/2))*scaleFator),0,"APPROVED",2,Text.alignmentType.MiddleCenter),

                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*2)*scaleFator,innerX1,innerY0+(20+7*2)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7+(7/2))*scaleFator),0,"STANDARD",2,Text.alignmentType.MiddleCenter),



                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*3)*scaleFator,innerX1,innerY0+(20+7*3)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*2+(7/2))*scaleFator),0,"CHECKED",2,Text.alignmentType.MiddleCenter),


                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*4)*scaleFator,innerX1,innerY0+(20+7*4)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*3+(7/2))*scaleFator),0,"DRAWN",2,Text.alignmentType.MiddleCenter),

                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*5)*scaleFator,innerX1,innerY0+(20+7*5)*scaleFator),
                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*4+(7/2))*scaleFator),0,"DESIGNED",2,Text.alignmentType.MiddleCenter),

                new Text(titleBoxX+(130+20+12.5)*scaleFator,titleBoxY-(5*scaleFator),0,"NAME",2,Text.alignmentType.MiddleCenter),
                new Text(titleBoxX+(130+20+25+5)*scaleFator,titleBoxY-(5*scaleFator),0,"DATE",2,Text.alignmentType.MiddleCenter),

              //  new Line(titleBoxX+(130 * scaleFator),innerY0+20+7*5,innerX1,innerY0+20+7*5),
                new Line(innerX1-(10*scaleFator),innerY0+(20*scaleFator),innerX1-(10 * scaleFator),titleBoxY),
                new Line(innerX1-((10+25)*scaleFator),innerY0+(20 * scaleFator),innerX1-((10+25)*scaleFator),titleBoxY),


                new Text(titleBoxX+(130+5)*scaleFator,innerY0 + (20 - 5)*scaleFator,0,"DRAWING NO.",4,Text.alignmentType.MiddleLeft),


            }, Color.Yellow);

            #region BOM old        
            var BOMBox = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                new Point2D(titleBoxX,titleBoxY),
                new Point2D(titleBoxX,titleBoxY+15),
                new Point2D(innerX1,titleBoxY+15),
                new Point2D(innerX1,titleBoxY),

            });
            drawing.Entities.Add(BOMBox);
            List<double> XCoordinate = new List<double>()
            {
                (titleBoxX+20+10),
                (titleBoxX+20+40+10),
                (titleBoxX+20+40+80+10),
                (titleBoxX+20+40+80+40+10),
                (titleBoxX+20+40+80+40+60+10),
                (titleBoxX+20+40+80+60+40+40+10),
                (titleBoxX+20+40+80+60+40+40+40+10),
            };

            drawing.Entities.AddRange(new Entity[]
            {
                  new Text(titleBoxX+10,(titleBoxY+15-(15/2)),0,"SR.NO",3,Text.alignmentType.MiddleCenter),
                  new Text(XCoordinate[0],(titleBoxY+15-(15/2)),0,"PART NO",3,Text.alignmentType.MiddleCenter),
                   new Text(XCoordinate[1],(titleBoxY+15-(15/2)),0,"PART NAME",3,Text.alignmentType.MiddleLeft),
                    new Text(XCoordinate[2],(titleBoxY+15-(15/2)),0,"MATERIAL",3,Text.alignmentType.MiddleLeft),
                   new Text(XCoordinate[3],(titleBoxY+15-(15/2)),0,"T.SPECIFICATION",3,Text.alignmentType.MiddleLeft),
                   new Text(XCoordinate[4], (titleBoxY+15-(15/2)),0,"QUANTITY",3,Text.alignmentType.MiddleLeft),
                   new Text(XCoordinate[5], (titleBoxY+15-(15/2)),0,"UMO",3,Text.alignmentType.MiddleCenter),
                  new Text(XCoordinate[6], (titleBoxY+15-(15/2)),0,"WEIGHT",3,Text.alignmentType.MiddleCenter),
            });
            double y = 0;
            for (int i = 0; i < panels.Count; i++)
            {
                y = titleBoxY + (15 * (i + 2));
                Line l = new Line(titleBoxX, titleBoxY + (15 * (i + 2)), innerX1, titleBoxY + (15 * (i + 2)));
                drawing.Entities.Add(l);
                //Passed value to BOM
                drawing.Entities.AddRange(new Entity[]
                {
                new Text(titleBoxX+10,(y-(15/2)),0,(i+1).ToString(),3,Text.alignmentType.MiddleCenter),
                new Text(XCoordinate[0],(y-(15/2)),0,"PART NO",3,Text.alignmentType.MiddleCenter),
                new Text(XCoordinate[1],(y-(15/2)),0,panels[i].PanelPosition,3,Text.alignmentType.MiddleLeft),
                new Text(XCoordinate[2],(y-(15/2)),0,"MATERIAL",3,Text.alignmentType.MiddleLeft),
                new Text(XCoordinate[3],(y-(15/2)),0,"T.SPECIFICATION",3,Text.alignmentType.MiddleLeft),
                new Text(XCoordinate[4], (y-(15/2)),0,"QUANTITY",3,Text.alignmentType.MiddleLeft),
                new Text(XCoordinate[5], (y-(15/2)),0,"UMO",3,Text.alignmentType.MiddleCenter),
                new Text(XCoordinate[6], (y-(15/2)),0,panels[i].PanelWeight.ToString(),3,Text.alignmentType.MiddleCenter),
                });

            }
            drawing.Entities.AddRange(new Entity[]
            {    new Line(titleBoxX,titleBoxY,titleBoxX,y),
                 new Line(XCoordinate[0] - 10,y,XCoordinate[0]-10,titleBoxY),  //sr.no           
                 new Line(XCoordinate[1]-10,y,XCoordinate[1]-10,titleBoxY),//part no
                 new Line(XCoordinate[2] - 10,y,XCoordinate[2] - 10,titleBoxY),//part name
                 new Line(XCoordinate[3] - 10,y,XCoordinate[3] - 10,titleBoxY),//material
                 new Line(XCoordinate[4] - 10,y,XCoordinate[4] - 10,titleBoxY),//SPECIFICATION
                 new Line(XCoordinate[5] - 10,y,XCoordinate[5]-10,titleBoxY),//QUANTITY
                 new Line(XCoordinate[6] - 10,y,XCoordinate[6] - 10,titleBoxY),
            });


            #endregion

            #endregion

            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/Standard Frame"))
                Directory.CreateDirectory(path + "/Standard Frame");

            string dwgFilePath = path + "/Standard Frame/" + "Frame" + DateTime.Now.ToString("hh-mm") + ".dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath

            };
        }

        #region Component entry doors
        public PaintBoothclass ComponantryEntrySideDoor(DoorDimensionsModel doorModel, PaintBoothModel model, int side)
        {
            try
            {
                drawing = new();
                drawing.Units = linearUnitsType.Millimeters;

                #region Layers
                drawing.Layers.Add(new Layer("DoorFrame", Color.BurlyWood));
                drawing.Layers.Add(new Layer("MetalSheet", Color.FloralWhite));
                drawing.Layers.Add(new Layer("MainDoorFrame", Color.DarkMagenta));
                drawing.Layers.Add(new Layer("HingeBackPlate", Color.Bisque));

                #endregion

                #region Hinges and handles blocks
                var path = _configuration["StpFilePathConfig:DrawingPath"].ToString();

                string hingeStepPath =
                    doorModel.TypeOfHinges == "Ball Bearing Hinges"
                        ? Path.Combine(path, "Ball_Bearing_Door_Hinge.step")
                        : Path.Combine(path, "Fabricated_Hing-1.step");

                string handleStepPath = Path.Combine(path, "Handel.step"); //Sliding Door Assembly - DESIGN - 001
                string slidingHandleStepPath = Path.Combine(path, "Sliding_Door_Assembly_DESIGN.step");

                List<string> hingeBlocks = AddStepBlocksToDrawing(drawing, hingeStepPath);
                List<string> handleBlocks = AddStepBlocksToDrawing(drawing, handleStepPath);
                List<string> slidingHandleBlocks = AddStepBlocksToDrawing(drawing, slidingHandleStepPath);

                string hingeBlockName;

                if (doorModel.TypeOfHinges == "Ball Bearing Hinges")
                {
                    hingeBlockName = drawing.Blocks.Select(b => b.Name).FirstOrDefault(n =>
                        n.Contains("Hinge", StringComparison.OrdinalIgnoreCase) &&
                      !n.Contains("Pin", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    hingeBlockName = hingeBlocks.FirstOrDefault(b =>
                        b.Contains("Fabricated", StringComparison.OrdinalIgnoreCase));
                }

                if (string.IsNullOrWhiteSpace(hingeBlockName))
                    throw new Exception("Valid hinge block not found in STEP file");

                string handleBlockName =
                    handleBlocks.FirstOrDefault(b =>
                        b.Contains("Handel", StringComparison.OrdinalIgnoreCase) &&
                        !b.Contains("Plate", StringComparison.OrdinalIgnoreCase) &&
                        !b.Contains("Washer", StringComparison.OrdinalIgnoreCase) &&
                        !b.Contains("M5", StringComparison.OrdinalIgnoreCase)
                    );

                string slidingHandleBlockName =
                    slidingHandleBlocks.FirstOrDefault(b =>
                        b.Contains("CONVEYORSTRAIGHT TRACK 1980 mm", StringComparison.OrdinalIgnoreCase)
                     );

                string bottomTrackBlockName =
                slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("Bottom Track", StringComparison.OrdinalIgnoreCase));

                string bottomWheel =
               slidingHandleBlocks.FirstOrDefault(b =>
                   b.Contains("Wheel-2", StringComparison.OrdinalIgnoreCase));

                string bottomSupportPipe =
                 slidingHandleBlocks.FirstOrDefault(b =>
                     b.Contains("Support Pipe Assembly", StringComparison.OrdinalIgnoreCase));
                #endregion

                double T = 3;
                double gap = 10;
                double x0 = doorModel.xOffeset;
                double yDirection = side == 0 ? -1 : 1;
                #region Door Calculations
                int doorLeafCount = doorModel.doorSubType == "Split Doors" ? 2 : 4;

                double innerDoorWidth =
                    (doorModel.doorWidth - (2 * T) - ((doorLeafCount + 1) * gap)) / doorLeafCount;

                double innerDoorHeight = doorModel.doorHeight - (2 * T);
                double frameThickness = 40;
                #endregion

                #region Outer Door Frame
                LinearPath doorPath = new LinearPath(new Point3D[]
                {
                        new Point3D(x0, doorModel.yOffeset, 0),
                        new Point3D(x0, doorModel.yOffeset, doorModel.doorHeight),
                        new Point3D(x0 + doorModel.doorWidth, doorModel.yOffeset, doorModel.doorHeight),
                        new Point3D(x0 + doorModel.doorWidth, doorModel.yOffeset, 0),
                        new Point3D(x0, doorModel.yOffeset, 0)
                });

                var outerSection = devregion.CreatePolygon(new Point3D[]
                {
                        new Point3D(x0, 0, 0),
                        new Point3D(x0 + 25, 0, 0),
                        new Point3D(x0 + 25, -T, 0),
                        new Point3D(x0 + T, -T, 0),
                        new Point3D(x0 + T, -25, 0),
                        new Point3D(x0, -25, 0),
                });

                Solid outerFrame = outerSection.SweepAsSolid(doorPath, 0);
                outerFrame.Translate(0, doorModel.yOffeset, 0);
                drawing.Entities.Add(outerFrame, "MainDoorFrame");
                #endregion

                #region Base Frame
                //double startX = doorModel.doorType== "Hinged" ? x0 + T + gap : x0 + T + 100;
                double startX = x0 + T + gap;
                LinearPath leafPath = new LinearPath(new Point3D[]
                {
                    new(startX,                     doorModel.yOffeset, 0),
                    new(startX,                     doorModel.yOffeset, innerDoorHeight),
                    new(startX + innerDoorWidth,    doorModel.yOffeset, innerDoorHeight),
                    new(startX + innerDoorWidth,    doorModel.yOffeset, 0),
                    new(startX,                     doorModel.yOffeset, 0)
                });

                var frameSection = devregion.CreatePolygon(new Point3D[]
                {
                    new(startX, 0, 0),
                    new(startX + frameThickness, 0, 0),
                    new(startX + frameThickness, -frameThickness, 0),
                    new(startX, -frameThickness, 0),
                });
                Solid baseFrame = frameSection.SweepAsSolid(leafPath, 0);

                #endregion

                #region Sheet
                double sheetInset = frameThickness;
                double sheetWidth = innerDoorWidth - (2 * sheetInset);
                double sheetStartX = startX + sheetInset;

                var sheetProfile = devregion.CreatePolygon(new Point3D[]
                {
                new(sheetStartX, -frameThickness, 0),
                new(sheetStartX, -frameThickness, innerDoorHeight),
                new(sheetStartX + sheetWidth, -frameThickness, innerDoorHeight),
                new(sheetStartX + sheetWidth, -frameThickness, 0),
                });

                Brep baseSheet = sheetProfile.ExtrudeAsBrep(-SheetThickness);
                #endregion

                List<double> hingeZs = new();
                #region Hinges Z positions
                if (doorModel.doorType != "Sliding")
                {
                    int hingeCount = GetHingeCount(innerDoorHeight);

                    double topClearance = 113;
                    double bottomClearance = 300;
                    //double topHingeGap = 275;
                    double topHingeGap = doorModel.TypeOfHinges == "Ball Bearing Hinges" ? 275 : 600;


                    // 1️ Top hinge
                    double topHingeZ = innerDoorHeight - topClearance;
                    hingeZs.Add(topHingeZ);

                    // 2️ Second hinge (150 mm below top)
                    if (hingeCount > 1)
                    {
                        hingeZs.Add(topHingeZ - topHingeGap);
                    }

                    // 3️ Remaining hinges → evenly spaced
                    if (hingeCount > 2)
                    {
                        double remainingHeight =
                            innerDoorHeight
                            - topClearance
                            - topHingeGap
                            - bottomClearance;

                        int remainingHinges = hingeCount - 2;

                        double spacing = remainingHeight / remainingHinges;

                        for (int i = 1; i <= remainingHinges; i++)
                        {
                            hingeZs.Add(topHingeZ - topHingeGap - (spacing * i));
                        }
                    }
                }
                #endregion

                #region Place Doors + Hardware
                double xShift = 0; double yShift = 0; double gapOffset = 60; double zShift = 0;
                zShift = doorModel.doorType == "Hinged" ? 0 : 80;
                for (int i = 0; i < doorLeafCount; i++)
                {
                    xShift = i * (innerDoorWidth + gap);

                    yShift = doorModel.doorType == "Hinged" ? doorModel.yOffeset * yDirection : doorModel.yOffeset + (gapOffset * yDirection);

                    Solid frame = (Solid)baseFrame.Clone();
                    Brep sheet = (Brep)baseSheet.Clone();

                    frame.Translate(xShift, yShift, zShift);
                    sheet.Translate(xShift, yShift, zShift);

                    // =======================
                    // WATCH GLASS CUT
                    // =======================
                    bool placeGlass;
                    if (model.chkWatchDoor)
                    {

                        //AddWatchGlassCutout(
                        //    sheet,
                        //    startX + xShift,
                        //    innerDoorWidth,
                        //    doorModel,
                        //    frameThickness,
                        //    model

                        //);
                        AddWatchGlassCutout(
                            sheet,
                            startX + xShift,
                            yShift,
                            zShift,
                            innerDoorWidth,
                            frameThickness,
                            model
                        );
                    }
                    drawing.Entities.Add(frame, "DoorFrame");
                    drawing.Entities.Add(sheet, "MetalSheet");

                    if (doorModel.doorType == "Sliding" &&
                      !string.IsNullOrWhiteSpace(bottomWheel))
                    {
                        double wheelY = yShift;
                        double wheelZ = zShift;


                        double leafStartX = startX + xShift;

                        //  Dynamic offset (1/4th of width)
                        double wheelOffset = innerDoorWidth / 4.0;

                        double wheelLeftX = leafStartX + wheelOffset;
                        double wheelRightX = leafStartX + innerDoorWidth - wheelOffset;

                        // LEFT wheel
                        BlockReference wheel1 =
                            new BlockReference(
                                new Point3D(0, 0, 0),
                                bottomWheel,
                                1, 1, 1,
                                0);

                        wheel1.Rotate(Math.PI, Vector3D.AxisX);
                        wheel1.Translate(wheelLeftX, wheelY, wheelZ);
                        drawing.Entities.Add(wheel1);

                        // RIGHT wheel
                        BlockReference wheel2 =
                            new BlockReference(
                                new Point3D(0, 0, 0),
                                bottomWheel,
                                1, 1, 1,
                                0);

                        wheel2.Rotate(Math.PI, Vector3D.AxisX);
                        wheel2.Translate(wheelRightX, wheelY, wheelZ);
                        drawing.Entities.Add(wheel2);
                    }

                    // =========================
                    // HINGE LOGIC
                    // =========================
                    if (doorModel.doorType != "Sliding")   // Skip hinges for sliding
                    {
                        bool placeHinge = false;
                        bool hingeOnLeftSide = false;

                        if (doorLeafCount == 2)
                        {
                            // Split doors → both leaves
                            placeHinge = true;
                            hingeOnLeftSide = (i == 0);
                        }
                        else
                        {
                            // Fold doors → only outer leaves
                            if (i == 0)
                            {
                                placeHinge = true;
                                hingeOnLeftSide = true;
                            }
                            else if (i == doorLeafCount - 1)
                            {
                                placeHinge = true;
                                hingeOnLeftSide = false;
                            }
                        }

                        // double hingeY = doorModel.yOffeset - 40 - (2 * SheetThickness) - 3;
                        double hingeY = doorModel.yOffeset + yDirection * (40 + (2 * SheetThickness) + 3);

                        Brep backPlateProto = CreateHingeBackPlateWithHoles();
                        if (placeHinge)
                        {
                            double hingeX = hingeOnLeftSide
                                ? startX + xShift
                                : startX + xShift + innerDoorWidth;

                            foreach (double hz in hingeZs)
                            {
                                InsertDoorHardware(
                                    drawing,
                                    hingeBlockName,
                                    hingeX,
                                    hingeY,
                                    hz,
                                    mirrorX: !hingeOnLeftSide,
                                    applyRotationFix: doorModel.TypeOfHinges == "Ball Bearing Hinges",
                                    rotationAngleRad: Math.PI / 2
                                );
                                if (doorModel.TypeOfHinges != "Ball Bearing Hinges")
                                    // BACK PLATE (follows hinge)
                                    InsertHingeBackPlate(
                                        drawing,
                                        backPlateProto,
                                        hingeX + 50,
                                        hingeY,
                                        hz - 93.5,
                                        mirrorX: !hingeOnLeftSide
                                    );
                            }
                        }
                    }
                    // =========================
                    // HANDLE LOGIC
                    // =========================
                    bool placeHandle;

                    if (doorLeafCount == 2)
                    {
                        placeHandle = true;
                    }
                    else
                    {
                        int leftMiddle = (doorLeafCount / 2) - 1;
                        int rightMiddle = (doorLeafCount / 2);

                        placeHandle = (i == leftMiddle || i == rightMiddle);
                    }

                    if (placeHandle)
                    {
                        double handleOffsetFromBottom = 1000;

                        double handleZ = Math.Min(
                            handleOffsetFromBottom,
                            innerDoorHeight - 100
                        );


                        // double handleY = doorModel.yOffeset - 40 - SheetThickness;
                        double handleY = doorModel.yOffeset + yDirection * (40 + SheetThickness);


                        if (doorModel.doorType == "Sliding")
                            handleY -= 100;
                        // Handles always near meeting edges
                        bool isLeftLeaf = (i < doorLeafCount / 2);

                        double handleX = isLeftLeaf
                            ? startX + xShift + innerDoorWidth - 80
                            : startX + xShift + 80;

                        InsertDoorHardware(
                            drawing,
                            handleBlockName,
                            handleX,
                            handleY,
                            handleZ,
                            mirrorX: !isLeftLeaf,
                            applyRotationFix: true,
                            rotationAngleRad: Math.PI / 2
                        );
                    }
                }

                #endregion

                #region back plate
                if (doorModel.doorType != "Sliding")
                {
                    Brep backPlate = CreateBackPlate(doorModel);

                    // Position it behind the hinges (same Y logic you already use)
                    //double backPlateX = xShift + doorModel.xOffeset + gap;
                    double backPlateX = xShift + doorModel.xOffeset;
                    double backPlateY = doorModel.yOffeset - 40 - SheetThickness; // behind hinge
                    double backPlateZ = 0;

                    backPlate.Translate(backPlateX, backPlateY, backPlateZ);

                    drawing.Entities.Add(backPlate, Color.LightBlue);
                }

                #endregion

                #region Sliding Top Track Block
                if (doorModel.doorType == "Sliding")
                {
                    if (!string.IsNullOrWhiteSpace(slidingHandleBlockName))
                    {
                        double gap1 = 100; // required gap between frame and assembly

                        // 🔹 Left side of opening
                        double trackX = x0;

                        // 🔹 Move OUTSIDE the door (not inside panel)
                        double trackY = side == 0 ? (doorModel.yOffeset - 40 - SheetThickness - gap1) : (doorModel.yOffeset + 40 + SheetThickness);

                        // 🔹 Exactly at top of frame
                        double trackZ = doorModel.doorHeight + zShift;

                        BlockReference leftTrack = new BlockReference(new Point3D(0, 0, 0), slidingHandleBlockName, 1, 1, 1, 0);

                        // 🔹 Rotate 90 degrees around X axis
                        leftTrack.Rotate(Math.PI / 2, Vector3D.AxisZ);
                        leftTrack.Translate(trackX, trackY, trackZ);

                        drawing.Entities.Add(leftTrack);


                        BlockReference rightTrack = new BlockReference(new Point3D(0, 0, 0), slidingHandleBlockName, 1, 1, 1, 0);

                        // 🔹 Rotate 90 degrees around X axis
                        rightTrack.Rotate(Math.PI / 2, Vector3D.AxisZ);
                        double rightTrackX = x0 + doorModel.doorWidth;
                        rightTrack.Translate(rightTrackX, trackY, trackZ);



                        drawing.Entities.Add(rightTrack);
                    }
                    //Bottom Track
                    if (!string.IsNullOrWhiteSpace(bottomTrackBlockName))
                    {
                        double bottomTrackY = doorModel.yOffeset + yDirection * gapOffset;
                        double bottomTrackZ = 0; // floor level
                        double BottomTrackX = x0 + innerDoorWidth;

                        // LEFT
                        BlockReference leftBottomTrack =
                            new BlockReference(
                                new Point3D(0, 0, 0),
                                bottomTrackBlockName,
                                1, 1, 1,
                                0);

                        // leftBottomTrack.Rotate(Math.PI / 2, Vector3D.AxisZ);
                        leftBottomTrack.Translate(BottomTrackX, bottomTrackY, bottomTrackZ);
                        drawing.Entities.Add(leftBottomTrack);

                    }


                }
                #endregion

                #region Write DWG
                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

                string dwgFilePath =
                    path + "/Bullows Panel Drawing/ComponantryEntrySideDoor_" +
                    doorModel.Side + "_" + DateTime.Now.ToString("HH-mm") + ".dwg";

                new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();
                #endregion

                return new PaintBoothclass
                {
                    drawing = drawing,
                    lstpath = dwgFilePath
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public PaintBoothclass ComponantryEntryFrontDoor(DoorDimensionsModel doorModel, PaintBoothModel model)
        {
            try
            {
                drawing = new();
                drawing.Units = linearUnitsType.Millimeters;

                double yOffeset = doorModel.xOffesetForFrontDoor;

                #region Layers
                drawing.Layers.Add(new Layer("DoorFrame", Color.BurlyWood));
                drawing.Layers.Add(new Layer("MetalSheet", Color.FloralWhite));
                drawing.Layers.Add(new Layer("MainDoorFrame", Color.DarkMagenta));
                #endregion

                double T = 3;
                double sectionWidth = 25;
                double sectionHeight = 25;
                double frameThickness = 40;
                double gap = 10;

                #region OUTER FRAME (Front Plane)

                LinearPath doorPath = new LinearPath(new Point3D[]
                {
                    new Point3D(0, yOffeset, 0),
                    new Point3D(0, yOffeset, doorModel.doorHeight),
                    new Point3D(0, yOffeset + doorModel.doorWidth, doorModel.doorHeight),
                    new Point3D(0, yOffeset + doorModel.doorWidth, 0),
                    new Point3D(0, yOffeset, 0)
                });

                var section = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(0, yOffeset, 0),
                    new Point3D(-sectionWidth, yOffeset, 0),
                    new Point3D(-sectionWidth, yOffeset + T, 0),
                    new Point3D(-T, yOffeset + T, 0),
                    new Point3D(-T, yOffeset + sectionHeight, 0),
                    new Point3D(0, yOffeset + sectionHeight, 0),
                });

                Solid outerFrame = section.SweepAsSolid(doorPath, 0);
                drawing.Entities.Add(outerFrame, "MainDoorFrame");

                #endregion

                #region Door Calculations

                int doorLeafCount = doorModel.doorSubType == "Split Doors" ? 2 : 4;

                double innerDoorWidth =
                    (doorModel.doorWidth - (2 * T)) / doorLeafCount;

                double innerDoorHeight = doorModel.doorHeight - (2 * T);

                #endregion

                #region Base Inner Frame + Sheet
                double startY = yOffeset + T + gap;
                LinearPath innerPath = new LinearPath(new Point3D[]
                {
                    new Point3D(0, startY, 0),
                    new Point3D(0, startY, innerDoorHeight),
                    new Point3D(0, startY+ innerDoorWidth, innerDoorHeight),
                    new Point3D(0, startY + innerDoorWidth, 0),
                    new Point3D(0, startY, 0)
                });

                var frameSection = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(-sectionWidth,  startY, 0),
                    new Point3D(-sectionWidth, startY + frameThickness, 0),
                    new Point3D(-(sectionWidth - frameThickness), startY + frameThickness, 0),
                    new Point3D(-(sectionWidth - frameThickness), startY, 0),
                });

                Solid baseFrame = frameSection.SweepAsSolid(innerPath, 0);

                var sheetProfile = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(-(sectionWidth - frameThickness), startY, 0),
                    new Point3D(-(sectionWidth - frameThickness),startY, innerDoorHeight),
                    new Point3D(-(sectionWidth - frameThickness), startY + innerDoorWidth, innerDoorHeight),
                    new Point3D(-(sectionWidth - frameThickness), startY + innerDoorWidth, 0),
                });

                Brep baseSheet = sheetProfile.ExtrudeAsBrep(SheetThickness);

                #endregion

                #region Load STEP Blocks (Same as Side)

                var path = _configuration["StpFilePathConfig:DrawingPath"].ToString();

                string hingeStepPath =
                     doorModel.TypeOfHinges == "Ball Bearing Hinges"
                         ? Path.Combine(path, "Ball_Bearing_Door_Hinge.step")
                         : Path.Combine(path, "Fabricated_Hing-1.step");

                string handleStepPath = Path.Combine(path, "Handel.step");

                List<string> hingeBlocks = AddStepBlocksToDrawing(drawing, hingeStepPath);
                List<string> handleBlocks = AddStepBlocksToDrawing(drawing, handleStepPath);

                string hingeBlockName;

                if (doorModel.TypeOfHinges == "Ball Bearing Hinges")
                {
                    hingeBlockName = drawing.Blocks.Select(b => b.Name).FirstOrDefault(n =>
                        n.Contains("Hinge", StringComparison.OrdinalIgnoreCase) &&
                      !n.Contains("Pin", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    hingeBlockName = hingeBlocks.FirstOrDefault(b =>
                        b.Contains("Fabricated", StringComparison.OrdinalIgnoreCase));
                }
                string handleBlockName =
                     handleBlocks.FirstOrDefault(b =>
                         b.Contains("Handel", StringComparison.OrdinalIgnoreCase) &&
                         !b.Contains("Plate", StringComparison.OrdinalIgnoreCase) &&
                         !b.Contains("Washer", StringComparison.OrdinalIgnoreCase) &&
                         !b.Contains("M5", StringComparison.OrdinalIgnoreCase)
                     );

                string slidingHandleStepPath = Path.Combine(path, "Sliding_Door_Assembly_DESIGN.step");
                List<string> slidingHandleBlocks = AddStepBlocksToDrawing(drawing, slidingHandleStepPath);

                string slidingHandleBlockName = slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("CONVEYORSTRAIGHT TRACK 1980 mm", StringComparison.OrdinalIgnoreCase));

                string bottomTrackBlockName = slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("Bottom Track", StringComparison.OrdinalIgnoreCase));

                string bottomWheel = slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("Wheel-2", StringComparison.OrdinalIgnoreCase));
                #endregion

                #region Hinge Z Positions

                List<double> hingeZs = new();

                if (doorModel.doorType != "Sliding")
                {
                    int hingeCount = GetHingeCount(innerDoorHeight);

                    double topClearance = 113;
                    double bottomClearance = 300;
                    double topHingeGap = doorModel.TypeOfHinges == "Ball Bearing Hinges" ? 275 : 600;

                    double topHingeZ = innerDoorHeight - topClearance;
                    hingeZs.Add(topHingeZ);

                    if (hingeCount > 1)
                        hingeZs.Add(topHingeZ - topHingeGap);

                    if (hingeCount > 2)
                    {
                        double remainingHeight =
                            innerDoorHeight - topClearance - topHingeGap - bottomClearance;

                        int remainingHinges = hingeCount - 2;

                        double spacing = remainingHeight / remainingHinges;

                        for (int i = 1; i <= remainingHinges; i++)
                            hingeZs.Add(topHingeZ - topHingeGap - spacing * i);
                    }
                }

                #endregion

                #region Place Leaves + Hardware
                double xShift = 0; double yShift = 0; double gapOffset = 60;
                double zShift = doorModel.doorType == "Hinged" ? 0 : 80;

                for (int i = 0; i < doorLeafCount; i++)
                {
                    yShift = i * (innerDoorWidth + gap);

                    Solid frame = (Solid)baseFrame.Clone();
                    Brep sheet = (Brep)baseSheet.Clone();

                    frame.Translate(0, yShift, zShift);
                    sheet.Translate(0, yShift, zShift);

                    drawing.Entities.Add(frame, "DoorFrame");
                    drawing.Entities.Add(sheet, "MetalSheet");
                    // ================= VIEWING GLASS =================
                    if (model.chkWatchDoor && doorModel.doorType != "Sliding")
                    {
                        double glassWidth = model.WatchglassWidth;
                        double glassHeight = model.WatchglassHeight;
                        double bottomOffset = model.WatchGlassBottomDistance + 90 + zShift;
                        double sheetFaceX = -(sectionWidth - frameThickness);
                        double glassStartY = startY + yShift + (innerDoorWidth - glassWidth) / 2;

                        devregion cutRegion = devregion.CreatePolygon(new Point3D[]
                        {
                            new Point3D(sheetFaceX, glassStartY,               bottomOffset),
                            new Point3D(sheetFaceX, glassStartY,               bottomOffset + glassHeight),
                            new Point3D(sheetFaceX, glassStartY + glassWidth,  bottomOffset + glassHeight),
                            new Point3D(sheetFaceX, glassStartY + glassWidth,  bottomOffset)
                        });
                        sheet.ExtrudeRemove(cutRegion, SheetThickness);
                    }

                    // ================= BOTTOM WHEELS (Sliding) =================
                    if (doorModel.doorType == "Sliding" && !string.IsNullOrWhiteSpace(bottomWheel))
                    {
                        double wheelX = -(sectionWidth - frameThickness);
                        double wheelZ = zShift;
                        double leafStartY = startY + yShift;
                        double wheelOffset = innerDoorWidth / 4.0;

                        BlockReference wheel1 = new BlockReference(new Point3D(0, 0, 0), bottomWheel, 1, 1, 1, 0);
                        wheel1.Rotate(Math.PI, Vector3D.AxisY);
                        wheel1.Translate(wheelX, leafStartY + wheelOffset, wheelZ);
                        drawing.Entities.Add(wheel1);

                        BlockReference wheel2 = new BlockReference(new Point3D(0, 0, 0), bottomWheel, 1, 1, 1, 0);
                        wheel2.Rotate(Math.PI, Vector3D.AxisY);
                        wheel2.Translate(wheelX, leafStartY + innerDoorWidth - wheelOffset, wheelZ);
                        drawing.Entities.Add(wheel2);
                    }

                    // ================= HINGES =================
                    if (doorModel.doorType != "Sliding")
                    {
                        bool placeHinge = false;
                        bool hingeOnLeft = false;

                        if (doorLeafCount == 2)
                        {
                            placeHinge = true;
                            hingeOnLeft = (i == 0);
                        }
                        else
                        {
                            if (i == 0) { placeHinge = true; hingeOnLeft = true; }
                            else if (i == doorLeafCount - 1) { placeHinge = true; hingeOnLeft = false; }
                        }

                        if (placeHinge)
                        {
                            double hingeX = -frameThickness;
                            double hingeY = hingeOnLeft
                                ? startY + yShift
                                : startY + yShift + innerDoorWidth;

                            foreach (double hz in hingeZs)
                            {
                                var hingeRotations = doorModel.TypeOfHinges == "Ball Bearing Hinges"
                                    ? new List<(double, Vector3D)>
                                      {
                                          (Math.PI / 2, Vector3D.AxisX),
                                          (Math.PI / 2, Vector3D.AxisZ)
                                      }
                                 : hingeOnLeft
                                ? new List<(double, Vector3D)>
                                  {
                                      (Math.PI / 2, Vector3D.AxisZ),
                                      (Math.PI, Vector3D.AxisX),
                                      (Math.PI, Vector3D.AxisY)        // ✅ flip to face door
                                  }
                                : new List<(double, Vector3D)>
                                  {
                                      (Math.PI / 2, Vector3D.AxisZ),
                                      (Math.PI, Vector3D.AxisY)        // ✅ flip to face door
                                  };

                                InsertDoorHardwareInFront(
                                    drawing,
                                    hingeBlockName,
                                    hingeX, hingeY, hz,
                                    flipForRightSide: false,   // ✅ NO mirror — rotations handle everything
                                    rotations: hingeRotations
                                );
                            }
                        }
                    }

                    // ================= HANDLES =================
                    bool placeHandle;
                    if (doorLeafCount == 2)
                        placeHandle = true;
                    else
                    {
                        int leftMiddle = (doorLeafCount / 2) - 1;
                        int rightMiddle = (doorLeafCount / 2);
                        placeHandle = (i == leftMiddle || i == rightMiddle);
                    }

                    if (placeHandle)
                    {
                        double handleZ = Math.Min(1000, innerDoorHeight - 100);
                        bool isLeftLeaf = (i < doorLeafCount / 2);

                        double handleX = -frameThickness;
                        double handleY = isLeftLeaf
                            ? startY + yShift + innerDoorWidth - 80  // near meeting edge
                            : startY + yShift + 80;

                        // ✅ Right leaf rotation works — use mirror for left leaf
                        InsertDoorHardwareInFront(
                            drawing,
                            handleBlockName,
                            handleX, handleY, handleZ,
                            flipForRightSide: isLeftLeaf,            // ✅ mirror LEFT leaf from right
                            rotations: new List<(double, Vector3D)>
                            {
                            (Math.PI / 2, Vector3D.AxisZ),
                            (-Math.PI / 2, Vector3D.AxisY)       // ✅ right-side working rotation
                            }
                        );
                    }
                }
                #endregion

                #region Write DWG

                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

                string dwgFilePath =
                    path + "/Bullows Panel Drawing/ComponantryEntryFrontDoor_" +
                    DateTime.Now.ToString("HH-mm") + ".dwg";

                new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();

                #endregion

                return new PaintBoothclass
                {
                    drawing = drawing,
                    lstpath = dwgFilePath
                };
            }
            catch
            {
                throw;
            }
        }
        public PaintBoothclass ComponantryEntryFrontDoorNew(DoorDimensionsModel doorModel, PaintBoothModel model)
        {
            try
            {
                drawing = new();
                drawing.Units = linearUnitsType.Millimeters;

                double yOffeset = doorModel.xOffesetForFrontDoor;

                #region Layers
                drawing.Layers.Add(new Layer("DoorFrame", Color.BurlyWood));
                drawing.Layers.Add(new Layer("MetalSheet", Color.FloralWhite));
                drawing.Layers.Add(new Layer("MainDoorFrame", Color.DarkMagenta));
                drawing.Layers.Add(new Layer("HingeBackPlate", Color.Bisque));
                #endregion

                double T = 3;
                double sectionWidth = 25;
                double sectionHeight = 25;
                double frameThickness = 40;
                double gap = 10;

                #region OUTER FRAME (Front Plane)

                LinearPath doorPath = new LinearPath(new Point3D[]
                {
                    new Point3D(0, yOffeset, 0),
                    new Point3D(0, yOffeset, doorModel.doorHeight),
                    new Point3D(0, yOffeset + doorModel.doorWidth, doorModel.doorHeight),
                    new Point3D(0, yOffeset + doorModel.doorWidth, 0),
                    new Point3D(0, yOffeset, 0)
                });

                var section = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(0, yOffeset, 0),
                    new Point3D(-sectionWidth, yOffeset, 0),
                    new Point3D(-sectionWidth, yOffeset + T, 0),
                    new Point3D(-T, yOffeset + T, 0),
                    new Point3D(-T, yOffeset + sectionHeight, 0),
                    new Point3D(0, yOffeset + sectionHeight, 0),
                });

                Solid outerFrame = section.SweepAsSolid(doorPath, 0);
                drawing.Entities.Add(outerFrame, "MainDoorFrame");

                #endregion

                #region Door Calculations

                int doorLeafCount = doorModel.doorSubType == "Split Doors" ? 2 : 4;

                double innerDoorWidth =
                    (doorModel.doorWidth - (2 * T)) / doorLeafCount;

                double innerDoorHeight = doorModel.doorHeight - (2 * T);

                #endregion

                #region Base Inner Frame + Sheet
                double startY = yOffeset + T + gap;
                LinearPath innerPath = new LinearPath(new Point3D[]
                {
                    new Point3D(0, startY, 0),
                    new Point3D(0, startY, innerDoorHeight),
                    new Point3D(0, startY+ innerDoorWidth, innerDoorHeight),
                    new Point3D(0, startY + innerDoorWidth, 0),
                    new Point3D(0, startY, 0)
                });

                var frameSection = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(-sectionWidth,  startY, 0),
                    new Point3D(-sectionWidth, startY + frameThickness, 0),
                    new Point3D(-(sectionWidth - frameThickness), startY + frameThickness, 0),
                    new Point3D(-(sectionWidth - frameThickness), startY, 0),
                });

                Solid baseFrame = frameSection.SweepAsSolid(innerPath, 0);

                var sheetProfile = devregion.CreatePolygon(new Point3D[]
                {
                    new Point3D(-(sectionWidth - frameThickness), startY, 0),
                    new Point3D(-(sectionWidth - frameThickness),startY, innerDoorHeight),
                    new Point3D(-(sectionWidth - frameThickness), startY + innerDoorWidth, innerDoorHeight),
                    new Point3D(-(sectionWidth - frameThickness), startY + innerDoorWidth, 0),
                });

                Brep baseSheet = sheetProfile.ExtrudeAsBrep(SheetThickness);

                #endregion

                #region Load STEP Blocks (Same as Side)

                var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

                string hingeStepPath =
                    doorModel.TypeOfHinges == "Ball Bearing Hinges"
                        ? Path.Combine(path, "Ball_Bearing_Door_Hinge.step")
                        : Path.Combine(path, "Fabricated_Hing-1.step");

                string handleStepPath = Path.Combine(path, "Handel.step");

                List<string> hingeBlocks = AddStepBlocksToDrawing(drawing, hingeStepPath);
                List<string> handleBlocks = AddStepBlocksToDrawing(drawing, handleStepPath);

                string hingeBlockName;

                if (doorModel.TypeOfHinges == "Ball Bearing Hinges")
                {
                    hingeBlockName = drawing.Blocks.Select(b => b.Name).FirstOrDefault(n =>
                        n.Contains("Hinge", StringComparison.OrdinalIgnoreCase) &&
                      !n.Contains("Pin", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    hingeBlockName = hingeBlocks.FirstOrDefault(b =>
                        b.Contains("Fabricated", StringComparison.OrdinalIgnoreCase));
                }
                string handleBlockName =
                     handleBlocks.FirstOrDefault(b =>
                         b.Contains("Handel", StringComparison.OrdinalIgnoreCase) &&
                         !b.Contains("Plate", StringComparison.OrdinalIgnoreCase) &&
                         !b.Contains("Washer", StringComparison.OrdinalIgnoreCase) &&
                         !b.Contains("M5", StringComparison.OrdinalIgnoreCase)
                     );

                string slidingHandleStepPath = Path.Combine(path, "Sliding_Door_Assembly_DESIGN.step");
                List<string> slidingHandleBlocks = AddStepBlocksToDrawing(drawing, slidingHandleStepPath);

                string slidingHandleBlockName = slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("CONVEYORSTRAIGHT TRACK 1980 mm", StringComparison.OrdinalIgnoreCase));

                string bottomTrackBlockName = slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("Bottom Track", StringComparison.OrdinalIgnoreCase));

                string bottomWheel = slidingHandleBlocks.FirstOrDefault(b =>
                    b.Contains("Wheel-2", StringComparison.OrdinalIgnoreCase));

                #endregion

                #region Hinge Z Positions

                List<double> hingeZs = new();

                if (doorModel.doorType != "Sliding")
                {
                    int hingeCount = GetHingeCount(innerDoorHeight);

                    double topClearance = 113;
                    double bottomClearance = 300;
                    double topHingeGap = doorModel.TypeOfHinges == "Ball Bearing Hinges" ? 275 : 600;

                    double topHingeZ = innerDoorHeight - topClearance;
                    hingeZs.Add(topHingeZ);

                    if (hingeCount > 1)
                        hingeZs.Add(topHingeZ - topHingeGap);

                    if (hingeCount > 2)
                    {
                        double remainingHeight =
                            innerDoorHeight - topClearance - topHingeGap - bottomClearance;

                        int remainingHinges = hingeCount - 2;

                        double spacing = remainingHeight / remainingHinges;

                        for (int i = 1; i <= remainingHinges; i++)
                            hingeZs.Add(topHingeZ - topHingeGap - spacing * i);
                    }
                }

                #endregion

                #region Place Leaves + Hardware
                double xShift = 0; double yShift = 0; double gapOffset = 60;
                double zShift = doorModel.doorType == "Hinged" ? 0 : 80;

                for (int i = 0; i < doorLeafCount; i++)
                {
                    yShift = i * (innerDoorWidth + gap);

                    Solid frame = (Solid)baseFrame.Clone();
                    Brep sheet = (Brep)baseSheet.Clone();

                    frame.Translate(0, yShift, zShift);
                    sheet.Translate(0, yShift, zShift);

                    drawing.Entities.Add(frame, "DoorFrame");
                    drawing.Entities.Add(sheet, "MetalSheet");

                    // ================= VIEWING GLASS =================
                    if (model.chkWatchDoor && doorModel.doorType != "Sliding")
                    {
                        double glassWidth = model.WatchglassWidth;
                        double glassHeight = model.WatchglassHeight;
                        double bottomOffset = model.WatchGlassBottomDistance + 90 + zShift;
                        double sheetFaceX = -(sectionWidth - frameThickness);
                        double glassStartY = startY + yShift + (innerDoorWidth - glassWidth) / 2;

                        devregion cutRegion = devregion.CreatePolygon(new Point3D[]
                        {
                            new Point3D(sheetFaceX, glassStartY,               bottomOffset),
                            new Point3D(sheetFaceX, glassStartY,               bottomOffset + glassHeight),
                            new Point3D(sheetFaceX, glassStartY + glassWidth,  bottomOffset + glassHeight),
                            new Point3D(sheetFaceX, glassStartY + glassWidth,  bottomOffset)
                        });
                        sheet.ExtrudeRemove(cutRegion, SheetThickness);
                    }

                    // ================= BOTTOM WHEELS (Sliding) =================
                    if (doorModel.doorType == "Sliding" && !string.IsNullOrWhiteSpace(bottomWheel))
                    {
                        double wheelX = -(sectionWidth - frameThickness);
                        double wheelZ = zShift;
                        double leafStartY = startY + yShift;
                        double wheelOffset = innerDoorWidth / 4.0;

                        BlockReference wheel1 = new BlockReference(new Point3D(0, 0, 0), bottomWheel, 1, 1, 1, 0);
                        wheel1.Rotate(Math.PI, Vector3D.AxisY);
                        wheel1.Translate(wheelX, leafStartY + wheelOffset, wheelZ);
                        drawing.Entities.Add(wheel1);

                        BlockReference wheel2 = new BlockReference(new Point3D(0, 0, 0), bottomWheel, 1, 1, 1, 0);
                        wheel2.Rotate(Math.PI, Vector3D.AxisY);
                        wheel2.Translate(wheelX, leafStartY + innerDoorWidth - wheelOffset, wheelZ);
                        drawing.Entities.Add(wheel2);
                    }

                    // ================= HINGES =================
                    if (doorModel.doorType != "Sliding")
                    {
                        bool placeHinge = false;
                        bool hingeOnLeft = false;

                        if (doorLeafCount == 2)
                        {
                            placeHinge = true;
                            hingeOnLeft = (i == 0);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                placeHinge = true;
                                hingeOnLeft = true;
                            }
                            else if (i == doorLeafCount - 1)
                            {
                                placeHinge = true;
                                hingeOnLeft = false;
                            }
                        }

                        if (placeHinge)
                        {
                            double hingeX = -frameThickness;
                            double hingeY = hingeOnLeft ? startY + yShift : startY + yShift + innerDoorWidth;

                            foreach (double hz in hingeZs)
                            {
                                InsertDoorHardwareInFront(
                                    drawing,
                                    hingeBlockName,
                                    hingeX,
                                    hingeY,
                                    hz,
                                    flipForRightSide: !hingeOnLeft,
                                    rotations: new List<(double, Vector3D)>
                                    {
                                        (Math.PI / 2, Vector3D.AxisZ)
                                    }
                                );
                            }
                        }
                    }


                    #region Handles
                    bool placeHandle;

                    if (doorLeafCount == 2)
                        placeHandle = true;
                    else
                    {
                        int leftMiddle = (doorLeafCount / 2) - 1;
                        int rightMiddle = (doorLeafCount / 2);
                        placeHandle = (i == leftMiddle || i == rightMiddle);
                    }

                    if (placeHandle)
                    {
                        double handleZ = Math.Min(1000, innerDoorHeight - 100);
                        bool isLeftLeaf = (i < doorLeafCount / 2);

                        double handleX = -frameThickness;
                        double handleY = isLeftLeaf ? startY + yShift + innerDoorWidth - 80 : startY + yShift + 80;

                        InsertDoorHardwareInFront(
                            drawing,
                            handleBlockName,
                            handleX,
                            handleY,
                            handleZ,
                            flipForRightSide: !isLeftLeaf,
                            rotations: new List<(double, Vector3D)>
                            {
                                            (Math.PI / 2, Vector3D.AxisZ),  // 1️ First rotate Z
                                            (Math.PI / 2, Vector3D.AxisY)   // 2️ Then rotate Y
                            }
                        );
                    }
                    #endregion
                }

                #endregion

                #region Back Plate (non-sliding)
                if (doorModel.doorType != "Sliding")
                {
                    Brep backPlate = CreateBackPlate(doorModel);
                    double backPlateX = -frameThickness - SheetThickness;
                    double backPlateY = startY;
                    backPlate.Translate(backPlateX, backPlateY, 0);
                    drawing.Entities.Add(backPlate, Color.LightBlue);
                }
                #endregion

                #region Sliding Top Track + Bottom Track
                if (doorModel.doorType == "Sliding")
                {
                    if (!string.IsNullOrWhiteSpace(slidingHandleBlockName))
                    {
                        double gap1 = 100;
                        double trackX = -frameThickness - SheetThickness - gap1;
                        double trackZ = doorModel.doorHeight + zShift;

                        BlockReference leftTrack = new BlockReference(new Point3D(0, 0, 0), slidingHandleBlockName, 1, 1, 1, 0);
                        leftTrack.Rotate(Math.PI / 2, Vector3D.AxisX);
                        leftTrack.Translate(trackX, startY, trackZ);
                        drawing.Entities.Add(leftTrack);

                        BlockReference rightTrack = new BlockReference(new Point3D(0, 0, 0), slidingHandleBlockName, 1, 1, 1, 0);
                        rightTrack.Rotate(Math.PI / 2, Vector3D.AxisX);
                        rightTrack.Translate(trackX, startY + doorModel.doorWidth, trackZ);
                        drawing.Entities.Add(rightTrack);
                    }

                    if (!string.IsNullOrWhiteSpace(bottomTrackBlockName))
                    {
                        double bottomTrackX = -frameThickness - SheetThickness;
                        double bottomTrackY = startY + innerDoorWidth;

                        BlockReference bottomTrack = new BlockReference(new Point3D(0, 0, 0), bottomTrackBlockName, 1, 1, 1, 0);
                        bottomTrack.Translate(bottomTrackX, bottomTrackY, 0);
                        drawing.Entities.Add(bottomTrack);
                    }
                }
                #endregion

                #region Write DWG

                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

                string dwgFilePath =
                    path + "/Bullows Panel Drawing/ComponantryEntryFrontDoor_" +
                    DateTime.Now.ToString("HH-mm") + ".dwg";

                new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFilePath).DoWork();

                #endregion

                return new PaintBoothclass
                {
                    drawing = drawing,
                    lstpath = dwgFilePath
                };
            }
            catch
            {
                throw;
            }
        }
        public Brep CreateBackPlate(DoorDimensionsModel doorModel)
        {
            Brep backPlateBrep = null;

            try
            {
                double plateWidth = 50;
                double plateHeight = doorModel.doorHeight;
                double thickNess = 5;
                var backPlate = devregion.CreatePolygon(new Point3D[]
                {
                new Point3D(0, 0, 0),
                new Point3D(plateWidth, 0, 0),
                new Point3D(plateWidth,0, plateHeight),
                new Point3D(0,0, plateHeight),
                });
                backPlateBrep = backPlate.ExtrudeAsBrep(thickNess);
                backPlateBrep.Regen(0.1);
                return backPlateBrep;
            }
            catch (Exception ex)
            {
                return backPlateBrep;
                throw;
            }
        }
        #region Hardware Parts
        public Brep CreateHingesBackPlate()
        {

            double plateWidth = 100;
            double plateHeight = 100;

            double thickNess = 16;
            var backPlate = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(plateWidth, 0, 0),
                new Point3D(plateWidth,0, plateHeight),
                new Point3D(0,0, plateHeight),
            });
            Brep backPlateBrep = backPlate.ExtrudeAsBrep(-thickNess);
            backPlateBrep.Regen(0.1);


            return backPlateBrep;
        }
        public Brep CreateHingeBackPlateWithHolesOld()
        {
            double plateWidth = 100;
            double plateHeight = 100;
            double thickness = 16;

            // 1️⃣ Create plate as SOLID
            var plateRegion = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(plateWidth, 0, 0),
                new Point3D(plateWidth, 0, plateHeight),
                new Point3D(0, 0, plateHeight),
            });

            Brep backPlate = plateRegion.ExtrudeAsBrep(-thickness);
            backPlate.Regen(0.1);

            // 2️⃣ Hole parameters (M10 THRU)
            double holeRadius = 5;               // M10
            double holeDepth = thickness + 2;    // thru cut

            // Hole center positions from drawing
            double[] holeX = { 20, 65 };
            double[] holeZ = { 20, 80 };

            // 3️⃣ Create & remove holes
            foreach (double x in holeX)
            {
                foreach (double z in holeZ)
                {
                    // Circle on YZ plane
                    devregion holeRegion = devregion.CreateCircle(
                        Plane.YZ,
                        new Point3D(x, 0, z),
                        holeRadius
                    );

                    // Remove material
                    backPlate.ExtrudeRemove(holeRegion, holeDepth, 0);
                    drawing.Entities.Add(backPlate);
                }
            }

            backPlate.Regen(0.1);
            return backPlate;
        }
        public Brep CreateHingeBackPlateWithHoles()
        {
            //drawing = new();
            double plateWidth = 100;
            double plateHeight = 100;
            double thickness = 16;

            // 1️⃣ Create plate on XZ plane
            var plateRegion = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, 0),
                new Point3D(plateWidth, 0, 0),
                new Point3D(plateWidth, 0, plateHeight),
                new Point3D(0, 0, plateHeight),
            });

            // Extrude in -Y direction
            Brep backPlate = plateRegion.ExtrudeAsBrep(thickness);
            backPlate.Regen(0.1);
            //drawing.Entities.Add(backPlate);
            // 2️⃣ Hole settings
            double holeRadius = 5; // M10 hole
            double[] holeX = { 20, 65 };
            double[] holeZ = { 50, 80 };

            foreach (double x in holeX)
            {
                foreach (double z in holeZ)
                {
                    // 3️ Create circle on XZ plane (front face of plate)
                    devregion circle = devregion.CreateCircle(
                        Plane.XZ,
                        new Point3D(x, 0, z),
                        holeRadius
                    );

                    // 4️ Extrude hole in SAME direction as plate (-Y)
                    Brep holeCylinder = circle.ExtrudeAsBrep(thickness);
                    holeCylinder.Regen(0.1);

                    // 5️ Boolean subtract
                    Brep[] result = Brep.Difference(backPlate, holeCylinder);


                    if (result != null && result.Length > 0)
                        backPlate = result[0];
                }
            }

            // 3️⃣ Add ONLY final plate
            drawing.Entities.Add(backPlate, Color.DarkGray);

            drawing.Entities.Regen();

            backPlate.Regen(0.1);


            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/Bullows Panel Drawing"))
            //    Directory.CreateDirectory(path + "/Bullows Panel Drawing");

            //string dwgFilePath = path + "/Bullows Panel Drawing/" + "Hinge_BackPlate" + DateTime.Now.ToString("hh-mm") + ".dwg";

            //// Save as DWG
            //WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            //WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            //dwgg1.DoWork();

            return backPlate;
        }
        #endregion

        public Brep AddWatchGlass(double innerDoorWidth, PaintBoothModel model, DoorDimensionsModel doorModel)
        {
            double watchGlassXPosition = innerDoorWidth / 2;
            Point3D[] cutout = new Point3D[]
                   {
                        new Point3D(watchGlassXPosition,                            doorModel.yOffeset,  model.WatchGlassBottomDistance),
                        new Point3D(watchGlassXPosition,                            doorModel.yOffeset,  model.WatchGlassBottomDistance + model.WatchglassHeight),
                        new Point3D(watchGlassXPosition + model.WatchglassWidth,    doorModel.yOffeset,   model.WatchGlassBottomDistance + model.WatchglassHeight),
                        new Point3D(watchGlassXPosition + model.WatchglassWidth,    doorModel.yOffeset,  model.WatchGlassBottomDistance)
                   };
            var cutout1 = devregion.CreatePolygon(cutout);
            Brep brep = cutout1.ExtrudeAsBrep(SheetThickness);
            brep.ExtrudeRemove(cutout1, SheetThickness);
            return brep;
        }
        private void AddWatchGlassCutout(Brep sheet, double absoluteStartX, double yShift, double zShift, double innerDoorWidth, double frameThickness, PaintBoothModel model)
        {

            double glassWidth = model.WatchglassWidth;
            double glassHeight = model.WatchglassHeight;
            double bottomOffset = model.WatchGlassBottomDistance + 90;

            double glassStartX =
                absoluteStartX + (innerDoorWidth - glassWidth) / 2;

            // ⭐ IMPORTANT: must match sheet FRONT face exactly
            double sheetFrontY = yShift - frameThickness;


            devregion cutRegion = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(glassStartX,                 sheetFrontY, bottomOffset),
                new Point3D(glassStartX,                 sheetFrontY, bottomOffset + glassHeight),
                new Point3D(glassStartX + glassWidth,    sheetFrontY, bottomOffset + glassHeight),
                new Point3D(glassStartX + glassWidth,    sheetFrontY, bottomOffset)
            });

            // ⭐ DO NOT use ExtrudeAsBrep
            sheet.ExtrudeRemove(cutRegion, -SheetThickness);

        }
        #endregion

        public PanelDrawResult CreateLoft(PaintBoothModel model, double extractionChemberHeight, int NoOfBlowers)
        {
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            DesignDocument drawing = new DesignDocument();
            drawing.Units = linearUnitsType.Millimeters;

            W = model.W;

            // ── Determine transition piece count ─────────────────────────────────
            int transitionCount;
            if (NoOfBlowers == 1)
            {
                transitionCount = 1;
            }
            else if (NoOfBlowers == 2)
            {
                transitionCount = 2;
            }
            else if (NoOfBlowers > 2 && NoOfBlowers % 2 == 0)
            {
                transitionCount = NoOfBlowers / 2;
            }
            else
            {
                throw new Exception("NoOfBlowers must be 1 or an even number.");
            }

            double pieceWidth = W / transitionCount;
            double loftWeight = 0;

            double x = model.D;
            double z1 = extractionChemberHeight;
            double z2 = extractionChemberHeight + 300;

            #region loft
            for (int i = 0; i < transitionCount; i++)
            {
                double yStart = i * pieceWidth;
                double yEnd = yStart + pieceWidth;

                // ── Bottom rectangle at z1 ────────────────────────────────────
                Line l1 = new Line(new Point3D(x, yStart, z1), new Point3D(x + model.D3, yStart, z1));
                Line l2 = new Line(new Point3D(x + model.D3, yStart, z1), new Point3D(x + model.D3, yEnd, z1));
                Line l3 = new Line(new Point3D(x + model.D3, yEnd, z1), new Point3D(x, yEnd, z1));
                Line l4 = new Line(new Point3D(x, yEnd, z1), new Point3D(x, yStart, z1));

                CompositeCurve rect = new CompositeCurve(new ICurve[] { l1, l2, l3, l4 });
                drawing.Entities.Add(rect, Color.AliceBlue);

                // ── Circle (top) centred within this slice at z2 ─────────────
                double centerX = x + model.D3 / 2;
                double centerY = yStart + pieceWidth / 2;
                double radius = model.D3 / 2;

                Point3D center = new Point3D(centerX, centerY, z2);

                Arc arc1 = new Arc(Plane.XY, center, radius, Utility.DegToRad(45), Utility.DegToRad(135));
                Arc arc2 = new Arc(Plane.XY, center, radius, Utility.DegToRad(135), Utility.DegToRad(225));
                Arc arc3 = new Arc(Plane.XY, center, radius, Utility.DegToRad(225), Utility.DegToRad(315));
                Arc arc4 = new Arc(Plane.XY, center, radius, Utility.DegToRad(315), Utility.DegToRad(405));

                CompositeCurve circleByArcs = new CompositeCurve(new ICurve[] { arc1, arc2, arc3, arc4 });

                // ── Loft surfaces ─────────────────────────────────────────────
                Surface[] loftA = Surface.Loft(new ICurve[] { arc1, l3 });
                Surface[] loftB = Surface.Loft(new ICurve[] { arc2, l4 });
                Surface[] loftC = Surface.Loft(new ICurve[] { arc3, l1 });
                Surface[] loftD = Surface.Loft(new ICurve[] { arc4, l2 });

                // ── Cap surfaces ──────────────────────────────────────────────
                devregion circle = new devregion(circleByArcs);
                PlanarSurface circleSurf = circle.ConvertToSurface();

                devregion rectangle = new devregion(rect);
                PlanarSurface rectSurf = rectangle.ConvertToSurface();
                rectSurf.FlipNormal();

                // ── Solidify ──────────────────────────────────────────────────
                Solidifier solid = new Solidifier(new Surface[] { loftA[0], loftB[0], loftC[0], loftD[0], circleSurf, rectSurf });
                solid.DoWork();

                drawing.Entities.Add(solid.Result, Color.Yellow);



                // ── Accumulate weight ─────────────────────────────────────────
                try
                {
                    Material mat = new Material(Materials);
                    solid.Result.Regen(0.1);
                    double mass = solid.Result.GetMass(
                        mat,
                        linearUnitsType.Millimeters,
                        massUnitsType.Kilograms,
                        out double _);
                    loftWeight += Math.Round(mass, 3);
                }
                catch
                {
                    // Weight calculation failed for this piece — continue
                }
            }
            #endregion

            // ── Save files ────────────────────────────────────────────────────────
            if (!Directory.Exists(Path.Combine(path, "Bullows Panel Drawing")))
                Directory.CreateDirectory(Path.Combine(path, "Bullows Panel Drawing"));

            string dwgFilePathforLoft = Path.Combine(path, "Bullows Panel Drawing",
                "loft_with_centrifugal_fan_" + DateTime.Now.ToString("hh-mm") + ".dwg");
            string dwgFilePathforLoftpdf = Path.Combine(path, "Bullows Panel Drawing",
                "loft_with_centrifugal_fan_" + DateTime.Now.ToString("hh-mm") + ".pdf");

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathforLoft);
            dwgg1.DoWork();

            Write3DPdfParams pdf = new Write3DPdfParams(drawing);
            Write3DPDF pdf1 = new Write3DPDF(pdf, dwgFilePathforLoftpdf);
            pdf1.DoWork();

            //CreateLoftDevelopment(model, extractionChemberHeight,NoOfBlowers);



            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = loftWeight,
                lstpath = dwgFilePathforLoft,
                NoOfTransitionPieces= transitionCount
            };
        }
        #region development drawing for loft
        // ── add near your other private helpers ────────────────────────────────
        private static double Dist(Point3D a, Point3D b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        // 2D triangle area from three planar points (z ignored — pattern lies on z=0)
        private static double TriArea(Point3D a, Point3D b, Point3D c)
        {
            return Math.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2.0;
        }
        // place C given placed P,Q and true lengths rP=|C-P|, rQ=|C-Q|; side = +1/-1
        private static double[] SolveThird(double px, double py, double qx, double qy,
                                           double rP, double rQ, double side)
        {
            double dx = qx - px, dy = qy - py;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 1e-9) return new double[] { px, py };
            double a = (rP * rP - rQ * rQ + d * d) / (2 * d);
            double h2 = rP * rP - a * a;
            double h = h2 > 0 ? Math.Sqrt(h2) : 0;
            double mx = px + a * dx / d;
            double my = py + a * dy / d;
            double ox = -dy / d * h * side;
            double oy = dx / d * h * side;
            return new double[] { mx + ox, my + oy };
        }

        public PanelDrawResult CreateLoftDevelopment(PaintBoothModel model,
                                             double extractionChemberHeight,
                                             int NoOfBlowers)
        {
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            W = model.W;

            // ── transition piece count (same logic as CreateLoft) ────────────────
            int transitionCount;
            if (NoOfBlowers == 1) transitionCount = 1;
            else if (NoOfBlowers == 2) transitionCount = 2;
            else if (NoOfBlowers > 2 && NoOfBlowers % 2 == 0) transitionCount = NoOfBlowers / 2;
            else throw new Exception("NoOfBlowers must be 1 or an even number.");

            double pieceWidth = W / transitionCount;

            double x = model.D;
            double z1 = extractionChemberHeight;
            double z2 = extractionChemberHeight + 300;
            double D3 = model.D3;
            double r = D3 / 2.0;

            Point3D center = new Point3D(x + D3 / 2.0, pieceWidth / 2.0, z2);

            // panel = arc quadrant + its base line, in the SAME pairing as your loft
            // A:arc1↔l3  B:arc2↔l4  C:arc3↔l1  D:arc4↔l2  (chains CCW, seam A-start/D-end)
            var panels = new (double a0, double a1, Point3D ls, Point3D le)[]
            {
        (45,  135, new Point3D(x + D3, pieceWidth, z1), new Point3D(x, pieceWidth, z1)), // A
        (135, 225, new Point3D(x, pieceWidth, z1),      new Point3D(x, 0, z1)),          // B
        (225, 315, new Point3D(x, 0, z1),               new Point3D(x + D3, 0, z1)),     // C
        (315, 405, new Point3D(x + D3, 0, z1),          new Point3D(x + D3, pieceWidth, z1)) // D
            };

            int n = 16; // subdivisions per quadrant (raise for smoother round edge)

            DesignDocument drawing = new DesignDocument();
            drawing.Units = linearUnitsType.Millimeters;

            // running world end-ruling of the previous panel
            double prevBotX = 0, prevBotY = 0, prevTopX = 0, prevTopY = 0;

            var bottomRail = new List<Point3D>();
            var topRail = new List<Point3D>();
            var rulings = new List<(Point3D b, Point3D t, int p, int k)>();

            var labels = new List<(Point3D pos, string txt)>();

            for (int p = 0; p < 4; p++)
            {
                var pan = panels[p];

                // ── sample the two 3D rails ─────────────────────────────────────
                Point3D[] M = new Point3D[n + 1]; // base line
                Point3D[] N = new Point3D[n + 1]; // arc
                for (int k = 0; k <= n; k++)
                {
                    double t = (double)k / n;
                    M[k] = new Point3D(
                        pan.ls.X + t * (pan.le.X - pan.ls.X),
                        pan.ls.Y + t * (pan.le.Y - pan.ls.Y),
                        pan.ls.Z + t * (pan.le.Z - pan.ls.Z));
                    double rad = (pan.a0 + t * (pan.a1 - pan.a0)) * Math.PI / 180.0;
                    N[k] = new Point3D(center.X + r * Math.Cos(rad),
                                       center.Y + r * Math.Sin(rad), center.Z);
                }

                // base is straight ⇒ its unrolled rail is a straight, evenly spaced line
                double baseLen = Dist(pan.ls, pan.le);
                double seg = baseLen / n;

                double[] mLx = new double[n + 1], mLy = new double[n + 1];
                double[] aLx = new double[n + 1], aLy = new double[n + 1];
                for (int k = 0; k <= n; k++) { mLx[k] = k * seg; mLy[k] = 0; }

                // each arc point fixed by true lengths to two adjacent base points (no drift)
                for (int k = 0; k < n; k++)
                {
                    double ruling = Dist(N[k], M[k]);
                    double diag = Dist(N[k], M[k + 1]);
                    var s = SolveThird(mLx[k], mLy[k], mLx[k + 1], mLy[k + 1], ruling, diag, +1);
                    aLx[k] = s[0]; aLy[k] = s[1];
                }
                {   // last arc point from (m[n-1], m[n])
                    double backdiag = Dist(N[n], M[n - 1]);
                    double ruling = Dist(N[n], M[n]);
                    var s = SolveThird(mLx[n - 1], mLy[n - 1], mLx[n], mLy[n], backdiag, ruling, +1);
                    aLx[n] = s[0]; aLy[n] = s[1];
                }
                // ── arc - rail by SEQUENTIAL triangulation(preserves round - edge length) ──
                // base rail is collinear along + x at y = 0; arc bulges to + y, so of the two
                // circle - circle solutions always take the one with the larger y.


                // ── rigid transform: attach this panel's start ruling onto prev end ─
                double cos, sin, ox, oy;
                if (p == 0) { cos = 1; sin = 0; ox = 0; oy = 0; }
                else
                {
                    double vpx = aLx[0], vpy = aLy[0];                 // local start ruling
                    double vqx = prevTopX - prevBotX, vqy = prevTopY - prevBotY; // world target
                    double ang = Math.Atan2(vqy, vqx) - Math.Atan2(vpy, vpx);
                    cos = Math.Cos(ang); sin = Math.Sin(ang);
                    ox = prevBotX; oy = prevBotY;
                }
                Func<double, double, Point3D> toWorld = (lx, ly) =>
                    new Point3D(ox + cos * lx - sin * ly, oy + sin * lx + cos * ly, 0);

                Point3D[] Mw = new Point3D[n + 1];
                Point3D[] Aw = new Point3D[n + 1];
                for (int k = 0; k <= n; k++)
                {
                    Mw[k] = toWorld(mLx[k], mLy[k]);
                    Aw[k] = toWorld(aLx[k], aLy[k]);
                }

                int startK = (p == 0) ? 0 : 1; // skip shared corner point when chaining rails
                for (int k = startK; k <= n; k++) { bottomRail.Add(Mw[k]); topRail.Add(Aw[k]); }
                for (int k = 0; k <= n; k++) rulings.Add((Mw[k], Aw[k], p, k));

                int mid = n / 2;
                labels.Add((new Point3D((Mw[mid].X + Aw[mid].X) / 2,
                                        (Mw[mid].Y + Aw[mid].Y) / 2, 0), "P" + (p + 1)));

                prevBotX = Mw[n].X; prevBotY = Mw[n].Y;
                prevTopX = Aw[n].X; prevTopY = Aw[n].Y;
            }

            // ── draw the pattern ────────────────────────────────────────────────
            drawing.Entities.Add(new LinearPath(bottomRail.ToArray()), Color.Yellow); // square edge
            drawing.Entities.Add(new LinearPath(topRail.ToArray()), Color.Yellow); // round edge

            foreach (var rr in rulings)
            {
                if (rr.p == 0 && rr.k == 0)                 // seam (A start)
                    drawing.Entities.Add(new Line(rr.b, rr.t), Color.Red);
                else if (rr.p == 3 && rr.k == n)            // seam (D end)
                    drawing.Entities.Add(new Line(rr.b, rr.t), Color.Red);
                else if (rr.k == 0)                         // corner fold line
                    drawing.Entities.Add(new Line(rr.b, rr.t), Color.Cyan);
                else                                        // triangulation / roll lines
                    drawing.Entities.Add(new Line(rr.b, rr.t), Color.FromArgb(110, 110, 110));
            }

            foreach (var lb in labels)
                drawing.Entities.Add(new Text(lb.pos, lb.txt, 20), Color.White);



            // ── flat-pattern area by summing the ruling quads as two triangles each ──
            double patternAreaOne = 0; // mm² for a single transition piece
            for (int i = 0; i < rulings.Count - 1; i++)
            {
                var q0 = rulings[i];
                var q1 = rulings[i + 1];
                // skip the seam gap: only sum within a continuous panel chain
                // (adjacent entries in `rulings` are always neighbours because we added
                //  every k for every panel in order, so no gap check is needed)
                patternAreaOne += TriArea(q0.b, q1.b, q1.t);   // bottom triangle
                patternAreaOne += TriArea(q0.b, q1.t, q0.t);   // top triangle
            }

            // ── convert to weight ───────────────────────────────────────────────
            double thicknessMm =ECSheetThickness > 0 ? ECSheetThickness : 1.6; // sheet gauge
            double densityKgM3 = 7850;                                          // mild steel

            double areaM2 = patternAreaOne / 1_000_000.0;   // mm² → m²
            double volM3 = areaM2 * (thicknessMm / 1000.0); // × thickness in m
            double weightOne = volM3 * densityKgM3;          // kg, one piece
            double weightTotal = weightOne * transitionCount;  // kg, all pieces
            double TransitionWeight = Math.Round(weightTotal, 3);
            UpdateTransitionDetails(model, NoOfBlowers, TransitionWeight, extractionChemberHeight);

            double devLen = 2 * (D3 + pieceWidth);
            Point3D anchor = bottomRail[0];
            drawing.Entities.Add(new Text(new Point3D(anchor.X, anchor.Y - 90, 0),
                "SQUARE-TO-ROUND TRANSITION - FLAT PATTERN  (1 of " + transitionCount + ")", 26),
                Color.White);
            drawing.Entities.Add(new Text(new Point3D(anchor.X, anchor.Y - 130, 0),
                "Base perimeter = " + devLen.ToString("F0") + " mm   |   Top dia = " +
                D3.ToString("F0") + " mm   |   Rise = 300 mm", 20), Color.White);

            // ── save (DWG + 3D PDF) ─────────────────────────────────────────────
            if (!Directory.Exists(Path.Combine(path, "Bullows Panel Drawing")))
                Directory.CreateDirectory(Path.Combine(path, "Bullows Panel Drawing"));

            string dwgFile = Path.Combine(path, "Bullows Panel Drawing",
                "loft_development_" + DateTime.Now.ToString("hh-mm") + ".dwg");
            string pdfFile = Path.Combine(path, "Bullows Panel Drawing",
                "loft_development_" + DateTime.Now.ToString("hh-mm") + ".pdf");

            WriteAutodesk dwg = new WriteAutodesk(new WriteAutodeskParams(drawing), dwgFile);
            dwg.DoWork();

            Write3DPDF pdf = new Write3DPDF(new Write3DPdfParams(drawing), pdfFile);
            pdf.DoWork();

            return new PanelDrawResult
            {
                drawing = drawing,
                Weight = 0,
                lstpath = dwgFile,
                NoOfTransitionPieces = transitionCount
            };
        }

        public void UpdateTransitionDetails(PaintBoothModel model, int NoOfBlowers, double TransitionWeight,double extractionChemberHeight)
        {
            // Soft delete existing rows
            var existing = _DbContext.PanelDetails
                              .Where(x => x.EnquiryId == EnquiryID && x.PanelPosition == "Transition_Piece")
                              .ToList();

            foreach (var item in existing)
            {
                item.StandardPanelDepth = model.D3;
                item.StandardPanelHeight = extractionChemberHeight;
                item.StandardPanelWidth = model.W;
                item.NoOfPanels = NoOfBlowers;
                item.PanelWeight = TransitionWeight;
                item.ModifiedDate = DateTime.Now;
            }
            _DbContext.SaveChanges();
        }

        #endregion
        public PaintBoothclass CreateLoftForPaintboothType5(PaintBoothModel model, string DraftSubType)
        {
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            DesignDocument drawing = new DesignDocument();
            drawing.Units = linearUnitsType.Millimeters;
            #region loft
            // Define base planes for the shapes
            Plane p1 = Plane.XY;
            Plane p2 = p1.Offset(model.H);


            if (model.PanelTypes == "2" || model.PanelTypesforW == "2" || model.PanelTypesforH == "2")
            {
                model.D = model.D;
                W = model.W;
            }
            else
            {
                model.D = model.StandardPanelWidthForD;
                if (model.StandardPanelsByW == 0)
                    W = model.RemainingPanelsByW;
            }
            double x = 0;
            if (DraftSubType == "7" || DraftSubType == "5" || DraftSubType == "4" || DraftSubType == "6")
                x = -D3 - (SettingStandardBend2 + SettingStandardBend1);
            else
                x = model.D;
            // Create the loft base 
            LinearPath rectangle = new LinearPath(p2, new Point3D[]
                {
                new Point3D(x, 0, model.H),
                new Point3D(x + model.D3, 0, model.H),
                new Point3D(x + model.D3, W, model.H),
                new Point3D(x, W, model.H),
                new Point3D(x, 0, model.H)
                });

            drawing.Entities.Add(rectangle, Color.Pink);

            LinearPath rectangle1 = new LinearPath(new Point3D[]// top rectangles
            {
                new Point3D(x, (W - D3) / 2, model.H + 300),
                new Point3D((x + model.D3), ((W - D3) / 2), model.H + 300),
                new Point3D((x + model.D3), ((W - D3) / 2) + D3, model.H + 300),
                new Point3D(x, ((W - D3) / 2) + D3, model.H + 300),
                new Point3D(x, (W - D3) / 2, model.H + 300)
            });
            drawing.Entities.Add(rectangle1, Color.Pink);

            // Create the loft between two rectangles
            Brep loft1 = Brep.Loft(new ICurve[] { rectangle, rectangle1 }, 2);
            drawing.Entities.Add(loft1, Color.Yellow);
            #endregion
            //code for reading step file 
            var stepFilePath = path + "/CasingModel.step";

            ReadSTEP stepReader = new ReadSTEP(stepFilePath);
            stepReader.DoWork();

            // Position of the imported STEP model on the loft
            double stepPositionX = x + (model.D3 / 2);
            double stepPositionY = W / 2;
            double stepPositionZ = model.H + 300 + 300;//centrifugal fan height
            double angle = 0;
            int i = 1;
            foreach (Entity stepEntity in stepReader.Entities)
            {
                // Check if the entity is a BlockReference and if its name is in the list of names to skip
                if (stepEntity is BlockReference blockRef)
                {
                    string blockName = blockRef.BlockName;
                    if (blockName == "Block_0" || blockName == "Block_1" || blockName == "Block_2" || blockName == "Block_3" || blockName == "Block_4")
                    {
                        continue;
                    }
                }

                // Translate the entity before adding it to the drawing
                stepEntity.Translate(stepPositionX, stepPositionY, stepPositionZ);
                // Rotate the entity 90 degrees (π/2 radians) around the X-axis at the same position
                Point3D centerPoint = new Point3D(stepPositionX, stepPositionY, stepPositionZ);
                // stepEntity.Rotate(Math.PI / 2, Vector3D.AxisX, centerPoint);

                // Rotate another 90 degrees around the Y-axis if needed
                if (model.BlowerOrientation == "0")
                    angle = 0;
                else if (model.BlowerOrientation == "90")
                    angle = Math.PI / 2;
                else if (model.BlowerOrientation == "180")
                    angle = Math.PI;
                else if (model.BlowerOrientation == "270")
                    angle = (Math.PI * 3) / 2;

                stepEntity.Rotate(Math.PI / 2, Vector3D.AxisX, centerPoint);
                stepEntity.Rotate(angle, Vector3D.AxisZ, centerPoint);
                drawing.Entities.Add(stepEntity);
                i++;
            }

            #region BendRevolve
            LinearPath l1 = new LinearPath(Plane.XY, new Point3D[]
               {
                    new Point3D(400, 0, stepPositionZ+400),
                    new Point3D(400+model.ExhaustWidth, 0, stepPositionZ+400),
                    new Point3D(400+model.ExhaustWidth, model.ExhaustDuctHeight, stepPositionZ+400),
                    new Point3D(400,model.ExhaustDuctHeight, stepPositionZ+400),
                    new Point3D(400,0, stepPositionZ+400)
               });
            LinearPath l2 = new LinearPath(Plane.XY, new Point3D[]
            {
                    new Point3D(400+model.ExhaustThickness, model.ExhaustThickness, stepPositionZ+400),
                    new Point3D(400+(model.ExhaustWidth-model.ExhaustThickness),model.ExhaustThickness, stepPositionZ+400),
                    new Point3D(400+(model.ExhaustWidth-model.ExhaustThickness), (model.ExhaustDuctHeight-model.ExhaustThickness), stepPositionZ+400),
                    new Point3D(400+model.ExhaustThickness,(model.ExhaustDuctHeight-model.ExhaustThickness), stepPositionZ+400),
                    new Point3D(400+model.ExhaustThickness, model.ExhaustThickness, stepPositionZ+400)
            });


            devregion region2 = new devregion(l1);
            devregion region3 = new devregion(l2);
            devregion region = devregion.Difference(region2, region3)[0];
            Brep brep1 = region.RevolveAsBrep(Utility.DegToRad(90), Vector3D.AxisY, new Point3D(0, 200, 0));
            brep1.Translate(stepPositionX + 300, stepPositionY - 365, stepPositionZ + 400);
            //drawing.Entities.Add(brep1, Color.LightGray);

            Material matBend = Material.StructuralSteel;
            matBend = new Material(Materials);
            brep1.Regen(0.1);
            double massofBend = brep1.GetMass(matBend, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Bend_Weight = Math.Round(massofBend, 3);
            model.BendWeight = Bend_Weight;



            #region  Exhaust Duct
            LinearPath d1 = new LinearPath(Plane.XY, new Point3D[]
                {
                    new Point3D(400, 0, stepPositionZ+400),
                    new Point3D(400+model.ExhaustWidth, 0, stepPositionZ+400),
                    new Point3D(400+model.ExhaustWidth, model.ExhaustDuctHeight, stepPositionZ+400),
                    new Point3D(400,model.ExhaustDuctHeight, stepPositionZ+400),
                    new Point3D(400,0, stepPositionZ+400)
                });

            LinearPath d2 = new LinearPath(Plane.XY, new Point3D[]
            {
                    new Point3D(400+model.ExhaustThickness, model.ExhaustThickness, stepPositionZ+400),
                    new Point3D(400+(model.ExhaustWidth-model.ExhaustThickness),model.ExhaustThickness, stepPositionZ+400),
                    new Point3D(400+(model.ExhaustWidth-model.ExhaustThickness), (model.ExhaustDuctHeight-model.ExhaustThickness), stepPositionZ+400),
                    new Point3D(400+model.ExhaustThickness,(model.ExhaustDuctHeight-model.ExhaustThickness), stepPositionZ+400),
                    new Point3D(400+model.ExhaustThickness, model.ExhaustThickness, stepPositionZ+400)
            });

            devregion regionduct1 = new devregion(d1);
            devregion regionduct2 = new devregion(d2);
            devregion regionduct = devregion.Difference(regionduct1, regionduct2)[0];
            Brep ductbrep = regionduct.ExtrudeAsBrep(300);
            ductbrep.Translate(stepPositionX + 300, stepPositionY - 365, stepPositionZ + 400);
            //drawing.Entities.Add(ductbrep, Color.Green);

            Material matduct = Material.StructuralSteel;
            matduct = new Material(Materials);
            ductbrep.Regen(0.1);
            double massofFrame = ductbrep.GetMass(matduct, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity12);
            double Frame_Weight = Math.Round(massofFrame, 3);
            model.DuctWeight = Frame_Weight;


            #endregion


            if (model.BlowerOrientation != "90")
            {
                Brep clonebrep1 = (Brep)brep1.Clone();
                clonebrep1.Rotate(-Math.PI / 2, Vector3D.AxisZ);
                clonebrep1.Rotate(Math.PI / 2, Vector3D.AxisY);
                clonebrep1.Translate(-300, 3000, stepPositionZ + 400);
                drawing.Entities.Add(clonebrep1, Color.LightGray);
            }
            #endregion



            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

            string dwgFilePathforLoft = path + "/Bullows Panel Drawing/" + "loft_with_centrifugal_fan_" + DateTime.Now.ToString("hh-mm") + ".dwg";
            string dwgFilePathforLoftpdf = path + "/Bullows Panel Drawing/" + "loft_with_centrifugal_fan_" + DateTime.Now.ToString("hh-mm") + ".pdf";

            // Save as DWG
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathforLoft);
            dwgg1.DoWork();

            // Save as PDF
            Write3DPdfParams pdf = new Write3DPdfParams(drawing);
            Write3DPDF pdf1 = new Write3DPDF(pdf, dwgFilePathforLoftpdf);
            pdf1.DoWork();

            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePathforLoft
            };
        }

        public PaintBoothclass TubelightCutout(int j, PaintBoothModel model)
        {
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            double tublightHeight = 265;
            double tublightWidth = 1340;
            double PaintBoothHeight = model.H;
            double ComponentInDSize = PanelWidth / 2;//model.D1 + (model.Depth/2);
            double ComponentInWSize = W / 2;//model.W1 + (model.Width / 2);double StandardBend1 = SettingStandardBend1;
            double StandardBend1 = SettingStandardBend1;
            double StandardBend2 = SettingStandardBend2;

            devregion tubelightCut = devregion.CreatePolygon(new Point3D[]
            {
                    new Point3D(ComponentInDSize-(tublightHeight/2), ComponentInWSize-(tublightWidth/2), PaintBoothHeight-10),
                    new Point3D(ComponentInDSize+(tublightHeight/2), (ComponentInWSize-(tublightWidth/2)), PaintBoothHeight-10),
                    new Point3D(ComponentInDSize+(tublightHeight/2), ComponentInWSize+(tublightWidth/2),PaintBoothHeight-10),
                    new Point3D(ComponentInDSize-(tublightHeight/2),ComponentInWSize+(tublightWidth/2), PaintBoothHeight-10)
            });
            drawing.Entities.Add(tubelightCut);
            #region Old Code
            //  var tubelightCut1 = devregion.CreatePolygon(new Point3D[]
            //{
            //          new Point3D(ComponentInDSize-(tublightHeight/2)-100, ComponentInWSize-(tublightWidth/2)-100, PaintBoothHeight+10),
            //          new Point3D(ComponentInDSize+(tublightHeight/2)+100, (ComponentInWSize-(tublightWidth/2))-100, PaintBoothHeight+10),
            //          new Point3D(ComponentInDSize+(tublightHeight/2)+100, ComponentInWSize+(tublightWidth/2)+100,PaintBoothHeight+10),
            //          new Point3D(ComponentInDSize-(tublightHeight/2)-100,ComponentInWSize+(tublightWidth/2)+100, PaintBoothHeight+10)
            //});
            //  Brep brep = tubelightCut1.ExtrudeAsBrep(5);

            // brep.ExtrudeRemove(tubelightCut, 300);
            //drawing.Entities.Add(brep,Color.Red);
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/Bullows Panel Drawing"))
            //    Directory.CreateDirectory(path + "/Bullows Panel Drawing");

            //string dwgFilePathforTubelight = path + "/Bullows Panel Drawing/" + "TubelightCutout_" + DateTime.Now.ToString("hh-mm") + ".dwg";          
            //// Save as DWG
            //WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            //WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathforTubelight);
            //dwgg1.DoWork();

            //return new PaintBoothclass
            //{
            //    drawing = drawing,
            //    lstpath = dwgFilePathforTubelight
            //};
            #endregion
            var tubelightCut1 = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0,0,PanelHeight),
                new Point3D(PanelWidth,0,PanelHeight),
                new Point3D(PanelWidth,W,PanelHeight),
                new Point3D(0,W,PanelHeight),
            });
            Brep brep = tubelightCut1.ExtrudeAsBrep(SheetThickness);

            brep.ExtrudeRemove(tubelightCut, 300);
            drawing.Entities.Add(brep, Color.Green);
            //calculate Weight of panels 
            Material mat = Material.StructuralSteel;
            mat = new Material(Materials);
            brep.Regen(0.1);
            double massofRectangle = brep.GetMass(mat, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity);
            double Rectangle_Weight = Math.Round(massofRectangle, 3);

            LinearPath rail = new LinearPath(new Point3D[]
            {
                new Point3D(0,0,PanelHeight),
                new Point3D(PanelWidth,0,PanelHeight),
                new Point3D(PanelWidth,W,PanelHeight),
                new Point3D(0,W,PanelHeight),
                new Point3D(0,0,PanelHeight),
            });

            var section = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(0, 0, PanelHeight),
                new Point3D(0,SheetThickness,PanelHeight),
                new Point3D(0,SheetThickness,(PanelHeight +(StandardBend2 - SheetThickness))),
                new Point3D(0,StandardBend1,(PanelHeight +(StandardBend2 - SheetThickness))),
                new Point3D(0,StandardBend1, (PanelHeight+StandardBend2)),
                new Point3D(0,0, (PanelHeight+StandardBend2))
            });
            Solid frame = section.SweepAsSolid(rail, 0);
            frame.Translate(0, 0, SheetThickness);

            frame = GenerateHoles(frame);
            //Calculate weight of panels
            Material mat1 = Material.StructuralSteel;
            mat1 = new(Materials);
            frame.Regen(0.1);
            double massofFrame = frame.GetMass(mat1, linearUnitsType.Millimeters, massUnitsType.Kilograms, out double convertedDensity1);
            double Frame_Weight = Math.Round(massofFrame, 3);
            TopPanel_Weight = Rectangle_Weight + Frame_Weight;
            drawing.Entities.Add(frame, Color.Yellow);

            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/TopPanel {j} {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
            #endregion
            return new PaintBoothclass
            {
                drawing = drawing,
                lstpath = dwgFilePath
            };

        }
        public PaintBoothclass DevelopmentForAllPanels(PaintBoothModel model, int side, int j)
        {
            #region Development
            drawing = new DesignDocument();

            SettingStandardBend1 = (double)model.standardbend1;
            SettingStandardBend2 = (double)model.standardbend2;
            SheetThickness = (double)model.SheetThickness;
            PitchDistance = (double)model.PitchDistance;
            SlotDimention = model.SlotDimention;

            StandardBend1 = SettingStandardBend1 - SheetThickness;
            StandardBend2 = SettingStandardBend2 - (SheetThickness * 2);
            int bendlineLength = 50;
            if (PanelWidth > 1000)
                bendlineLength = 160;
            else
                bendlineLength = 110;


            const string Dim = "Dimension";
            drawing.Layers.Add(new Layer(Dim, Color.CornflowerBlue));
            Plane verticalPlane = Plane.XY;
            verticalPlane.Rotate(Math.PI / 2, Vector3D.AxisZ);

            #region Bottom Left
            #region Horizontal
            #region Inner
            //Add InnerRectangle for Left Bottom
            LinearPath Innerrectangleleft = new LinearPath(new Point3D[]
            {
                 new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)+10, 0),
                  new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))+bendlineLength, 0)
            });
            Layer mylayer = new Layer("bendlayer");
            mylayer.Color = Color.Yellow;
            drawing.Layers.Add(mylayer);
            Innerrectangleleft.LayerName = "bendlayer";
            drawing.Entities.Add(Innerrectangleleft);
            #region  upwords Dimensions
            // Calculate the midpoint X position
            double midX = (PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + bendlineLength + 10; // Shift 10 units right

            // Calculate the midpoint Y position
            double midY = 0;  // Keep 

            // Create the Text Entity
            Text UpwordsText = new Text(
                new Point3D(midX, midY, 0), // Position at the midpoint
                "90 Up",  // The text content
               10  // Font size
            )
            {
                LayerName = Dim,  // Assign the same layer as the dimension               
            };
            // Add Text to the Drawing
            drawing.Entities.Add(UpwordsText);
            #endregion


            #endregion
            #region Outer
            LinearPath bendlineBottom = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2))+10,-(StandardBend2)),
                  new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)+bendlineLength),-(StandardBend2)),
            });
            bendlineBottom.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineBottom);
            #region  upwords Dimensions
            // Calculate the midpoint X position
            double bendlineBottomX = (PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + bendlineLength + 10; // Shift 10 units right           
            double bendlineBottomY = -StandardBend2;  // Keep 

            // Create the Text Entity
            Text bendlineBottomText = new Text(
                new Point3D(bendlineBottomX, bendlineBottomY, 0), // Position at the midpoint
                "90 Up",  // The text content
               10  // Font size
            )
            {
                LayerName = Dim,  // Assign the same layer as the dimension               
            };
            // Add Text to the Drawing
            drawing.Entities.Add(bendlineBottomText);
            #endregion
            #endregion
            #endregion

            #region Vertical
            #region Inner
            // inner rectangle left bottom
            LinearPath InnerrectangleleftSide = new LinearPath(new Point3D[]
            {
                 new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2), 10),
                  new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)), bendlineLength)
            });
            InnerrectangleleftSide.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleleftSide);
            #region Dimensions
            // Calculate the midpoint X position
            double InnerRectangleLeftSideX = (PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + 5; // Shift 10 units right

            // Calculate the midpoint Y position
            double InnerRectangleLeftSideY = bendlineLength + 10;

            // Create the Text Entity
            Text InnerRectangleText = new Text(verticalPlane,
                new Point3D(InnerRectangleLeftSideX, InnerRectangleLeftSideY, 0), // Position at the midpoint
                "90 Up",
               10  // Font size
            )
            {
                LayerName = Dim,
            };
            // Add Text to the Drawing
            drawing.Entities.Add(InnerRectangleText);
            #endregion 
            #endregion
            #region Outer
            LinearPath bendlineLeft = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)-(StandardBend2)),10),
                  new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)-(StandardBend2)),bendlineLength),
            });
            bendlineLeft.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineLeft);
            #region Dimensions
            //Adding 90 Up text for this bendline here
            double bendlineLeftX = (PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) + (StandardBend2)) - 65;
            double bendlineLeftY = bendlineLength + 10;
            Text bendlineLeftText = new Text(verticalPlane, new Point3D(bendlineLeftX, bendlineLeftY, 0), "90 Up", 10) { LayerName = Dim };
            drawing.Entities.Add(bendlineLeftText);
            #endregion
            #endregion
            #endregion

            #endregion

            #region Bottom Right

            #region Horizontal
            #region Inner
            //for innerRectangle Right Bottom
            LinearPath InnerrectangleRightBottom = new LinearPath(new Point3D[]
            {
                new Point3D(((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness)-10, 0),
                new Point3D((((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness))-bendlineLength, 0),
            });
            InnerrectangleRightBottom.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleRightBottom);
            #region   Text Dimensions
            // Calculate the midpoint X position
            double BottonRight = (PanelWidth * 2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness) - bendlineLength - 50; // Shift 10 units right

            // Calculate the midpoint Y position
            double BottonRightmidY = 0;  // Keep 

            // Create the Text Entity
            Text BottonRightUpwordsText = new Text(
                new Point3D(BottonRight, BottonRightmidY, 0), // Position at the midpoint
                "90 Up",  // The text content
               10  // Font size
            )
            {
                LayerName = Dim,  // Assign the same layer as the dimension               
            };
            // Add Text to the Drawing
            drawing.Entities.Add(BottonRightUpwordsText);
            #endregion
            #endregion

            #region Outer
            LinearPath bendlineBottomRight = new LinearPath(new Point3D[]
            {
                  new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness)-10,-(StandardBend2)),
              new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness)-bendlineLength,-(StandardBend2)),
            });
            bendlineBottomRight.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineBottomRight);
            #region Text Dimensions
            // Calculate the midpoint X position
            double BottonRightX = (PanelWidth * 2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness) - bendlineLength - 50; // Shift 10 units right           
            double BottonRightY = -(StandardBend2);
            Text BottonRightText = new Text(
                new Point3D(BottonRightX, BottonRightY, 0), "90 Up", 10)
            {
                LayerName = Dim,
            };
            drawing.Entities.Add(BottonRightText);
            #endregion
            #endregion
            #endregion

            #region Vertical
            //  inner rectangle Rightside bottom
            #region Inner
            LinearPath InnerrectangleRightSide = new LinearPath(new Point3D[]
               {
                 new Point3D((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)-(2*SheetThickness)), 10),
                  new Point3D((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)-(2*SheetThickness)), bendlineLength),
               });
            InnerrectangleRightSide.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleRightSide);
            #region Dimensions
            // Calculate the midpoint X position
            double InnerRectangleRightSideTopX = (PanelWidth * 2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)); // Shift 10 units right

            // Calculate the midpoint Y position
            double InnerRectangleRightSideTopY = bendlineLength + 10;

            // Create the Text Entity
            Text InnerRectangleRightTopText1 = new Text(verticalPlane,
                new Point3D(InnerRectangleRightSideTopX, InnerRectangleRightSideTopY, 0), // Position at the midpoint
                "90 Up",
               10  // Font size
            )
            {
                LayerName = Dim,
            };
            // Add Text to the Drawing
            drawing.Entities.Add(InnerRectangleRightTopText1);
            #endregion 
            #endregion

            #region Outer 
            LinearPath bendlineright = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2)+(StandardBend2))-2*SheetThickness,10),
                  new Point3D((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2)+(StandardBend2))-2*SheetThickness,bendlineLength),
           });
            bendlineright.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineright);
            //Adding 90 Up text for this bendline here
            double bendlinerightX = (PanelWidth * 2) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) + (StandardBend2)) - 2 * SheetThickness;
            double bendlinerightY = bendlineLength + 10;
            Text bendlinerightText = new Text(verticalPlane, new Point3D(bendlinerightX, bendlinerightY, 0), "90 Up", 10) { LayerName = Dim };
            drawing.Entities.Add(bendlinerightText);
            #endregion

            #endregion

            #endregion

            #region Top Left
            #region Horizontal
            #region Inner
            //Add InnerRectangle for Left Top
            LinearPath InnerrectangleleftTop = new LinearPath(new Point3D[]
            {
                 new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)+10,  PanelHeight - 2 * SheetThickness),
                  new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)+bendlineLength), ( PanelHeight - 2 * SheetThickness))
            });
            InnerrectangleleftTop.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleleftTop);
            #region   Left Top upwords Dimensions
            // Calculate the midpoint X position
            double LeftTopbendX = (PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + bendlineLength + 10; // Shift 10 units right

            // Calculate the midpoint Y position
            double LeftTopbendinY = PanelHeight - 2 * SheetThickness;  // Keep 

            // Create the Text Entity
            Text leftTopUpwordsText = new Text(
                new Point3D(LeftTopbendX, LeftTopbendinY, 0), // Position at the midpoint
                "90 Up",  // The text content
               10  // Font size
            )
            {
                LayerName = Dim,  // Assign the same layer as the dimension               
            };
            // Add Text to the Drawing
            drawing.Entities.Add(leftTopUpwordsText);
            #endregion
            #endregion

            #region Outer

            Line bendlineBottom1 = new Line(Plane.XY,

                 new Point3D((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + 10, (PanelHeight - 2 * SheetThickness) + (StandardBend2)),
                 new Point3D((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) + bendlineLength), (PanelHeight - 2 * SheetThickness) + StandardBend2)
           );
            bendlineBottom1.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineBottom1);

            #region Dimentions
            double x = (((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2))) + ((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) + bendlineLength))) / 2;
            LinearDim bendlineBottom1Dim = new LinearDim(Plane.XY,

            bendlineBottom1.StartPoint,
            bendlineBottom1.EndPoint,
            new Point3D(bendlineBottom1.MidPoint.X, bendlineBottom1.MidPoint.Y + 30), 10
                )
            { ExtLineOffset = 0 };
            drawing.Entities.Add(bendlineBottom1Dim, Dim);
            Text bendlineBottom1Text = new Text(bendlineBottom1.EndPoint.X + 30, bendlineBottom1.EndPoint.Y, 0, "90 Up", 10)
            {
                LayerName = Dim,
                Alignment = Text.alignmentType.MiddleCenter
            };
            drawing.Entities.Add(bendlineBottom1Text);
            #endregion
            #endregion
            #endregion

            #region Vertical
            #region Inner
            // inner rectangle left Top
            LinearPath InnerrectangleleftSideTop = new LinearPath(new Point3D[]
            {
                 new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2), (PanelHeight - 2 * SheetThickness)-10),
                  new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)),((PanelHeight - 2 * SheetThickness)-bendlineLength) )
            });
            InnerrectangleleftSideTop.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleleftSideTop);
            #region Dimensions
            //// Calculate the midpoint X position
            //double InnerRectangleLeftSideTopX = (PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - 2; // Shift 10 units right

            //// Calculate the midpoint Y position
            //double InnerRectangleLeftSideTopY = ((PanelHeight - 2 * SheetThickness) - bendlineLength) - 20;

            // Create the Text Entity
            Text InnerRectangleLeftTopText = new Text(verticalPlane,
                new Point3D(InnerrectangleleftSideTop.EndPoint.X, InnerrectangleleftSideTop.EndPoint.Y - 30, 0), // Position at the midpoint
                "90 Up",
               10  // Font size
            )
            {
                LayerName = Dim,
                Alignment = Text.alignmentType.MiddleCenter
            };
            // Add Text to the Drawing
            drawing.Entities.Add(InnerRectangleLeftTopText);
            #endregion
            #endregion

            #region Outer
            LinearPath bendlineLeft1 = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)-(StandardBend2)),(PanelHeight - 2 * SheetThickness)-10),
                 new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)-(StandardBend2)),(PanelHeight - 2 * SheetThickness)-bendlineLength)
            });
            bendlineLeft1.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineLeft1);
            #region Dimensions
            //Adding 90 Up text for this bendline here
            //double bendlineLeft1X = (PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) + (StandardBend2)) - 65;
            //double bendlineLeft1Y = PanelHeight - 2 * SheetThickness - bendlineLength - 35;
            //bendlineLeft1X = bendlineLeft1.EndPoint.X;
            //bendlineLeft1Y = bendlineLeft1.EndPoint.Y - 30;

            Text bendlineLeft1Text = new Text(verticalPlane, new Point3D(bendlineLeft1.EndPoint.X, bendlineLeft1.EndPoint.Y - 30, 0), "90 Up", 10)
            {
                LayerName = Dim,
                Alignment = Text.alignmentType.MiddleCenter
            };
            drawing.Entities.Add(bendlineLeft1Text);
            #endregion
            #endregion

            #endregion
            #endregion

            #region Top Right

            #region Horizontal
            #region Inner
            //for innerRectangle Right Top
            LinearPath InnerrectangleRightTop = new LinearPath(new Point3D[]
            {
                new Point3D(((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness)-10,  PanelHeight - 2 * SheetThickness),
                new Point3D((((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness))-bendlineLength,  PanelHeight - 2 * SheetThickness),
            });
            InnerrectangleRightTop.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleRightTop);
            #region   Right Top upwords Dimensions
            // Calculate the midpoint X position
            double RightTopbendX = (PanelWidth * 2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness) - bendlineLength - 50; // Shift 10 units right

            // Calculate the midpoint Y position
            double RightTopbendinY = PanelHeight - 2 * SheetThickness;  // Keep 

            // Create the Text Entity
            Text RightTopUpwordsText = new Text(
                new Point3D(RightTopbendX, RightTopbendinY, 0), // Position at the midpoint
                "90 Up",
               10  // Font size
            )
            {
                LayerName = Dim,
            };
            // Add Text to the Drawing
            drawing.Entities.Add(RightTopUpwordsText);
            #endregion
            #endregion

            #region Outer

            LinearPath bendlineBottomRightTop = new LinearPath(new Point3D[]
            {
                new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness)-10,(PanelHeight - 2 * SheetThickness)+(StandardBend2)),
                new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness)-bendlineLength,(PanelHeight - 2 * SheetThickness)+StandardBend2)

            });
            bendlineBottomRightTop.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineBottomRightTop);
            Text bendlineBottomRightTopText = new Text(bendlineBottomRightTop.EndPoint.X - 30, bendlineBottomRightTop.EndPoint.Y, "90 Up", 10)
            {
                LayerName = Dim,
                Alignment = Text.alignmentType.MiddleCenter
            };
            drawing.Entities.Add(bendlineBottomRightTopText);

            #endregion
            #endregion

            #region Vertical
            #region Inner
            LinearPath InnerrectangleRightSideTop = new LinearPath(new Point3D[]
  {
                 new Point3D((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)-(2*SheetThickness)), (PanelHeight - 2 * SheetThickness)-10),
                  new Point3D((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)-(2*SheetThickness)),((PanelHeight - 2 * SheetThickness)-bendlineLength) )
  });
            InnerrectangleRightSideTop.LayerName = "bendlayer";
            drawing.Entities.Add(InnerrectangleRightSideTop);
            #region Dimensions
            // Calculate the midpoint X position
            double RectangleRightSideTopX = (PanelWidth * 2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness); // Shift 10 units right

            // Calculate the midpoint Y position
            double RectangleRightSideTopY = ((PanelHeight - 2 * SheetThickness) - bendlineLength) - 30;

            // Create the Text Entity
            Text RectangleRightTopText1 = new Text(verticalPlane,
                new Point3D(RectangleRightSideTopX, RectangleRightSideTopY, 0), // Position at the midpoint
                "90 Up",
               10  // Font size
            )
            {
                LayerName = Dim,
                Alignment = Text.alignmentType.MiddleCenter
            };
            // Add Text to the Drawing
            drawing.Entities.Add(RectangleRightTopText1);
            #endregion 
            #endregion

            #region Outer
            LinearPath bendlineright1 = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2)+(StandardBend2))-2*SheetThickness,(PanelHeight - 2 * SheetThickness)-10),
                 new Point3D((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2)+(StandardBend2))-2*SheetThickness,(PanelHeight - 2 * SheetThickness)-bendlineLength),
           });
            bendlineright1.LayerName = "bendlayer";
            drawing.Entities.Add(bendlineright1);
            #region Dimensions
            Text bendlineright1Text = new Text(verticalPlane, new Point3D(bendlineright1.EndPoint.X, bendlineright1.EndPoint.Y - 30, 0), "90 Up", 10)
            {
                LayerName = Dim,
                Alignment = Text.alignmentType.MiddleCenter
            };
            drawing.Entities.Add(bendlineright1Text);
            #endregion
            #endregion

            #endregion

            #endregion

            #region Outer Bottom Line
            // //for bottom
            LinearPath linearBottom = new LinearPath(new Point3D[]
            {
                  new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2), 0),
                  new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)),-(StandardBend2)),
                  new Point3D(((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))+(StandardBend1),-(StandardBend2+StandardBend1)),
                  new Point3D(((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - ((- SheetThickness)+StandardBend1),-(StandardBend1+StandardBend2)),
                  new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))-(2*SheetThickness)),-(StandardBend2)),
                  new Point3D(((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness),0),
            });
            drawing.Entities.Add(linearBottom, Color.White);
            //LinearBottomOuter
            #region Outer Dimmention
            #region Outer 1


            x = ((0) + (-(StandardBend2))) / 2;
            LinearDim linearBottomdim = new(verticalPlane,
               new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2), 0),
                  new Point3D((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)), -(StandardBend2)),
                  new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2) - 20, x), 10)
            { ArrowheadSize = 10 };
            drawing.Entities.Add(linearBottomdim, Dim);
            #endregion

            #region Outer 2
            x = (((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2))) + (((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + (StandardBend1))) / 2;
            linearBottomdim = new(Plane.XY,
                new Point3D((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)), -(StandardBend2)),
                  new Point3D(((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + (StandardBend1), -(StandardBend2 + StandardBend1)),
                 new Point3D(x - 10, -(StandardBend2 + StandardBend1) - 40), 10)
            { ArrowheadSize = 10 };
            drawing.Entities.Add(linearBottomdim, Dim);
            #endregion
            #region Outer 3
            x = ((((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) + (StandardBend1 - SheetThickness)) + (((PanelWidth * 2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - ((-SheetThickness) + StandardBend1))) / 2;
            linearBottomdim = new(Plane.XY,
                  new Point3D((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)), -(StandardBend2)),
                 new Point3D(((PanelWidth * 2) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) - (2 * SheetThickness)), -(StandardBend2)),
                 new Point3D(x, -(StandardBend2 + StandardBend1) - 40), 10)
            { ArrowheadSize = 10 };
            drawing.Entities.Add(linearBottomdim, Dim);
            #endregion
            #endregion
            //For Top

            double Panelheight = PanelHeight - 2 * SheetThickness;
            LinearPath linearTop = new LinearPath(new Point3D[]
            {
                 new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2), Panelheight),
                  new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)),StandardBend2 + Panelheight),
                  new Point3D(((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))+(StandardBend1),Panelheight+(StandardBend2+StandardBend1)),
                  new Point3D(((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness+StandardBend1),Panelheight+(StandardBend1+StandardBend2)),
                    new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness),Panelheight+(StandardBend2)),
                  new Point3D(((PanelWidth*2) + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (2 * SheetThickness),Panelheight),
            });

            drawing.Entities.Add(linearTop, Color.White);
            // For LeftSide

            LinearPath linearLeft = new LinearPath(new Point3D[]
            {
                new Point3D(PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2), 0),
                new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)-(StandardBend2)),0),
                new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))-(StandardBend1+StandardBend2), StandardBend1),
                new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))-(StandardBend1+StandardBend2),Panelheight-StandardBend1),
                 new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))-(StandardBend2),Panelheight),
                new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)),Panelheight),
            });
            drawing.Entities.Add(linearLeft, Color.White);
            #region Dimension
            double y = ((StandardBend1) + (Panelheight - StandardBend1)) / 2;//calculate midpoint of y for placing dimention text
            LinearDim linearLefttDim = new(verticalPlane,
                new Point3D((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) - (StandardBend2)), 0),
                 new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (StandardBend2), Panelheight),
                new Point3D((PanelWidth + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)) - (StandardBend1 + StandardBend2) - 40, y), 20);
            drawing.Entities.Add(linearLefttDim, Dim);

            #endregion

            //For Right Sides

            LinearPath linearRight = new LinearPath(new Point3D[]
            {
               new Point3D((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2)-2*SheetThickness), 0),
                new Point3D((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2)+(StandardBend2-2*SheetThickness)),0),
                new Point3D(((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))+(StandardBend1+StandardBend2)-2*SheetThickness), StandardBend1),
                new Point3D(((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))+(StandardBend1+StandardBend2)-2*SheetThickness),Panelheight-StandardBend1),

                 new Point3D((PanelWidth*2 + ((PanelWidth / 2 + PanelWidth / 2) * 0.2))+(StandardBend2-2*SheetThickness),Panelheight),
                new Point3D(((PanelWidth*2+ ((PanelWidth / 2 + PanelWidth / 2) * 0.2))-2*SheetThickness),Panelheight),
            });
            drawing.Entities.Add(linearRight, Color.White);
            #endregion


            #region Notching
            LinearPath BottomLeftBottmNotching = new LinearPath(new Point3D[]
            {
                new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)),-(StandardBend2)),
                new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2))+(StandardBend1),-(StandardBend2+StandardBend1)),
            });
            //Point3D trimPoint = new Point3D(-StandardBend1, 0, 0);
            //BottomLeftNotching.TrimBy(trimPoint, true);
            drawing.Entities.Add(BottomLeftBottmNotching, Color.White);

            //Left bottom Notching 
            LinearPath BottomLeftBottmNotching1 = new LinearPath(new Point3D[]
            {
                new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2))-(StandardBend2),0),
                new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2))-(StandardBend2+StandardBend1),StandardBend1),

            });
            drawing.Entities.Add(BottomLeftBottmNotching1, Color.White);
            //for right side 
            LinearPath BottomRightBottmNotching = new LinearPath(new Point3D[]
            {
                new Point3D(((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness),-(StandardBend2)),


                new Point3D((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2))-(StandardBend1-SheetThickness),-(StandardBend2+StandardBend1)),
            });

            drawing.Entities.Add(BottomRightBottmNotching, Color.White);
            LinearPath BottomRightBottmNotching1 = new LinearPath(new Point3D[]
            {
                new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2)))+(StandardBend2 -(2* SheetThickness)),0),
                new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))+(StandardBend2+StandardBend1))-2*SheetThickness,StandardBend1),

            });
            drawing.Entities.Add(BottomRightBottmNotching1, Color.White);
            //For TopRight Side

            LinearPath BottomRightTopNotching = new LinearPath(new Point3D[]
            {
                 new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))+(StandardBend2))-2*SheetThickness,PanelHeight - 2 * SheetThickness),
                new Point3D(((PanelWidth * 2)+((PanelWidth/2+PanelWidth/2)*(0.2))+(StandardBend2+StandardBend1)-2*SheetThickness),(PanelHeight - 2 * SheetThickness)-StandardBend1),
            });

            drawing.Entities.Add(BottomRightTopNotching, Color.White);
            LinearPath BottomRightTopNotching1 = new LinearPath(new Point3D[]
            {
                 new Point3D(((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness),(PanelHeight - 2 * SheetThickness)+(StandardBend2)),
                new Point3D((((PanelWidth*2)+((PanelWidth/2+PanelWidth/2)*(0.2))-2*SheetThickness))-StandardBend1,(PanelHeight - 2 * SheetThickness)+(StandardBend2+StandardBend1)),

            });
            drawing.Entities.Add(BottomRightTopNotching1, Color.White);
            //For Left Top Side
            LinearPath TopleftNotching = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2))-(StandardBend2),PanelHeight - 2 * SheetThickness),
                new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2))-(StandardBend2+StandardBend1),(PanelHeight - 2 * SheetThickness)-StandardBend1),
            });

            drawing.Entities.Add(TopleftNotching, Color.White);
            LinearPath TopleftNotching1 = new LinearPath(new Point3D[]
            {
                 new Point3D((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)),(PanelHeight - 2 * SheetThickness)+(StandardBend2)),
                new Point3D(((PanelWidth)+((PanelWidth/2+PanelWidth/2)*(0.2)))+StandardBend1,(PanelHeight - 2 * SheetThickness)+(StandardBend2+StandardBend1)),

            });
            drawing.Entities.Add(TopleftNotching1, Color.White);
            #endregion
            #region Create Slots
            string[] dimensions = SlotDimention.Split('-');
            if (dimensions.Length != 2)
            {
                throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
            }

            // Parse the width and height
            if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
            {
                throw new ArgumentException("Slot dimensions must be numeric values.");
            }

            #region holes on PanelHeight


            double PanelHeightSheetThickness = PanelHeight - 2 * SheetThickness;
            double divisionresult = PanelHeightSheetThickness / PitchDistance;

            // Get the number of holes
            int wholenumberpart = (int)Math.Round(divisionresult);

            // Calculate remaining space and distribute evenly at start & end
            double multipliedresult = (wholenumberpart - 1) * PitchDistance;
            double samespacedivide = (PanelHeightSheetThickness - multipliedresult) / 2;

            // **Shift first hole by 1mm closer**
            samespacedivide -= 1;

            // Create slots and add them to the drawing
            for (int i = 0; i < wholenumberpart; i++)
            {
                double centerz = samespacedivide + (i * PitchDistance);

                // Create first slot
                devregion slot = devregion.CreateSlot(Plane.XY,
                    (PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) - (StandardBend2 / 2)),
                    centerz,
                    (slotLength - slotWidth),
                    slotWidth / 2,
                    1.5708);

                // Create second slot
                devregion slot1 = devregion.CreateSlot(Plane.XY,
                    (PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2) - (StandardBend2 / 2) + (PanelWidth + StandardBend2)),
                    centerz,
                    (slotLength - slotWidth),
                    slotWidth / 2,
                    1.5708);

                slot.Color = Color.Yellow;

                // Add slots to drawing
                drawing.Entities.Add(slot, Color.White);
                drawing.Entities.Add(slot1, Color.White);
            }
            #endregion



            #region Holes on PanelWidth

            // Calculate available panel width excluding sheet thickness
            double PanelWidthSheetThickness = PanelWidth - 2 * SheetThickness;

            // Calculate number of holes that fit within the panel width
            double divisionresult1 = PanelWidthSheetThickness / PitchDistance;
            int wholenumberpart1 = (int)Math.Floor(divisionresult1);

            // Calculate the total occupied space by the slots
            double multipliedresult1 = wholenumberpart1 * PitchDistance;

            // Calculate the remaining space to distribute evenly
            double samespacedivide1 = (PanelWidthSheetThickness - multipliedresult1) / 2;

            // Adjust first slot position by shifting it 1mm closer
            samespacedivide1 -= 1;

            // Loop through each hole position and create slots
            for (int i = 0; i <= wholenumberpart1; i++)
            {
                // Calculate the Y-coordinate for slot placement
                double centery = samespacedivide1 + (i * PitchDistance);

                // Create slot at the bottom of the panel
                devregion slot2 = devregion.CreateSlot(Plane.XY,
                    ((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + centery),
                    (-StandardBend2 / 2),
                    (slotLength - slotWidth),
                    slotWidth / 2,
                    0);

                // Create slot at the top of the panel
                devregion slot3 = devregion.CreateSlot(Plane.XY,
                    ((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + centery),
                    (PanelHeight + StandardBend2 / 2),
                    (slotLength - slotWidth),
                    slotWidth / 2,
                    0);

                // Adjust slot position for equal spacing
                slot2.Translate(0, 0, samespacedivide1 / 2);

                // Set color for visualization
                slot2.Color = Color.Yellow;

                // Add slots to the drawing
                drawing.Entities.Add(slot2, Color.White);
                drawing.Entities.Add(slot3, Color.White);
            }
            #endregion
            #region Dimension
            double centerz1 = (samespacedivide1 / 2 + (3) * PitchDistance) + 20;
            double centerz2 = (samespacedivide1 / 2 + (4) * PitchDistance) + 20;
            x = ((((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + centerz1)) + (((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + centerz2))) / 2;
            LinearDim slotsDim = new LinearDim(Plane.XY,
              new Point3D(((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + centerz1), (-StandardBend2 / 2)),
               new Point3D(((PanelWidth) + ((PanelWidth / 2 + PanelWidth / 2) * (0.2)) + centerz2), (-StandardBend2 / 2)),
              new Point3D(x, StandardBend2), 20);

            drawing.Entities.Add(slotsDim, Dim);


            #endregion
            #endregion
            #endregion development
            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();


            if (!Directory.Exists(path + "/Development"))
                Directory.CreateDirectory(path + "/Development");

            var dwgFilePathfordevelopment = "";
            dwgFilePathfordevelopment = (side == 0) ?
            dwgFilePathfordevelopment = Path.Combine(path, "Development", "Right Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg") :
            dwgFilePathfordevelopment = Path.Combine(path, "Development", "Left Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");

            switch (side)
            {
                case 0:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "Right Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
                case 1:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "Left Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
                case 2:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "D3 Right Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
                case 3:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "D3 Left Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
                case 4:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "Top Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
                case 5:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "Back Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
                case 6:
                    dwgFilePathfordevelopment = Path.Combine(path, "Development", "Front Panel Development" + j + DateTime.Now.ToString("hh-mm") + ".dwg");
                    break;
            }


            // var dwgFilePathfordevelopment = Path.Combine(path, "Development", "Development" + DateTime.Now.ToString("hh-mm") + ".dwg");
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
            dwgg1.DoWork();
            #endregion

            return new PaintBoothclass
            {
                developmentpath = dwgFilePathfordevelopment,
            };

        }
        public string devlopmentForCSection(BendSectionModel model)
        {
            double X = (double)model.Length;
            decimal H = model.H - 2 * model.T;
            decimal W = model.W - model.T;
            decimal Y = H + (2 * W);

            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            var rectangle = devregion.CreatePolygon(Plane.XY, new Point3D[]
            {
                new Point3D(0,0),
                new Point3D(X,0),
                new Point3D(X,(double)Y),
                new Point3D(0,(double)Y),
            });
            drawing.Entities.Add(rectangle, Color.White);
            LinearPath BendctangleleftBottom = new LinearPath(new Point3D[]
            {
                 new Point3D(0,(double)(model.W-model.T)),
                  new Point3D(50,(double)(model.W-model.T))
            });
            drawing.Entities.Add(BendctangleleftBottom, Color.Yellow);
            LinearPath BendctangleleftTop = new LinearPath(new Point3D[]
            {
                 new Point3D(0,(double)(Y-(model.W-model.T))),

                  new Point3D(50,(double)(Y-(model.W-model.T)))

            });
            drawing.Entities.Add(BendctangleleftTop, Color.Yellow);
            LinearPath BendctangleRightBottom = new LinearPath(new Point3D[]
            {
                 new Point3D(X,(double)(model.W-model.T)),
                  new Point3D(X-50,(double)(model.W-model.T))
            });
            drawing.Entities.Add(BendctangleRightBottom, Color.Yellow);
            LinearPath BendctangleRightTop = new LinearPath(new Point3D[]
            {
                 new Point3D(X,(double)(Y-(model.W-model.T))),
                  new Point3D(X-50,(double)(Y-(model.W-model.T)))
            });
            drawing.Entities.Add(BendctangleRightTop, Color.Yellow);


            #region WriteFile
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/Bend Development"))
                Directory.CreateDirectory(path + "/Bend Development");
            var dwgFilePathfordevelopment = Path.Combine(path, "Bend Development", "BendDevelopment" + DateTime.Now.ToString("hh-mm") + ".dwg");
            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
            dwgg1.DoWork();
            return dwgFilePathfordevelopment;
            #endregion
        }
        public PaintBoothclass Civil(PaintBoothModel model)
        {
            #region Layers
            drawing = new();
            drawing.Units = linearUnitsType.Millimeters;
            drawing.Layers.Add(new Layer("CivilTank", Color.WhiteSmoke));
            drawing.Layers.Add(new Layer("ZigZag", Color.GreenYellow));
            drawing.Layers.Add(new Layer("Rectangle", Color.White));
            drawing.Layers.Add(new Layer("SmallRectangle", Color.YellowGreen));
            #endregion

            #region Variable assigning

            double extraSpace = 100;
            double x = model.D + extraSpace;
            double y = model.W + 3000;
            double z = -70;
            double stepW = 40;
            double stepH = 40;
            double drop1 = 115;
            double drop2 = 125;
            double rectW = 1330;
            double rectH = 1198;
            double zigH = 237;
            double extrude = 2100;

            double zZigTop = 0;
            double zRectTop = -zigH;
            double zRectBot = -zigH - rectH;

            // ── Centre both pits using BetweenCenterDistance ──────────
            double slabCenterX = model.D / 2.0;

            // Zigzag profile runs from stepW*1=40 to stepW*7+rectW=1610
            // True centre = 40 + (1570/2) = 825
            double zigProfileStart = stepW * 1;
            double zigProfileEnd = (stepW * 7) + rectW;
            double zigProfileWidth = zigProfileEnd - zigProfileStart;
            double zigProfileCenter = zigProfileStart + (zigProfileWidth / 2.0);

            // Pit centres — symmetric around slab centre
            double leftPitCenterX = slabCenterX - (model.BetweenCenterDistance / 2.0);
            double rightPitCenterX = slabCenterX + (model.BetweenCenterDistance / 2.0);

            // Left edge of each zigzag = pit centre - profile centre
            double pitOffsetX = leftPitCenterX - zigProfileCenter;
            double rightPitOffsetX = rightPitCenterX - zigProfileCenter;

            // Rectangle X faces
            // Left pit:  profile at pitX1, extrudes +rectWidth rightward
            // Right pit: profile at rX2,   extrudes -rectWidth leftward
            double pitX1 = pitOffsetX + (stepW * 4);
            double rX2 = rightPitOffsetX + (stepW * 4) + rectW; // ← right face of right pit

            // ── Y extents ─────────────────────────────────────────────
            double yStart = model.W1;
            double yEnd = model.W1 + model.Width;
            double pitDepth = yEnd - yStart;
            #endregion

            #region Civil tank
            // ── 1. BASE CONCRETE SLAB ─────────────────────────────────
            var civilTank = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(-extraSpace, -3000, z),
                new Point3D(x,           -3000, z),
                new Point3D(x,            y,          z),
                new Point3D(-extraSpace,  y,          z)
            });
            Brep baseBrep = civilTank.ExtrudeAsBrep(-extrude);
            drawing.Entities.Add(baseBrep, "CivilTank");
            #endregion

            #region ZigZag frame
            // ── 2. ZIGZAG FRAME ───────────────────────────────────────
            Point3D[] frameProfile = new Point3D[]
            {
                new Point3D(stepW * 1,         0, zZigTop),
                new Point3D(stepW * 1,         0, zZigTop - stepH),
                new Point3D(stepW * 2,         0, zZigTop - stepH),
                new Point3D(stepW * 2,         0, zZigTop - drop1),
                new Point3D(stepW * 3,         0, zZigTop - drop1),
                new Point3D(stepW * 3,         0, zZigTop - drop1 - drop2 + stepH),
                new Point3D(stepW * 4,         0, zZigTop - drop1 - drop2 + stepH),
                new Point3D(stepW * 4,         0, zRectTop),
                new Point3D(stepW * 4 + rectW, 0, zRectTop),
                new Point3D(stepW * 4 + rectW, 0, zZigTop - drop1 - drop2 + stepH),
                new Point3D(stepW * 5 + rectW, 0, zZigTop - drop1 - drop2 + stepH),
                new Point3D(stepW * 5 + rectW, 0, zZigTop - drop1),
                new Point3D(stepW * 6 + rectW, 0, zZigTop - drop1),
                new Point3D(stepW * 6 + rectW, 0, zZigTop - stepH),
                new Point3D(stepW * 7 + rectW, 0, zZigTop - stepH),
                new Point3D(stepW * 7 + rectW, 0, zZigTop),
            };

            // Left pit zigzag — cuts in -Y direction
            var frameRegion = devregion.CreatePolygon(frameProfile);
            frameRegion.Translate(pitOffsetX, yStart, 0);
            baseBrep.ExtrudeRemove(frameRegion, -pitDepth);
            drawing.Entities.Add(baseBrep, "ZigZag");

            // Right pit zigzag — cuts in -Y direction, different X
            var frameRegionRight = devregion.CreatePolygon(frameProfile);
            frameRegionRight.Translate(rightPitOffsetX, yStart, 0);
            baseBrep.ExtrudeRemove(frameRegionRight, -pitDepth);
            drawing.Entities.Add(baseBrep, "ZigZag");
            #endregion

            #region Rectangular Pit
            // ── 3. RECTANGULAR PIT ────────────────────────────────────
            double rectWidth = rectW;   // = 1330
            double rectHeight = rectH;   // = 1198
            double rY1 = yStart;
            double rY2 = yEnd;
            double rZ1 = zRectTop;
            double rZ2 = zRectTop - rectHeight;

            #region Left pit
            // Left pit — profile at LEFT face (pitX1), extrude +rectWidth rightward
            Line l1 = new Line(new Point3D(pitX1, rY1, rZ1),
                               new Point3D(pitX1, rY1, rZ2));
            Line l2 = new Line(new Point3D(pitX1, rY1, rZ2),
                               new Point3D(pitX1, rY2, -389));
            Line l3 = new Line(new Point3D(pitX1, rY2, -389),
                               new Point3D(pitX1, rY2, rZ1));
            Line l4 = new Line(new Point3D(pitX1, rY2, rZ1),
                               new Point3D(pitX1, rY1, rZ1));

            CompositeCurve profileLeft = new CompositeCurve(new ICurve[] { l1, l2, l3, l4 });
            var pitRegionLeft = new devregion(profileLeft);
            baseBrep.ExtrudeRemove(pitRegionLeft, rectWidth);   // ← +X rightward
            drawing.Entities.Add(baseBrep, "Rectangle");

            #region small rectangles with 1330*1000 in XZ plane
            // ── PIT OPENING RECTANGLES IN XZ PLANE ───────────────────────
            // XZ plane = vertical section view
            // X = pit width (1330mm)
            // Z = pit depth downward (1000mm, negative Z direction)

            double pitRectW = 1330;    // width in X
            double pitRectH = 1000;    // height in Z (downward)


            // Y position — fixed slice through the pit
            double rectYPos = yStart;   // single Y plane for XZ rectangle

            // ── Left pit rectangle in XZ plane ────────────────────────
            double leftRectX1 = leftPitCenterX - pitRectW / 2.0;
            double leftRectX2 = leftPitCenterX + pitRectW / 2.0;
            double rectZBot = zRectTop - rectHeight;        // = -237 - 1198 = -1435  (bottom of pit)
            double rectZTop = rectZBot + pitRectH;          // = -1435 + 1000 = -435  (1000mm up from bottom)


            // ── Right pit rectangle in XZ plane ───────────────────────
            double rightRectX1 = rightPitCenterX - pitRectW / 2.0;
            double rightRectX2 = rightPitCenterX + pitRectW / 2.0;
            double rectArc = 500;
            double extrudeArc = 1800 - rectArc;

            // ── Left pit small rectangle in XZ plane ──────────────────
            var leftSmallRect = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(leftRectX1,  rectYPos, rectZTop),   // top-left
                new Point3D(leftRectX2,  rectYPos, rectZTop),   // top-right
                new Point3D(leftRectX2,  rectYPos, rectZBot),   // bottom-right
                new Point3D(leftRectX1,  rectYPos, rectZBot)    // bottom-left
            });
            Brep leftSmallBrep = leftSmallRect.ExtrudeAsBrep(-extrudeArc);
            //leftSmallBrep.RevolveRemove(leftSmallRect, devDept.Geometry.Utility.DegToRad(-90), Vector3D.AxisY, cornerPoint);
            drawing.Entities.Add(leftSmallBrep, "SmallRectangle");



            // ── Right pit small rectangle in XZ plane ─────────────────
            var rightSmallRect = devregion.CreatePolygon(new Point3D[]
            {
                new Point3D(rightRectX1, rectYPos, rectZTop),   // top-left
                new Point3D(rightRectX2, rectYPos, rectZTop),   // top-right
                new Point3D(rightRectX2, rectYPos, rectZBot),   // bottom-right
                new Point3D(rightRectX1, rectYPos, rectZBot)    // bottom-left
            });
            Brep rightSmallBrep = rightSmallRect.ExtrudeAsBrep(-extrudeArc);
            drawing.Entities.Add(rightSmallBrep, "SmallRectangle");
            #endregion

            #endregion
            #region Right pit
            // Right pit — profile at RIGHT face (rX2), extrude -rectWidth leftward
            Line r1 = new Line(new Point3D(rX2, rY1, rZ1),
                               new Point3D(rX2, rY1, rZ2));
            Line r2 = new Line(new Point3D(rX2, rY1, rZ2),
                               new Point3D(rX2, rY2, -389));
            Line r3 = new Line(new Point3D(rX2, rY2, -389),
                               new Point3D(rX2, rY2, rZ1));
            Line r4 = new Line(new Point3D(rX2, rY2, rZ1),
                               new Point3D(rX2, rY1, rZ1));

            CompositeCurve profileRight = new CompositeCurve(new ICurve[] { r1, r2, r3, r4 });
            var pitRegionRight = new devregion(profileRight);
            baseBrep.ExtrudeRemove(pitRegionRight, -rectWidth);  // ← -X leftward
            drawing.Entities.Add(baseBrep, "Rectangle");
            #endregion




            #endregion

            #region Write files
            // ── Save ──────────────────────────────────────────────────
            string path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/CivilTank"))
                Directory.CreateDirectory(path + "/CivilTank");

            string ts = DateTime.Now.ToString("hh-mm");
            string dwgPath = path + "/CivilTank/CivilTank" + ts + ".dwg";
            string pdfPath = path
                + "/CivilTank/CivilTank" + ts + ".pdf";

            new WriteAutodesk(new WriteAutodeskParams(drawing), dwgPath).DoWork();
            new Write3DPDF(new Write3DPdfParams(drawing), pdfPath).DoWork();
            #endregion

            return new PaintBoothclass { drawing = drawing, lstpath = dwgPath };
        }


        #region Export Views
        public static List<string> ExportAllViewsWithSection(DesignDocument designDoc, string outputFolder)// Y-coordinate for the XZ-plane section
        {
            var exportedFiles = new List<string>();
            // ---  Export Top, Front, Side Views ---
            exportedFiles.Add(ExportView(designDoc, outputFolder, viewType.Top, "Top"));
            exportedFiles.Add(ExportView(designDoc, outputFolder, viewType.Front, "Front"));
            exportedFiles.Add(ExportView(designDoc, outputFolder, viewType.Left, "Side"));
            return exportedFiles;
        }
        private static string ExportView(DesignDocument designDoc, string outputFolder, viewType vType, string suffix)
        {
            string filePath = Path.Combine($"Bullows_{suffix}_{DateTime.Now:yyyyMMdd_HHmmss}.dwg");
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            //if (!Directory.Exists(path + "/Bullows Panel Drawing"))
            //    Directory.CreateDirectory(path + "/Bullows Panel Drawing");
            var settings = new HiddenLinesViewSettingsEx(vType, designDoc)
            {
                KeepEntityColor = true,
                KeepHiddenSegments = true,   //  hidden lines visible
                KeepEntityLineWeight = true,

            };

            var export = new HiddenLinesViewOnFileAutodesk(settings, filePath, 0);
            export.DoWork();

            return filePath;
        }
        #endregion

        #region Hinges
        public List<string> AddStepBlocksToDrawing(DesignDocument drawing, string stepPath)
        {
            ReadSTEP reader = new ReadSTEP(stepPath);
            reader.DoWork();

            List<string> blockNames = new();

            foreach (Block stepBlock in reader.Blocks)
            {
                if (!drawing.Blocks.Contains(stepBlock.Name))
                {
                    drawing.Blocks.Add(stepBlock);
                }

                blockNames.Add(stepBlock.Name);
            }

            return blockNames;
        }
        public void InsertHingeOld(DesignDocument drawing, string blockName, double x, double y, double z, double rotationDeg, string TypeOfHinges)
        {
            // 1️⃣ Move hinge to its location
            Transformation move = new Translation(x, y, z);

            // 2️⃣ Rotate around hinge itself (local pivot)
            double rad = rotationDeg * Math.PI / 180;
            Transformation rotate = new Rotation(
                rad,
                Vector3D.AxisZ,
                new Point3D(x, y, z)
            );

            Transformation finalTr;

            if (TypeOfHinges == "Ball Bearing Hinges")
            {
                // move → rotate
                finalTr = rotate * move;
            }
            else
            {
                finalTr = rotate * move;
            }

            BlockReference br = new BlockReference(finalTr, blockName);
            drawing.Entities.Add(br, Color.DarkGray);
        }
        public void InsertHinge(DesignDocument drawing, string blockName, double x, double y, double z, bool mirrorX, string TypeOfHinges)
        {
            Transformation tr = new Translation(x, y, z);

            if (mirrorX)
            {
                // Mirror around local YZ plane
                Transformation mirror =
                    new Mirror(new Plane(new Point3D(x, y, z), Vector3D.AxisX));

                tr = mirror * tr;
            }
            if (TypeOfHinges == "Ball Bearing Hinges")
            {
                // Ball bearing STEP needs extra 90° correction
                Transformation rotateFix =
                    new Rotation(Math.PI / 2, Vector3D.AxisX, new Point3D(x, y, z));

                tr = rotateFix * tr;
            }
            BlockReference br = new BlockReference(tr, blockName);
            drawing.Entities.Add(br, Color.DarkGray);
        }
        public void InsertDoorHardware(DesignDocument drawing, string blockName, double x, double y, double z, bool mirrorX, bool applyRotationFix, double rotationAngleRad = 0, Vector3D? rotationAxis = null)
        {
            // Base translation
            Transformation tr = new Translation(x, y, z);
            // Mirror (for opposite leaf)
            if (mirrorX)
            {
                Transformation mirror = new Mirror(new Plane(new Point3D(x, y, z), Vector3D.AxisX));
                tr = mirror * tr;
            }

            // Rotation Fix
            if (applyRotationFix && rotationAngleRad != 0)
            {
                // If no axis provided → default to AxisX (side door case)
                Vector3D axis = rotationAxis ?? Vector3D.AxisX;

                Transformation rotate = new Rotation(rotationAngleRad, axis, new Point3D(x, y, z));

                tr = rotate * tr;
            }
            BlockReference br = new BlockReference(tr, blockName);
            drawing.Entities.Add(br, Color.DimGray);
        }
        public void InsertDoorHardwareInFront(DesignDocument drawing, string blockName, double x, double y, double z, bool flipForRightSide, List<(double angle, Vector3D axis)> rotations = null)
        {
            // 1️⃣ Base translation
            Transformation tr = new Translation(x, y, z);

            // 2️⃣ Mirror for opposite leaf — ✅ AxisY for front door (not AxisX like side door)
            if (flipForRightSide)
            {
                Transformation mirror = new Mirror(new Plane(new Point3D(x, y, z), Vector3D.AxisY));
                tr = mirror * tr;
            }

            // 3️⃣ Apply rotations
            if (rotations != null)
            {
                foreach (var r in rotations)
                {
                    Transformation rot = new Rotation(r.angle, r.axis, new Point3D(x, y, z));
                    tr = rot * tr;
                }
            }

            BlockReference br = new BlockReference(tr, blockName);
            drawing.Entities.Add(br, Color.DimGray);
        }
        private int GetHingeCount(double doorHeight)
        {
            if (doorHeight <= 1200) return 2;
            if (doorHeight <= 2000) return 3;
            if (doorHeight <= 2600) return 4;
            if (doorHeight <= 3200) return 5;
            return 6; // very tall / heavy doors
        }
        public void InsertHingeBackPlate(DesignDocument drawing, Brep backPlateProto, double hingeX, double hingeY, double hingeZ, bool mirrorX)
        {
            Brep plate = (Brep)backPlateProto.Clone();

            // 🔹 Position plate behind hinge
            double backPlateOffsetY = 16 + 3; // thickness + small gap
            plate.Translate(
                hingeX,
                hingeY - backPlateOffsetY,
                hingeZ - 50  // center plate vertically (100 / 2)
            );

            // 🔹 Mirror for right-side doors
            if (mirrorX)
            {
                Transformation mirror =
                    new Mirror(new Plane(
                        new Point3D(hingeX, hingeY, hingeZ),
                        Vector3D.AxisX
                    ));

                plate.TransformBy(mirror);
            }

            drawing.Entities.Add(plate, Color.LightPink);
        }
        public void DeleteOldPanelDetails(int enquiryId)
        {
            var oldRecords = _DbContext.PanelDetails
                                .Where(x => x.EnquiryId == enquiryId)
                                .ToList();

            _DbContext.PanelDetails.RemoveRange(oldRecords);
            _DbContext.SaveChanges();
        }
        public void DeleteOldMetalBaffelsDetails(int enquiryId)
        {
            var oldRecords = _DbContext.MetalBaffleDetails
                                .Where(x => x.EnquiryID == enquiryId)
                                .ToList();

            _DbContext.MetalBaffleDetails.RemoveRange(oldRecords);
            _DbContext.SaveChanges();
        }
        public void DeleteOldPaintArresterFilterDetails(int enquiryId)
        {
            var oldRecords = _DbContext.FilterFrameDetails
                                .Where(x => x.EnquiryID == enquiryId)
                                .ToList();

            _DbContext.FilterFrameDetails.RemoveRange(oldRecords);
            _DbContext.SaveChanges();
        }
        #endregion


    }
    public class PaintBoothclass
    {
        public DesignDocument drawing { get; set; }
        public string lstpath { get; set; }
        public string developmentpath { get; set; }

    }
}
