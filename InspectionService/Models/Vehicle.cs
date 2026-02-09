using System.Text.Json.Serialization;

namespace InspectionService.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string? Brand { get; set; }
        public int Year { get; set; }
        public int Kilometrage { get; set; }
        public decimal Price { get; set; }
        public Segment Segment { get; set; }
        public SourceOfPurchase Source { get; set; }
        public string? Model { get; set; }
        public string? VIN { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Segment
    {
        Premium = 0,
        Middle = 1,
        Working = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SourceOfPurchase
    {
        Import = 0,
        Dealer = 1,
        PrivateSeller = 2
    }
    public enum RequestStatus
    {
        New,
        InReview,
        Verified,
        Rejected,
        DealPrepared,
        Closed
    }

}

