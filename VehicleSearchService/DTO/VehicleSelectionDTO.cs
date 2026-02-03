namespace VehicleSearchService.DTO
{
    public class VehicleSelectionDto
    {
        public int Id { get; set; }
        public int ClientRequestId { get; set; }
        public List<VehicleDto> Vehicles { get; set; } = new();
    }
}
