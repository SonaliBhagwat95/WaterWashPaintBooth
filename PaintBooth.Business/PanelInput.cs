
//using Bullows.Model;
//using devDept.Eyeshot;
//using devDept.Eyeshot.Control;
//using devDept.Eyeshot.Entities;
//using devDept.Eyeshot.Translators;
//using devDept.Geometry;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using System.DirectoryServices;
//using System.Drawing;
//using Xbim.Ifc2x3.PresentationResource;
//using devregion = devDept.Eyeshot.Entities.Region;
//using Region = devDept.Eyeshot.Entities.Region;

//namespace bullows.business
//{
//    public class PanelInput
//    {
//        private IHttpContextAccessor httpcontextaccessor;
//        public DesignDocument drawing;
//        public DesignDocument docdrawing;
//        private Sheet sheet;
//        private string name;
//        public int ProjectID { get; set; }
//        private string bendlayer { get; set; }
//        public double PanelWidth { get; set; }
//        public double PanelHeight { get; set; }
//        public double SheetThickness { get; set; }
//        public double StandardBend1 { get; set; }
//        public double StandardBend2 { get; set; }
//        public double PitchDistance { get; set; }
//        public int NoofPanels { get; set; }       
//        public int[] shroudarr;
//        private int[] Panelinputs;
//        public int[] stiff;
//        public double CutoutLength { get; set; }
//        public double CutoutWidth { get; set; }
//        public double CutoutXDistance { get; set; }
//        public double CutoutYDistance { get; set; }
//        public Point3D[] Points { get; private set; }
//        IHttpContextAccessor httpContextAccessor;
//        private readonly ISession Session;
//        public PanelInput()
//        {
//            drawing = new DesignDocument();
//            docdrawing = new DesignDocument();
//            Points = new Point3D[4];
//        }
//        public PanelInput(Sheet sheet, string name, IHttpContextAccessor httpContextAccessor)
//        {
//            this.sheet = sheet;
//            this.name = name;
//            this.Session = httpContextAccessor.HttpContext.Session;
//        }
//        public string SweepMethod(PanelInputModel model)
//        {
//            try
//            {
//                DesignDocument drawing = new DesignDocument();
//                drawing.Units = linearUnitsType.Millimeters;

//                // Create main panel
//                var rectangle1 = devregion.CreatePolygon(new Point3D[]
//                {
//                    new Point3D(0, 0, 0),
//                    new Point3D(0, 0, model.PanelHeight),
//                    new Point3D(model.PanelWidth, 0, model.PanelHeight),
//                    new Point3D(model.PanelWidth, 0, 0)
//                });
//                Brep brep = rectangle1.ExtrudeAsBrep(model.SheetThickness);
//                drawing.Entities.Add(brep, Color.Green);
//                #region cutout
//                if (model.PartName != 5)
//                {
//                    Point3D[] cutout = new Point3D[]
//                                   {
//                    new Point3D(model.CutoutXDistance, 0, model.CutoutYDistance),
//                    new Point3D((model.CutoutXDistance + model.CutoutWidth), 0, model.CutoutYDistance),
//                    new Point3D((model.CutoutXDistance + model.CutoutWidth), 0, (model.CutoutYDistance + model.CutoutLength)),
//                    new Point3D(model.CutoutXDistance, 0, (model.CutoutYDistance + model.CutoutLength)),
//                                   };
//                    var cutout1 = devregion.CreatePolygon(cutout);
//                    Brep brep1 = rectangle1.ExtrudeAsBrep(model.SheetThickness);
//                    brep1.ExtrudeRemove(cutout1, -model.PanelWidth);
//                    drawing.Entities.Add(brep1);
//                }
//                #endregion cutout

//                #region sweep

//                LinearPath rail = new LinearPath(new Point3D[]
//                {
//                    new Point3D(0, 0, 0),
//                    new Point3D(0, 0, model.PanelHeight),
//                    new Point3D(model.PanelWidth, 0, model.PanelHeight),
//                    new Point3D(model.PanelWidth, 0, 0),
//                    new Point3D(0, 0, 0)
//                });

//                var section = devregion.CreatePolygon(new Point3D[]
//                {
//                    new Point3D(0, 0, 0),
//                    new Point3D(model.SheetThickness, 0, 0),
//                    new Point3D(model.SheetThickness, model.StandardBend2 - model.SheetThickness, 0),
//                    new Point3D(model.StandardBend1, model.StandardBend2 - model.SheetThickness, 0),
//                    new Point3D(model.StandardBend1, model.StandardBend2, 0),
//                    new Point3D(0, model.StandardBend2, 0)
//                });

//                devDept.Eyeshot.Entities.Solid frame = section.SweepAsSolid(rail, 0);
//                frame.Translate(0, 0, model.SheetThickness);
//                drawing.Entities.Add(frame);

//                #endregion sweep

//                #region holes creation
//                string[] dimensions = model.SlotDimentions.Split('-');
//                if (dimensions.Length != 2)
//                {
//                    throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//                }

//                // Parse the width and height
//                if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//                {
//                    throw new ArgumentException("Slot dimensions must be numeric values.");
//                }

//                // For height (YZ plane)
//                double divisionResultHeight = model.PanelHeight / model.PitchDistance;
//                int wholeNumberPartHeight = (int)Math.Floor(divisionResultHeight);
//                double multipliedResultHeight = wholeNumberPartHeight * model.PitchDistance;
//                double sameSpaceDivideHeight = model.PanelHeight - multipliedResultHeight;

//                for (int i = 0; i <= wholeNumberPartHeight; i++)
//                {
//                    double centerZ;
//                    if (i == 0)
//                        centerZ = sameSpaceDivideHeight / 2;
//                    else
//                        centerZ = sameSpaceDivideHeight / 2 + i * model.PitchDistance;

//                    devregion slot = devregion.CreateSlot(Plane.YZ, (model.StandardBend2 / 2), centerZ, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//                    slot.Translate(0, 0, centerZ);
//                    frame.ExtrudeRemove(slot, model.PanelWidth, 0);
//                }


//                double divisionResultWidth = model.PanelWidth / model.PitchDistance;
//                int wholeNumberPartWidth = (int)Math.Floor(divisionResultWidth);
//                double multipliedResultWidth = wholeNumberPartWidth * model.PitchDistance;
//                double sameSpaceDivideWidth = model.PanelWidth - multipliedResultWidth;

//                for (int i = 0; i < wholeNumberPartWidth; i++)
//                {
//                    double centerX;
//                    if (i == 0)
//                        centerX = sameSpaceDivideWidth / 2;
//                    else
//                        centerX = sameSpaceDivideWidth / 2 + i * model.PitchDistance;

//                    devregion circle = devregion.CreateSlot(Plane.XY, centerX, (model.StandardBend2 / 2), (slotLength - slotWidth), slotWidth / 2);
//                    frame.ExtrudeRemove(circle, model.PanelHeight, 0);
//                }

//                drawing.Entities.Add(frame, Color.Yellow);

//                var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();
//                path += "/" + model.ProjectID;

//                if (!Directory.Exists(path + "/Bullows Panel Drawing"))
//                    Directory.CreateDirectory(path + "/Bullows Panel Drawing");

//                string dwgFilePath = path + "/Bullows Panel Drawing/" + "PanelDrawing" + DateTime.Now.ToString("hh-mm") + ".dwg";

//                WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//                WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//                dwgg1.DoWork();

//                Write3DPdfParams pdf = new Write3DPdfParams(drawing);
//                Write3DPDF pdf1 = new Write3DPDF(pdf, path + "/bullows_Paneldrawing.pdf");
//                pdf1.DoWork();
//                CreateLoft();
//                #endregion holes creation

