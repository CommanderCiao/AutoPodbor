using VehicleSearchService.Models;

namespace VehicleSearchService.DTO
{
    public class VehicleCard
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Kilometrage { get; set; }
        public Status Status { get; set; }

    }
}
