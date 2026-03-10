using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransferenciasController : ControllerBase
    {
        private readonly ITransferenciasService _transferenciasService;

        public TransferenciasController(ITransferenciasService transferenciasService)
        {
            _transferenciasService = transferenciasService;
        }

        // GET: api/Transferencias
        [HttpGet]
        public async Task<ActionResult<List<TransferenciaDto>>> GetTransferencias()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transferencias = await _transferenciasService.GetTransferenciasAsync(userId!);

            return Ok(transferencias);
        }

        // POST: api/Transferencias
        [HttpPost]
        public async Task<ActionResult<TransferenciaDto>> PostTransferencia(TransferenciaCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var (result, error) = await _transferenciasService.CreateTransferenciaAsync(userId!, dto);

            if (error != null)
                return BadRequest(error);

            return CreatedAtAction(nameof(GetTransferencias), new { id = result!.Id }, result);
        }
    }
}
