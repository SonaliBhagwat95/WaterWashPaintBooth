

using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using PaintBooth.Business;
using PaintBooth.Database;
using PaintBooth.Model;
using PaintBooth.Models;
using System.Drawing;


namespace PaintBooth.Service
{
    public class PaintBoothService(AppDbContext _DbContext, IConfiguration _configuration)
    {

        DesignDocument drawing = new DesignDocument();
        #region List objects
        List<DesignDocument> rightSideDrawings = new List<DesignDocument>();
        List<DesignDocument> leftSideDrawings = new List<DesignDocument>();
        List<DesignDocument> TopAssembly = new List<DesignDocument>();
        List<DesignDocument> D3PanelRightDrawings = new List<DesignDocument>();
        List<DesignDocument> D3PanelLeftDrawings = new List<DesignDocument>();
        List<DesignDocument> D3PanelRightDrawingsBeforeD = new List<DesignDocument>();
        List<DesignDocument> D3PanelLeftDrawingsBeforeD = new List<DesignDocument>();
        List<DesignDocument> loftDrawing = new List<DesignDocument>();
        List<DesignDocument> FilterDrawings = new List<DesignDocument>();
        List<DesignDocument> TopFrameDrawing = new List<DesignDocument>();
        List<DesignDocument> BaseDrawings = new List<DesignDocument>();
        #endregion

        string BaseFilePath = _configuration["FolderPathConfig:DrawingPath"].ToString();
        #region Variables
        public Dictionary<string, List<DesignDocument>> designdictionary = new();
        public static int noOfPanelsforD = 0; public static double totalPanelsforD = 0;
        public static int noOfPanelsforH = 0; public static double totalPanelsforH = 0;
        public static double totalPanelsAboveEC = 0;
        public static double noOfPanelsAboveEC = 0; public static double smallPanelWidthAboveEC = 0;
        public static double totalPanelsAboveSideCompoDoor = 0; public static double noOfPanelsAboveSideCompoDoor = 0; public static double smallPanelWidthAboveSideCompoDoor = 0;

        public static double smallPanelWidthforH, smallPanelWidthForBackSide; public static double smallPanelWidthforD = 0;
        public static int noOfPanelsforW, noOfPanelsForBackSide; public static double totalPanelsforW = 0;
        public static double smallPanelWidthforW = 0; public static double TotalBackPanels = 0;
        public static double smallPanelsWidthDoors = 0;
        public static double smallPanelsWidthFrontDoors = 0;
        public static double noOfPanelsInSideDoor, RemainingSpanceinD;
        public static double noOfPanelsInFrontDoor = 0;
        public static double totalPanelsAboveCompEntryDoor = 0;
        public static double noOfPanelsAboveCompEntryDoor = 0;
        List<string> filePaths = new List<string>();
        #endregion


