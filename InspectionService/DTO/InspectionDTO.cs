namespace InspectionService.DTO
{
    public class InspectionDTO
    {
        public int ClientRequestId { get; set; }
        public int VehicleId { get; set; }
        public TechnicalInspectionDTO Technical { get; set; } = new();
        public LegalInspectionDTO Legal { get; set; } = new();
    }
}
