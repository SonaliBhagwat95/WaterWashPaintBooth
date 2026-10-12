
//using devDept.Eyeshot;
//using devDept.Eyeshot.Entities;
//using devDept.Eyeshot.Translators;
//using devDept.Geometry;
//using Microsoft.Extensions.Configuration;


//using System.Drawing;
//using devregion = devDept.Eyeshot.Entities.Region;

//namespace PaintBooth.Business
//{
//    public class BendSection
//    {
//        public DesignDocument drawing { get; set; }
//        #region 3D Dwg file
//        public string CSection3DDrawing(BendSectionModel model)
//        {
//            double W = (double)model.W;
//            double H = (double)model.H;
//            double T = (double)model.T;
//            double Length = (double)model.Length;

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(new Point3D[]
//            {
//               new Point3D(0,0),
//               new Point3D(W,0),
//               new Point3D(W,T),
//               new Point3D(T,T),
//               new Point3D(T,(H-T)),
//               new Point3D(W,(H-T)),
//               new Point3D(W,H),
//               new Point3D(0,H),

//            });
//            Brep brep = rectangle.ExtrudeAsBrep(Length);
//            drawing.Entities.Add(brep, Color.Yellow);
//            #region Create Slots
//            //string[] dimensions = model.SlotDimentions.Split('-');
//            //if (dimensions.Length != 2)
//            //{
//            //    throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//            //}
//            //// Parse the width and height
//            //if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//            //{
//            //    throw new ArgumentException("Slot dimensions must be numeric values.");
//            //}
//            //#region XZ plane
//            //// For height (XZ plane)
//            //double divisionResultonLength = Length / model.PitchDistance;


//            //int NoofHoles = (int)Math.Floor(divisionResultonLength);
//            //double multipliedResultHeight = NoofHoles * model.PitchDistance;
//            //double sameSpaceDivideHeight = Length - multipliedResultHeight;


//            ////for (int i = 0; i <= NoofHoles; i++)
//            ////{
//            ////    //double centerZ = (i == 0) ? (sameSpaceDivideHeight / 2) : sameSpaceDivideHeight / 2 + i * model.PitchDistance;

//            ////    double centerZ = (i == 0) ? (sameSpaceDivideHeight / 2) : sameSpaceDivideHeight/2+ i * model.PitchDistance;

//            ////    // Create the slot on the ZX plane               
//            ////    devregion slot = devregion.CreateSlot(Plane.ZX, centerZ, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//            ////    // The slot needs to be translated into position correctly in the ZX plane
//            ////    slot.Translate(W/2, 0,centerZ);                            
//            ////    drawing.Entities.Add(slot, Color.Red);
//            ////}
//            //// Calculate the starting Z position
//            //double startZ = sameSpaceDivideHeight / 2;

//            //if (model.SlotLocation == "Top Sides")
//            //{

           
//            //    for (int i = 0; i <= NoofHoles; i++)
//            //    {
//            //        double centerZ = startZ + i * model.PitchDistance;

//            //        // Create the slot on the ZX plane
//            //        if (i == 0)
//            //        {
//            //            devregion slot = devregion.CreateSlot(Plane.ZX, startZ / 2, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//            //            slot.Translate(W / 2, 0, startZ / 2);
//            //            brep.ExtrudeRemove(slot, T, 0);
//            //            drawing.Entities.Add(brep, Color.Red);
//            //        }
//            //        else
//            //        {
//            //            devregion slot = devregion.CreateSlot(Plane.ZX, centerZ / 2, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//            //            slot.Translate(W / 2, 0, centerZ / 2);
//            //            //drawing.Entities.Add(slot, Color.Red);
//            //            brep.ExtrudeRemove(slot, T, 0);
//            //            drawing.Entities.Add(brep, Color.Red);
//            //        }
//            //    }
//            //}

//            //if (model.SlotLocation == "Both Sides")
//            //{


//            //    for (int i = 0; i <= NoofHoles; i++)
//            //    {
//            //        double centerZ = startZ + i * model.PitchDistance;

//            //        // Create the slot on the ZX plane
//            //        if (i == 0)
//            //        {
//            //            devregion slot = devregion.CreateSlot(Plane.ZX, startZ / 2, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//            //            slot.Translate(W / 2, 0, startZ / 2);
//            //            brep.ExtrudeRemove(slot, H, 0);
//            //            drawing.Entities.Add(brep, Color.Red);
//            //        }
//            //        else
//            //        {
//            //            devregion slot = devregion.CreateSlot(Plane.ZX, centerZ / 2, (slotLength - slotWidth), slotWidth / 2, 1.5708);
//            //            slot.Translate(W / 2, 0, centerZ / 2);
//            //            //drawing.Entities.Add(slot, Color.Red);
//            //            brep.ExtrudeRemove(slot, H, 0);
//            //            drawing.Entities.Add(brep, Color.Red);
//            //        }
//            //    }
//            //}
//            //#endregion



//            #endregion
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/CSectionDWG"))
//                Directory.CreateDirectory(path + "/CSectionDWG");
//            var dwgFilePathfor3D = Path.Combine(path, "CSectionDWG", "CSectionDWG" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfor3D);
//            dwgg1.DoWork();
//            return dwgFilePathfor3D;
//        }
//        public string LSection3DDrawing(BendSectionModel model)
//        {
//            double W = (double)model.W;
//            double H = (double)model.H;
//            double T = (double)model.T;
//           double Length = (double)model.Length;

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(new Point3D[]
//            {
//               new Point3D(0,0),
//               new Point3D(T,0),
//               new Point3D(T,(H-T)),
//               new Point3D(W,(H-T)),
//               new Point3D(W,H),             
//               new Point3D(0,H),

//            });
//           Brep brep = rectangle.ExtrudeAsBrep(Length);
//            drawing.Entities.Add(brep,Color.Yellow);

//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/LSectionDWG"))
//                Directory.CreateDirectory(path + "/LSectionDWG");
//            var dwgFilePathfor3D = Path.Combine(path, "LSectionDWG", "LSectionDWG" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfor3D);
//            dwgg1.DoWork();
//            return dwgFilePathfor3D;
//        }

//        public string L1Section3DDrawing(BendSectionModel model)
//        {
//            double W = (double)model.W;
//            double H = (double)model.H;
//            double T = (double)model.T;
//            double L = (double)model.L;
//            double Length = (double)model.Length;

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(new Point3D[]
//            {
//               new Point3D(0,0),
//               new Point3D(T,0),
//               new Point3D(T,(W+T)),
//               new Point3D(-(H-T),(W+T)),
//               new Point3D(-(H-T),(W+L)-T),

//               new Point3D(T,(W+L)-T),
//               new Point3D(T,(W+W+L)),
//               new Point3D(0,(W+W+L)),
//               new Point3D(0,L+W),
//               new Point3D(-H,L+W),
//               new Point3D(-H,W),
//               new Point3D(0,W),

//            });
//            Brep brep = rectangle.ExtrudeAsBrep(Length);
//            drawing.Entities.Add(brep,Color.Yellow);

//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/L1SectionDWG"))
//                Directory.CreateDirectory(path + "/L1SectionDWG");
//            var dwgFilePathfor3D = Path.Combine(path, "L1SectionDWG", "L1SectionDWG" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfor3D);
//            dwgg1.DoWork();
//            return dwgFilePathfor3D;
//        }
//        public string PanelSupport3DDrawing(BendSectionModel model)
//        {
//            double W = (double)model.W;
//            double H = (double)model.H;
//            double T = (double)model.T;
//            double L = (double)model.L;
//            double Length = (double)model.Length;

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(new Point3D[]
//            {
//               new Point3D(0,0),
//               new Point3D(0,H),
//               new Point3D(L,H),
//               new Point3D(L,(H-W)),
//               new Point3D((L-T),(H-W)),
//               new Point3D((L-T),(H-T)),
//               new Point3D(T,(H-T)),
//               new Point3D(T,T),
//               new Point3D((L-T),T),
//               new Point3D((L-T),W),
//               new Point3D(L,W),
//               new Point3D(L,0)


//            });
//            Brep brep = rectangle.ExtrudeAsBrep(Length);
//            drawing.Entities.Add(brep, Color.Yellow);

//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/PanelSupportDWG"))
//                Directory.CreateDirectory(path + "/PanelSupportDWG");
//            var dwgFilePathfor3D = Path.Combine(path, "PanelSupportDWG", "PanelSupportDWG" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfor3D);
//            dwgg1.DoWork();
//            return dwgFilePathfor3D;
//        }
//        public  string Corners3DDrawing(BendSectionModel model)
//        {
//            double W = (double)model.W;
//            double H = (double)model.H;
//            double T = (double)model.T;
//            double L = (double)model.L;
//            double L1 = (double)model.L1;
//            double Length = (double)model.Length;

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(new Point3D[]
//            {
//               new Point3D(0,0),
//               new Point3D(0,H),
//               new Point3D(L,H),
//               new Point3D(L,(H-W)),

//               new Point3D((L-L1),(H-W)),
//               new Point3D((L-L1),(H-(W-T))),
//               new Point3D((L-T),(H-(W-T))),

//               new Point3D((L-T),(H-T)),
//                new Point3D(T,(H-T)),
//               new Point3D(T,T),
              
//               new Point3D((W-T),T),
//               new Point3D((W-T),L1),
//               new Point3D(W,L1),
//               new Point3D(W,0)


//            });
//           Brep brep = rectangle.ExtrudeAsBrep(Length);
//            drawing.Entities.Add(brep, Color.Yellow);

//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/CornerDWG"))
//                Directory.CreateDirectory(path + "/CornerDWG");
//            var dwgFilePathfor3D = Path.Combine(path, "CornerDWG", "CornerDWG" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfor3D);
//            dwgg1.DoWork();
//            return dwgFilePathfor3D;
//        }
//        #endregion
//        #region Development
//        public string devlopmentForCSection(BendSectionModel model, List<SlotDetail> slotDetails)
//        {
//            double X = (double)model.Length;
//            decimal H = model.H - 2 * model.T;
//            decimal W = model.W - model.T;
//            decimal Y = H + (2 * W);

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(Plane.XY, new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(X,0),
//                new Point3D(X,(double)Y),
//                new Point3D(0,(double)Y),
//            });
//            drawing.Entities.Add(rectangle, Color.White);
//            LinearPath BendctangleleftBottom = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(model.W-model.T)),
//                  new Point3D(50,(double)(model.W-model.T))
//            });
//            drawing.Entities.Add(BendctangleleftBottom, Color.Yellow);
//            LinearPath BendctangleleftTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(Y-(model.W-model.T))),

//                  new Point3D(50,(double)(Y-(model.W-model.T)))

//            });
//            drawing.Entities.Add(BendctangleleftTop, Color.Yellow);
//            LinearPath BendctangleRightBottom = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(model.W-model.T)),
//                  new Point3D(X-50,(double)(model.W-model.T))
//            });
//            drawing.Entities.Add(BendctangleRightBottom, Color.Yellow);
//            LinearPath BendctangleRightTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-(model.W-model.T))),
//                  new Point3D(X-50,(double)(Y-(model.W-model.T)))
//            });
//            drawing.Entities.Add(BendctangleRightTop, Color.Yellow);
//            #region Create Slots
//            if(slotDetails.Count!=0)
//            {
//            foreach (var slotDetail in slotDetails)
//            {
//                double pitchDistance = (double)slotDetail.PitchDistance;
//                string slotLocation = slotDetail.SlotLocation;
//                string[] dimensions = slotDetail.SlotDimensions.Split('-');

//                if (dimensions.Length != 2)
//                {
//                    throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//                }

//                if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//                {
//                    throw new ArgumentException("Slot dimensions must be numeric values.");
//                }

//                // Create slots for the specified location
//                double divisionResult = X / pitchDistance;
//                int wholeNumberPart = (int)Math.Floor(divisionResult);

//                double multipliedResult = wholeNumberPart * pitchDistance;
//                if (multipliedResult == X)
//                {
//                    int totalHoles = wholeNumberPart - 1;
//                    multipliedResult = totalHoles * model.PitchDistance;
//                    // wholeNumberPart = totalHoles;
//                }

//                double remainingSpace = X - multipliedResult;

//                for (int i = 0; i < wholeNumberPart; i++)
//                {
//                     double centerX;
//                if (i == 0)
//                        centerX = remainingSpace / 2; // Adjust for the first slot position
//                    else
//                        centerX = remainingSpace / 2 + (i) * model.PitchDistance;

//                    // Create the slot (hole) at the calculated position
//                    devregion slot2 = null; devregion slot3 = null;
//                    if (slotDetail.SlotLocation=="Top Sides")
//                    {
//                         slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - ((model.W - model.T) / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                    }
//                    else if(slotDetail.SlotLocation == "Bottom Sides")
//                    {
//                         slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(((model.W - model.T) / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                    } 
//                    else if(slotDetail.SlotLocation == "On Height")
//                    {
//                        slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(H - ((model.W - model.T) / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                    }
//                    else if (slotDetail.SlotLocation == "Both Sides")
//                    {
//                        slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - ((model.W - model.T) / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        slot3 = devregion.CreateSlot(Plane.XY, centerX, (double)(((model.W - model.T) / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        drawing.Entities.Add(slot3, Color.White);
//                    }

//                    // Optional: Translate the slot if needed (e.g., adjust the Z-position)
//                    slot2.Translate(0, 0, remainingSpace / 2);
//                    slot2.Color = Color.Yellow;

//                    // Add the slot to the drawing
//                    drawing.Entities.Add(slot2, Color.White);
//                }
//            }
//            }
            
//            #endregion

//            #endregion
//            #region WriteFile
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bend Development"))
//                Directory.CreateDirectory(path + "/Bend Development");
//            var dwgFilePathfordevelopment = Path.Combine(path, "Bend Development", "BendDevelopment" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
//            dwgg1.DoWork();
//            return dwgFilePathfordevelopment;
//            #endregion
//        }
       
//        public string devlopmentForLSection(BendSectionModel model,List<SlotDetail> slotDetails)
//        {
//            double X = (double)model.Length;
//            decimal H = model.H - model.T;
//            decimal W = model.W - model.T;
//            decimal Y = H + W;
//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(Plane.XY, new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(X,0),
//                new Point3D(X,(double)Y),
//                new Point3D(0,(double)Y),
//            });
//            drawing.Entities.Add(rectangle, Color.White);
           
//            LinearPath BendctangleleftTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(Y-(model.W-model.T))),
//                  new Point3D(50,(double)(Y-(model.W-model.T)))
//            });
//            drawing.Entities.Add(BendctangleleftTop, Color.Yellow);
           
//            LinearPath BendctangleRightTop = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-(model.W-model.T))),
//                  new Point3D(X-50,(double)(Y-(model.W-model.T)))
//            });
//            drawing.Entities.Add(BendctangleRightTop, Color.Yellow);

//            #region Create Slots
//            if (slotDetails.Count != 0)
//            {
//                foreach (var slotDetail in slotDetails)
//            {
//                double pitchDistance = (double)slotDetail.PitchDistance;
//                string slotLocation = slotDetail.SlotLocation;
//                string[] dimensions = slotDetail.SlotDimensions.Split('-');

//                if (dimensions.Length != 2)
//                {
//                    throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//                }

//                if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//                {
//                    throw new ArgumentException("Slot dimensions must be numeric values.");
//                }

//                // Create slots for the specified location
//                double divisionResult = X / pitchDistance;
//                int wholeNumberPart = (int)Math.Floor(divisionResult);

//                double multipliedResult = wholeNumberPart * pitchDistance;
//                if (multipliedResult == X)
//                {
//                    int totalHoles = wholeNumberPart - 1;
//                    multipliedResult = totalHoles * model.PitchDistance;
//                    // wholeNumberPart = totalHoles;
//                }

//                double remainingSpace = X - multipliedResult;

//                for (int i = 0; i < wholeNumberPart; i++)
//                {
//                    double centerX;
//                    if (i == 0)
//                        centerX = remainingSpace / 2; // Adjust for the first slot position
//                    else
//                        centerX = remainingSpace / 2 + (i) * model.PitchDistance;

//                    // Create the slot (hole) at the calculated position
//                    devregion slot2 = null;
//                    if (slotDetail.SlotLocation == "Top Sides")
//                    {
//                        slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - ((model.W - model.T) / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                    }
                   
//                    else if (slotDetail.SlotLocation == "On Height")
//                    {
//                        slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(H/2), (slotLength - slotWidth), slotWidth / 2, 0);
//                    }

//                    // Optional: Translate the slot if needed (e.g., adjust the Z-position)
//                    slot2.Translate(0, 0, remainingSpace / 2);
//                    slot2.Color = Color.Yellow;

//                    // Add the slot to the drawing
//                    drawing.Entities.Add(slot2, Color.White);
//                }
//            }
//            }
//            #endregion
//            #region WriteFile
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bend Development"))
//                Directory.CreateDirectory(path + "/Bend Development");
//            var dwgFilePathfordevelopment = Path.Combine(path, "Bend Development", "BendDevelopment" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
//            dwgg1.DoWork();
//            return dwgFilePathfordevelopment;
//            #endregion
//        }
//        public string devlopmentForL1Section(BendSectionModel model, List<SlotDetail> slotDetails)
//        {
//            double X = (double)model.Length;
//            decimal H = model.H - model.T;
//            decimal W = model.W - model.T;
//            decimal L = model.L - 2*model.T;
//            //decimal Y =(2*(model.H + model.W))+model.L;
//            decimal Y = (2 * (H + W)) + L;

//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(Plane.XY, new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(X,0),
//                new Point3D(X,(double)Y),
//                new Point3D(0,(double)Y),
//            });
//            drawing.Entities.Add(rectangle, Color.White);
//            #region bendline Left side
//            //leftBottomW
//            LinearPath BendrectangleleftBottomW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(model.W-model.T)),
//                  new Point3D(50,(double)(model.W-model.T))
//            });
//            drawing.Entities.Add(BendrectangleleftBottomW, Color.Yellow);
//            //leftBottomH
//            //decimal W = model.W - model.T;
//            //decimal H = model.H - model.T;
//            LinearPath BendrectangleleftbottomH = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(W+H)),
//                  new Point3D(50,(double)(W+H))
//            });
//            drawing.Entities.Add(BendrectangleleftbottomH, Color.Yellow);

//            //leftTopW
            
//            LinearPath BendrectangleleftTopW = new LinearPath(new Point3D[]
//            {
               
//                 new Point3D(0,(double)(Y-W)),
//                  new Point3D(50,(double)(Y-W))
//            });
//            drawing.Entities.Add(BendrectangleleftTopW, Color.Yellow);
//            //leftTopH
//            LinearPath BendrectangleleftTopH = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(Y-(W+H))),
//                  new Point3D(50,(double)(Y-(W+H)))
//            });
//            drawing.Entities.Add(BendrectangleleftTopH, Color.Yellow);
//            #endregion

//            #region bendline Right side
//            //leftBottomW
//            LinearPath BendrectangleleRightBottomW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)W),
//                  new Point3D(X-50,(double)W)
//            });
//            drawing.Entities.Add(BendrectangleleRightBottomW, Color.Yellow);
//            //leftBottomH
//            LinearPath BendrectanglelRightbottomH = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(W+H)),
//                  new Point3D(X-50,(double)(W+H))
//            });
//            drawing.Entities.Add(BendrectanglelRightbottomH, Color.Yellow);

//            //leftTopW
//            LinearPath BendrectangleRightTopW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-W)),
//                  new Point3D(X-50,(double)(Y-W))
//            });
//            drawing.Entities.Add(BendrectangleRightTopW, Color.Yellow);
//            //leftTopH
//            LinearPath BendrectangleRightTopH = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-(W+H))),
//                  new Point3D(X-50,(double)(Y-(W+H)))
//            });
//            drawing.Entities.Add(BendrectangleRightTopH, Color.Yellow);
//            #endregion

//            #region Create Slots
//            if (slotDetails.Count != 0)
//            {
//                foreach (var slotDetail in slotDetails)
//                {
//                    double pitchDistance = (double)slotDetail.PitchDistance;
//                    string slotLocation = slotDetail.SlotLocation;
//                    string[] dimensions = slotDetail.SlotDimensions.Split('-');

//                    if (dimensions.Length != 2)
//                    {
//                        throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//                    }

//                    if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//                    {
//                        throw new ArgumentException("Slot dimensions must be numeric values.");
//                    }

//                    // Create slots for the specified location
//                    double divisionResult = X / pitchDistance;
//                    int wholeNumberPart = (int)Math.Floor(divisionResult);

//                    double multipliedResult = wholeNumberPart * pitchDistance;
//                    if (multipliedResult == X)
//                    {
//                        int totalHoles = wholeNumberPart - 1;
//                        multipliedResult = totalHoles * model.PitchDistance;
//                        // wholeNumberPart = totalHoles;
//                    }

//                    double remainingSpace = X - multipliedResult;

//                    for (int i = 0; i < wholeNumberPart; i++)
//                    {
//                        double centerX;
//                        if (i == 0)
//                            centerX = remainingSpace / 2; // Adjust for the first slot position
//                        else
//                            centerX = remainingSpace / 2 + (i) * model.PitchDistance;

//                        // Create the slot (hole) at the calculated position
//                        devregion slot2 = null;
//                        if (slotDetail.SlotLocation == "Top Sides")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y / 2), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }

//                        else if (slotDetail.SlotLocation == "On Height")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)((L / 2) + W / 2), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "Bottom Flange")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(W / 2), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "Top Flange")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - ((H / 2) + W)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "Bottom Sides")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - (W / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }

//                        // Optional: Translate the slot if needed (e.g., adjust the Z-position)
//                        slot2.Translate(0, 0, remainingSpace / 2);
//                        slot2.Color = Color.Yellow;

//                        // Add the slot to the drawing
//                        drawing.Entities.Add(slot2, Color.White);
//                    }
//                }
//            }
//            #endregion

//            #region WriteFile
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bend Development"))
//                Directory.CreateDirectory(path + "/Bend Development");
//            var dwgFilePathfordevelopment = Path.Combine(path, "Bend Development", "BendDevelopment" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
//            dwgg1.DoWork();
//            return dwgFilePathfordevelopment;
//            #endregion
//        }
//        public string devlopmentForPanelSupport(BendSectionModel model, List<SlotDetail> slotDetails)
//        {
//            double X = (double)model.Length;
//            decimal L = model.L - model.T;
//            decimal H = model.H - 2*model.T;
//            decimal W = model.W - model.T;

//            //decimal Y = (2*(model.L+model.W)+model.H) - 2 * model.T;                    
//            decimal Y = (2*(L+W)+H);


//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(Plane.XY, new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(X,0),
//                new Point3D(X,(double)Y),
//                new Point3D(0,(double)Y),
//            });
//            drawing.Entities.Add(rectangle, Color.White);
//            #region bendline Left side
//            //leftBottomW
//            LinearPath BendrectangleleftBottomW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)W),
//                  new Point3D(50,(double)W)
//            });
//            drawing.Entities.Add(BendrectangleleftBottomW, Color.Yellow);
//            //leftBottomH
//            LinearPath BendrectangleleftbottomL = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(L+W)),
//                  new Point3D(50,(double)(L+W))
//            });
//            drawing.Entities.Add(BendrectangleleftbottomL, Color.Yellow);

//            //leftTopW
//            LinearPath BendrectangleleftTopW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(Y-W)),
//                  new Point3D(50,(double)(Y-W))
//            });
//            drawing.Entities.Add(BendrectangleleftTopW, Color.Yellow);
//            //leftTopH
//            LinearPath BendrectangleleftTopL = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(Y-(L+W))),
//                  new Point3D(50,(double)(Y-(L+W)))
//            });
//            drawing.Entities.Add(BendrectangleleftTopL, Color.Yellow);
//            #endregion

//            #region bendline Right side
//            //leftBottomW
//            LinearPath BendrectangleleRightBottomW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)W),
//                  new Point3D(X-50,(double)W)
//            });
//            drawing.Entities.Add(BendrectangleleRightBottomW, Color.Yellow);
//            //leftBottomH
//            LinearPath BendrectanglelRightbottomL = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(L+W)),
//                  new Point3D(X-50,(double)(L+W))
//            });
//            drawing.Entities.Add(BendrectanglelRightbottomL, Color.Yellow);

//            //leftTopW
//            LinearPath BendrectangleRightTopW = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-W)),
//                  new Point3D(X-50,(double)(Y-W))
//            });
//            drawing.Entities.Add(BendrectangleRightTopW, Color.Yellow);
//            //leftTopH
//            LinearPath BendrectangleRightTopL = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-(L+W))),
//                  new Point3D(X-50,(double)(Y-(L+W)))
//            });
//            drawing.Entities.Add(BendrectangleRightTopL, Color.Yellow);
//            #endregion
//            #region Create Slots
//            if (slotDetails.Count != 0)
//            {
//                foreach (var slotDetail in slotDetails)
//                {
//                    double pitchDistance = (double)slotDetail.PitchDistance;
//                    string slotLocation = slotDetail.SlotLocation;
//                    string[] dimensions = slotDetail.SlotDimensions.Split('-');

//                    if (dimensions.Length != 2)
//                    {
//                        throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//                    }

//                    if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//                    {
//                        throw new ArgumentException("Slot dimensions must be numeric values.");
//                    }

//                    // Create slots for the specified location
//                    double divisionResult = X / pitchDistance;
//                    int wholeNumberPart = (int)Math.Floor(divisionResult);

//                    double multipliedResult = wholeNumberPart * pitchDistance;
//                    if (multipliedResult == X)
//                    {
//                        int totalHoles = wholeNumberPart - 1;
//                        multipliedResult = totalHoles * model.PitchDistance;
//                        // wholeNumberPart = totalHoles;
//                    }

//                    double remainingSpace = X - multipliedResult;

//                    for (int i = 0; i < wholeNumberPart; i++)
//                    {
//                        double centerX;
//                        if (i == 0)
//                            centerX = remainingSpace / 2; // Adjust for the first slot position
//                        else
//                            centerX = remainingSpace / 2 + (i) * model.PitchDistance;

//                        // Create the slot (hole) at the calculated position
//                        devregion slot2 = null;
//                        if (slotDetail.SlotLocation == "Top Sides")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(W + L + H + (L / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "Bottom Sides")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(W + (L / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "On Height")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)((L + W) + (H / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "Top Flange")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - (W / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "Bottom Flange")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(W / 2), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }

//                        // Optional: Translate the slot if needed (e.g., adjust the Z-position)
//                        slot2.Translate(0, 0, remainingSpace / 2);
//                        slot2.Color = Color.Yellow;

//                        // Add the slot to the drawing
//                        drawing.Entities.Add(slot2, Color.White);
//                    }
//                }
//                #endregion
//            }
//            #region WriteFile
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bend Development"))
//                Directory.CreateDirectory(path + "/Bend Development");
//            var dwgFilePathfordevelopment = Path.Combine(path, "Bend Development", "BendDevelopment" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
//            dwgg1.DoWork();
//            return dwgFilePathfordevelopment;
//            #endregion
//        }

//        public string devlopmentForCorner(BendSectionModel model, List<SlotDetail> slotDetails)
//        {
//            double X = (double)model.Length;
//            //decimal Y = (2 * (model.L + model.W) + model.H);

//            decimal L = model.L - model.T ;
//            decimal W = model.W-model.T;
//            decimal L1 = model.L1-model.T;
//            decimal H = model.H - model.T;
//            decimal Y = (2 * (L + W + L1) + H);
//            drawing = new();
//            drawing.Units = linearUnitsType.Millimeters;
//            var rectangle = devregion.CreatePolygon(Plane.XY, new Point3D[]
//            {
//                new Point3D(0,0),
//                new Point3D(X,0),
//                new Point3D(X,(double)Y),
//                new Point3D(0,(double)Y),
//            });
//            drawing.Entities.Add(rectangle, Color.White);
//            #region bendline Left side
//            #region L1
//            //leftBottomL1
//            LinearPath BendrectangleleftBottomL1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)L1),
//                  new Point3D(50,(double)L1)
//            });
//            drawing.Entities.Add(BendrectangleleftBottomL1, Color.Yellow);
//            //left Top L1
//            LinearPath BendrectangleleftTopL1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D(0,(double)(Y-L1)),
//                  new Point3D(50,(double)(Y-L1))
//            });
//            drawing.Entities.Add(BendrectangleleftTopL1, Color.Yellow);
//            // //right side L1
//            LinearPath BendrectangleRightBottomL1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)L1),
//                  new Point3D(X-50,(double)L1)
//            });
//            drawing.Entities.Add(BendrectangleRightBottomL1, Color.Yellow);
//            //left Top L1
//            LinearPath BendrectangleRightTopL1 = new LinearPath(new Point3D[]
//            {
//                 new Point3D(X,(double)(Y-L1)),
//                  new Point3D(X-50,(double)(Y-L1))
//            });
//            drawing.Entities.Add(BendrectangleRightTopL1, Color.Yellow);
//            #endregion
//            #region W
//            // //leftBottomW
//            LinearPath BendrectangleleftBottomW = new LinearPath(new Point3D[]
//            {
//                  new Point3D(0,(double)(L1+W)),
//                   new Point3D(50,(double)(L1+W))
//            });
//            drawing.Entities.Add(BendrectangleleftBottomW, Color.Yellow);
//            //left Top W
//            LinearPath BendrectangleleftTopW = new LinearPath(new Point3D[]
//            {
//                  new Point3D(0,(double)(Y-(L1+W))),
//                   new Point3D(50,(double)(Y-(L1+W)))
//            });
//            drawing.Entities.Add(BendrectangleleftTopW, Color.Yellow);
//            //Right side W
//            LinearPath BendrectangleRightBottomW = new LinearPath(new Point3D[]
//            {
//                  new Point3D(X,(double)(L1+W)),
//                   new Point3D(X - 50,(double)(L1+W))
//            });
//            drawing.Entities.Add(BendrectangleRightBottomW, Color.Yellow);
//            //Right Top W
//            LinearPath BendrectangleRightTopW = new LinearPath(new Point3D[]
//            {
//                  new Point3D(X,(double)(Y-(L1+W))),
//                   new Point3D(X - 50,(double)(Y-(L1+W)))
//            });
//            drawing.Entities.Add(BendrectangleRightTopW, Color.Yellow);
//            #endregion
//            // //leftBottomL
//            LinearPath BendrectangleleftbottomL = new LinearPath(new Point3D[]
//            {
//                  new Point3D(0,(double)(L+W+L1+H)),
//                   new Point3D(50,(double)(L+W+L1+H))
//            });
//            drawing.Entities.Add(BendrectangleleftbottomL, Color.Yellow);
//            //Right side L
//            LinearPath BendrectanglerightbottomL = new LinearPath(new Point3D[]
//            {
//                  new Point3D(X,(double)(L+W+L1+H)),
//                   new Point3D(X-50,(double)(L+W+L1+H))
//            });
//            drawing.Entities.Add(BendrectanglerightbottomL, Color.Yellow);
//            //Right H
//            LinearPath BendrectangleleftbottomH = new LinearPath(new Point3D[]
//            {
//                  new Point3D(0,(double)(W+L1+H)),
//                   new Point3D(50,(double)(W+L1+H))
//            });
//            drawing.Entities.Add(BendrectangleleftbottomH, Color.Yellow);
//            LinearPath BendrectanglerightbottomH = new LinearPath(new Point3D[]
//            {
//                  new Point3D(X,(double)(W+L1+H)),
//                   new Point3D(X-50,(double)(W+L1+H))
//            });
//            drawing.Entities.Add(BendrectanglerightbottomH, Color.Yellow);

//            #endregion

//            #region Create Slots
//            if (slotDetails.Count != 0)
//            {
//                foreach (var slotDetail in slotDetails)
//                {
//                    double pitchDistance = (double)slotDetail.PitchDistance;
//                    string slotLocation = slotDetail.SlotLocation;
//                    string[] dimensions = slotDetail.SlotDimensions.Split('-');

//                    if (dimensions.Length != 2)
//                    {
//                        throw new ArgumentException("Invalid slot dimensions format. Expected format is 'width-height'.");
//                    }

//                    if (!double.TryParse(dimensions[0], out double slotWidth) || !double.TryParse(dimensions[1], out double slotLength))
//                    {
//                        throw new ArgumentException("Slot dimensions must be numeric values.");
//                    }

//                    // Create slots for the specified location
//                    double divisionResult = X / pitchDistance;
//                    int wholeNumberPart = (int)Math.Floor(divisionResult);

//                    double multipliedResult = wholeNumberPart * pitchDistance;
//                    if (multipliedResult == X)
//                    {
//                        int totalHoles = wholeNumberPart - 1;
//                        multipliedResult = totalHoles * model.PitchDistance;
//                        // wholeNumberPart = totalHoles;
//                    }

//                    double remainingSpace = X - multipliedResult;

//                    for (int i = 0; i < wholeNumberPart; i++)
//                    {
//                        double centerX;
//                        if (i == 0)
//                            centerX = remainingSpace / 2; // Adjust for the first slot position
//                        else
//                            centerX = remainingSpace / 2 + (i) * model.PitchDistance;

//                        // Create the slot (hole) at the calculated position
//                        devregion slot2 = null; devregion slot3 = null;
//                        if (slotDetail.SlotLocation == "L1")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(L1 / 2), (slotLength - slotWidth), slotWidth / 2, 0);
//                            slot3 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - (L1 / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                            drawing.Entities.Add(slot3, Color.White);
//                        }
//                        else if (slotDetail.SlotLocation == "W")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(L1 + (W / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                            slot3 = devregion.CreateSlot(Plane.XY, centerX, (double)(Y - (L1 + (W / 2))), (slotLength - slotWidth), slotWidth / 2, 0);
//                            drawing.Entities.Add(slot3, Color.White);
//                        }
//                        else if (slotDetail.SlotLocation == "H")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(L1 + W + (H / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                        }
//                        else if (slotDetail.SlotLocation == "L")
//                        {
//                            slot2 = devregion.CreateSlot(Plane.XY, centerX, (double)(L1 + W + H + (L / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                            slot3 = devregion.CreateSlot(Plane.XY, centerX, (double)(L1 + W + H + L + (L / 2)), (slotLength - slotWidth), slotWidth / 2, 0);
//                            drawing.Entities.Add(slot3, Color.White);
//                        }
//                        // Optional: Translate the slot if needed (e.g., adjust the Z-position)
//                        slot2.Translate(0, 0, remainingSpace / 2);
//                        slot2.Color = Color.Yellow;

//                        // Add the slot to the drawing
//                        drawing.Entities.Add(slot2, Color.White);

//                    }
//                }
//                #endregion
//            }
//            #region WriteFile
//            var path = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("FolderPathConfig")["AbsolutePath"].ToString();

//            if (!Directory.Exists(path + "/Bend Development"))
//                Directory.CreateDirectory(path + "/Bend Development");
//            var dwgFilePathfordevelopment = Path.Combine(path, "Bend Development", "BendDevelopment" + DateTime.Now.ToString("hh-mm") + ".dwg");
//            WriteAutodeskParams auto = new WriteAutodeskParams(drawing);
//            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePathfordevelopment);
//            dwgg1.DoWork();
//            return dwgFilePathfordevelopment;
//            #endregion
//        }
        
//    }
//}
