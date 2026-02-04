using InspectionService.Data;
using InspectionService.DTO;
using InspectionService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;

namespace InspectionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public InspectionController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> StartTechincalInspection([FromBody] InspectionDTO inspection)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://client-service:80/");

            var legal = new LegalInspection
            {
                ClientRequestId = inspection.ClientRequestId,
                VehicleId = inspection.VehicleId,
                HasLien = inspection.Legal.HasLien,
                IsStolen = inspection.Legal.IsStolen,
                OwnedByLegalEntity = inspection.Legal.OwnedByLegalEntity,
                RegisteredInGIBDD = inspection.Legal.RegisteredInGIBDD,
                CompletedAt = DateTime.UtcNow
            };

            var technical = new TechnicalInspection
            {
                ClientRequestId = inspection.ClientRequestId,
                VehicleId = inspection.VehicleId,
                IsBodyDamaged = inspection.Technical.IsBodyDamaged,
                KilometrageVerified = inspection.Technical.KilometrageVerified,
                Recommendations = inspection.Technical.Recommendations,
                CompletedAt = DateTime.UtcNow
            };

            _context.LegalInspections.Add(legal);
            _context.TechnicalInspections.Add(technical);

            await _context.SaveChangesAsync();

            var isVerified = legal.HasLien == false && legal.IsStolen == false &&
                legal.OwnedByLegalEntity == false && legal.RegisteredInGIBDD == true &&
                technical.IsBodyDamaged == false && technical.KilometrageVerified == true;

            httpClient.DefaultRequestHeaders.Add("X-User-Role", "analyst");
            var status = isVerified ? RequestStatus.Verified : RequestStatus.Rejected;
            var statusUpdateRequest = new StatusUpdateDTO { Status = status };

            var response = await httpClient.PutAsJsonAsync($"api/clientrequests/{legal.ClientRequestId}/status", statusUpdateRequest);

            return Ok(new
            {
                message = "Проверка завершена",
                requestId = legal.ClientRequestId,
                vehicleId = legal.VehicleId,
                result = status.ToString()
            });
        }

        [HttpGet("{clientRequestId}")]
        public async Task<IActionResult> GetInspection(int clientRequestId)
        {
            var legal = await _context.LegalInspections.FirstOrDefaultAsync(x => x.ClientRequestId == clientRequestId);

            var technical = await _context.TechnicalInspections.FirstOrDefaultAsync(x => x.ClientRequestId == clientRequestId);

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
