using VehicleSearchService.Models;

namespace VehicleSearchService.DTO
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; }
        public int Kilometrage { get; set; }
        public decimal Price { get; set; }
        public Segment Segment { get; set; }
        public SourceOfPurchase Source { get; set; }
        public string VIN { get; set; } = default!;
    }
}
