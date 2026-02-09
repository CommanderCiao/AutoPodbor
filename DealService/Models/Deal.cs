namespace DealService.Models
{
    public class Deal
    {
        public int Id { get; set; }
        public int ClientRequestId { get; set; }
        public int VehicleId { get; set; }
        public decimal VehiclePrice { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ContractNumber { get; set; } = string.Empty;
    }
}