//                StandardFrame(2);
//                // MetalBaffle();
//                // FilterFrame1();
//                // FilterFrame2();
//                return dwgFilePath;
//            }
//            catch (Exception ex)
//            {
//                return $"Error: {ex.Message}";
//            }
//        }
//        public string Development(PanelInputModel model)
//        {
//            #region Development
//            StandardBend1 = model.StandardBend1 - model.SheetThickness;
//            StandardBend2 = model.StandardBend2 - (model.SheetThickness * 2);
//            int bendlineLength = 50;
//            if (model.PanelWidth > 1000)
//                bendlineLength = 160;
//            else
//                bendlineLength = 110;
//            const string Dim = "Dimension";
//            docdrawing.Layers.Add(new Layer(Dim, Color.CornflowerBlue));
//            Plane verticalPlane = Plane.XY;
//            verticalPlane.Rotate(Math.PI / 2, Vector3D.AxisZ);

//            #region inner Rectangle Dimensions
//            #region Inner Rectangle Bottom
//            //Add InnerRectangle for Left Bottom
//            LinearPath Innerrectangleleft = new LinearPath(new Point3D[]
//            {
//                 new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)+10, 0),
//                  new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))+bendlineLength, 0)
//            });

//            Layer mylayer = new Layer("bendlayer");
//            mylayer.Color = Color.Yellow;
//            docdrawing.Layers.Add(mylayer);
//            Innerrectangleleft.LayerName = "bendlayer";
//            docdrawing.Entities.Add(Innerrectangleleft);
//            //for innerRectangle Right Bottom
//            LinearPath InnerrectangleRightBottom = new LinearPath(new Point3D[]
//            {
//                new Point3D(((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness)-10, 0),
//                new Point3D((((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness))-bendlineLength, 0),
//            });
//            InnerrectangleRightBottom.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleRightBottom);
//            #endregion
//            #region Inner Rectangle Top
//            //Add InnerRectangle for Left Top
//            LinearPath InnerrectangleleftTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)+10,  model.PanelHeight - 2 * model.SheetThickness),
//                  new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)+bendlineLength), ( model.PanelHeight - 2 * model.SheetThickness))
//            });
//            InnerrectangleleftTop.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleleftTop);
//            //for innerRectangle Right Top
//            LinearPath InnerrectangleRightTop = new LinearPath(new Point3D[]
//            {
//                new Point3D(((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness)-10,  model.PanelHeight - 2 * model.SheetThickness),
//                new Point3D((((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness))-bendlineLength,  model.PanelHeight - 2 * model.SheetThickness),
//            });
//            InnerrectangleRightTop.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleRightTop);
//            #endregion
//            #region Inner Rectangle Left
//            // inner rectangle left bottom
//            LinearPath InnerrectangleleftSide = new LinearPath(new Point3D[]
//            {
//                 new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2), 10),
//                  new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)), bendlineLength)
//            });
//            InnerrectangleleftSide.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleleftSide);
//            // inner rectangle left Top
//            LinearPath InnerrectangleleftSideTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2), (model.PanelHeight - 2 * model.SheetThickness)-10),
//                  new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)),((model.PanelHeight - 2 * model.SheetThickness)-bendlineLength) )
//            });
//            InnerrectangleleftSideTop.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleleftSideTop);
//            #endregion
//            #region Inner Rectangle Right
//            //  inner rectangle Rightside bottom
//            LinearPath InnerrectangleRightSide = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)-(2*model.SheetThickness)), 10),
//                  new Point3D((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)-(2*model.SheetThickness)), bendlineLength),
//            });
//            InnerrectangleRightSide.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleRightSide);
//            //left Top
//            LinearPath InnerrectangleRightSideTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)-(2*model.SheetThickness)), (model.PanelHeight - 2 * model.SheetThickness)-10),
//                  new Point3D((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)-(2*model.SheetThickness)),((model.PanelHeight - 2 * model.SheetThickness)-bendlineLength) )
//            });
//            InnerrectangleRightSideTop.LayerName = "bendlayer";
//            docdrawing.Entities.Add(InnerrectangleRightSideTop);
//            #endregion
//            #endregion
//            #region bend
//            #region RightSideBendLine
//            //for bendline right side
//            LinearPath bendlineright = new LinearPath(new Point3D[]
//             {
//                 new Point3D((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+(StandardBend2))-2*model.SheetThickness,10),
//                  new Point3D((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+(StandardBend2))-2*model.SheetThickness,bendlineLength),
//            });
//            bendlineright.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineright);
//            LinearPath bendlineright1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+(StandardBend2))-2*model.SheetThickness,(model.PanelHeight - 2 * model.SheetThickness)-10),
//                 new Point3D((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+(StandardBend2))-2*model.SheetThickness,(model.PanelHeight - 2 * model.SheetThickness)-bendlineLength),

//           });
//            bendlineright1.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineright1);
//            #endregion
//            #region LeftSideBendLine
//            //for bendline right side
//            LinearPath bendlineLeft = new LinearPath(new Point3D[]
//             {
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)-(StandardBend2)),10),
//                  new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)-(StandardBend2)),bendlineLength),
//            });
//            bendlineLeft.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineLeft);
//            LinearPath bendlineLeft1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)-(StandardBend2)),(model.PanelHeight - 2 * model.SheetThickness)-10),
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)-(StandardBend2)),(model.PanelHeight - 2 * model.SheetThickness)-bendlineLength)

//           });
//            bendlineLeft1.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineLeft1);
//            #endregion
//            #region BottomBend Line
//            //for bendline Bottom side
//            LinearPath bendlineBottom = new LinearPath(new Point3D[]
//             {
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))+10,-(StandardBend2)),
//                  new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+bendlineLength),-(StandardBend2)),
//            });
//            bendlineBottom.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineBottom);
//            LinearPath bendlineBottom1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))+10,(model.PanelHeight - 2 * model.SheetThickness)+(StandardBend2)),
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+bendlineLength),(model.PanelHeight - 2 * model.SheetThickness)+StandardBend2)
//           });
//            bendlineBottom1.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineBottom1);

//            #region Dimentions
//            double x = (((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2))) + ((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) + bendlineLength))) / 2;
//            LinearDim bendlineBottom1Dim = new LinearDim(Plane.XY,
//            new Point3D((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2))+10, (model.PanelHeight - 2 * model.SheetThickness) + (StandardBend2)),
//                 new Point3D((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) + bendlineLength), (model.PanelHeight - 2 * model.SheetThickness) + StandardBend2),
//                new Point3D(x, (model.PanelHeight - 2 * model.SheetThickness) + (StandardBend2) + bendlineLength), 10);
//            docdrawing.Entities.Add(bendlineBottom1Dim, Dim);
//            #endregion
//            #endregion
//            #region BottomBend Right Line
//            //for bendline Bottom side
//            LinearPath bendlineBottomRight = new LinearPath(new Point3D[]
//             {
//                  new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness)-10,-(StandardBend2)),
//              new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness)-bendlineLength,-(StandardBend2)),
//            });
//            bendlineBottomRight.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineBottomRight);

//            LinearPath bendlineBottomRightTop = new LinearPath(new Point3D[]
//            {
//                new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness)-10,(model.PanelHeight - 2 * model.SheetThickness)+(StandardBend2)),
//                new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness)-bendlineLength,(model.PanelHeight - 2 * model.SheetThickness)+StandardBend2)

