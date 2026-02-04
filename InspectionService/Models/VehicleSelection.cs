namespace InspectionService.Models
{
    public class VehicleSelection
    {
        public int Id { get; set; }
        public int ClientRequestId { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new();

    }
}
