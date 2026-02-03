using VehicleSearchService.Models;

namespace VehicleSearchService.DTO
{
    public class ClientRequestDTO
    {
        public int Id { get; set; }
        public string? PreferredBand { get; set; }
        public int? MaxKilometrage { get; set; }
        public decimal Budget { get; set; }
        public int YearOfManufacture { get; set; }
        public Segment Segment { get; set; }
        public SourceOfPurchase Source { get; set; }
        public RequestStatus Status { get; set; }
    }

}