        #region Left and Right Panels
        public void LeftRightPanels(PaintBoothModel model, PaintBoothDesignBusiness paintBooth)
        {
            try
            {

                #region  D Calculate Number of panels in D
                model.PanelWidth = paintBooth.PanelWidth;
                totalPanelsforD = model.D / model.PanelWidth;
                noOfPanelsforD = (int)Math.Floor(totalPanelsforD);
                smallPanelWidthforD = model.D - (noOfPanelsforD * model.PanelWidth);
                #endregion

                #region H Calculate Number of Panels In H
                model.PanelHeight = paintBooth.PanelHeight;
                totalPanelsforH = model.H / model.PanelHeight;
                noOfPanelsforH = (int)Math.Floor(totalPanelsforH);
                smallPanelWidthforH = model.H - (noOfPanelsforH * model.PanelHeight);
                #endregion

                #region D * H left right
                int i = 0;

                // STEP 1: Delete old panel records for this enquiry
                for (i = 0; i < 2; i++)
                {
                    paintBooth.PanelLength = (i == 0) ? 0 : model.W;

                    List<DesignDocument> documents = new List<DesignDocument>();

                    for (int k = 0; k < noOfPanelsforH; k++)
                    {
                        for (int j = 0; j < noOfPanelsforD; j++)
                        {

                            PanelDrawResult panelDrawingPath = paintBooth.PanelsInPaintBooth(j + 1, model, i);
                            documents.Add(panelDrawingPath.drawing);
                            filePaths.Add(panelDrawingPath.lstpath);
                            paintBooth.SavePanelDetails(
                                   model,
                                   i == 0 ? "PARight" : "PALeft",
                                   1,
                                   panelDrawingPath.Weight
                            );
                        }
                        if (smallPanelWidthforD > 0)
                        {
                            paintBooth.PanelWidth = smallPanelWidthforD;

                            PanelDrawResult smallPanelDrawingPath = paintBooth.PanelsInPaintBooth(noOfPanelsforD + 1, model, i);
                            documents.Add(smallPanelDrawingPath.drawing);
                            filePaths.Add(smallPanelDrawingPath.lstpath);
                            paintBooth.SavePanelDetails(
                            model,
                            i == 0 ? "PARight" : "PALeft",
                            1,
                             smallPanelDrawingPath.Weight
                            );

                            paintBooth.PanelWidth = model.PanelWidth;
                        }

                    }
                    if (smallPanelWidthforH > 0)
                    {
                        paintBooth.PanelHeight = smallPanelWidthforH;

                        for (int j = 0; j < noOfPanelsforD; j++)
                        {
                            PanelDrawResult panelDrawingPath = paintBooth.PanelsInPaintBooth(j + 1, model, i);
                            documents.Add(panelDrawingPath.drawing);
                            filePaths.Add(panelDrawingPath.lstpath);
                            paintBooth.SavePanelDetails(
                            model,
                            i == 0 ? "PARight" : "PALeft",
                            1,
                             panelDrawingPath.Weight
                            );
                        }
                        if (smallPanelWidthforD > 0)
                        {
                            paintBooth.PanelWidth = smallPanelWidthforD;

                            PanelDrawResult smallPanelDrawingPath = paintBooth.PanelsInPaintBooth(noOfPanelsforD + 1, model, i);
                            documents.Add(smallPanelDrawingPath.drawing);
                            filePaths.Add(smallPanelDrawingPath.lstpath);
                            paintBooth.SavePanelDetails(
                            model,
                            i == 0 ? "PARight" : "PALeft",
                            1,
                             smallPanelDrawingPath.Weight
                            );
                            paintBooth.PanelWidth = model.PanelWidth;

                        }
                        paintBooth.PanelHeight = model.PanelHeight;
                    }
                    if (i == 0)
                    {
                        rightSideDrawings.AddRange(documents);

                    }
                    else if (i == 1)
                    {
                        leftSideDrawings.AddRange(documents);
                    }
                }
                // Replace the nested loop in LeftRightPanels with batch saves:


                #region RightSide And LeftSide Assembly               
                DesignDocument rightSideAssembly = RightSideWallAssembly(rightSideDrawings, 0, model.PanelHeight);
                string rightSideDwgFilePath = BaseFilePath + "/PARight.dwg";
                WriteAutodeskParams autoRight = new WriteAutodeskParams(rightSideAssembly);
                WriteAutodesk dwgWriterRight = new WriteAutodesk(autoRight, rightSideDwgFilePath);
                dwgWriterRight.DoWork();

                //pdf file
                string rightSidepdfFilePath = BaseFilePath + "/PARight.pdf";
                Write3DPdfParams pdf = new Write3DPdfParams(rightSideAssembly);
                Write3DPDF pdf1 = new Write3DPDF(pdf, rightSidepdfFilePath);
                pdf1.DoWork();


                DesignDocument leftSideAssembly = LeftSideWallAssembly(leftSideDrawings, 0);
                string leftSideDwgFilePath = BaseFilePath + "/PALeft.dwg";
                WriteAutodeskParams autoLeft = new WriteAutodeskParams(leftSideAssembly);
                WriteAutodesk dwgWriterLeft = new WriteAutodesk(autoLeft, leftSideDwgFilePath);
                dwgWriterLeft.DoWork();
                string leftSidepdfFilePath = BaseFilePath + "/PALeft.pdf";

                Write3DPdfParams pdfauto = new Write3DPdfParams(leftSideAssembly);
                Write3DPDF pdf12 = new Write3DPDF(pdfauto, leftSidepdfFilePath);
                pdf12.DoWork();

                designdictionary = new();
                designdictionary.Add("rightSideDrawings", rightSideDrawings);
                designdictionary.Add("leftSideDrawings", leftSideDrawings);
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                // _uow.exceptionHandlerRepository.SaveException("PaintBoothService", "LeftRightPanels", ex.Message);
                throw;
            }

        }
        public void LeftRightPanelsForSideDoor(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, DoorDimensionsModel doorModel, string PaintBoothTypefromEnquiry)
        {
            try
            {
                List<DesignDocument> rightSideDoorDrawings = new List<DesignDocument>();
                List<DesignDocument> leftSideDoorDrawings = new List<DesignDocument>();
                List<DesignDocument> rightSideDoorDrawings2 = new List<DesignDocument>(); // ✅ panels after door
                List<DesignDocument> leftSideDoorDrawings2 = new List<DesignDocument>();  // ✅ panels after door
                List<DesignDocument> rightSideDoorNearExtractionCDrawings = new List<DesignDocument>();
                List<DesignDocument> leftSideDoorNearExtractionCDrawings = new List<DesignDocument>();
                List<DesignDocument> ComponantEntrySideDoor = new List<DesignDocument>();
                List<DesignDocument> panelsAboveEntryDoors = new List<DesignDocument>();

                #region H Calculate Number of Panels In H
                model.PanelHeight = paintBooth.PanelHeight;
                totalPanelsforH = model.H / model.PanelHeight;
                noOfPanelsforH = (int)Math.Floor(totalPanelsforH);
                smallPanelWidthforH = model.H - (noOfPanelsforH * model.PanelHeight);

                int totalNoOfPanels = (int)(Math.Ceiling(totalPanelsforH)) * 2;
                #endregion

                if (doorModel.sideDoorLocation != "")
                {
                    RemainingSpanceinD = (Math.Floor((model.D - (model.Width + 500))) / 2);    //model.Width means component width
                    if (RemainingSpanceinD > model.PanelWidth)
                    {
                        noOfPanelsInSideDoor = Math.Floor(RemainingSpanceinD / model.PanelWidth);
                        smallPanelsWidthDoors = RemainingSpanceinD - (noOfPanelsInSideDoor * model.PanelWidth);
                        paintBooth.PanelWidthForSideDoors = model.PanelWidth;
                    }
                    else
                    {
                        noOfPanelsInSideDoor = 1;
                        smallPanelsWidthDoors = 0;
                        paintBooth.PanelWidthForSideDoors = doorModel.RemainingSpanceinDepth = RemainingSpanceinD;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (i == 0)
                        {
                            doorModel.yOffeset = paintBooth.PanelLengthForSideDoors = 0;
                            doorModel.Side = DoorSide.Right;

                        }
                        else
                        {
                            doorModel.yOffeset = paintBooth.PanelLengthForSideDoors = model.W;
                            doorModel.Side = DoorSide.Left;
                        }
                        List<DesignDocument> documents = new List<DesignDocument>();
                        List<DesignDocument> SideDoorNearExtractionC = new List<DesignDocument>();
                        List<DesignDocument> CoveringPanels = new List<DesignDocument>();
                        for (int k = 0; k < noOfPanelsforH; k++)
                        {
                            for (int p = 0; p < noOfPanelsInSideDoor; p++)
                            {

                                paintBooth.PanelHeightForSideDoors = model.PanelHeight;
                                PanelDrawResult DoorPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                                documents.Add(DoorPath.drawing);
                                filePaths.Add(DoorPath.lstpath);
                                SideDoorNearExtractionC.Add(DoorPath.drawing);

                                paintBooth.SavePanelDetails(
                                   model,
                                   i == 0 ? "PARight_Panel_Doors" : "PALeft_Panel_Doors",
                                   1,
                                   DoorPath.Weight
                                );
                            }
                            if (smallPanelsWidthDoors > 0)
                            {
                                paintBooth.PanelWidthForSideDoors = smallPanelsWidthDoors;

                                PanelDrawResult smallDoorPanelDrawingPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                                documents.Add(smallDoorPanelDrawingPath.drawing);
                                filePaths.Add(smallDoorPanelDrawingPath.lstpath);
                                SideDoorNearExtractionC.Add(smallDoorPanelDrawingPath.drawing);
                                paintBooth.SavePanelDetails(
                                   model,
                                   i == 0 ? "PARight_Panel_Doors" : "PALeft_Panel_Doors",
                                   1,
                                   smallDoorPanelDrawingPath.Weight
                                );
                                paintBooth.PanelWidthForSideDoors = model.PanelWidth;
                            }
                        }
                        if (smallPanelWidthforH > 0)
                        {
                            paintBooth.PanelHeightForSideDoors = smallPanelWidthforH;

                            for (int p = 0; p < noOfPanelsInSideDoor; p++)
                            {
                                PanelDrawResult DoorPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                                documents.Add(DoorPath.drawing);
                                filePaths.Add(DoorPath.lstpath);
                                SideDoorNearExtractionC.Add(DoorPath.drawing);
                                paintBooth.SavePanelDetails(
                                  model,
                                  i == 0 ? "PARight_Panel_Doors" : "PALeft_Panel_Doors",
                                  1,
                                  DoorPath.Weight
                               );
                            }
                            if (smallPanelsWidthDoors > 0)
                            {
                                paintBooth.PanelWidthForSideDoors = smallPanelsWidthDoors;

                                PanelDrawResult smallDoorPanelDrawingPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                                documents.Add(smallDoorPanelDrawingPath.drawing);
                                filePaths.Add(smallDoorPanelDrawingPath.lstpath);
                                SideDoorNearExtractionC.Add(smallDoorPanelDrawingPath.drawing);
                                paintBooth.SavePanelDetails(
                                  model,
                                  i == 0 ? "PARight_Panel_Doors" : "PALeft_Panel_Doors",
                                  1,
                                  smallDoorPanelDrawingPath.Weight
                               );
                                paintBooth.PanelWidthForSideDoors = model.PanelWidth;
                            }

                            paintBooth.PanelHeightForSideDoors = model.PanelHeight;
                        }

                        doorModel.xOffeset = RemainingSpanceinD;
                        PaintBoothclass entryDoor = paintBooth.ComponantryEntrySideDoor(doorModel, model, i);
                        ComponantEntrySideDoor.Add(entryDoor.drawing);
                        filePaths.Add(entryDoor.lstpath);
                        #region Old code
                        //// After the existing panels loop and door drawing, before #endregion
                        //// ✅ Panels AFTER the component entry door (mirrored second span)

                        //double xStartAfterDoor = RemainingSpanceinD + model.Width + 500;
                        //paintBooth.PanelWidthForSideDoors = model.PanelWidth;  // reset

                        //for (int k = 0; k < noOfPanelsforH; k++)
                        //{
                        //    for (int p = 0; p < noOfPanelsInSideDoor; p++)
                        //    {
                        //        paintBooth.PanelHeightForSideDoors = model.PanelHeight;

                        //        // ✅ Override xOffset to start after the door
                        //        // DoorsOnBothSide uses PanelLengthForSideDoors for Y, 
                        //        // and xOffset is applied during assembly — so we use a 
                        //        // separate list and pass xStartAfterDoor as xoffset in CombineAssemblies2

                        //        PanelDrawResult DoorPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);

                        //        if (i == 0)
                        //            rightSideDoorDrawings2.Add(DoorPath.drawing);
                        //        else
                        //            leftSideDoorDrawings2.Add(DoorPath.drawing);

                        //        filePaths.Add(DoorPath.lstpath);
                        //        paintBooth.SavePanelDetails(
                        //            model,
                        //            i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2",
                        //            1,
                        //            DoorPath.Weight
                        //        );
                        //    }
                        //    if (smallPanelsWidthDoors > 0)
                        //    {
                        //        paintBooth.PanelWidthForSideDoors = smallPanelsWidthDoors;
                        //        PanelDrawResult smallPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);

                        //        if (i == 0)
                        //            rightSideDoorDrawings2.Add(smallPath.drawing);
                        //        else
                        //            leftSideDoorDrawings2.Add(smallPath.drawing);

                        //        filePaths.Add(smallPath.lstpath);
                        //        paintBooth.SavePanelDetails(
                        //            model,
                        //            i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2",
                        //            1,
                        //            smallPath.Weight
                        //        );
                        //        paintBooth.PanelWidthForSideDoors = model.PanelWidth;
                        //    }
                        //}

                        //if (smallPanelWidthforH > 0)
                        //{
                        //    paintBooth.PanelHeightForSideDoors = smallPanelWidthforH;
                        //    for (int p = 0; p < noOfPanelsInSideDoor; p++)
                        //    {
                        //        PanelDrawResult DoorPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                        //        if (i == 0) rightSideDoorDrawings2.Add(DoorPath.drawing);
                        //        else leftSideDoorDrawings2.Add(DoorPath.drawing);
                        //        filePaths.Add(DoorPath.lstpath);
                        //        paintBooth.SavePanelDetails(model,
                        //            i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2", 1, DoorPath.Weight);
                        //    }
                        //    if (smallPanelsWidthDoors > 0)
                        //    {
                        //        paintBooth.PanelWidthForSideDoors = smallPanelsWidthDoors;
                        //        PanelDrawResult smallPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                        //        if (i == 0) rightSideDoorDrawings2.Add(smallPath.drawing);
                        //        else leftSideDoorDrawings2.Add(smallPath.drawing);
                        //        filePaths.Add(smallPath.lstpath);
                        //        paintBooth.SavePanelDetails(model,
                        //            i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2", 1, smallPath.Weight);
                        //        paintBooth.PanelWidthForSideDoors = model.PanelWidth;
                        //    }
                        //    paintBooth.PanelHeightForSideDoors = model.PanelHeight;
                        //}

                        #endregion
                        // ✅ After door — small panel FIRST (mirror of before-door)
                        for (int k = 0; k < noOfPanelsforH; k++)
                        {
                            // ✅ Small panel FIRST if exists
                            if (smallPanelsWidthDoors > 0)
                            {
                                paintBooth.PanelWidthForSideDoors = smallPanelsWidthDoors;
                                PanelDrawResult smallPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);

                                if (i == 0) rightSideDoorDrawings2.Add(smallPath.drawing);
                                else leftSideDoorDrawings2.Add(smallPath.drawing);

                                filePaths.Add(smallPath.lstpath);
                                paintBooth.SavePanelDetails(model,
                                    i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2", 1, smallPath.Weight);

                                paintBooth.PanelWidthForSideDoors = model.PanelWidth; // ✅ reset before full panels
                            }

                            // ✅ Full panels AFTER small
                            paintBooth.PanelHeightForSideDoors = model.PanelHeight;
                            for (int p = 0; p < noOfPanelsInSideDoor; p++)
                            {
                                PanelDrawResult DoorPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);

                                if (i == 0) rightSideDoorDrawings2.Add(DoorPath.drawing);
                                else leftSideDoorDrawings2.Add(DoorPath.drawing);

                                filePaths.Add(DoorPath.lstpath);
                                paintBooth.SavePanelDetails(model,
                                    i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2", 1, DoorPath.Weight);
                            }
                        }

                        // ✅ Small height row last
                        if (smallPanelWidthforH > 0)
                        {
                            paintBooth.PanelHeightForSideDoors = smallPanelWidthforH;

                            // small width first
                            if (smallPanelsWidthDoors > 0)
                            {
                                paintBooth.PanelWidthForSideDoors = smallPanelsWidthDoors;
                                PanelDrawResult smallPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                                if (i == 0) rightSideDoorDrawings2.Add(smallPath.drawing);
                                else leftSideDoorDrawings2.Add(smallPath.drawing);
                                filePaths.Add(smallPath.lstpath);
                                paintBooth.SavePanelDetails(model,
                                    i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2", 1, smallPath.Weight);
                                paintBooth.PanelWidthForSideDoors = model.PanelWidth;
                            }

                            // full panels after
                            for (int p = 0; p < noOfPanelsInSideDoor; p++)
                            {
                                PanelDrawResult DoorPath = paintBooth.DoorsOnBothSide(model, i, PaintBoothTypefromEnquiry);
                                if (i == 0) rightSideDoorDrawings2.Add(DoorPath.drawing);
                                else leftSideDoorDrawings2.Add(DoorPath.drawing);
                                filePaths.Add(DoorPath.lstpath);
                                paintBooth.SavePanelDetails(model,
                                    i == 0 ? "PARight_Panel_Doors2" : "PALeft_Panel_Doors2", 1, DoorPath.Weight);
                            }

                            paintBooth.PanelHeightForSideDoors = model.PanelHeight;
                        }

                        //---------------

                        #region Panels above Component entry doors
                        totalPanelsAboveCompEntryDoor = doorModel.doorWidth / model.PanelWidth;
                        noOfPanelsAboveCompEntryDoor = Math.Floor(totalPanelsAboveCompEntryDoor);
                        double smallPanelsWidthCompEntryDoor = doorModel.doorWidth - (noOfPanelsAboveCompEntryDoor * model.PanelWidth);

                        doorModel.panelHeightforAboveCompEntryPanels = model.H - doorModel.doorHeight;

                        totalPanelsAboveSideCompoDoor = doorModel.panelHeightforAboveCompEntryPanels / model.PanelHeight;
                        noOfPanelsAboveSideCompoDoor = Math.Floor(totalPanelsAboveSideCompoDoor);
                        smallPanelWidthAboveSideCompoDoor = doorModel.panelHeightforAboveCompEntryPanels
                                                           - (noOfPanelsAboveSideCompoDoor * model.PanelHeight);

                        doorModel.panelWidthforAboveCompEntryPanels = model.PanelWidth;
                        doorModel.panelZOffsetAboveDoor = 0;  // ✅ reset Z accumulator before starting

                        for (int n = 0; n < noOfPanelsAboveSideCompoDoor; n++)
                        {
                            doorModel.PHforAboveCompEntryPanels = model.PanelHeight;
                            doorModel.xOffeset = RemainingSpanceinD;  // ✅ reset X each row

                            for (int m = 0; m < noOfPanelsAboveCompEntryDoor; m++)
                            {
                                PanelDrawResult panelAboveEntryDoor = paintBooth.PanelsAboveEntryDoor(i, doorModel);
                                doorModel.xOffeset += model.PanelWidth;
                                filePaths.Add(panelAboveEntryDoor.lstpath);
                                CoveringPanels.Add(panelAboveEntryDoor.drawing);
                                paintBooth.SavePanelDetails(
                                    model,
                                    i == 0 ? "PARight_Panel_Above_CE_Doors" : "PALeft_Panel_Above_CE_Doors",
                                    1,
                                    panelAboveEntryDoor.Weight);
                            }
                            if (smallPanelsWidthCompEntryDoor > 0)
                            {
                                doorModel.panelWidthforAboveCompEntryPanels = smallPanelsWidthCompEntryDoor;
                                PanelDrawResult smallPanelAboveEntryDoor = paintBooth.PanelsAboveEntryDoor(i, doorModel);
                                filePaths.Add(smallPanelAboveEntryDoor.lstpath);
                                CoveringPanels.Add(smallPanelAboveEntryDoor.drawing);
                                paintBooth.SavePanelDetails(
                                    model,
                                    i == 0 ? "PARight_Panel_Above_CE_Doors" : "PALeft_Panel_Above_CE_Doors",
                                    1,
                                    smallPanelAboveEntryDoor.Weight);
                                doorModel.panelWidthforAboveCompEntryPanels = model.PanelWidth;  // ✅ reset after small
                            }

                            doorModel.panelZOffsetAboveDoor += model.PanelHeight;  // ✅ Z steps up after each full row
                        }

                        if (smallPanelWidthAboveSideCompoDoor > 0)
                        {
                            doorModel.PHforAboveCompEntryPanels = smallPanelWidthAboveSideCompoDoor;
                            doorModel.xOffeset = RemainingSpanceinD;  // ✅ reset X for last row

                            for (int m = 0; m < noOfPanelsAboveCompEntryDoor; m++)
                            {
                                PanelDrawResult panelAboveEntryDoor = paintBooth.PanelsAboveEntryDoor(i, doorModel);
                                doorModel.xOffeset += model.PanelWidth;
                                filePaths.Add(panelAboveEntryDoor.lstpath);
                                CoveringPanels.Add(panelAboveEntryDoor.drawing);
                                paintBooth.SavePanelDetails(
                                    model,
                                    i == 0 ? "PARight_Panel_Above_CE_Doors" : "PALeft_Panel_Above_CE_Doors",
                                    1,
                                    panelAboveEntryDoor.Weight);
                            }
                            if (smallPanelsWidthCompEntryDoor > 0)
                            {
                                doorModel.panelWidthforAboveCompEntryPanels = smallPanelsWidthCompEntryDoor;
                                PanelDrawResult smallPanelAboveEntryDoor = paintBooth.PanelsAboveEntryDoor(i, doorModel);
                                filePaths.Add(smallPanelAboveEntryDoor.lstpath);
                                CoveringPanels.Add(smallPanelAboveEntryDoor.drawing);
                                paintBooth.SavePanelDetails(
                                    model,
                                    i == 0 ? "PARight_Panel_Above_CE_Doors" : "PALeft_Panel_Above_CE_Doors",
                                    1,
                                    smallPanelAboveEntryDoor.Weight);
                                doorModel.panelWidthforAboveCompEntryPanels = model.PanelWidth;  // ✅ reset after small
                            }
                            doorModel.PHforAboveCompEntryPanels = model.PanelHeight;
                            // no Z increment needed — last row



                        }
                        if (i == 0)
                        {
                            rightSideDoorDrawings.AddRange(documents);
                            rightSideDoorNearExtractionCDrawings.AddRange(SideDoorNearExtractionC);
                            panelsAboveEntryDoors.AddRange(CoveringPanels);
                        }
                        else if (i == 1)
                        {
                            leftSideDoorDrawings.AddRange(documents);
                            leftSideDoorNearExtractionCDrawings.AddRange(SideDoorNearExtractionC);
                            panelsAboveEntryDoors.AddRange(CoveringPanels);
                        }
                        #endregion


                    }
                    designdictionary.Add("rightSideDoorDrawings", rightSideDoorDrawings);
                    designdictionary.Add("leftSideDoorDrawings", leftSideDoorDrawings);
                    // After existing designdictionary.Add calls:
                    designdictionary.Add("rightSideDoorDrawings2", rightSideDoorDrawings2);
                    designdictionary.Add("leftSideDoorDrawings2", leftSideDoorDrawings2);
                    ////designdictionary.Add("rightSideDoorNearExtractionCDrawings", rightSideDoorNearExtractionCDrawings);
                    ////designdictionary.Add("leftSideDoorNearExtractionCDrawings", leftSideDoorNearExtractionCDrawings);
                    designdictionary.Add("ComponantEntrySideDoor", ComponantEntrySideDoor);
                    designdictionary.Add("panelsAboveEntryDoors", panelsAboveEntryDoors);
                }
            }
            catch (Exception ex)
            {
                // _uow.exceptionHandlerRepository.SaveException("PaintBoothService", "LeftRightPanelsForSideDoor", ex.Message);

                throw;
            }
        }


        #endregion
        #region Top Panels
        public void TopPanels(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, string paintboothType)
        {
            try
            {
                #region  D Calculate Number of panels in D
                model.PanelWidth = paintBooth.PanelWidth;
                totalPanelsforD = model.D / model.PanelWidth;
                noOfPanelsforD = (int)Math.Floor(totalPanelsforD);
                smallPanelWidthforD = model.D - (noOfPanelsforD * model.PanelWidth);
                #endregion
                #region W Calculate Number of panels In W
                model.PanelHeight = paintBooth.PanelHeight;
                totalPanelsforW = model.W / paintBooth.PanelHeight;
                noOfPanelsforW = (int)Math.Floor(totalPanelsforW);
                smallPanelWidthforW = model.W - (noOfPanelsforW * paintBooth.PanelHeight);
                #endregion
                #region W*D Top 
                List<DesignDocument> docdrawing = new List<DesignDocument>();
                List<string> Topsidepath = new List<string>();
                for (int i = 0; i < noOfPanelsforW; i++)
                {
                    for (int k = 0; k < noOfPanelsforD; k++)
                    {
                        paintBooth.PanelLengthForTopPanels = model.PanelHeight;
                        PanelDrawResult Topside = paintBooth.TopSidePanels(k + 1, model, i, paintboothType);
                        docdrawing.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATop", 1, Topside.Weight);


                    }
                    if (smallPanelWidthforD > 0)
                    {
                        paintBooth.PanelWidth = smallPanelWidthforD;
                        PanelDrawResult Topside = paintBooth.TopSidePanels(noOfPanelsforW + 1, model, i, paintboothType);
                        docdrawing.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATop", 1, Topside.Weight);

                        paintBooth.PanelWidth = model.PanelWidth;
                    }
                }
                if (smallPanelWidthforW > 0)
                {
                    //paintBooth.PanelLength = smallPanelWidthforW;
                    paintBooth.PanelLengthForTopPanels = smallPanelWidthforW;
                    for (int k = 0; k < noOfPanelsforD; k++)
                    {
                        PanelDrawResult Topside = paintBooth.TopSidePanels(k + 1, model, k, paintboothType);
                        docdrawing.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATop", 1, Topside.Weight);

                    }
                    if (smallPanelWidthforD > 0)
                    {
                        paintBooth.PanelWidth = smallPanelWidthforD;
                        PanelDrawResult Topside = paintBooth.TopSidePanels(noOfPanelsforW + 1, model, 0, paintboothType);
                        docdrawing.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATop", 1, Topside.Weight);

                        paintBooth.PanelWidth = model.PanelWidth;
                    }

                }
                TopAssembly.AddRange(docdrawing);

                DesignDocument TopDrawing = TopWallAssembly(TopAssembly, 0);
                string TopDwgFilePath = BaseFilePath + "/PATop.dwg";
                WriteAutodeskParams Topdwg = new WriteAutodeskParams(TopDrawing);
                WriteAutodesk dwgWriterTop = new WriteAutodesk(Topdwg, TopDwgFilePath);
                dwgWriterTop.DoWork();
                string ToppdfFilePath = BaseFilePath + "/PATop.pdf";

                Write3DPdfParams pdfauto = new Write3DPdfParams(TopDrawing);
                Write3DPDF pdf12 = new Write3DPDF(pdfauto, ToppdfFilePath);
                pdf12.DoWork();
                designdictionary.Add("TopAssembly", TopAssembly);

                #endregion
            }
            catch (Exception ex)
            {
                //_uow.exceptionHandlerRepository.SaveException("PaintBoothService", "TopPanels", ex.Message);
                throw;
            }
        }
        
        public void TopFilterFrames(PaintBoothModel model, PaintBoothDesignBusiness paintBooth)
        {
            try
            {
                //  One call — all filters in one drawing
                PanelDrawResult filterResult = paintBooth.TopFilterFramesDrawing(model);

                List<DesignDocument> docdrawing = new List<DesignDocument> { filterResult.drawing };

                paintBooth.SavePanelDetails(model, "FilterTop", 1, filterResult.Weight);

                // Export assembly
                DesignDocument filterAssembly = TopWallAssembly(docdrawing, 0);
                string dwgPath = BaseFilePath + "/FilterTop.dwg";
                string pdfPath = BaseFilePath + "/FilterTop.pdf";
                new WriteAutodesk(new WriteAutodeskParams(filterAssembly), dwgPath).DoWork();
                new Write3DPDF(new Write3DPdfParams(filterAssembly), pdfPath).DoWork();

                designdictionary.Add("FilterAssembly", docdrawing);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region Front Panels
        public void FrontPanels(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, string paintboothType)
        {
            try
            {
                paintBooth.BackPanelLength = model.PanelWidth;
                TotalBackPanels = model.W / model.PanelWidth;
                noOfPanelsForBackSide = (int)Math.Floor(TotalBackPanels);
                smallPanelWidthForBackSide = model.W - (noOfPanelsForBackSide * model.PanelWidth);
                paintBooth.PanelHeight = model.PanelHeight;


                model.PanelHeight = paintBooth.PanelHeight;
                totalPanelsforH = model.H / model.PanelHeight;
                noOfPanelsforH = (int)Math.Floor(totalPanelsforH);
                smallPanelWidthforH = model.H - (noOfPanelsforH * model.PanelHeight);
                List<DesignDocument> frontDrawingList = new List<DesignDocument>();
                //List<string> frontDrawingPath = new List<string>();
                for (int k = 0; k < noOfPanelsforH; k++)
                {
                    for (int j = 0; j < noOfPanelsForBackSide; j++)
                    {
                        paintBooth.FrontPanelLength = model.PanelWidth;
                        paintBooth.FrontPanelHeight = model.PanelHeight;
                        PanelDrawResult frontPanelsPath = paintBooth.FrontPanels(model);
                        frontDrawingList.Add(frontPanelsPath.drawing);
                        filePaths.Add(frontPanelsPath.lstpath);
                        paintBooth.SavePanelDetails(model, "PAFront", 1, frontPanelsPath.Weight);

                    }
                    if (smallPanelWidthForBackSide > 0)
                    {
                        paintBooth.FrontPanelLength = smallPanelWidthForBackSide;

                        PanelDrawResult smallFrontPanelDrawingPath = paintBooth.FrontPanels(model);
                        frontDrawingList.Add(smallFrontPanelDrawingPath.drawing);
                        filePaths.Add(smallFrontPanelDrawingPath.lstpath);
                        paintBooth.SavePanelDetails(model, "PAFront", 1, smallFrontPanelDrawingPath.Weight);
                        paintBooth.FrontPanelLength = model.PanelWidth;
                    }
                }
                if (smallPanelWidthforH > 0)
                {
                    paintBooth.FrontPanelHeight = smallPanelWidthforH;

                    for (int j = 0; j < noOfPanelsForBackSide; j++)
                    {
                        PanelDrawResult frontPanelsPath = paintBooth.FrontPanels(model);
                        frontDrawingList.Add(frontPanelsPath.drawing);
                        filePaths.Add(frontPanelsPath.lstpath);
                        paintBooth.SavePanelDetails(model, "PAFront", 1, frontPanelsPath.Weight);
                    }
                    if (smallPanelWidthForBackSide > 0)
                    {
                        paintBooth.FrontPanelLength = smallPanelWidthForBackSide;

                        PanelDrawResult smallFrontPanelDrawingPath = paintBooth.FrontPanels(model);
                        frontDrawingList.Add(smallFrontPanelDrawingPath.drawing);
                        filePaths.Add(smallFrontPanelDrawingPath.lstpath);
                        paintBooth.SavePanelDetails(model, "PAFront", 1, smallFrontPanelDrawingPath.Weight);
                        paintBooth.FrontPanelLength = model.PanelWidth;
                    }
                }
                designdictionary.Add("frontDrawingList", frontDrawingList);


                // Mirror to front EC for types 8 & 9
                if (paintboothType == "4" || paintboothType == "5" || paintboothType == "6" || paintboothType == "7")
                {
                    double translateX = model.D;

                    List<DesignDocument> BackPanelsDownDraft = new List<DesignDocument>();

                    foreach (DesignDocument doc in frontDrawingList)
                        BackPanelsDownDraft.Add(MirrorAndTranslate(doc, translateX, 0));

                    designdictionary.Add("BackPanelsDownDraft", BackPanelsDownDraft);

                }
            }
            catch (Exception ex)
            {
                //_uow.exceptionHandlerRepository.SaveException("PaintBoothService", "FrontPanels", ex.Message);

                throw;
            }
        }
        public double FrontDoorsType(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, DoorDimensionsModel doorModel)
        {
            List<DesignDocument> frontDoorsDrawingList = new List<DesignDocument>();
            List<DesignDocument> ComponantEntryFrontDoor = new List<DesignDocument>();

            double SameSpaceBetweenDoors = 0;
            List<DesignDocument> panelsAboveFrontEntryDoors = new List<DesignDocument>();

            #region H Calculate Number of Panels In H
            model.PanelHeight = paintBooth.PanelHeight;
            totalPanelsforH = model.H / model.PanelHeight;
            noOfPanelsforH = (int)Math.Floor(totalPanelsforH);
            smallPanelWidthforH = model.H - (noOfPanelsforH * model.PanelHeight);

            int totalNoOfPanels = (int)(Math.Ceiling(totalPanelsforH)) * 2;
            #endregion
            //double smallPanelsWidthDoors = 0;
            SameSpaceBetweenDoors = ((model.W1 + model.W2) - 500) / 2;
            doorModel.yOffsetForFrontDoors = SameSpaceBetweenDoors;
            if (SameSpaceBetweenDoors > model.PanelWidth)
            {
                noOfPanelsInFrontDoor = Math.Floor(SameSpaceBetweenDoors / model.PanelWidth);
                smallPanelsWidthFrontDoors = SameSpaceBetweenDoors - (noOfPanelsInFrontDoor * model.PanelWidth);
                paintBooth.PanelWidthForFrontDoors = model.PanelWidth;
            }
            else
            {
                noOfPanelsInFrontDoor = 2;
                smallPanelsWidthFrontDoors = 0;
                paintBooth.PanelWidthForFrontDoors = SameSpaceBetweenDoors;
            }

            List<DesignDocument> CoveringPanels = new List<DesignDocument>();

            //for (int m = 0; m < 2; m++)
            //{
            for (int k = 0; k < noOfPanelsforH; k++)
            {
                for (int p = 0; p < noOfPanelsInFrontDoor; p++)
                {

                    paintBooth.PanelHeightForFrontDoors = model.PanelHeight;
                    PanelDrawResult DoorPath = paintBooth.FrontPanelsWithDoors(SameSpaceBetweenDoors, model);
                    frontDoorsDrawingList.Add(DoorPath.drawing);
                    filePaths.Add(DoorPath.lstpath);

                    paintBooth.SavePanelDetails(
                       model,
                       "PAFront_Doors",
                       2,
                       DoorPath.Weight
                    );
                }
                if (smallPanelsWidthFrontDoors > 0)
                {
                    paintBooth.PanelWidthForFrontDoors = smallPanelsWidthFrontDoors;

                    PanelDrawResult smallDoorPanelDrawingPath = paintBooth.FrontPanelsWithDoors(SameSpaceBetweenDoors, model);
                    frontDoorsDrawingList.Add(smallDoorPanelDrawingPath.drawing);
                    filePaths.Add(smallDoorPanelDrawingPath.lstpath);

                    paintBooth.SavePanelDetails(
                       model,
                        "PAFront_Doors",
                       2,
                       smallDoorPanelDrawingPath.Weight
                    );
                    paintBooth.PanelWidthForFrontDoors = model.PanelWidth;
                }
            }
            if (smallPanelWidthforH > 0)
            {
                paintBooth.PanelHeightForFrontDoors = smallPanelWidthforH;

                for (int p = 0; p < noOfPanelsInFrontDoor; p++)
                {
                    PanelDrawResult DoorPath = paintBooth.FrontPanelsWithDoors(SameSpaceBetweenDoors, model);
                    frontDoorsDrawingList.Add(DoorPath.drawing);
                    filePaths.Add(DoorPath.lstpath);
                    paintBooth.SavePanelDetails(
                      model,    
                        "PAFront_Doors",
                      2,
                      DoorPath.Weight
                   );
                }
                if (smallPanelsWidthFrontDoors > 0)
                {
                    paintBooth.PanelWidthForFrontDoors = smallPanelsWidthFrontDoors;

                    PanelDrawResult smallDoorPanelDrawingPath = paintBooth.FrontPanelsWithDoors(SameSpaceBetweenDoors, model);
                    frontDoorsDrawingList.Add(smallDoorPanelDrawingPath.drawing);
                    filePaths.Add(smallDoorPanelDrawingPath.lstpath);
                    paintBooth.SavePanelDetails(
                      model,
                      "PAFront_Doors",
                      1,
                      smallDoorPanelDrawingPath.Weight
                   );
                    paintBooth.PanelWidthForFrontDoors = model.PanelWidth;
                }

                paintBooth.PanelHeightForFrontDoors = model.PanelHeight;
            }
            designdictionary.Add("frontDoorsDrawingList", frontDoorsDrawingList);
            doorModel.xOffesetForFrontDoor = SameSpaceBetweenDoors;
            PaintBoothclass entryDoor = paintBooth.ComponantryEntryFrontDoor(doorModel, model);
            ComponantEntryFrontDoor.Add(entryDoor.drawing);
            filePaths.Add(entryDoor.lstpath);
            designdictionary.Add("ComponantEntryFrontDoor", ComponantEntryFrontDoor);

            #region Panels above Componant entry doors
            //calculations for no of panels above componant entry doors
            totalPanelsAboveCompEntryDoor = doorModel.doorWidth / model.PanelWidth;
            noOfPanelsAboveCompEntryDoor = Math.Floor(totalPanelsAboveCompEntryDoor);
            double smallPanelsWidthCompEntryDoor = doorModel.doorWidth - (noOfPanelsAboveCompEntryDoor * model.PanelWidth);

            doorModel.panelHeightforAboveCompEntryPanels = model.H - doorModel.doorHeight;
            doorModel.panelWidthforAboveCompEntryPanels = model.PanelWidth;
            for (int m = 0; m < noOfPanelsAboveCompEntryDoor; m++)
            {

                PanelDrawResult panelAboveEntryDoor = paintBooth.PanelsAboveFrontEntryDoor(doorModel);
                doorModel.xOffeset += model.PanelWidth;
                filePaths.Add(panelAboveEntryDoor.lstpath);
                panelsAboveFrontEntryDoors.Add(panelAboveEntryDoor.drawing);
                paintBooth.SavePanelDetails(
                           model,
                           "PAFront_Above_Comp_Entry_Doors",
                           1,
                           panelAboveEntryDoor.Weight
                );
            }
            if (smallPanelsWidthCompEntryDoor > 0)
            {
                doorModel.panelWidthforAboveCompEntryPanels = smallPanelsWidthCompEntryDoor;
                PanelDrawResult smallPanelAboveEntryDoor = paintBooth.PanelsAboveFrontEntryDoor(doorModel);
                filePaths.Add(smallPanelAboveEntryDoor.lstpath);
                panelsAboveFrontEntryDoors.Add(smallPanelAboveEntryDoor.drawing);
                paintBooth.SavePanelDetails(
                           model,
                           "PAFront_Above_Comp_Entry_Doors",
                           1,
                           smallPanelAboveEntryDoor.Weight
                );
            }
            #endregion
            designdictionary.Add("panelsAboveFrontEntryDoors", panelsAboveFrontEntryDoors);
            return SameSpaceBetweenDoors;


        }
        #endregion
        #region D3 Panels


        public void D3Panels(PaintBoothModel model, PaintBoothDesignBusiness paintBooth,
                     double ExtractionC_Height, string paintboothType)
        {
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    // ✅ Fresh list each iteration
                    List<DesignDocument> documents = new List<DesignDocument>();
                    List<DesignDocument> D3beforeD = new List<DesignDocument>();

                    PanelDrawResult panelDrawingPathD3 = paintBooth.D3Panels(i, model, ExtractionC_Height);
                    documents.Add(panelDrawingPathD3.drawing);
                    filePaths.Add(panelDrawingPathD3.lstpath);
                    paintBooth.SavePanelDetails(
                        model,
                        i == 0 ? "ECRight_Panels" : "ECLeft_Panels",
                        1,
                        panelDrawingPathD3.Weight
                    );

                    if (paintboothType == "8" || paintboothType == "9")
                    {
                        PanelDrawResult panelsBeforeD = paintBooth.D3PanelsBeforeD(i, model, ExtractionC_Height);
                        D3beforeD.Add(panelsBeforeD.drawing);
                        filePaths.Add(panelsBeforeD.lstpath);
                        paintBooth.SavePanelDetails(
                            model,
                            i == 0 ? "ECRight_PanelsBeforeD" : "ECLeft_PanelsBeforeD",
                            1,
                            panelsBeforeD.Weight
                        );
                    }

                    if (i == 0)
                    {
                        D3PanelRightDrawings.AddRange(documents);
                        D3PanelRightDrawingsBeforeD.AddRange(D3beforeD);
                    }
                    else
                    {
                        D3PanelLeftDrawings.AddRange(documents);
                        D3PanelLeftDrawingsBeforeD.AddRange(D3beforeD);
                    }
                }

                designdictionary.Add("D3PanelRightDrawings", D3PanelRightDrawings);
                designdictionary.Add("D3PanelLeftDrawings", D3PanelLeftDrawings);
                designdictionary.Add("D3PanelRightDrawingsBeforeD", D3PanelRightDrawingsBeforeD);
                designdictionary.Add("D3PanelLeftDrawingsBeforeD", D3PanelLeftDrawingsBeforeD);


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Back Panels


        public void backPanels(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, double ExtractionC_Height, string PaintBoothTypefromEnquiry)
        {
            try
            {
                paintBooth.BackPanelLength = model.PanelWidth;
                TotalBackPanels = model.W / model.PanelWidth;
                noOfPanelsForBackSide = (int)Math.Floor(TotalBackPanels);
                smallPanelWidthForBackSide = model.W - (noOfPanelsForBackSide * model.PanelWidth);
                paintBooth.PanelHeight = model.PanelHeight;

                #region H Calculate Number of Panels In H
                model.PanelHeight = paintBooth.PanelHeight;
                totalPanelsforH = model.H / model.PanelHeight;
                noOfPanelsforH = (int)Math.Floor(totalPanelsforH);
                smallPanelWidthforH = model.H - (noOfPanelsforH * model.PanelHeight);

                //divides panels in PanelHeightAboveFilterFrame with respect to panelWidth
                totalPanelsAboveEC = model.PanelHeightAboveFilterFrame / model.PanelHeight;
                noOfPanelsAboveEC = (int)Math.Floor(totalPanelsAboveEC);
                smallPanelWidthAboveEC = model.PanelHeightAboveFilterFrame - (noOfPanelsAboveEC * model.PanelHeight);

                #endregion

                #region W * H back and front
                List<DesignDocument> backDrawingList = new List<DesignDocument>();
                List<DesignDocument> backDrawingListBeforeExtractionC = new List<DesignDocument>();

                for (int k = 0; k < noOfPanelsforH; k++)
                {
                    for (int j = 0; j < noOfPanelsForBackSide; j++)
                    {
                        PanelDrawResult panelDrawingPath = paintBooth.BackPanelsAfterExtractionChamber(
                            model, ExtractionC_Height, PaintBoothTypefromEnquiry);
                        backDrawingList.Add(panelDrawingPath.drawing);
                        filePaths.Add(panelDrawingPath.lstpath);
                        paintBooth.SavePanelDetails(model, "ECRear_Panels", 1, panelDrawingPath.Weight);

                        //if (model.PanelHeightAboveFilterFrame > 0)
                        //{
                        //    PanelDrawResult backpanelPath = paintBooth.BackPanelsBeforeEC(
                        //        model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);
                        //    backDrawingListBeforeExtractionC.Add(backpanelPath.drawing);
                        //    filePaths.Add(backpanelPath.lstpath);
                        //    paintBooth.SavePanelDetails(model, "ECRear_Panels_Before_EC", 1, backpanelPath.Weight);
                        //}
                    }
                    if (smallPanelWidthForBackSide > 0)
                    {
                        paintBooth.BackPanelLength = smallPanelWidthForBackSide;

                        PanelDrawResult smallPanelDrawingPath = paintBooth.BackPanelsAfterExtractionChamber(
                            model, ExtractionC_Height, PaintBoothTypefromEnquiry);
                        backDrawingList.Add(smallPanelDrawingPath.drawing);
                        paintBooth.SavePanelDetails(model, "ECRear_Panels", 1, smallPanelDrawingPath.Weight);

                        //if (model.PanelHeightAboveFilterFrame > 0)
                        //{
                        //    PanelDrawResult backpanelSmallPath = paintBooth.BackPanelsBeforeEC(
                        //        model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);
                        //    backDrawingListBeforeExtractionC.Add(backpanelSmallPath.drawing);
                        //    filePaths.Add(backpanelSmallPath.lstpath);
                        //    paintBooth.SavePanelDetails(model, "ECRear_Panels_Before_EC", 1, backpanelSmallPath.Weight);
                        //}
                        paintBooth.BackPanelLength = model.PanelWidth;
                    }
                }

                designdictionary.Add("backDrawingList", backDrawingList);
                if (model.PanelHeightAboveFilterFrame > 0)
                {
                    for (int k = 0; k < noOfPanelsAboveEC; k++)
                    {
                        paintBooth.PHForAboveECPanels = model.PanelHeight;
                        for (int j = 0; j < noOfPanelsForBackSide; j++)
                        {

                            PanelDrawResult backpanelPath = paintBooth.BackPanelsBeforeEC(model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);
                            backDrawingListBeforeExtractionC.Add(backpanelPath.drawing);
                            filePaths.Add(backpanelPath.lstpath);
                            paintBooth.SavePanelDetails(model, "ECRear_Panels_Before_EC", 1, backpanelPath.Weight);

                        }
                        if (smallPanelWidthForBackSide > 0)
                        {
                            paintBooth.BackPanelLength = smallPanelWidthForBackSide;

                            PanelDrawResult backpanelSmallPath = paintBooth.BackPanelsBeforeEC(
                                model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);
                            backDrawingListBeforeExtractionC.Add(backpanelSmallPath.drawing);
                            filePaths.Add(backpanelSmallPath.lstpath);
                            paintBooth.SavePanelDetails(model, "ECRear_Panels_Before_EC", 1, backpanelSmallPath.Weight);

                            paintBooth.BackPanelLength = model.PanelWidth;
                        }
                    }
                    if (smallPanelWidthAboveEC > 0)
                    {
                        paintBooth.PHForAboveECPanels = smallPanelWidthAboveEC;
                        for (int j = 0; j < noOfPanelsForBackSide; j++)
                        {

                            PanelDrawResult backpanelPath = paintBooth.BackPanelsBeforeEC(model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);
                            backDrawingListBeforeExtractionC.Add(backpanelPath.drawing);
                            filePaths.Add(backpanelPath.lstpath);
                            paintBooth.SavePanelDetails(model, "ECRear_Panels_Before_EC", 1, backpanelPath.Weight);

                        }
                        if (smallPanelWidthForBackSide > 0)
                        {
                            paintBooth.BackPanelLength = smallPanelWidthForBackSide;

                            PanelDrawResult backpanelSmallPath = paintBooth.BackPanelsBeforeEC(
                                model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);
                            backDrawingListBeforeExtractionC.Add(backpanelSmallPath.drawing);
                            filePaths.Add(backpanelSmallPath.lstpath);
                            paintBooth.SavePanelDetails(model, "ECRear_Panels_Before_EC", 1, backpanelSmallPath.Weight);

                            paintBooth.BackPanelLength = model.PanelWidth;
                        }
                        paintBooth.PHForAboveECPanels = model.PanelHeight;
                    }
                }

                designdictionary.Add("backDrawingListBeforeExtractionC", backDrawingListBeforeExtractionC);

                // ── Types 8 and 9 (Semi Down Draft) have extraction chambers on BOTH
                //    front and rear sides so we mirror the rear EC panels to the front
                if (PaintBoothTypefromEnquiry == "8" || PaintBoothTypefromEnquiry == "9")
                {
                    double translateX = model.D;

                    // ── Mirror rear outer EC wall → front outer EC wall ──────────
                    List<DesignDocument> frontECBackDrawings = new List<DesignDocument>();
                    foreach (DesignDocument doc in backDrawingList)
                        frontECBackDrawings.Add(MirrorAndTranslate(doc, translateX, 0));

                    // ── Mirror rear inner EC wall → front inner EC wall ──────────
                    List<DesignDocument> frontECInnerDrawings = new List<DesignDocument>();
                    foreach (DesignDocument doc in backDrawingListBeforeExtractionC)
                        frontECInnerDrawings.Add(MirrorAndTranslate(doc, translateX, 0));

                    designdictionary.Add("FrontECBackDrawings", frontECBackDrawings);
                    designdictionary.Add("FrontECInnerDrawings", frontECInnerDrawings);

                    // ── Save panel details for front EC panels ────────────────────
                    // Front EC panels are identical in size and weight to the rear ones
                    // We use separate position keys (ECFront_Panels / ECFront_Panels_Before_EC)
                    // so they appear as distinct line items in the BOM
                    paintBooth.BackPanelLength = model.PanelWidth;

                    for (int k = 0; k < noOfPanelsforH; k++)
                    {
                        for (int j = 0; j < noOfPanelsForBackSide; j++)
                        {
                            PanelDrawResult frontPanelPath = paintBooth.BackPanelsAfterExtractionChamber(
                                model, ExtractionC_Height, PaintBoothTypefromEnquiry);

                            paintBooth.SavePanelDetails(model, "ECFront_Panels", 1, frontPanelPath.Weight);

                            if (model.PanelHeightAboveFilterFrame > 0)
                            {
                                PanelDrawResult frontBeforePath = paintBooth.BackPanelsBeforeEC(
                                    model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);

                                paintBooth.SavePanelDetails(model, "ECFront_Panels_Before_EC", 1, frontBeforePath.Weight);
                            }
                        }

                        if (smallPanelWidthForBackSide > 0)
                        {
                            paintBooth.BackPanelLength = smallPanelWidthForBackSide;

                            PanelDrawResult frontSmallPath = paintBooth.BackPanelsAfterExtractionChamber(
                                model, ExtractionC_Height, PaintBoothTypefromEnquiry);

                            paintBooth.SavePanelDetails(model, "ECFront_Panels", 1, frontSmallPath.Weight);

                            if (model.PanelHeightAboveFilterFrame > 0)
                            {
                                PanelDrawResult frontBeforeSmallPath = paintBooth.BackPanelsBeforeEC(
                                    model, model.PanelHeightAboveFilterFrame, PaintBoothTypefromEnquiry);

                                paintBooth.SavePanelDetails(model, "ECFront_Panels_Before_EC", 1, frontBeforeSmallPath.Weight);
                            }

                            paintBooth.BackPanelLength = model.PanelWidth;
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                // _uow.exceptionHandlerRepository.SaveException("PaintBoothService", "backPanels", ex.Message);
                throw;
            }
        }
        #endregion

        #region Loft and Filters

        public void CreateLoft(PaintBoothModel model, PaintBoothDesignBusiness paintBooth,
                       double ExtractionC_Height, string paintBoothType,int NoOfBlowers)
        {
            try
            {
                PanelDrawResult loft = paintBooth.CreateLoft(model, ExtractionC_Height, NoOfBlowers);
                string filepathforLoft = loft.lstpath;
                DesignDocument loftdrawing = loft.drawing;
                double loftWeight = loft.Weight;

                // ── Save first (rear) loft ────────────────────────────────────────
                //paintBooth.SavePanelDetails(model, "Transition_Piece", 1, loftWeight);
                paintBooth.SavePanelDetails(model, "Transition_Piece", loft.NoOfTransitionPieces, 0);

                WriteAutodeskParams loftdw = new WriteAutodeskParams(loftdrawing);
                WriteAutodesk dwgWriterloft = new WriteAutodesk(loftdw, filepathforLoft);
                dwgWriterloft.DoWork();
                loftDrawing.Add(loftdrawing);

                // ── Types 8 and 9 (Semi Down Draft) have blowers on BOTH ends ─────
                // so they need a second transition piece mirrored to the front side
                if (paintBoothType == "8" || paintBoothType == "9")
                {
                    // Mirror and translate rear loft drawing to front position
                    DesignDocument frontLoftDrawing = MirrorAndTranslate(loftdrawing, model.D - model.D3, 0);

                    // Save second loft with same weight — quantity 1 but SavePanelDetails
                    // will increment the existing Transition_Piece record to 2 total
                    //paintBooth.SavePanelDetails(model, "Transition_Piece", 1, loftWeight);
                    paintBooth.SavePanelDetails(model, "Transition_Piece", loft.NoOfTransitionPieces, 0);

                    loftDrawing.Add(frontLoftDrawing);
                }
                paintBooth.CreateLoftDevelopment(model, ExtractionC_Height, NoOfBlowers);
                designdictionary.Add("loftDrawing", loftDrawing);
            }
            catch (Exception ex)
            {
                // _uow.exceptionHandlerRepository.SaveException("PaintBoothService", "CreateLoft", ex.Message);
                throw;
            }
        }
        public void FiltersAndBaffles(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, double ExtractionC_Height, string paintBoothType)
        {
            try
            {
                List<PaintBoothclass> filterFrames = paintBooth.OuterFilterFrame(model, ExtractionC_Height, paintBoothType);
                List<DesignDocument> filterDrawings = new List<DesignDocument>();
                List<string> filterPaths = new List<string>();
                foreach (PaintBoothclass filterFrame in filterFrames)
                {
                    filterDrawings.Add(filterFrame.drawing);
                    filterPaths.Add(filterFrame.lstpath);
                }
                FilterDrawings.AddRange(filterDrawings);
                designdictionary.Add("FilterDrawings", FilterDrawings);
                // Mirror filters to front EC for types 8 & 9
                if (paintBoothType == "8" || paintBoothType == "9")
                {
                    // Rear filters are at x = model.D (offsetX in OuterFilterFrame)
                    // Mirror → translate by model.D → lands at x = -model.D3 area
                    double translateX = model.D;

                    List<DesignDocument> frontFilterDrawings = new List<DesignDocument>();
                    foreach (DesignDocument doc in filterDrawings)
                        frontFilterDrawings.Add(MirrorAndTranslate(doc, translateX, 0));

                    if (designdictionary.ContainsKey("FrontFilterDrawings"))
                        designdictionary["FrontFilterDrawings"].AddRange(frontFilterDrawings);
                    else
                        designdictionary.Add("FrontFilterDrawings", frontFilterDrawings);
                }
            }
            catch (Exception ex)
            {
                //_uow.exceptionHandlerRepository.SaveException("PaintBoothService", "FiltersAndBaffles", ex.Message);

                throw;
            }
        }
        #endregion

        #region Structure frames
        public void TopStructureFrame(PaintBoothModel model, PaintBoothDesignBusiness paintBooth)
        {
            try
            {
                PanelDrawResult Topstructure = paintBooth.TopStructureFrame(model);
                List<DesignDocument> TopStructuredrawing = new List<DesignDocument>();
                List<string> TopFramepath = new List<string>();
                TopStructuredrawing.Add(Topstructure.drawing);
                TopFramepath.Add(Topstructure.lstpath);
                TopFrameDrawing.AddRange(TopStructuredrawing);
                paintBooth.SavePanelDetails(model, "TopStructureFrame", 1, Topstructure.Weight);
                designdictionary.Add("TopFrameDrawing", TopFrameDrawing);

                DesignDocument TopDrawingAss = TopStructure(TopStructuredrawing, 0);
                string TopFrameDwgFilePath = BaseFilePath + "/TopStructure_Drawing.dwg";
                WriteAutodeskParams topframedwg = new WriteAutodeskParams(TopDrawingAss);
                WriteAutodesk dwgWritertopframe = new WriteAutodesk(topframedwg, TopFrameDwgFilePath);
                dwgWritertopframe.DoWork();
            }
            catch (Exception ex)
            {
                //_uow.exceptionHandlerRepository.SaveException("PaintBoothService", "TopStructureFrame", ex.Message);

                throw;
            }
        }

        public void BaseStructureFrame(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, string PaintBoothTypefromEnquiry)
        {
            try
            {
                PaintBoothclass basestructure = paintBooth.BaseStructure(model, PaintBoothTypefromEnquiry);
                List<DesignDocument> basedrawing = new List<DesignDocument>();
                List<string> basepath = new List<string>();
                basedrawing.Add(basestructure.drawing);
                basepath.Add(basestructure.lstpath);
                BaseDrawings.AddRange(basedrawing);
                designdictionary.Add("BaseDrawings", BaseDrawings);
                DesignDocument BaseDrawingAss = BaseStructure(basedrawing, 0);
                string BaseDwgFilePath = BaseFilePath + "/BaseStructure_Drawing.dwg";
                WriteAutodeskParams Basedwg = new WriteAutodeskParams(BaseDrawingAss);
                WriteAutodesk dwgWriterBase = new WriteAutodesk(Basedwg, BaseDwgFilePath);
                dwgWriterBase.DoWork();
            }
            catch (Exception ex)
            {
                // _uow.exceptionHandlerRepository.SaveException("PaintBoothService", "BaseStructureFrame", ex.Message);

                throw;
            }

        }
        #endregion

        #region FAS
        public void FASPanelsFrontAndBack(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, int PlenumHeight, EnquiryMaster FASDoor)
        {
            try
            {
                #region W * H back and front(optional) 
                string BackPanelsDwgFilePathForTop = "";
                string FrontPanelsDwgFilePathInTop = "";
                List<DesignDocument> backDrawingListInTopSide = new List<DesignDocument>();
                List<DesignDocument> FrontDrawingListInTopSide = new List<DesignDocument>();


                for (int k = 0; k < noOfPanelsforH; k++)
                {
                    for (int j = 0; j < noOfPanelsForBackSide; j++)
                    {
                        PanelDrawResult panelDrawingInTopSide = paintBooth.BackPanelsForFAS(model, PlenumHeight, FASDoor, j);
                        backDrawingListInTopSide.Add(panelDrawingInTopSide.drawing);
                        filePaths.Add(panelDrawingInTopSide.lstpath);
                        paintBooth.SavePanelDetails(model, "PARearFAS", 1, panelDrawingInTopSide.Weight);

                        PanelDrawResult frontPanelsPathInTopSide = paintBooth.FrontPanelsForFAS(model, PlenumHeight, FASDoor, j);
                        FrontDrawingListInTopSide.Add(frontPanelsPathInTopSide.drawing);
                        filePaths.Add(frontPanelsPathInTopSide.lstpath);

                    }
                    if (smallPanelWidthForBackSide > 0)
                    {
                        paintBooth.BackPanelLength = smallPanelWidthForBackSide;
                        PanelDrawResult smallPanelDrawingInTopSide = paintBooth.BackPanelsForFAS(model, PlenumHeight, FASDoor, 0);
                        backDrawingListInTopSide.Add(smallPanelDrawingInTopSide.drawing);
                        paintBooth.SavePanelDetails(model, "PARearFAS", 1, smallPanelDrawingInTopSide.Weight);

                        PanelDrawResult smallFrontPanelDrawingInTopSide = paintBooth.FrontPanelsForFAS(model, PlenumHeight, FASDoor, 0);
                        FrontDrawingListInTopSide.Add(smallFrontPanelDrawingInTopSide.drawing);
                        filePaths.Add(smallFrontPanelDrawingInTopSide.lstpath);
                        paintBooth.BackPanelLength = model.PanelWidth;
                    }
                }

                DesignDocument BackPanelsDrawingforTop = BackPanelsAssembly(backDrawingListInTopSide, 0);
                BackPanelsDwgFilePathForTop = BaseFilePath + "/RearPanelInTop.dwg";
                WriteAutodeskParams backdwg1 = new WriteAutodeskParams(BackPanelsDrawingforTop);
                WriteAutodesk backinTop = new WriteAutodesk(backdwg1, BackPanelsDwgFilePathForTop);
                backinTop.DoWork();

                //front panels
                DesignDocument FrontPanelsDrawingforTop = FrontPanelsAssembly(FrontDrawingListInTopSide, 0);
                FrontPanelsDwgFilePathInTop = BaseFilePath + "/FrontPanelInTop.dwg";
                WriteAutodeskParams frontdwgInTop = new WriteAutodeskParams(FrontPanelsDrawingforTop);
                WriteAutodesk FrontTop = new WriteAutodesk(frontdwgInTop, FrontPanelsDwgFilePathInTop);
                FrontTop.DoWork();
                designdictionary.Add("backDrawingListInTopSide", backDrawingListInTopSide);
                designdictionary.Add("FrontDrawingListInTopSide", FrontDrawingListInTopSide);

                #endregion
            }
            catch (Exception ex)
            {
                //_uow.exceptionHandlerRepository.SaveException("PaintBoothService", "FASPanelsFrontAndBack", ex.Message);

                throw;
            }
        }
        public void FASPanelsRightAndLeft(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, int PlenumHeight, EnquiryMaster FASDoor)
        {
            try
            {
                List<DesignDocument> RightPanelsListInTopSideForFAS = new List<DesignDocument>();
                List<DesignDocument> LeftPanelsListInTopSideForFAS = new List<DesignDocument>();
                #region  D Calculate Number of panels in D
                model.PanelWidth = paintBooth.PanelWidth;
                totalPanelsforD = model.D / model.PanelWidth;
                noOfPanelsforD = (int)Math.Floor(totalPanelsforD);
                smallPanelWidthforD = model.D - (noOfPanelsforD * model.PanelWidth);
                #endregion
                for (int l = 0; l < 2; l++)
                {
                    paintBooth.PanelLength = (l == 0) ? 0 : model.W;
                    List<DesignDocument> documents = new List<DesignDocument>();

                    //for (int k = 0; k < noOfPanelsforH; k++)
                    //{

                    for (int j = 0; j < noOfPanelsforD; j++)
                    {
                        PanelDrawResult panelDrawingPathForType5 = paintBooth.PanelsInPaintBoothForPlenum(j + 1, model, l, PlenumHeight, FASDoor);
                        documents.Add(panelDrawingPathForType5.drawing);
                        filePaths.Add(panelDrawingPathForType5.lstpath);
                        paintBooth.SavePanelDetails(model, l == 0 ? "PARightFAS" : "PALeftFAS", 1, panelDrawingPathForType5.Weight);
                    }
                    if (smallPanelWidthforD > 0)
                    {
                        paintBooth.PanelWidth = smallPanelWidthforD;

                        PanelDrawResult smallPanelDrawingPathForType5 = paintBooth.PanelsInPaintBoothForPlenum(noOfPanelsforD + 1, model, l, PlenumHeight, FASDoor);
                        documents.Add(smallPanelDrawingPathForType5.drawing);
                        filePaths.Add(smallPanelDrawingPathForType5.lstpath);
                        paintBooth.SavePanelDetails(model, l == 0 ? "PARightFAS" : "PALeftFAS", 1, smallPanelDrawingPathForType5.Weight);

                        paintBooth.PanelWidth = model.PanelWidth;
                    }
                    //}
                    if (l == 0)
                    {
                        RightPanelsListInTopSideForFAS.AddRange(documents);

                    }
                    else if (l == 1)
                    {
                        LeftPanelsListInTopSideForFAS.AddRange(documents);
                    }
                }
                designdictionary.Add("RightPanelsListInTopSideForFAS", RightPanelsListInTopSideForFAS);
                designdictionary.Add("LeftPanelsListInTopSideForFAS", LeftPanelsListInTopSideForFAS);
            }
            catch (Exception ex)
            {
                //_uow.exceptionHandlerRepository.SaveException("PaintBoothService", "FASPanelsRightAndLeft", ex.Message);

                throw;
            }
            //paintBooth.PanelLength = PanelLengthTemp;


        }
        public void FASPanelsForTop(PaintBoothModel model, PaintBoothDesignBusiness paintBooth, int PlenumHeight)
        {
            try
            {
                #region W*D Top 
                List<DesignDocument> TopPanelsForFAS = new List<DesignDocument>();
                List<DesignDocument> TopAssemblyForFAS = new List<DesignDocument>();
                model.PanelHeight = paintBooth.PanelHeight;
                totalPanelsforW = model.W / paintBooth.PanelHeight;
                noOfPanelsforW = (int)Math.Floor(totalPanelsforW);
                smallPanelWidthforW = model.W - (noOfPanelsforW * paintBooth.PanelHeight);
                List<string> Topsidepath = new List<string>();
                for (int i = 0; i < noOfPanelsforW; i++)
                {
                    for (int k = 0; k < noOfPanelsforD; k++)
                    {
                        paintBooth.PanelLengthForTopPanels = model.PanelHeight;
                        PanelDrawResult Topside = paintBooth.TopSidePanelsForFAS(k + 1, model, i, PlenumHeight);
                        TopPanelsForFAS.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATopFAS", 1, Topside.Weight);
                    }
                    if (smallPanelWidthforD > 0)
                    {
                        paintBooth.PanelWidth = smallPanelWidthforD;
                        PanelDrawResult Topside = paintBooth.TopSidePanelsForFAS(noOfPanelsforW + 1, model, i, PlenumHeight);
                        TopPanelsForFAS.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATopFAS", 1, Topside.Weight);
                        paintBooth.PanelWidth = model.PanelWidth;
                    }
                }
                if (smallPanelWidthforW > 0)
                {
                    paintBooth.PanelLengthForTopPanels = smallPanelWidthforW;
                    for (int k = 0; k < noOfPanelsforD; k++)
                    {
                        PanelDrawResult Topside = paintBooth.TopSidePanelsForFAS(k + 1, model, k, PlenumHeight);
                        TopPanelsForFAS.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATopFAS", 1, Topside.Weight);
                    }
                    if (smallPanelWidthforD > 0)
                    {
                        paintBooth.PanelWidth = smallPanelWidthforD;
                        PanelDrawResult Topside = paintBooth.TopSidePanelsForFAS(noOfPanelsforW + 1, model, 0, PlenumHeight);
                        TopPanelsForFAS.Add(Topside.drawing);
                        Topsidepath.Add(Topside.lstpath);
                        paintBooth.SavePanelDetails(model, "PATopFAS", 1, Topside.Weight);
                        paintBooth.PanelWidth = model.PanelWidth;
                    }
                }
                TopAssemblyForFAS.AddRange(TopPanelsForFAS);

                designdictionary.Add("TopAssemblyForFAS", TopAssemblyForFAS);

                #endregion
            }
            catch (Exception ex)
            {
                // _uow.exceptionHandlerRepository.SaveException("PaintBoothService", "FASPanelsForTop", ex.Message);
                throw;
            }
        }
        #endregion

        #region Assembly Calculations
        private DesignDocument RightSideWallAssembly(List<DesignDocument> drawings, double yOffset, double panelheight)
        {
            //PanelInputModel p = new PanelInputModel();


            var assemblyDrawing = new DesignDocument();
            assemblyDrawing.Units = linearUnitsType.Millimeters;
            Layer mylayer = new Layer("bendlayer");
            mylayer.Color = Color.FromArgb(165, 82, 165);
            assemblyDrawing.Layers.Add(mylayer);

            double currentXOffset = 0, Woffset = 0;

            int i = 0;
            int TotalPanelOfH = 0;

            foreach (var drawing in drawings)
            {

                if (smallPanelWidthforH > 0)
                    TotalPanelOfH = noOfPanelsforH + 1;

                if (i == TotalPanelOfH)
                {
                    currentXOffset = 0;
                    Woffset = panelheight;

                }
                Block blk = new Block("Block_" + Guid.NewGuid());

                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, Woffset, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
                i++;
            }
            return assemblyDrawing;
        }
        private DesignDocument LeftSideWallAssembly(List<DesignDocument> drawings, double yOffset)
        {
            // Same as RightSideWallAssembly but with the yOffset parameter
            var assemblyDrawing = new DesignDocument();
            Layer mylayer = new Layer("bendlayer");
            mylayer.Color = Color.FromArgb(165, 82, 165);
            assemblyDrawing.Layers.Add(mylayer);
            assemblyDrawing.Units = linearUnitsType.Millimeters;

            double currentXOffset = 0;

            foreach (var drawing in drawings)
            {
                Block blk = new Block("Block_" + Guid.NewGuid());
                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, 0, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
            }
            return assemblyDrawing;
        }
        private DesignDocument TopWallAssembly(List<DesignDocument> Docdrawing, double yOffset)
        {
            #region old code
            var assemblyDrawing = new DesignDocument();
            Layer mylayer = new Layer("bendlayer");
            mylayer.Color = Color.FromArgb(165, 82, 165);
            assemblyDrawing.Layers.Add(mylayer);
            assemblyDrawing.Units = linearUnitsType.Millimeters;

            double currentXOffset = 0;

            foreach (var drawing in Docdrawing)
            {

                Block blk = new Block("Block_" + Guid.NewGuid());

                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, 0, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
            }
            return assemblyDrawing;
            #endregion
        }
        private DesignDocument TopStructure(List<DesignDocument> TopStructuredrawing, double yOffset)
        {
            var assemblyDrawing = new DesignDocument();
            assemblyDrawing.Units = linearUnitsType.Millimeters;

            double currentXOffset = 0;

            foreach (var drawing in TopStructuredrawing)
            {

                Block blk = new Block("Block_" + Guid.NewGuid());

                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, 0, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
            }
            return assemblyDrawing;
        }
        private DesignDocument BaseStructure(List<DesignDocument> basedrawing, double yOffset)
        {
            var assemblyDrawing = new DesignDocument();
            assemblyDrawing.Units = linearUnitsType.Millimeters;

            double currentXOffset = 0;

            foreach (var drawing in basedrawing)
            {

                Block blk = new Block("Block_" + Guid.NewGuid());

                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, 0, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
            }
            return assemblyDrawing;
        }
        private DesignDocument BackPanelsAssembly(List<DesignDocument> YAxisdrawing, double yOffset)
        {
            var assemblyDrawing = new DesignDocument();
            Layer mylayer = new Layer("bendlayer");
            mylayer.Color = Color.FromArgb(165, 82, 165);
            assemblyDrawing.Layers.Add(mylayer);
            assemblyDrawing.Units = linearUnitsType.Millimeters;

            double currentXOffset = 0;

            foreach (var drawing in YAxisdrawing)
            {

                Block blk = new Block("Block_" + Guid.NewGuid());

                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, 0, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
            }
            return assemblyDrawing;
        }
        private DesignDocument FrontPanelsAssembly(List<DesignDocument> YAxisdrawing, double yOffset)
        {
            var assemblyDrawing = new DesignDocument();
            Layer mylayer = new Layer("bendlayer");
            mylayer.Color = Color.FromArgb(165, 82, 165);
            assemblyDrawing.Layers.Add(mylayer);
            assemblyDrawing.Units = linearUnitsType.Millimeters;

            double currentXOffset = 0;

            foreach (var drawing in YAxisdrawing)
            {

                Block blk = new Block("Block_" + Guid.NewGuid());

                blk.Entities.AddRange(drawing.Entities);
                assemblyDrawing.Blocks.Add(blk);

                double drawingWidth = CalculateDrawingWidth(drawing);
                BlockReference blkReference = new BlockReference(currentXOffset, yOffset, 0, blk.Name, 0);
                assemblyDrawing.Entities.Add(blkReference);

                currentXOffset += drawingWidth;
            }
            return assemblyDrawing;
        }

        private double CalculateDrawingWidth(DesignDocument drawing)
        {
            if (drawing.Entities.Count == 0)
                return 0;

            double minX = double.MaxValue;
            double maxX = double.MinValue;

            foreach (var entity in drawing.Entities)
            {
                if (entity.BoxMin.X < minX)
                    minX = entity.BoxMin.X;

                if (entity.BoxMax.X > maxX)
                    maxX = entity.BoxMax.X;
            }

            return maxX - minX;
        }

        #endregion

        public void CreateLoftAtFrontSide(PaintBoothModel model, PaintBoothDesignBusiness paintBooth,
                                  double ExtractionC_Height)
        {
            try
            {
                PaintBoothclass loft = paintBooth.CreateLoftForPaintboothType5(model, "5");
                string filepathforLoft = loft.lstpath;
                DesignDocument loftdrawing = loft.drawing;

                WriteAutodeskParams loftdw = new WriteAutodeskParams(loftdrawing);
                WriteAutodesk dwgWriterloft = new WriteAutodesk(loftdw, filepathforLoft);
                dwgWriterloft.DoWork();

                // Append to existing loftDrawing key already added by CreateLoft
                if (designdictionary.ContainsKey("loftDrawing"))
                    designdictionary["loftDrawing"].Add(loftdrawing);
                else
                {
                    loftDrawing.Add(loftdrawing);
                    designdictionary.Add("loftDrawing", loftDrawing);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void FiltersAndBafflesAtZeroX(PaintBoothModel model, PaintBoothDesignBusiness paintBooth)
        {
            try
            {
                List<PaintBoothclass> filterFrames = paintBooth.OuterFilterFrameAtZeroX(model, "7");
                List<DesignDocument> filterDrawingsAtZeroX = new List<DesignDocument>();

                foreach (PaintBoothclass filterFrame in filterFrames)
                {
                    filterDrawingsAtZeroX.Add(filterFrame.drawing);
                }

                if (designdictionary.ContainsKey("FilterDrawingsWithZeroX"))
                    designdictionary["FilterDrawingsWithZeroX"].AddRange(filterDrawingsAtZeroX);
                else
                    designdictionary.Add("FilterDrawingsWithZeroX", filterDrawingsAtZeroX);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void CivilTank(PaintBoothModel model, PaintBoothDesignBusiness paintBooth)
        {
            try
            {
                PaintBoothclass civilTank = paintBooth.Civil(model);
                List<DesignDocument> civilDrawing = new List<DesignDocument> { civilTank.drawing };
                paintBooth.SavePanelDetails(model, "Civil_Tank", 1, 0);
                designdictionary.Add("CivilTankDrawing", civilDrawing);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private DesignDocument MirrorAndTranslate(DesignDocument source, double translateX, double z)
        {
            DesignDocument mirrored = new DesignDocument();
            mirrored.Units = linearUnitsType.Millimeters;

            foreach (Entity entity in source.Entities)
            {
                Entity cloned = (Entity)entity.Clone();
                // Mirror across YZ plane at X=0 (negate X coordinates)
                cloned.TransformBy(new Mirror(Plane.YZ));
                // Then translate to correct front position
                cloned.Translate(translateX, 0, z);
                mirrored.Entities.Add(cloned, entity.Color);
            }
            return mirrored;
        }
    }
}
