namespace InspectionService.Models
{
    public class TechnicalInspection
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int ClientRequestId { get; set; }
        public bool IsBodyDamaged { get; set; }
        public bool KilometrageVerified { get; set; }
        public string Recommendations { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
