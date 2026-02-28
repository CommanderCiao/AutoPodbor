namespace DeliveryService.Models
{
    public class DeliveryRequest
    {
       public int Id { get; set; }
       public int ClientRequestId { get; set; }   
       public int VehicleId { get; set; }
       public DeliveryType DeliveryType { get; set; }
       public string? DestinationAddress { get; set; }
       public bool RequiresCustomsClearence => DeliveryType == DeliveryType.International;
       public string? OriginCountry { get; set; }
       public DateTime EstimatedDeliveryDate { get; set; }
       public DeliveryStatus Status { get; set; }
    }

    public enum DeliveryStatus
    {
        Scheduled,
        InTransit,
        ReadyForHandover,
        Delivered,
        Cancelled,
        Delayed
    }

    public enum DeliveryType
    {
        SelfPickup,
        Courier,
        International
    }

}
