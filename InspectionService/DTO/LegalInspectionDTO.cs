namespace InspectionService.DTO
{
    public class LegalInspectionDTO
    {
        public bool IsStolen { get; set; }
        public bool HasLien { get; set; }
        public bool RegisteredInGIBDD { get; set; }
        public bool OwnedByLegalEntity { get; set; }
    }
}