//            });
//            bendlineBottomRightTop.LayerName = "bendlayer";
//            docdrawing.Entities.Add(bendlineBottomRightTop);
//            #endregion
//            #region Outer Bottom Line
//            // //for bottom
//            LinearPath linearBottom = new LinearPath(new Point3D[]
//            {
//                  new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2), 0),
//                  new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)),-(StandardBend2)),
//                  new Point3D(((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))+(StandardBend1),-(StandardBend2+StandardBend1)),
//                  new Point3D(((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - ((- model.SheetThickness)+StandardBend1),-(StandardBend1+StandardBend2)),
//                  new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-(2*model.SheetThickness)),-(StandardBend2)),
//                  new Point3D(((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness),0),
//            });
//            docdrawing.Entities.Add(linearBottom, Color.White);
//            //LinearBottomOuter
//            #region Outer Dimmention
//            #region Outer 1


//            x = ((0) + (-(StandardBend2))) / 2;
//            LinearDim linearBottomdim = new(verticalPlane,
//               new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2), 0),
//                  new Point3D((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)), -(StandardBend2)),
//                  new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2) - 20, x), 10)
//            { ArrowheadSize = 10 };
//            docdrawing.Entities.Add(linearBottomdim, Dim);
//            #endregion

//            #region Outer 2
//            x = (((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2))) + (((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) + (StandardBend1))) / 2;
//            linearBottomdim = new(Plane.XY,
//                new Point3D((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)), -(StandardBend2)),
//                  new Point3D(((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) + (StandardBend1), -(StandardBend2 + StandardBend1)),
//                 new Point3D(x - 10, -(StandardBend2 + StandardBend1) - 40), 10)
//            { ArrowheadSize = 10 };
//            docdrawing.Entities.Add(linearBottomdim, Dim);
//            #endregion
//            #region Outer 3
//            x = ((((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) + (StandardBend1 - model.SheetThickness)) + (((model.PanelWidth * 2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - ((-model.SheetThickness) + StandardBend1))) / 2;
//            linearBottomdim = new(Plane.XY,
//                  new Point3D((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)), -(StandardBend2)),
//                 new Point3D(((model.PanelWidth * 2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) - (2 * model.SheetThickness)), -(StandardBend2)),
//                 new Point3D(x, -(StandardBend2 + StandardBend1) - 40), 10)
//            { ArrowheadSize = 10 };
//            docdrawing.Entities.Add(linearBottomdim, Dim);
//            #endregion
//            #endregion
//            //For Top

//            double Panelheight = model.PanelHeight - 2 * model.SheetThickness;
//            LinearPath linearTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2), Panelheight),
//                  new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)),StandardBend2 + Panelheight),
//                  new Point3D(((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))+(StandardBend1),Panelheight+(StandardBend2+StandardBend1)),
//                  new Point3D(((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness+StandardBend1),Panelheight+(StandardBend1+StandardBend2)),
//                    new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness),Panelheight+(StandardBend2)),
//                  new Point3D(((model.PanelWidth*2) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (2 * model.SheetThickness),Panelheight),
//            });

//            docdrawing.Entities.Add(linearTop, Color.White);
//            // For LeftSide

//            LinearPath linearLeft = new LinearPath(new Point3D[]
//            {
//                new Point3D(model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2), 0),
//                new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)-(StandardBend2)),0),
//                new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))-(StandardBend1+StandardBend2), StandardBend1),
//                new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))-(StandardBend1+StandardBend2),Panelheight-StandardBend1),
//                 new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))-(StandardBend2),Panelheight),
//                new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)),Panelheight),
//            });
//            docdrawing.Entities.Add(linearLeft, Color.White);
//            #region Dimension
//            double y = ((StandardBend1) + (Panelheight - StandardBend1)) / 2;//calculate midpoint of y for placing dimention text
//            LinearDim linearLefttDim = new(verticalPlane,
//                new Point3D((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) - (StandardBend2)), 0),
//                 new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (StandardBend2), Panelheight),
//                new Point3D((model.PanelWidth + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)) - (StandardBend1 + StandardBend2) - 40, y), 20);
//            docdrawing.Entities.Add(linearLefttDim, Dim);

//            #endregion

//            //For Right Sides

//            LinearPath linearRight = new LinearPath(new Point3D[]
//            {
//               new Point3D((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2)-2*model.SheetThickness), 0),
//                new Point3D((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)+(StandardBend2-2*model.SheetThickness)),0),
//                new Point3D(((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))+(StandardBend1+StandardBend2)-2*model.SheetThickness), StandardBend1),
//                new Point3D(((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))+(StandardBend1+StandardBend2)-2*model.SheetThickness),Panelheight-StandardBend1),

//                 new Point3D((model.PanelWidth*2 + ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))+(StandardBend2-2*model.SheetThickness),Panelheight),
//                new Point3D(((model.PanelWidth*2+ ((model.PanelWidth / 2 + model.PanelWidth / 2) * 0.2))-2*model.SheetThickness),Panelheight),
//            });
//            docdrawing.Entities.Add(linearRight, Color.White);
//            #endregion


//            #endregion bend
//            #region Notching
//            LinearPath BottomLeftBottmNotching = new LinearPath(new Point3D[]
//            {
//                new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)),-(StandardBend2)),
//                new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))+(StandardBend1),-(StandardBend2+StandardBend1)),
//            });
//            //Point3D trimPoint = new Point3D(-StandardBend1, 0, 0);
//            //BottomLeftNotching.TrimBy(trimPoint, true);
//            docdrawing.Entities.Add(BottomLeftBottmNotching, Color.White);

//            //Left bottom Notching 
//            LinearPath BottomLeftBottmNotching1 = new LinearPath(new Point3D[]
//            {
//                new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-(StandardBend2),0),
//                new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-(StandardBend2+StandardBend1),StandardBend1),

//            });
//            docdrawing.Entities.Add(BottomLeftBottmNotching1, Color.White);
//            //for right side 
//            LinearPath BottomRightBottmNotching = new LinearPath(new Point3D[]
//            {
//                new Point3D(((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness),-(StandardBend2)),


//                new Point3D((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-(StandardBend1-model.SheetThickness),-(StandardBend2+StandardBend1)),
//            });

//            docdrawing.Entities.Add(BottomRightBottmNotching, Color.White);
//            LinearPath BottomRightBottmNotching1 = new LinearPath(new Point3D[]
//            {
//                new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)))+(StandardBend2 -(2* model.SheetThickness)),0),
//                new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))+(StandardBend2+StandardBend1))-2*model.SheetThickness,StandardBend1),

//            });
//            docdrawing.Entities.Add(BottomRightBottmNotching1, Color.White);
//            //For TopRight Side

//            LinearPath BottomRightTopNotching = new LinearPath(new Point3D[]
//            {
//                 new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))+(StandardBend2))-2*model.SheetThickness,model.PanelHeight - 2 * model.SheetThickness),
//                new Point3D(((model.PanelWidth * 2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))+(StandardBend2+StandardBend1)-2*model.SheetThickness),(model.PanelHeight - 2 * model.SheetThickness)-StandardBend1),
//            });

//            docdrawing.Entities.Add(BottomRightTopNotching, Color.White);
//            LinearPath BottomRightTopNotching1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D(((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness),(model.PanelHeight - 2 * model.SheetThickness)+(StandardBend2)),
//                new Point3D((((model.PanelWidth*2)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-2*model.SheetThickness))-StandardBend1,(model.PanelHeight - 2 * model.SheetThickness)+(StandardBend2+StandardBend1)),

//            });
//            docdrawing.Entities.Add(BottomRightTopNotching1, Color.White);
//            //For Left Top Side
//            LinearPath TopleftNotching = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-(StandardBend2),model.PanelHeight - 2 * model.SheetThickness),
//                new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2))-(StandardBend2+StandardBend1),(model.PanelHeight - 2 * model.SheetThickness)-StandardBend1),
//            });

