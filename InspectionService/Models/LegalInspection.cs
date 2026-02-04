namespace InspectionService.Models
{
    public class LegalInspection
    {
        public int Id { get; set; }
        public int ClientRequestId { get; set; }
        public int VehicleId { get; set; }
        public bool IsStolen { get; set; }
        public bool HasLien { get; set; }
        public bool RegisteredInGIBDD { get; set; }
        public bool OwnedByLegalEntity { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
