namespace InspectionService.DTO
{
    public class InspectionRequestDto
    {
        public int ClientRequestId { get; set; }
        public List<int> VehicleIds { get; set; } = new List<int>();
    }
}
