using ClientService.DTO;
using DealService.Data;
using DealService.DTO;
using DealService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DealService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public DealController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }



        [HttpPost("{clientRequestId}")]
        public async Task<IActionResult> MakeContract(int clientRequestId, [FromBody] VehicleGetterDto dto)
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

        

            if (clientCriteria.Status != RequestStatus.Verified)
            {
                if (clientCriteria.Status == RequestStatus.DealPrepared)
                {
                    return BadRequest("Сделка по данному автомобилю уже создана");
                }
                return BadRequest("Данный автомобиль еще не прошел проверку");
            } 

                var deal = new Deal
                {
                    ClientRequestId = clientRequestId,
                    VehicleId = dto.Id,
                    VehiclePrice = dto.Price,
                    CreatedAt = DateTime.UtcNow,
                    ContractNumber = $"DEAL-{DateTime.UtcNow:yyyyMMddHHmmss}-{dto.Id}"
                };

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            httpClient.DefaultRequestHeaders.Add("X-User-Role", "manager");
           
            var statusUpdateRequest = new StatusUpdateDTO { Status = RequestStatus.DealPrepared };

            response = await httpClient.PutAsJsonAsync($"api/clientrequests/{clientRequestId}/status", statusUpdateRequest);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Не удалось обновить статус");

            return Ok(new
            {
                message = $"Сделка {deal.Id} создана",
                requestId = deal.ClientRequestId,
                vehicleId = deal.VehicleId,
                result = statusUpdateRequest.Status.ToString()
            });
        }

        [HttpGet("{dealId}")]
        public async Task<IActionResult> GetContract(int dealId)
        {
            var deal = await _context.Deals.FindAsync(dealId);

            if (deal == null)
                return NotFound();

            return Ok(deal);
        }
    }
}