//            docdrawing.Entities.Add(TopleftNotching, Color.White);
//            LinearPath TopleftNotching1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)),(model.PanelHeight - 2 * model.SheetThickness)+(StandardBend2)),
//                new Point3D(((model.PanelWidth)+((model.PanelWidth/2+model.PanelWidth/2)*(0.2)))+StandardBend1,(model.PanelHeight - 2 * model.SheetThickness)+(StandardBend2+StandardBend1)),

//            });
//            docdrawing.Entities.Add(TopleftNotching1, Color.White);
//            #endregion
//            #region Create Slots
//            string[] dimensions = model.SlotDimentions.Split('-');
//            if (dimensions.Length != 2)
//            {
//                throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//            }

//            // Parse the width and height
//            if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//            {
//                throw new ArgumentException("Slot dimensions must be numeric values.");
//            }

//            #region holes on PanelHeight
//            ////creating holes on vertical direction 
//            //double PanelHeightSheetThickness = model.PanelHeight - 2 * model.SheetThickness;
//            ////double divisionresult = model.PanelHeight / model.PitchDistance;
//            //double divisionresult = PanelHeightSheetThickness / model.PitchDistance;

//            //int wholenumberpart = (int)Math.Floor(divisionresult);

//            //// calculate the remaining space after creating whole slots
//            //double multipliedresult = (wholenumberpart) * model.PitchDistance;
//            //double samespacedivide = PanelHeightSheetThickness - multipliedresult;

//            //// create slots and add them to the drawing
//            //for (int i = 0; i <= wholenumberpart; i++)
//            //{
//            //    double centerz;
//            //    if (i == 0)
//            //        centerz = samespacedivide / 2;
//            //    else

//            //        centerz = samespacedivide / 2 + (i) * model.PitchDistance;
//            //    // create a slot
//            //    devregion slot = devregion.CreateSlot(Plane.XY, (model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) - (model.StandardBend2 / 2)), centerz, (slotLength - slotWidth), slotWidth / 2, 1.5708);

//            //    devregion slot1 = devregion.CreateSlot(Plane.XY, (model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) - (model.StandardBend2 / 2) + (model.PanelWidth + model.StandardBend2)), centerz, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//            //    slot.Translate(0, 0, samespacedivide / 2);
//            //    slot.Color = Color.Yellow;
//            //    // add the slot to the drawing
//            //    docdrawing.Entities.Add(slot, Color.White);
//            //    docdrawing.Entities.Add(slot1, Color.White);
//            //} 
           
//            double PanelHeightSheetThickness = model.PanelHeight - 2 * model.SheetThickness;
//            double divisionresult = PanelHeightSheetThickness / model.PitchDistance;

//            // Get the number of holes
//            int wholenumberpart = (int)Math.Round(divisionresult);

//            // Calculate remaining space and distribute evenly at start & end
//            double multipliedresult = (wholenumberpart - 1) * model.PitchDistance;
//            double samespacedivide = (PanelHeightSheetThickness - multipliedresult) / 2;

//            // **Shift first hole by 1mm closer**
//            samespacedivide -= 1;

//            // Create slots and add them to the drawing
//            for (int i = 0; i < wholenumberpart; i++)
//            {
//                double centerz = samespacedivide + (i * model.PitchDistance);

//                // Create first slot
//                devregion slot = devregion.CreateSlot(Plane.XY,
//                    (model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) - (model.StandardBend2 / 2)),
//                    centerz,
//                    (slotLength - slotWidth),
//                    slotWidth / 2,
//                    1.5708);

//                // Create second slot
//                devregion slot1 = devregion.CreateSlot(Plane.XY,
//                    (model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2) - (model.StandardBend2 / 2) + (model.PanelWidth + model.StandardBend2)),
//                    centerz,
//                    (slotLength - slotWidth),
//                    slotWidth / 2,
//                    1.5708);

//                slot.Color = Color.Yellow;

//                // Add slots to drawing
//                docdrawing.Entities.Add(slot, Color.White);
//                docdrawing.Entities.Add(slot1, Color.White);
//            }
//            #endregion
//            #region Holes in PanelWidth
//            ////create slots on width
//            //double PanelWidthSheetThickness = model.PanelWidth - 2 * model.SheetThickness;

//            //double divisionresult1 = PanelWidthSheetThickness / model.PitchDistance;
//            //int wholenumberpart1 = (int)Math.Floor(divisionresult1);

//            //// calculate the remaining space after creating whole slots
//            //double multipliedresult1 = wholenumberpart1 * model.PitchDistance;
//            //double samespacedivide1 = PanelWidthSheetThickness - multipliedresult1;
//            //samespacedivide1 -= 1;
//            //// create slots and add them to the drawing
//            //for (int i = 0; i <= wholenumberpart1; i++)
//            //{
//            //    double centery;
//            //    centery = samespacedivide1 + (i * model.PitchDistance);
//            //    //if (i == 0)
//            //    //    centery = samespacedivide1 / 2;
//            //    //else
//            //    //centery = /*(PanelWidth) - (((i + 0) * PitchDistance) - (samespacedivide1 / 2));*/
//            //    //centery = samespacedivide1 / 2 + (i) * model.PitchDistance;

//            //    // create a slot
//            //    devregion slot2 = devregion.CreateSlot(Plane.XY, ((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centery), (-model.StandardBend2 / 2), (slotLength - slotWidth), slotWidth / 2, 0);

//            //    devregion slot3 = devregion.CreateSlot(Plane.XY, ((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centery), (model.PanelHeight + model.StandardBend2 / 2), (slotLength - slotWidth), slotWidth / 2, 0);
//            //    slot2.Translate(0, 0, samespacedivide1 / 2);
//            //    slot2.Color = Color.Yellow;
//            //    // add the slot to the drawing
//            //    docdrawing.Entities.Add(slot2, Color.White);
//            //    docdrawing.Entities.Add(slot3, Color.White);
//            //} 

//            // Calculate available panel width excluding sheet thickness
//            double PanelWidthSheetThickness = model.PanelWidth - 2 * model.SheetThickness;

//            // Calculate number of holes that fit within the panel width
//            double divisionresult1 = PanelWidthSheetThickness / model.PitchDistance;
//            int wholenumberpart1 = (int)Math.Floor(divisionresult1);

//            // Calculate the total occupied space by the slots
//            double multipliedresult1 = wholenumberpart1 * model.PitchDistance;

//            // Calculate the remaining space to distribute evenly
//            double samespacedivide1 = (PanelWidthSheetThickness - multipliedresult1)/2;

//            // Adjust first slot position by shifting it 1mm closer
//            samespacedivide1 -= 1;

//            // Loop through each hole position and create slots
//            for (int i = 0; i <= wholenumberpart1; i++)
//            {
//                // Calculate the Y-coordinate for slot placement
//                double centery = samespacedivide1 + (i * model.PitchDistance);

//                // Create slot at the bottom of the panel
//                devregion slot2 = devregion.CreateSlot(Plane.XY,
//                    ((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centery),
//                    (-model.StandardBend2 / 2),
//                    (slotLength - slotWidth),
//                    slotWidth / 2,
//                    0);

//                // Create slot at the top of the panel
//                devregion slot3 = devregion.CreateSlot(Plane.XY,
//                    ((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centery),
//                    (model.PanelHeight + model.StandardBend2 / 2),
//                    (slotLength - slotWidth),
//                    slotWidth / 2,
//                    0);

//                // Adjust slot position for equal spacing
//                slot2.Translate(0, 0, samespacedivide1 / 2);

//                // Set color for visualization
//                slot2.Color = Color.Yellow;

