namespace DeliveryService.DTO
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
    public enum RequestStatus
    {
        New,
        InReview,
        Verified,
        Rejected,
        DealPrepared,
        InTransit,
        Delivered,
        Completed,
        Closed
    }

    public enum Segment
    {
        Premium,
        Middle,
        Working
    }

    public enum SourceOfPurchase
    {
        Import,
        Dealer,
        PrivateSeller
    }


}
