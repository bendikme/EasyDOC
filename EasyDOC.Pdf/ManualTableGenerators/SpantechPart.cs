namespace EasyDOC.Pdf.ManualTableGenerators
{
    internal class SpantechPart
    {
        public string ProjectNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartDesc { get; set; }
        public string PartDescNo { get; set; }
        public string PartDescSe { get; set; }
        public string Drawing { get; set; }
        public string Unit { get; set; }
        public double TotalAmount { get; set; }
        public string SpareAmount { get; set; }
        public string ConveyorWidth { get; set; }
        public string UnitOfMeasurement { get; set; }
        public int Code { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string PartReference { get; set; }
        public bool IsIncludedInList { get; set; }
    }
}