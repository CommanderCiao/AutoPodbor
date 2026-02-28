using DeliveryService.Data;
using DeliveryService.DTO;
using DeliveryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public DeliveryRequestController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("{clientRequestId}")]
        public async Task<IActionResult> CreateDeliveryRequest(int clientRequestId, [FromBody] DeliveryDto dto)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://client-service:80/");
            var response = await httpClient.GetAsync($"api/clientrequests/{clientRequestId}");

            if (!response.IsSuccessStatusCode)
                return NotFound("Заявка не найдена");

            var clientCriteria = await response.Content.ReadFromJsonAsync<ClientRequestDTO>(
               new JsonSerializerOptions
               {
                   PropertyNameCaseInsensitive = true,
                   Converters = { new JsonStringEnumConverter() }
               });

            if (clientCriteria.Status != RequestStatus.DealPrepared)
            {
                return BadRequest("Сделка по данному автомобилю еще не создана");
            }


            var deliveryRequest = new DeliveryRequest
            {
                ClientRequestId = clientRequestId,
                VehicleId = dto.VehicleID,
                DeliveryType = dto.DeliveryType,
                OriginCountry = dto.OriginCountry,
                EstimatedDeliveryDate = dto.EstimatedDeliveryDate,
                Status = dto.Status,
                DestinationAddress = dto.DestinationAddress
            };

            _context.DeliveryRequests.Add(deliveryRequest);
            await _context.SaveChangesAsync();

            httpClient.DefaultRequestHeaders.Add("X-User-Role", "logistician");
            var statusUpdate = new { Status = RequestStatus.InTransit };
            await httpClient.PutAsJsonAsync($"api/clientrequests/{clientRequestId}/status", statusUpdate);

            return Ok(deliveryRequest);
        }

        [HttpPut("{deliveryId}/updateStatus")]
        public async Task<IActionResult> CompleteDelivery(int deliveryId, [FromBody] DeliveryStatusDTO dto)
        {
            var deliveryRequest = await _context.DeliveryRequests.FindAsync(deliveryId);

            if (deliveryRequest == null) 
            {
                return BadRequest("Заказ доставки не найден");
            }

            deliveryRequest.Status = dto.Status;
            await _context.SaveChangesAsync();


            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://client-service:80/");

            RequestStatus newClientRequestStatus = dto.Status switch
            {
                DeliveryStatus.Cancelled => RequestStatus.Rejected,
                _ => RequestStatus.InTransit
            };

            httpClient.DefaultRequestHeaders.Add("X-User-Role", "logistician");
            var statusUpdate = new { Status = newClientRequestStatus };
            var response = await httpClient.PutAsJsonAsync($"api/clientrequests/{deliveryRequest.ClientRequestId}/status", statusUpdate);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Не удалось обновить статус заявки");

            return Ok(deliveryRequest);
        }

        [HttpGet("{deliveryId}")]
        public async Task<IActionResult> GetDeliveryRequest(int deliveryId)
        {
            var delivery = await _context.DeliveryRequests.FindAsync(deliveryId);

            if (delivery == null) { return NotFound("Заказ доставки не найден"); }

            return Ok(delivery);
        }

        [HttpGet("by-request/{clientRequestId}")]
        public async Task<IActionResult> GetDeliveryByRequest(int clientRequestId)
        {
            var delivery = await _context.DeliveryRequests
                .FirstOrDefaultAsync(d => d.ClientRequestId == clientRequestId);

            if (delivery == null)
                return NotFound();

            return Ok(delivery);
        }

        [HttpPost("ready/{deliveryId}")]
        public async Task<IActionResult> MarkReadyForHandover(int deliveryId)
        {
            var deliveryRequest = await _context.DeliveryRequests.FindAsync(deliveryId);

            if (deliveryRequest == null)
            {
                return BadRequest("Заказ доставки не найден");
            }

            deliveryRequest.Status = DeliveryStatus.ReadyForHandover;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                comment = "Заказ "
            });
        }


        [HttpPost("confirmByClient/{deliveryId}")]
        
        public async Task<IActionResult> ConfirmReceiptByClient(int deliveryId)
        {
            var delivery = await _context.DeliveryRequests.FindAsync(deliveryId);

            if (delivery == null)
            {
                return BadRequest($"Доставки с номером {deliveryId} не существует");
            }

            if (delivery.Status != DeliveryStatus.ReadyForHandover)
            {
                return BadRequest($"Автомобиль не готов к выдаче");
            }

            delivery.Status = DeliveryStatus.Delivered;
            await _context.SaveChangesAsync();

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://client-service:80/");
            httpClient.DefaultRequestHeaders.Add("X-User-Role", "logistician");
            var statusUpdate = new { Status = RequestStatus.Completed };
            var response = await httpClient.PutAsJsonAsync($"api/clientrequests/{delivery.ClientRequestId}/status", statusUpdate);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Не удалось обновить статус заявки");
            return Ok(new { message = "Получено клиентом" });
        }
    }
}
