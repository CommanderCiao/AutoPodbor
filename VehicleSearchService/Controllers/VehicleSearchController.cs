using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using VehicleSearchService.Data;
using VehicleSearchService.DTO;
using VehicleSearchService.Models;

namespace VehicleSearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleSearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<VehicleSearchController> _logger;

        public VehicleSearchController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<VehicleSearchController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> StartSelection([FromBody] StartSelectionRequest request)
        {
            _logger.LogInformation($"Запущен подбор для заявки {request.ClientRequestId}");
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://client-service:80/");

            var response = await httpClient.GetAsync($"api/clientrequests/{request.ClientRequestId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Заявка не найдена");
                return NotFound("Заявка не найдена");
            }
               
            var clientCriteria = await response.Content.ReadFromJsonAsync<ClientRequestDTO>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

            if (clientCriteria.Status != RequestStatus.New)
            {
                _logger.LogInformation("Подбор уже был выполнен для этой заявки");
                return BadRequest("Подбор уже был выполнен для этой заявки");
            }

            var vehicles = await _context.Vehicles
                .Where(v =>
                    v.Price <= clientCriteria.Budget &&
                    v.Year >= clientCriteria.YearOfManufacture &&
                    (!clientCriteria.MaxKilometrage.HasValue || v.Kilometrage <= clientCriteria.MaxKilometrage.Value) &&
                    (string.IsNullOrEmpty(clientCriteria.PreferredBand) || v.Brand == clientCriteria.PreferredBand) &&
                    v.Segment == clientCriteria.Segment 
                )
                .ToListAsync();

            var selection = new VehicleSelection
            {
                ClientRequestId = request.ClientRequestId,
                Vehicles = vehicles
            };

            _context.VehicleSelections.Add(selection);
            await _context.SaveChangesAsync();

            httpClient.DefaultRequestHeaders.Add("X-User-Role", "analyst");

            var statusUpdateRequest = new StatusUpdateDTO { Status = RequestStatus.InReview };

            var statusResponse = await httpClient.PutAsJsonAsync(
                $"api/clientrequests/{request.ClientRequestId}/status",
                statusUpdateRequest
            );

            if (!statusResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось обновить статус заявки {Id}", request.ClientRequestId);
            }

            _logger.LogInformation($"Подбор завершён. Найдено {vehicles.Count} автомобилей");

            return Ok(new
            {
                message = "Подбор завершён",
                selectionId = selection.Id,
                found = vehicles.Count
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSelection(int id)
        {
            var selection = await _context.VehicleSelections.Include(vs => vs.Vehicles).FirstOrDefaultAsync(s => s.Id == id);

            if (selection == null)
            {
                return NotFound();
            }

            var dto = new VehicleSelectionDto
            {
                Id = selection.Id,
                ClientRequestId = selection.ClientRequestId,
                Vehicles = selection.Vehicles.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    Kilometrage = v.Kilometrage,
                    Price = v.Price,
                    Segment = v.Segment,
                    Source = v.Source,
                    VIN = v.VIN
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpGet("getVehicle/{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var vehicleDto = new VehicleGetterDto
            {
                Id = vehicle.Id,
                Price = vehicle.Price
            };
          
            return Ok(vehicleDto);
        }

    }


}