//                // Add slots to the drawing
//                docdrawing.Entities.Add(slot2, Color.White);
//                docdrawing.Entities.Add(slot3, Color.White);
//            }
//            #endregion
//            #region Dimension
//            double centerz1 = samespacedivide1 / 2 + (2) * model.PitchDistance;
//            double centerz2 = samespacedivide1 / 2 + (3) * model.PitchDistance;
//            x = ((((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centerz1)) + (((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centerz2))) / 2;
//            LinearDim slotsDim = new LinearDim(Plane.XY,
//              new Point3D(((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centerz1), (-model.StandardBend2 / 2)),
//               new Point3D(((model.PanelWidth) + ((model.PanelWidth / 2 + model.PanelWidth / 2) * (0.2)) + centerz2), (-model.StandardBend2 / 2)),
//              new Point3D(x, (-model.StandardBend2 / 2) + bendlineLength), 20);

//            docdrawing.Entities.Add(slotsDim, Dim);
//            #endregion
//            #endregion
//            #region WriteFile
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();
//            path += "/" + model.ProjectID;

//            if (!Directory.Exists(path + "/Development"))
//                Directory.CreateDirectory(path + "/Development");
//            var dwgFilePathfordevelopment = Path.Combine(path, "Development", "Development" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(docdrawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
//            dwgg1.DoWork();
//            #endregion
//            return dwgFilePathfordevelopment;
//            #endregion development
//        }
//        public void StandardFrame(int scaleFator)
//        {

//            double UntrimmedWidth = 625 * scaleFator;
//            double UntrimmedHeight = 450 * scaleFator;
//            double trimmedWidth = 594 * scaleFator;
//            double trimmedHeight = 420 * scaleFator;

//            double outerX0 = 0, outerX1 = UntrimmedWidth, outerY0 = 0, outerY1 = UntrimmedHeight;

//            double iX0 = (UntrimmedWidth - trimmedWidth) / 2;
//            double iY0 = (UntrimmedHeight - trimmedHeight) / 2;

//            double innerX0 = iX0, innerX1 = trimmedWidth + iX0, innerY0 = iY0, innerY1 = trimmedHeight + iY0;
//            var outerRectangle = devregion.CreatePolygon(Plane.XY, new Point2D[]
//            {
//                new Point2D(outerX0,outerY0),
//                new Point2D(outerX1,outerY0),
//                new Point2D(outerX1,outerY1),
//                new Point2D(outerY0,outerY1),

//            });
//            drawing.Entities.Add(outerRectangle, Color.Yellow);

//            var innerRectangle = devregion.CreatePolygon(Plane.XY, new Point2D[]
//            {
//                new Point2D(innerX0,innerY0),
//                new Point2D(innerX1,innerY0),
//                new Point2D(innerX1,innerY1),
//                new Point2D(innerX0,innerY1),

//            });
//            drawing.Entities.Add(innerRectangle, Color.Yellow);

//            #region TitleBox
//            double titleBoxWidth = 185 * scaleFator;
//            double titleBoxHeight = 65 * scaleFator;
//            double titleBoxX = innerX1 - titleBoxWidth, titleBoxY = innerY0 + titleBoxHeight;
//            double nameOfFirmX = titleBoxX + (130 * scaleFator);
//            double scaleX = titleBoxX + 25 * scaleFator;
//            double scaleY = titleBoxY + 20 * scaleFator;
//            var titleBox = devregion.CreatePolygon(Plane.XY, new Point2D[]//titleBoxX-left top corner of title box,innerX1-right bottom corner of titlebox
//            {
//                new Point2D(titleBoxX,titleBoxY),
//                new Point2D(innerX1,titleBoxY),
//                new Point2D(innerX1,innerY0),
//                new Point2D(titleBoxX,innerY0),

//            });

//            drawing.Entities.Add(titleBox, Color.Yellow);
//            drawing.Entities.AddRange(new Entity[]
//            {
//                  //left
//                new Line(titleBoxX,innerY0+(10*scaleFator),titleBoxX+(25*scaleFator),innerY0+(10*scaleFator)),
//                new Line(titleBoxX,innerY0+(20 * scaleFator),innerX1,innerY0+(20*scaleFator)),
//                new Line(titleBoxX+(130 * scaleFator),titleBoxY,titleBoxX+(130 * scaleFator),innerY0),

//                new Line(titleBoxX+(25 * scaleFator),innerY0+(20 * scaleFator),titleBoxX+(25 * scaleFator),innerY0),
//                new Text(titleBoxX+(10*scaleFator),titleBoxY-(10*scaleFator),"NAME OF THE FIRM",4),
//                new Text(titleBoxX+((25/2)*scaleFator),innerY0+((20-5)*scaleFator),0,"SCALE",4,Text.alignmentType.MiddleCenter),
//                new Text(titleBoxX+((25+(25/2))*scaleFator),innerY0+((20-5)*scaleFator),0,"TITLE",4,Text.alignmentType.MiddleCenter),
//                //Right
//                //new Line(titleBoxX+130+10,innerY0+20,titleBoxX+130+10,titleBoxY),
//                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7)*scaleFator,innerX1,innerY0+(20+7)*scaleFator),
//                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+(7/2))*scaleFator),0,"APPROVED",2,Text.alignmentType.MiddleCenter),

//                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*2)*scaleFator,innerX1,innerY0+(20+7*2)*scaleFator),
//                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7+(7/2))*scaleFator),0,"STANDARD",2,Text.alignmentType.MiddleCenter),



//                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*3)*scaleFator,innerX1,innerY0+(20+7*3)*scaleFator),
//                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*2+(7/2))*scaleFator),0,"CHECKED",2,Text.alignmentType.MiddleCenter),


//                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*4)*scaleFator,innerX1,innerY0+(20+7*4)*scaleFator),
//                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*3+(7/2))*scaleFator),0,"DRAWN",2,Text.alignmentType.MiddleCenter),

//                new Line(titleBoxX+(130 * scaleFator),innerY0+(20+7*5)*scaleFator,innerX1,innerY0+(20+7*5)*scaleFator),
//                new Text(titleBoxX+(130+10)*scaleFator,innerY0+((20+7*4+(7/2))*scaleFator),0,"DESIGNED",2,Text.alignmentType.MiddleCenter),

//                new Text(titleBoxX+(130+20+12.5)*scaleFator,titleBoxY-(5*scaleFator),0,"NAME",2,Text.alignmentType.MiddleCenter),
//                new Text(titleBoxX+(130+20+25+5)*scaleFator,titleBoxY-(5*scaleFator),0,"DATE",2,Text.alignmentType.MiddleCenter),

//              //  new Line(titleBoxX+(130 * scaleFator),innerY0+20+7*5,innerX1,innerY0+20+7*5),
//                new Line(innerX1-(10*scaleFator),innerY0+(20*scaleFator),innerX1-(10 * scaleFator),titleBoxY),
//                new Line(innerX1-((10+25)*scaleFator),innerY0+(20 * scaleFator),innerX1-((10+25)*scaleFator),titleBoxY),


//                new Text(titleBoxX+(130+5)*scaleFator,innerY0 + (20 - 5)*scaleFator,0,"DRAWING NO.",4,Text.alignmentType.MiddleLeft),


//            }, Color.Yellow);

//            //BOM BOx
//            var BOMBox = devregion.CreatePolygon(Plane.XY, new Point2D[]
//            {
//                new Point2D(titleBoxX,titleBoxY),
//                new Point2D(titleBoxX,titleBoxY+15),
//                new Point2D(innerX1,titleBoxY+15),
//                new Point2D(innerX1,titleBoxY),

//            });
//            drawing.Entities.Add(BOMBox, Color.White);
//            List<double> XCoordinate = new List<double>()
//            {
//                titleBoxX+20+10,
//                titleBoxX+20+40+10,
//                titleBoxX+20+40+80+10,
//                titleBoxX+20+40+80+40+10,
//                titleBoxX+20+40+80+40+60+10,
//                titleBoxX+20+40+80+60+40+40+10,
//                titleBoxX+20+40+80+60+40+40+40+10,
//            };

