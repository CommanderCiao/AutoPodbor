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

        [HttpPost("drafts/save")]
        public async Task<IActionResult> SaveDraft(ClientRequestDTO dto)
        {
            var existingDraft = await _context.ClientRequests
                .FirstOrDefaultAsync(x => x.ClientId == dto.ClientId && x.Status == RequestStatus.Draft);
            string message = "";
            int clientRequestId = 0;
            if (existingDraft != null)
            {
               existingDraft.FullName = dto.FullName;
                existingDraft.ClientId = dto.ClientId;
                existingDraft.Status = RequestStatus.Draft;
                existingDraft.Phone = dto.Phone;
                existingDraft.Email = dto.Email;
                existingDraft.PreferredBrand = dto.PreferredBrand;
                existingDraft.MaxKilometrage = dto.MaxKilometrage;
                existingDraft.Budget = dto.Budget;
                existingDraft.YearOfManufacture = dto.YearOfManufacture;
                existingDraft.Segment = dto.Segment;
                existingDraft.Source = dto.Source;
                existingDraft.UpdatedAt = DateTime.UtcNow;

                clientRequestId = existingDraft.Id;
                message = "Черновик заявки обновлен";
            }
            else
            {
                var newDraft = new ClientRequest
                {
                    ClientId = dto.ClientId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Status = RequestStatus.Draft,
                    PreferredBrand = dto.PreferredBrand,
                    MaxKilometrage = dto.MaxKilometrage,
                    Budget = dto.Budget,
                    YearOfManufacture = dto.YearOfManufacture,
                    Segment = dto.Segment,
                    Source = dto.Source,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                    
                };
                message = "Черновик заявки создан";
                clientRequestId = newDraft.Id;

                _context.ClientRequests.Add(newDraft);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ClientRequestId = clientRequestId,
                Message = message
            });
        }

        [HttpGet("drafts/getlatest/{clientRequestId}")]
        public async Task<IActionResult> GetLatestDraft(int clientRequestId)
        {
            var latestDraft = await _context.ClientRequests
                .Where(x => x.Id == clientRequestId && x.Status == RequestStatus.Draft)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (latestDraft == null)
            {
                return NotFound($"Черновик не найден");
            }

            var clientRequest = new ClientRequest
            {
                Id = latestDraft.Id,
                ClientId = latestDraft.ClientId,
                FullName = latestDraft.FullName,
                Phone = latestDraft.Phone,
                Email = latestDraft.Email,
                PreferredBrand = latestDraft.PreferredBrand,
                MaxKilometrage = latestDraft.MaxKilometrage,
                Budget = latestDraft.Budget,
                YearOfManufacture = latestDraft.YearOfManufacture,
                Segment = latestDraft.Segment,
                Source = latestDraft.Source,
                Status = latestDraft.Status,
            };

            return Ok(clientRequest);
        }

        [HttpPost("requests/submit/{clientRequestId}")]
        public async Task<IActionResult> CreateRequest(int clientRequestId)
        {
            var draft = await _context.ClientRequests.FirstOrDefaultAsync(x => x.Id == clientRequestId && x.Status == RequestStatus.Draft);

            if (draft == null) { return NotFound($"Черновик не найден"); }

            var validation = ValidateRequest(draft);

            if (!string.IsNullOrWhiteSpace(validation))
            {
                return BadRequest(validation);
            }

            draft.Status = RequestStatus.New;
            draft.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Id = draft.Id,
                Message = "Заявка сохранена"
            });
        }

        [HttpGet("requests/{clientRequestId}")]
        public async Task<IActionResult> Get(int clientRequestId)
        {
            var request = await _context.ClientRequests.FirstOrDefaultAsync(x => x.Id == clientRequestId && x.Status == RequestStatus.New);

            if (request == null) return NotFound($"Заявка с номером {clientRequestId} не найдена");

            var clientRequest = new ClientRequest
            {
                Id = request.Id,
                ClientId = request.ClientId,
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email,
                PreferredBrand = request.PreferredBrand,
                MaxKilometrage = request.MaxKilometrage,
                Budget = request.Budget,
                YearOfManufacture = request.YearOfManufacture,
                Segment = request.Segment,
                Source = request.Source,
                Status = request.Status
            };

            return Ok(clientRequest);
        }

        [HttpGet("requests/getall")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RequestStatus? status = null,
            [FromQuery] int? clientId = null)
        {
            if (pageNumber < 1) { pageNumber = 1; }
            if (pageSize > 50) { pageSize = 50; }

            var query = _context.ClientRequests.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            if (clientId.HasValue)
            {
                query = query.Where(q => q.ClientId == clientId);
            }

            var totalCount = await query.CountAsync();

            var requests = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var clientRequests = requests.Select(request => new ClientRequest
            {   
                Id = request.Id,
                ClientId = request.ClientId,
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email,
                PreferredBrand = request.PreferredBrand,
                MaxKilometrage = request.MaxKilometrage,
                Budget = request.Budget,
                YearOfManufacture = request.YearOfManufacture,
                Segment = request.Segment,
                Source = request.Source,
                Status = request.Status
            }).ToList();

            return Ok(new {
                Items = clientRequests,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpPut("requests/{clientRequestId}")]
        public async Task<IActionResult> Update(int clientRequestId, [FromBody] ClientRequest clientRequest)
        {
            var requestForEdition = await _context.ClientRequests.FirstOrDefaultAsync(x => x.Id == clientRequestId && x.Status == RequestStatus.New);

            if (requestForEdition == null) { return NotFound($"Заявка с номером {clientRequestId} не найдена"); }

            requestForEdition.FullName = clientRequest.FullName;
            requestForEdition.Phone = clientRequest.Phone;
            requestForEdition.Email = clientRequest.Email;
            requestForEdition.PreferredBrand = clientRequest.PreferredBrand;
            requestForEdition.MaxKilometrage = clientRequest.MaxKilometrage;
            requestForEdition.Budget = clientRequest.Budget;
            requestForEdition.YearOfManufacture = clientRequest.YearOfManufacture;
            requestForEdition.Segment = clientRequest.Segment;
            requestForEdition.Source = clientRequest.Source;
            requestForEdition.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(requestForEdition);
        }

        [HttpDelete("requests/{clientRequestId}")]
        public async Task<IActionResult> Delete(int clientRequestId)
        {
            var requestForDelete = await _context.ClientRequests.FirstOrDefaultAsync(x => x.Id == clientRequestId && x.Status == RequestStatus.New);

            if (requestForDelete == null) { return NotFound($"Заявка с номером {clientRequestId} не найдена"); }

            requestForDelete.Status = RequestStatus.Closed;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("requests/{clientRequestId}/status")]
        public async Task<IActionResult> UpdateStatus(int clientRequestId, [FromBody] StatusUpdateDTO statusDTO)
        {
            var requestForStatusUpdate = await _context.ClientRequests.FirstOrDefaultAsync(x => x.Id == clientRequestId);

            if (requestForStatusUpdate == null) { return NotFound($"Заявка с номером {clientRequestId} не найдена"); }

            requestForStatusUpdate.Status = statusDTO.Status;
            requestForStatusUpdate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(requestForStatusUpdate);
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
    }
}
