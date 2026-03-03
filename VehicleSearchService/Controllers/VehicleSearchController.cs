using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using VehicleSearchService.Data;
using VehicleSearchService.DTO;
using VehicleSearchService.Models;
using static System.Collections.Specialized.BitVector32;

namespace VehicleSearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleSearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<VehicleSearchController> _logger;
        private readonly IConfiguration _config;

        public VehicleSearchController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<VehicleSearchController> logger, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config;
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> StartSelection(int id)
        {
            _logger.LogInformation($"Запущен подбор для заявки {id}");
            var httpClient = _httpClientFactory.CreateClient();
            var baseUrl = _config["ClientServiceUrl"];

            if (await _context.VehicleSelections.AnyAsync(vs => vs.ClientRequestId == id))
            {
                _logger.LogInformation("Подбор по данной заявке ранее был сформирован");
                return Conflict("Подбор по данной заявке ранее был сформирован");
            }


            var response = await httpClient.GetAsync($"{baseUrl}/api/clientrequests/requests/{id}");
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
                _logger.LogInformation("Статус заявки не соответствует подбору");
                return BadRequest($"Статус заявки не соответствует подбору: {clientCriteria.Status}");
            }
          
            var vehicles = await _context.Vehicles
            .Where(v =>
            v.Price <= clientCriteria.Budget &&
            v.Year >= clientCriteria.YearOfManufacture &&
            (!clientCriteria.MaxKilometrage.HasValue || v.Kilometrage <= clientCriteria.MaxKilometrage.Value) &&
            (string.IsNullOrEmpty(clientCriteria.PreferredBrand) || v.Brand == clientCriteria.PreferredBrand) &&
            (clientCriteria.Segment == 0 || v.Segment == clientCriteria.Segment) &&
            (clientCriteria.Source == 0 || v.Source == clientCriteria.Source) &&
            (v.Status == VehicleStatus.Available)
            )
            .ToListAsync();

            var selection = new VehicleSelection
            {
                ClientRequestId = id,
                Vehicles = vehicles
            };

            _context.VehicleSelections.Add(selection);
            await _context.SaveChangesAsync();

            var statusUpdateRequest = new StatusUpdateDTO { Status = RequestStatus.InReview };

            var statusResponse = await httpClient.PutAsJsonAsync(
                $"{baseUrl}/api/clientrequests/requests/{id}/status",
                statusUpdateRequest
            );

            if (!statusResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Не удалось обновить статус заявки {id}");
            }

            _logger.LogInformation($"Подбор #{selection.Id} завершён. Найдено {vehicles.Count} автомобилей");

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
                return NotFound($"Подбор с {id} не найден");
            }

            var vehicleSelection = new
            {
                Id = selection.Id,
                ClientRequestId = selection.ClientRequestId,
                Vehicles = selection.Vehicles.Select(v => new VehicleCard
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Kilometrage = v.Kilometrage,
                    Price = v.Price,
                    Status = v.Status
                }).ToList()
            };
            return Ok(vehicleSelection);
        }

        [HttpPut("statusUpdate/{vehicleId}")]
        public async Task<IActionResult> AddVehicle(int vehicleId, [FromBody] VehicleStatusDto status)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            if (vehicle == null)
                return NotFound($"Автомобиль с номером {vehicleId} не найден");

            vehicle.Status = status.Status;

            return Ok(new
            {
                Message = "Статус успешно обновлен",
                Status = status.Status,
                VehicleId = vehicleId
            });
        }

        [HttpPost("{id}/vehicles/{vehicleId}/attach")]
        public async Task<IActionResult> AddVehicle(int id, int vehicleId)
        {
            var selection = await _context.VehicleSelections
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (selection == null)
                return NotFound($"Подборка с номером {id} не найдена");

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            if (vehicle == null)
                return NotFound($"Автомобиль с номером {vehicleId} не найден");

            if (selection.Vehicles.Any(s => s.Id == vehicleId))
                return NotFound($"Данный автомобиль с #{vehicleId} уже добавлен в список");

            selection.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Id = id,
                Message = $"Добавлен новый авто в подбор",
                Vehicle = vehicle
            });
        }

        [HttpDelete("{id}/vehicles/{vehicleId}/remove")]
        public async Task<IActionResult> RemoveVehicle(int id, int vehicleId)
        {
            var selection = await _context.VehicleSelections
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (selection == null)
                return NotFound($"Подборка с номером {id} не найдена");

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(s => s.Id == vehicleId);

            if (vehicle == null)
              return NotFound($"Автомобиль с номером {vehicleId} не найден");

            if (selection.Vehicles.All(s => s.Id != vehicleId))
                return BadRequest($"Данный автомобиль с #{vehicleId} уже отсутствует в списке");

            selection.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Id = id,
                Message = "Из подбора удален автомобиль"
            });    
        }

        [HttpGet("getVehicleInfo/{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return NotFound($"Автомобиль с данным id {id} не найден");
            }
          
            return Ok(vehicle);
        }

        [HttpGet("getVehiclesCards")]
        public async Task<IActionResult> GetAllVehicles([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int budget = 1000000000,
            [FromQuery] int yearOfManufacture = 1900,
            [FromQuery] int maxKilometrage = 10000000,
            [FromQuery] string? preferredBrand = null,
            [FromQuery] Segment segment = 0,
            [FromQuery] SourceOfPurchase source = 0)
        {

            if (pageNumber < 1) { pageNumber = 1; }
            if (pageSize > 50) { pageSize = 50; }

            var vehicles = await _context.Vehicles
                .Where(v =>
                v.Price <= budget &&
                v.Year >= yearOfManufacture &&
                (v.Kilometrage <= maxKilometrage) &&
                (string.IsNullOrEmpty(preferredBrand) || v.Brand == preferredBrand) &&
                (segment == 0 || v.Segment == segment) &&
                (source == 0 || v.Source == source))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Vehicles.CountAsync(v =>
                v.Price <= budget &&
                v.Year >= yearOfManufacture &&
                (v.Kilometrage <= maxKilometrage) &&
                (string.IsNullOrEmpty(preferredBrand) || v.Brand == preferredBrand) &&
                (segment == 0 || v.Segment == segment) &&
                (source == 0 || v.Source == source));

            var dto = new 
            {
                Items = vehicles.Select(v => new VehicleCard
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Price = v.Price,
                    Kilometrage = v.Kilometrage,
                    Status = v.Status
                }).ToList(),
                TotalCount = totalCount, 
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
            return Ok(dto);
        }
    }
}