//            drawing.Entities.AddRange(new Entity[]
//            {
//                  new Text(titleBoxX+10,(titleBoxY+15-(15/2)),0,"SR.NO",3,Text.alignmentType.MiddleCenter),
//                  new Text(XCoordinate[0],(titleBoxY+15-(15/2)),0,"PART NO",3,Text.alignmentType.MiddleCenter),
//                   new Text(XCoordinate[1],(titleBoxY+15-(15/2)),0,"PART NAME",3,Text.alignmentType.MiddleLeft),
//                    new Text(XCoordinate[2],(titleBoxY+15-(15/2)),0,"MATERIAL",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[3],(titleBoxY+15-(15/2)),0,"T.SPECIFICATION",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[4], (titleBoxY+15-(15/2)),0,"QUANTITY",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[5], (titleBoxY+15-(15/2)),0,"UMO",3,Text.alignmentType.MiddleCenter),
//                  new Text(XCoordinate[6], (titleBoxY+15-(15/2)),0,"WEIGHT",3,Text.alignmentType.MiddleCenter),
//            }, Color.Pink);
//            double y = 0;
//            for (int i = 0; i < 10; i++)
//            {
//                y = titleBoxY + (15 * (i + 2));
//                Line l = new Line(titleBoxX, titleBoxY + (15 * (i + 2)), innerX1, titleBoxY + (15 * (i + 2)));
//                drawing.Entities.Add(l, Color.White);
//                drawing.Entities.AddRange(new Entity[]
//            {
//                   new Text(titleBoxX+10,(y-(15/2)),0,(i+1).ToString(),3,Text.alignmentType.MiddleCenter),
//                   new Text(XCoordinate[0],(y-(15/2)),0,"PART NO",3,Text.alignmentType.MiddleCenter),
//                   new Text(XCoordinate[1],(y-(15/2)),0,"PART NAME",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[2],(y-(15/2)),0,"MATERIAL",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[3],(y-(15/2)),0,"T.SPECIFICATION",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[4], (y-(15/2)),0,"QUANTITY",3,Text.alignmentType.MiddleLeft),
//                   new Text(XCoordinate[5], (y-(15/2)),0,"UMO",3,Text.alignmentType.MiddleCenter),
//                   new Text(XCoordinate[6], (y-(15/2)),0,"WEIGHT",3,Text.alignmentType.MiddleCenter),
//            }, Color.Pink);

//            }
//            drawing.Entities.AddRange(new Entity[]
//            {    new Line(titleBoxX,titleBoxY,titleBoxX,y),
//                 new Line(XCoordinate[0] - 10,y,XCoordinate[0]-10,titleBoxY),  //sr.no           
//                 new Line(XCoordinate[1]-10,y,XCoordinate[1]-10,titleBoxY),//part no
//                 new Line(XCoordinate[2] - 10,y,XCoordinate[2] - 10,titleBoxY),//part name
//                 new Line(XCoordinate[3] - 10,y,XCoordinate[3] - 10,titleBoxY),//material
//                 new Line(XCoordinate[4] - 10,y,XCoordinate[4] - 10,titleBoxY),//SPECIFICATION
//                 new Line(XCoordinate[5] - 10,y,XCoordinate[5]-10,titleBoxY),//QUANTITY
//                 new Line(XCoordinate[6] - 10,y,XCoordinate[6] - 10,titleBoxY),
//            }, Color.White);

//            #endregion

//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Standard Frame"))
//                Directory.CreateDirectory(path + "/Standard Frame");

//            string dwgFilePath = path + "/Standard Frame/" + "Frame" + DateTime.Now.ToString("hh-mm") + ".dwg";

//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//            dwgg1.DoWork();
//        }
//        public void CreateLoft()
//        {
//            DesignDocument drawing = new DesignDocument();
//            drawing.Units = linearUnitsType.Millimeters;

//            #region MyRegion
//            //// Define base planes for the shapes
//            //Plane p1 = Plane.XY;
//            //Plane p2 = p1.Offset(2000);


//            //Plane p3 = p1.Offset(2400);

//            //LinearPath rectangle = new LinearPath(p2,new Point3D[]
//            //{
//            //    new Point3D(0, 0, 0),
//            //    new Point3D(2400, 0, 0),
//            //    new Point3D(2400, 700, 0),
//            //    new Point3D(0, 700, 0),
//            //    new Point3D(0, 0, 0) 
//            //});
//            //LinearPath rectangle1 = new LinearPath(p3,new Point3D[]
//            //            {
//            //    new Point3D(800, 0, 0),
//            //    new Point3D(1600, 0, 0),
//            //    new Point3D(1600, 700, 0),
//            //    new Point3D(800, 700, 0),
//            //    new Point3D(800, 0, 0)
//            //            });
//            //// Create the loft
//            //Brep loft1 = Brep.Loft(new ICurve[] { rectangle1, rectangle },2);

//            //// Add the created loft to the drawing or model
//            //drawing.Entities.Add(loft1,Color.Yellow); 
//            #endregion           
            
//            try
//            {
//                //double innerRadius = 180; // Adjust as per your dimension
//                //double outerRadius = 230; // Adjust as per your dimension
//                //double angleStart = 0; // Start angle of the arc
//                //double angleEnd = Math.PI / 2; // End angle (90 degrees)
//                //double extrudeHeight = 100;
//                //double thickness = 10;

//                //// Create the two arcs (inner and outer)

//                //Arc innerArc = new Arc(Plane.XY, Point2D.Origin, innerRadius, angleStart, angleEnd);
//                //Arc outerArc = new Arc(Plane.XY, Point2D.Origin, outerRadius, angleStart, angleEnd);
//                //// Create lines to close the profile
//                //Line line1 = new Line(innerArc.EndPoint, outerArc.EndPoint);
//                //Line line2 = new Line(outerArc.StartPoint, innerArc.StartPoint);

//                //CompositeCurve profile = new CompositeCurve(innerArc, line1, outerArc, line2);               
//                //Region region = new Region(profile);
//                //Brep brep = region.ExtrudeAsBrep(extrudeHeight);
//                //drawing.Entities.Add(brep);
//                //Plane p1 = Plane.XY;
//                //p1 = p1.Offset(10);
//                //Arc innerArc1 = new Arc(p1, Point2D.Origin, innerRadius+thickness, angleStart, angleEnd);
//                //Arc outerArc1 = new Arc(p1, Point2D.Origin, outerRadius-thickness, angleStart, angleEnd);
//                //// Create lines to close the profile
//                // line1 = new Line(innerArc.EndPoint, outerArc.EndPoint);
//                // line2 = new Line(outerArc.StartPoint, innerArc.StartPoint);

//                // profile = new CompositeCurve(innerArc, line1, outerArc, line2);

//                //// Optionally, if you want to create a region from the profile (in case devDept supports regions)
//                //Region region2 = new Region(profile);
//                //Brep brep1 = region.ExtrudeAsBrep(extrudeHeight);
//                //drawing.Entities.Add(brep1,Color.Blue);



//                LinearPath rectangle = new LinearPath(Plane.XY, new Point3D[]
//                {
//                    new Point3D(400, 0, 0),
//                    new Point3D(400+50, 0, 0),
//                    new Point3D(400+50, 50, 0),
//                    new Point3D(400, 50, 0),
//                    new Point3D(400, 0, 0)
//                });

//                LinearPath rectangle1 = new LinearPath(Plane.XY, new Point3D[]
//                {
//                    new Point3D(410, 10, 0),
//                    new Point3D(400+40, 10, 0),
//                    new Point3D(400+40, 40, 0),
//                    new Point3D(410, 40, 0),
//                    new Point3D(410, 10, 0)
//                });

