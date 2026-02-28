namespace DealService.Models
{
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
