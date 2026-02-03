namespace ClientService.Models
{
    public class ClientRequest
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone {  get; set; }
        public string? Email { get; set; }
        public string? PreferredBand { get; set; }
        public int? MaxKilometrage { get; set; }
        public decimal Budget { get; set; }
        public int YearOfManufacture { get; set; }
        public Segment Segment { get; set; }
        public SourceOfPurchase Source { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.New;
    }

    public enum RequestStatus
    {
        New,
        InReview,
        Approved,
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