//                CompositeCurve compositeCurve = new CompositeCurve(new ICurve[] { rectangle, rectangle1 });
//                devDept.Eyeshot.Entities.Region region2 = new devDept.Eyeshot.Entities.Region(rectangle);
//                devDept.Eyeshot.Entities.Region region3 = new devDept.Eyeshot.Entities.Region(rectangle1);
//                devDept.Eyeshot.Entities.Region region = devDept.Eyeshot.Entities.Region.Difference(region2, region3)[0];
//                Brep brep1 = region.RevolveAsBrep(Utility.DegToRad(90), Vector3D.AxisY, new Point3D(0, 200, 0));
//                // Brep brep2 = region3.RevolveAsBrep(Utility.DegToRad(90), Vector3D.AxisY, Point3D(0,));
//                drawing.Entities.Add(brep1, Color.Yellow);

//                var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//                if (!Directory.Exists(path + "/Bullows Panel Drawing"))
//                    Directory.CreateDirectory(path + "/Bullows Panel Drawing");

//                string dwgFilePath = path + "/Bullows Panel Drawing/" + "loft" + DateTime.Now.ToString("hh-mm") + ".dwg";

//                // Save as DWG
//                WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//                WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//                dwgg1.DoWork();

//                // Save as PDF
//                Write3DPdfParams pdf = new Write3DPdfParams(drawing);
//                Write3DPDF pdf1 = new Write3DPDF(pdf, path + "/loft.pdf");
//                pdf1.DoWork();
//            }
//            catch (Exception ex)
//            {

//            }

           
           
           
            

//        }
//        public void MetalBaffle()
//        {
//            DesignDocument drawing = new DesignDocument();
//            drawing.Units = linearUnitsType.Millimeters;
//            double W = 75;
//            double H = 20;
//            double ST = 1.2; //SheetThickness
//            double offsetDistance = 34;
//            double gap = 9.5;
//            double ExtrudeHeight = 912;
//            LinearPath rail = new LinearPath(new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(0,H),
//                new Point3D(W,H),
//                new Point3D(W,0),
//                new Point3D(W-ST,0),
//                new Point3D(W-ST,H-ST),
//                new Point3D(ST,H-ST),
//                new Point3D(ST,0),
//                new Point3D(0,0),
//            });
//            Brep brep = rail.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0);
//            drawing.Entities.Add(brep, Color.Yellow);
//            int i = 1;
//            LinearPath railClone = rail;
//            LinearPath oppositeRail = new LinearPath(new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(0,H),
//                new Point3D(W,H),
//                new Point3D(W,0),
//                new Point3D(W-ST,0),
//                new Point3D(W-ST,H-ST),
//                new Point3D(ST,H-ST),
//                new Point3D(ST,0),
//                new Point3D(0,0),
//            });
//            while (i < 7)
//            {
//                railClone = (LinearPath)railClone.Clone();
//                railClone.Translate(W + offsetDistance, 0, 0);
//                Brep brepClone = railClone.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0); // Extrude each clone
//                drawing.Entities.Add(brepClone, Color.Yellow);
//                // drawing.Entities.Add(railClone, Color.Yellow);
//                i++;
//            }
//            i = 0;
//            Point3D centerPoint = new Point3D(W / 2, 0); // Adjusted to be in the middle of the shape

//            oppositeRail.Rotate(Math.PI, Vector3D.AxisZ, centerPoint);
//            oppositeRail.Translate(54.5, 1.6);
//            Brep oppositeBrepClone = oppositeRail.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0); // Extrude each opposite clone
//            drawing.Entities.Add(oppositeBrepClone, Color.Yellow);
//            while (i < 5)
//            {
//                oppositeRail = (LinearPath)oppositeRail.Clone();
//                oppositeRail.Translate(W + offsetDistance, 1.6);
//                oppositeBrepClone = oppositeRail.ExtrudeAsBrep(Vector3D.AxisZ * ExtrudeHeight, 0); // Extrude each opposite clone
//                drawing.Entities.Add(oppositeBrepClone, Color.Yellow);

//                i++;
//            }

//            // Define the Bottom Plate with dimensions 572.6 x 40 and position it 150 units from the bottom
//            double bottomPlateWidth = (620 + 108);
//            double bottomPlateHeight = 40;
//            double bottomPlateYPosition = 150;

//            LinearPath bottomPlate = new LinearPath(new Point3D[]
//            {
//                new Point3D(0, 0),
//                new Point3D(bottomPlateWidth, 0),
//                new Point3D(bottomPlateWidth, bottomPlateHeight),
//                new Point3D(0, bottomPlateHeight),
//                new Point3D(0, 0)
//            });

//            bottomPlate.Translate(0, 20, bottomPlateYPosition);
//            // Define the hole radius and positions (20 units from each side)
//            double holeRadius = 5; // Adjust hole radius as needed
//            double holeOffsetFromEdge = 20;

//            // Left Hole position
//            Circle leftHole = new Circle(new Point3D(holeOffsetFromEdge, bottomPlateHeight, bottomPlateYPosition), holeRadius);

//            // Right Hole position
//            Circle rightHole = new Circle(new Point3D(bottomPlateWidth - holeOffsetFromEdge, bottomPlateHeight, bottomPlateYPosition), holeRadius);
//            //// Extrude the Bottom Plate
//            Brep bottomPlateBrep = bottomPlate.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0); // Extrude by 5 units           
//            Brep leftHoleBrep = leftHole.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
//            Brep rightHoleBrep = rightHole.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);

//            // Add the Bottom Plate with holes to the drawing

//            drawing.Entities.Add(leftHoleBrep, Color.Green);
//            drawing.Entities.Add(rightHoleBrep, Color.Green);
//            drawing.Entities.Add(bottomPlateBrep, Color.Green);

//            LinearPath bottomPlateClone = (LinearPath)bottomPlate.Clone();
//            // Adjust the Y translation for the cloned Bottom Plate
//            bottomPlateClone.Translate(0, 0, 612);
//            // Clone and translate the hole circles to the second Bottom Plate position
//            Circle leftHoleClone = (Circle)leftHole.Clone();
//            leftHoleClone.Translate(0, 0, 612);
//            Circle rightHoleClone = (Circle)rightHole.Clone();
//            rightHoleClone.Translate(0, 0, 612);

//            // Extrude and subtract holes from the second Bottom Plate
//            Brep leftHoleBrep2 = leftHoleClone.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
//            Brep rightHoleBrep2 = rightHoleClone.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0);
//            // Extrude and add the second Bottom Plate
//            Brep bottomPlateBrep2 = bottomPlateClone.ExtrudeAsBrep(Vector3D.AxisZ * 5, 0); // Extrude by 5 units
//            drawing.Entities.Add(leftHoleClone, Color.Green);
//            drawing.Entities.Add(rightHoleClone, Color.Green);

//            drawing.Entities.Add(bottomPlateBrep2, Color.Green);

//            #region Write file         
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
//                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

//            string dwgFilePath = path + "/Bullows Panel Drawing/" + "CSection" + DateTime.Now.ToString("hh-mm") + ".dwg";

//            // Save as DWG
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//            dwgg1.DoWork();
//            #endregion
//        }


//        public void FilterFrame1()
//        {
//            drawing = new DesignDocument();
//            drawing.Units = linearUnitsType.Millimeters;
//            double frameWidth = 600;
//            double frameHeight = 1490.4;
//            double railWidth = 30;
//            double railHeight = 40;
//            double sheetThickness = 1.2;
//            double holeRadius = 7 / 2;

