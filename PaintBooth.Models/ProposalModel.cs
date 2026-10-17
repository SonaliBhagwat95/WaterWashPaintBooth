namespace PaintBooth.Models
{
    public  class ProposalModel
    {
        public string SalesNO { get; set; }
        public int  EnquiryID { get; set; }
       public int RevisionNo { get; set; }
        public string CompanyName { get; set; }
        public string CompAddress { get; set; }
        public string ContactPerson { get; set; }
        public string Designation { get; set; }
        public string CompEmail { get; set; }
        public string MobileNo { get; set; }
        public string Component { get; set; }
        public string MaterialOfConstruction { get; set; }
        public string DraftType { get; set; }
        public string SubTypeOfDraftType { get; set; }
        public double WidthSize { get; set; }
        public double HeightSize { get; set; }
        public double Length { get; set; }
        public double Workingdays { get; set; }
        public double NumberofShifts { get; set; }
        public double EffectiveWorking { get; set; }
        public double Weight { get; set; }
        public int ProductionRequirement { get; set; }
        public string ComponentHandling { get; set; }
        public double Viscosity { get; set; }
        public double DFT { get; set; }
        
        public int NoOfCoats { get; set; }
        public int Paint { get; set; }

        public int NoOfColors { get; set; }
        public double SpecificHeat { get; set; }
        public string LoadingUnloading { get; set; }
        public string Category { get; set; }
        public string CategoryText { get; set; }

        public string Materials { get; set; }
        public double EXhaustCapacity { get; set; }
        //public decimal Motorcapacity { get; set; }
        public decimal Lumens { get; set; }
        public decimal LuxLevel { get; set; }
        public int TubelightQuantity { get; set; }
        public decimal MotorCapacity { get; set; }
        public string TypeOfPaint { get; set; }
        public string TypeOfPaintName { get; set; }
        public string ComponantHandlingName { get; set; }
        public decimal CycleTime { get; set; }
        public string ExtractionChamberHeight { get; set; }
        public string PaintBoothMaterial { get; set; }
        public string ECMaterials { get; set; }
        public double ComponantWeight { get; set; }
        public float D { get; set; }
        public float W { get; set; }
        public float H { get; set; }
        public float D3 { get; set; }
        public int NoOfBlowers { get; set; }
        public string LightSubType { get; set; }
        public string LightType { get; set; }
        public int SalesEngineerId { get; set; }
        public string? DoorType { get; set; }
        public string DoorSubType { get; set; }
        public string TypeOfHingedDoor { get; set; }
        public decimal DoorWidth { get; set; }
        public decimal DoorHeight { get; set; }
        public double  PASheetThickness { get; set; }
        public double ECSheetThickness { get; set; }
        public string MotorType { get; set; }
        public string ImagePath { get; set; }

    }
}
