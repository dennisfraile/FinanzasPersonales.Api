using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/gastos-compartidos")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class GastosCompartidosController : ControllerBase
    {
        private readonly IGastosCompartidosService _service;
        private readonly FinanzasDbContext _context;

        public GastosCompartidosController(IGastosCompartidosService service, FinanzasDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GastoCompartidoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GastoCompartidoDto>>> GetGastosCompartidos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await _service.GetGastosCompartidosAsync(userId!));
        }

        [HttpGet("resumen")]
        [ProducesResponseType(typeof(ResumenSplitDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenSplitDto>> GetResumen()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await _service.GetResumenAsync(userId!));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GastoCompartidoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GastoCompartidoDto>> GetGastoCompartido(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _service.GetGastoCompartidoAsync(userId!, id);
            return gasto == null ? NotFound() : Ok(gasto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GastoCompartidoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GastoCompartidoDto>> CreateGastoCompartido(CreateGastoCompartidoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var gasto = await _service.CreateGastoCompartidoAsync(userId!, dto);
                return CreatedAtAction(nameof(GetGastoCompartido), new { id = gasto.Id }, gasto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGastoCompartido(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.DeleteGastoCompartidoAsync(userId!, id);
            return result ? NoContent() : NotFound();
        }

        [HttpPut("{gastoId}/participantes/{participanteId}/liquidar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LiquidarParticipante(int gastoId, int participanteId, LiquidarParticipanteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.LiquidarParticipanteAsync(userId!, gastoId, participanteId, dto);
            return result ? NoContent() : NotFound();
        }

        [HttpPost("{id}/compartir")]
        public async Task<ActionResult> GenerarLinkCompartido(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _context.GastosCompartidos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (gasto == null) return NotFound();

            // Generate unique token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_").Replace("+", "-").TrimEnd('=');

            var compartidoToken = new GastoCompartidoToken
            {
                GastoCompartidoId = id,
                Token = token,
                FechaExpiracion = DateTime.UtcNow.AddDays(30),
                UserId = userId!
            };

            _context.GastosCompartidosTokens.Add(compartidoToken);
            await _context.SaveChangesAsync();

            return Ok(new { token, expira = compartidoToken.FechaExpiracion });
        }

        [HttpGet("shared/{token}")]
        [AllowAnonymous]
        public async Task<ActionResult> VerCompartido(string token)
        {
            var compartidoToken = await _context.GastosCompartidosTokens
                .Include(t => t.GastoCompartido)
                    .ThenInclude(g => g!.Participantes)
                .Include(t => t.GastoCompartido)
                    .ThenInclude(g => g!.Categoria)
                .FirstOrDefaultAsync(t => t.Token == token && t.Activo && t.FechaExpiracion > DateTime.UtcNow);

            if (compartidoToken == null)
                return NotFound(new { error = "Link no válido o expirado" });

            var gasto = compartidoToken.GastoCompartido!;

            return Ok(new
            {
                descripcion = gasto.Descripcion,
                montoTotal = gasto.MontoTotal,
                fecha = gasto.Fecha,
                metodoDivision = gasto.MetodoDivision,
                categoria = gasto.Categoria?.Nombre,
                participantes = gasto.Participantes.Select(p => new
                {
                    nombre = p.Nombre,
                    montoAsignado = p.MontoAsignado,
                    montoPagado = p.MontoPagado,
                    liquidado = p.Liquidado
                })
            });
        }

        [HttpDelete("{id}/compartir")]
        public async Task<ActionResult> RevocarLinksCompartidos(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokens = await _context.GastosCompartidosTokens
                .Where(t => t.GastoCompartidoId == id && t.UserId == userId && t.Activo)
                .ToListAsync();

            foreach (var t in tokens)
                t.Activo = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
