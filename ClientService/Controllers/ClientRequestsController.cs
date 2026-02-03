using ClientService.Data;
using ClientService.DTO;
using ClientService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientRequestsController : ControllerBase
    {
    
        private readonly ApplicationDbContext _context;

        public ClientRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }
        private bool IsManager()
        {
            return Request.Headers["X-User-Role"].ToString() == "manager";
        }

        private bool IsAnalyst()
        {
            return Request.Headers["X-User-Role"].ToString() == "analyst";
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _context.ClientRequests.ToListAsync();
            if (!IsManager() && !IsAnalyst())
            {
                var dtos = requests.Select(request => new ClientRequestDTO
                {
                    Id = request.Id,
                    PreferredBand = request.PreferredBand,
                    MaxKilometrage = request.MaxKilometrage,
                    Budget = request.Budget,
                    YearOfManufacture = request.YearOfManufacture,
                    Segment = request.Segment,
                    Source = request.Source,
                    Status = request.Status
                }).ToList();
                return Ok(dtos);
            } 
            else
            {
                return Ok(requests);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var request = await _context.ClientRequests.FindAsync(id);
            if (request == null) return NotFound(); 
            if (!IsManager() && !IsAnalyst())
            {
                var criteria = new ClientRequestDTO
                {
                    Id = request.Id,
                    PreferredBand = request.PreferredBand,
                    MaxKilometrage = request.MaxKilometrage,
                    Budget = request.Budget,
                    YearOfManufacture = request.YearOfManufacture,
                    Segment = request.Segment,
                    Source = request.Source,
                    Status = request.Status
                };

                return Ok(criteria);
            } 
            else
            {
                return Ok(request);
            }
        }

        public string ValidateRequest(ClientRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return "Необходимо имя пользователя";
            }
            if (request.Budget <= 0)
            {
                return "Необходим бюджет";
            }
            if (request.YearOfManufacture < 1900 || request.YearOfManufacture > DateTime.Now.Year + 1)
            {
                return "Год выпуска вне допустимого диапазона";
            }
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return "Некорректный email";
            }
            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return "Необходим номер телефона";
            }

            return "";
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClientRequest request) 
        {
            if (!IsManager()) return StatusCode(StatusCodes.Status403Forbidden, "Требуется роль менеджера");
            var validation = ValidateRequest(request);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                return BadRequest(validation);
            }
            _context.ClientRequests.Add(request);

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = request.Id }, request);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDTO statusDTO)
        {
           if (!IsAnalyst() && !IsManager()) return StatusCode(StatusCodes.Status403Forbidden, "Требуется роль менеджера");
            var requestForStatusUpdate = await _context.ClientRequests.FindAsync(id);
            if (requestForStatusUpdate == null) { return NotFound(); }
            requestForStatusUpdate.Status = statusDTO.Status;
            await _context.SaveChangesAsync();
            return Ok(requestForStatusUpdate);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClientRequest clientRequest)
        {
            if (!IsManager()) return StatusCode(StatusCodes.Status403Forbidden, "Требуется роль менеджера");
            var requestForEdition = await _context.ClientRequests.FindAsync(id);

            if (requestForEdition == null) { return NotFound(); }

            if (requestForEdition.Status != RequestStatus.New)
            {
                return BadRequest("Нельзя редактировать заявку на этапе подбора!");
            }

            requestForEdition.FullName = clientRequest.FullName;
            requestForEdition.Phone = clientRequest.Phone;
            requestForEdition.Email = clientRequest.Email;
            requestForEdition.PreferredBand = clientRequest.PreferredBand;
            requestForEdition.MaxKilometrage = clientRequest.MaxKilometrage;
            requestForEdition.Budget = clientRequest.Budget;
            requestForEdition.YearOfManufacture = clientRequest.YearOfManufacture;
            requestForEdition.Segment = clientRequest.Segment;
            requestForEdition.Source = clientRequest.Source;

            await _context.SaveChangesAsync();
            return Ok(requestForEdition);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) 
        {
            if (!IsManager()) return StatusCode(StatusCodes.Status403Forbidden, "Требуется роль менеджера");
            var requestForDelete = await _context.ClientRequests.FindAsync(id);
            if (requestForDelete == null) { return NotFound(); }
      
            if (requestForDelete.Status != RequestStatus.New)
            {
                return BadRequest("Нельзя удалять заявку после начала этапа подбора!");
            }

            _context.ClientRequests.Remove(requestForDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
