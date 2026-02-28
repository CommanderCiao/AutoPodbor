using ClientService.Models;

namespace ClientService.DTO
{
    public class ClientRequestDTO
    {
        public int ClientId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? PreferredBrand { get; set; }
        public int? MaxKilometrage { get; set; }
        public decimal Budget { get; set; }
        public int YearOfManufacture { get; set; }
        public Segment Segment { get; set; }
        public SourceOfPurchase Source { get; set; }
        public RequestStatus Status { get; set; }

    }
}
