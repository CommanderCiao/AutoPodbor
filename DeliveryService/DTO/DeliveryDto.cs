using DeliveryService.Models;

namespace DeliveryService.DTO
{
    public class DeliveryDto
    {
        public int ClientRequestId { get; set; }
        public int VehicleID { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string? DestinationAddress { get; set; }
        public string? OriginCountry { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public DeliveryStatus Status { get; set; }
    }



}