//            // Frame Outline
//            LinearPath rail = new LinearPath(new Point3D[]
//            {
//                new Point3D(0, 0),
//                new Point3D(frameWidth, 0),
//                new Point3D(frameWidth, frameHeight),
//                new Point3D(0, frameHeight),
//                new Point3D(0, 0)
//            });

//            // Frame Section for Extrusion
//            var section = devregion.CreatePolygon(new Point3D[]
//            {
//                new Point3D(0, 0,0),
//                new Point3D(0, 0, railHeight),
//                new Point3D(0, railWidth, railHeight),
//                new Point3D(0, railWidth, railHeight - sheetThickness),
//                new Point3D(0, sheetThickness, railHeight - sheetThickness),
//                new Point3D(0, sheetThickness, 0),
//            });
//            Solid frame = section.SweepAsSolid(rail, 0);
//            frame.Translate(0, 0, sheetThickness);
//            devregion centercircle = devregion.CreateCircle(Plane.XY, new Point3D(frameWidth / 2, railHeight / 2), holeRadius);
//            frame.ExtrudeRemove(centercircle, 50, 0);
//            devregion leftCircle = devregion.CreateCircle(Plane.XY, new Point3D((frameWidth / 2) - 285, railHeight / 2), holeRadius);
//            frame.ExtrudeRemove(leftCircle, 50, 0);
//            devregion rightCircle = devregion.CreateCircle(Plane.XY, new Point3D((frameWidth / 2) + 285, railHeight / 2), holeRadius);
//            frame.ExtrudeRemove(rightCircle, 50, 0);

//            // Cloning and positioning holes for the top side
//            var topCenterCircle = (devregion)centercircle.Clone();
//            topCenterCircle.Translate(0, frameHeight - railHeight, 0);
//            frame.ExtrudeRemove(topCenterCircle, 50, 0);

//            var topLeftCircle = (devregion)leftCircle.Clone();
//            topLeftCircle.Translate(0, frameHeight - railHeight, 0);
//            frame.ExtrudeRemove(topLeftCircle, 50, 0);

//            var topRightCircle = (devregion)rightCircle.Clone();
//            topRightCircle.Translate(0, frameHeight - railHeight, 0);
//            frame.ExtrudeRemove(topRightCircle, 50, 0);

//            // New holes on the left side, equally spaced from top to bottom
//            double leftSideHoleSpacing = frameHeight / 3; // Dividing frame height by 3 for equal spacing

//            devregion leftSideTopHole = devregion.CreateCircle(Plane.XY, new Point3D(railWidth / 2, frameHeight - leftSideHoleSpacing), holeRadius);
//            frame.ExtrudeRemove(leftSideTopHole, 50, 0);

//            devregion leftSideBottomHole = devregion.CreateCircle(Plane.XY, new Point3D(railWidth / 2, leftSideHoleSpacing), holeRadius);
//            frame.ExtrudeRemove(leftSideBottomHole, 50, 0);
//            // Clone the left side holes for the right side
//            var rightSideTopHole = (devregion)leftSideTopHole.Clone();
//            rightSideTopHole.Translate(frameWidth - railWidth, 0, 0); // Move to the right side
//            frame.ExtrudeRemove(rightSideTopHole, 50, 0);

//            var rightSideBottomHole = (devregion)leftSideBottomHole.Clone();
//            rightSideBottomHole.Translate(frameWidth - railWidth, 0, 0); // Move to the right side
//            frame.ExtrudeRemove(rightSideBottomHole, 50, 0);

//            drawing.Entities.Add(frame, Color.Yellow);

//            #region Write file         
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
//                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

//            string dwgFilePath = path + "/Bullows Panel Drawing/" + "FilterFrame" + DateTime.Now.ToString("hh-mm") + ".dwg";

//            // Save as DWG
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//            dwgg1.DoWork();
//            #endregion
//        }
//        public void FilterFrame2()
//        {
//            drawing = new DesignDocument();
//            drawing.Units = linearUnitsType.Millimeters;
//            double frameWidth = 30;
//            double frameHeight = 1486;
//            double sheetThickness = 2;
//            double offset = frameWidth + 540;
//            double bottomRectangleWidth = 540;
//            double circleCenterX = frameWidth + (bottomRectangleWidth / 2); // Center in X
//            double circleCenterY = frameWidth / 2; // Center in Y
//            double holeRadius = 7 / 2;
//            //Left side 
//            devregion rectangle = devregion.CreatePolygon(new Point3D[]
//            {
//                 new Point3D(0, 0),
//                new Point3D(frameWidth, 0),
//                new Point3D(frameWidth, frameHeight),
//                new Point3D(0, frameHeight),
//            });
//            Brep brepRectangle = rectangle.ExtrudeAsBrep(sheetThickness, 0, 0);

//            double leftSideHoleSpacing = frameHeight / 3; // Dividing frame height by 3 for equal spacing

//            devregion leftSideTopHole = devregion.CreateCircle(Plane.XY, new Point3D(frameWidth / 2, frameHeight - leftSideHoleSpacing), holeRadius);
//            brepRectangle.ExtrudeRemove(leftSideTopHole, 50, 0);

//            devregion leftSideBottomHole = devregion.CreateCircle(Plane.XY, new Point3D(frameWidth / 2, leftSideHoleSpacing), holeRadius);
//            brepRectangle.ExtrudeRemove(leftSideBottomHole, 50, 0);

//            devregion leftCircle = devregion.CreateCircle(Plane.XY, new Point3D((frameWidth / 2), 15), holeRadius);
//            brepRectangle.ExtrudeRemove(leftCircle, 50, 0);

//            devregion leftCircleTop = devregion.CreateCircle(Plane.XY, new Point3D((frameWidth / 2), frameHeight - 15), holeRadius);
//            brepRectangle.ExtrudeRemove(leftCircleTop, 50, 0);



//            //  devregion leftCentreCircle = devregion.CreateCircle(Plane.XY, new Point3D(frameWidth/2, frameHeight / 2), 3.5);
//            // drawing.Entities.Add(leftCentreCircle, Color.Red);
//            drawing.Entities.Add(brepRectangle, Color.Yellow);
//            //right side 
//            Brep brepRectangle2 = (Brep)brepRectangle.Clone();

//            // Apply translation to the cloned rectangle to position it 540 units to the right
//            brepRectangle2.Translate(offset, 0, 0);
//            drawing.Entities.Add(brepRectangle2, Color.Yellow);
//            //Bottom plate
//            devregion bottomRect = devregion.CreatePolygon(new Point3D[]
//            {
//                new Point3D(frameWidth,0),
//                new Point3D(bottomRectangleWidth+frameWidth,0),
//                new Point3D(bottomRectangleWidth+frameWidth,frameWidth),
//                new Point3D(frameWidth,frameWidth),
//            });
//            Brep brepRectangle3 = bottomRect.ExtrudeAsBrep(sheetThickness, 0, 0);
//            devregion BottomCircle = devregion.CreateCircle(Plane.XY, circleCenterX, circleCenterY, 3.5);
//            brepRectangle3.ExtrudeRemove(BottomCircle, 50, 0);
//            drawing.Entities.Add(brepRectangle3, Color.Yellow);

//            Brep toprectangle = (Brep)brepRectangle3.Clone();
//            toprectangle.Translate(0, 1456, 0);
//            drawing.Entities.Add(toprectangle, Color.Yellow);
//            #region Write file         
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bullows Panel Drawing"))
//                Directory.CreateDirectory(path + "/Bullows Panel Drawing");

//            string dwgFilePath = path + "/Bullows Panel Drawing/" + "FilterFrame2" + "" + DateTime.Now.ToString("hh-mm") + ".dwg";

//            // Save as DWG
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//            dwgg1.DoWork();
//            #endregion
//        }

//    }
//}