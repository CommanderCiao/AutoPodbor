using InspectionService.Data;
using InspectionService.DTO;
using InspectionService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InspectionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public InspectionController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost("inspectionrequest")]
        public async Task<IActionResult> CreateInspectionRequest([FromBody] InspectionRequestDto dto)
        {
            var baseUrl = _config["VehicleSearchUrl"];
            var httpVehicleClient = _httpClientFactory.CreateClient();

            var createdInspections = new List<object>();

            foreach (var id in dto.VehicleIds)
            {
                var response = await httpVehicleClient.GetAsync($"{baseUrl}/api/VehicleSearch/getVehicleInfo/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound("Автомобиль не найден");
                }

                var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    });

                if (vehicle.Status != VehicleStatus.Available)
                {
                    return BadRequest($"Автомобиль недоступен для проверки: {vehicle.Status}");
                }

                var responseVehicle = await httpVehicleClient.PutAsJsonAsync($"{baseUrl}/api/vehiclesearch/statusUpdate/{id}", new VehicleStatusDto { Status = VehicleStatus.UnderCheck });

                var technicalInspection = new TechnicalInspection
                {
                    VehicleId = id,
                    ClientRequestId = dto.ClientRequestId,
                    Status = InspectionStatus.New
                };

                var legalInspection = new LegalInspection
                {
                    VehicleId = id,
                    ClientRequestId = dto.ClientRequestId,
                    Status = InspectionStatus.New
                };

                await _context.TechnicalInspections.AddAsync(technicalInspection);
                await _context.LegalInspections.AddAsync(legalInspection);
                await _context.SaveChangesAsync();

                createdInspections.Add(new
                {
                    vehicleId = id,
                    technicalInspectionId = technicalInspection.Id,
                    legalInspectionId = legalInspection.Id
                });
            }

            return Ok(new
            {
                Message = "Созданы заявки на тех. и юр. проверки для автомобиля",
                ClientRequestId = dto.ClientRequestId,
                Inspections = createdInspections
            });
        }

        [HttpPost("technicalinspection/{id}")]
        public async Task<IActionResult> StartTechincalInspection(int id, [FromBody] TechnicalInspectionDTO inspection)
        {
            var technical = await _context.TechnicalInspections.Where(x => x.Id == id && x.Status == InspectionStatus.New).FirstOrDefaultAsync();

            if (technical == null)
                return NotFound("Заявки на техническую проверку для данного авто не существует");

            technical.IsBodyDamaged = inspection.IsBodyDamaged;
            technical.KilometrageVerified = inspection.KilometrageVerified;
            technical.Recommendations = inspection.Recommendations;
            technical.CompletedAt = DateTime.UtcNow;
            technical.Status = InspectionStatus.Inspected;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Данные по технической проверке внесены",
                Id = id
            });
        }

        [HttpPut("technicalinspection/{id}")]
        public async Task<IActionResult> EditTechnicalInspection(int id, [FromBody] TechnicalInspectionDTO inspection)
        {
            var technical = await _context.TechnicalInspections.Where(x => x.Id == id && x.Status == InspectionStatus.Inspected).FirstOrDefaultAsync();

            if (technical == null)
                return NotFound("Заявки на техническую проверку для данного авто не существует");

            technical.IsBodyDamaged = inspection.IsBodyDamaged;
            technical.KilometrageVerified = inspection.KilometrageVerified;
            technical.Recommendations = inspection.Recommendations;
            technical.CompletedAt = DateTime.UtcNow;
            technical.Status = InspectionStatus.Inspected;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Данные по технической проверке изменены",
                Id = id
            });
        }


        [HttpPost("legalinspection/{id}")]
        public async Task<IActionResult> StartLegalInspection(int id, [FromBody] LegalInspectionDTO inspection)
        {
            var legal = await _context.LegalInspections.Where(x => x.Id == id && x.Status == InspectionStatus.New).FirstOrDefaultAsync();

            if (legal == null)
                return NotFound("Заявки на юридическую проверку для данного авто не существует");

            legal.IsStolen = inspection.IsStolen;
            legal.HasLien = inspection.HasLien;
            legal.RegisteredInGIBDD = inspection.RegisteredInGIBDD;
            legal.CompletedAt = DateTime.UtcNow;
            legal.Status = InspectionStatus.Inspected;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Данные по юридической проверке внесены",
                Id = id
            });
        }

        [HttpPut("legalinspection/{id}")]
        public async Task<IActionResult> EditLegalInspection(int id, [FromBody] LegalInspectionDTO inspection)
        {
            var legal = await _context.LegalInspections.Where(x => x.Id == id && x.Status == InspectionStatus.Inspected).FirstOrDefaultAsync();

            if (legal == null)
                return NotFound("Заявки на юридическую проверку для данного авто не существует");

            legal.IsStolen = inspection.IsStolen;
            legal.HasLien = inspection.HasLien;
            legal.RegisteredInGIBDD = inspection.RegisteredInGIBDD;
            legal.CompletedAt = DateTime.UtcNow;
            legal.Status = InspectionStatus.Inspected;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Данные по юридической проверке изменены",
                Id = id
            });
        }

        [HttpGet("alltechnicalinspections")]
        public async Task<IActionResult> GetAllTechInspections(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) { pageNumber = 1; }
            if (pageSize > 50) { pageSize = 50; }

            var technical = _context.TechnicalInspections.AsQueryable();

            if (!technical.Any())
                return NotFound("Нет задач на тенхическую проверку");

            var totalCount = await technical.CountAsync();

            var requests = await technical
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Items = requests,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpGet("alllegalinspections")]
        public async Task<IActionResult> GetAllLegalInspections(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) { pageNumber = 1; }
            if (pageSize > 50) { pageSize = 50; }

            var technical = _context.LegalInspections.AsQueryable();

            if (!technical.Any())
                return NotFound("Нет задач на юридическую проверку");

            var totalCount = await technical.CountAsync();

            var requests = await technical
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Items = requests,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetInspection(int id)
        {
            var legal = await _context.LegalInspections.FirstOrDefaultAsync(x => x.Id == id);

            var technical = await _context.TechnicalInspections.FirstOrDefaultAsync(x => x.Id == id);

            if (legal == null || technical == null)
            {
                return NotFound(new { message = "Проверка не существует" });
            }

            var inspection = new InspectionDTO
            {
                ClientRequestId = legal.ClientRequestId,
                VehicleId = legal.VehicleId,
                Technical = new TechnicalInspectionDTO
                {
                    IsBodyDamaged = technical.IsBodyDamaged,
                    KilometrageVerified = technical.KilometrageVerified,
                    Recommendations = technical.Recommendations
                },
                Legal = new LegalInspectionDTO
                {
                    HasLien = legal.HasLien,
                    IsStolen = legal.IsStolen,
                    OwnedByLegalEntity = legal.OwnedByLegalEntity,
                    RegisteredInGIBDD = legal.RegisteredInGIBDD
                }
            };
            return Ok(inspection);
        } 
    }
}
