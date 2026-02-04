namespace InspectionService.DTO
{
    public class TechnicalInspectionDTO
    {
        public bool IsBodyDamaged { get; set; }
        public bool KilometrageVerified { get; set; }
        public string Recommendations { get; set; } = string.Empty;
    }
}
