using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransferenciasController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public TransferenciasController(FinanzasDbContext context)
        {
            _context = context;
        }

        // GET: api/Transferencias
        [HttpGet]
        public async Task<ActionResult<List<TransferenciaDto>>> GetTransferencias()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transferencias = await _context.Transferencias
                .Where(t => t.UserId == userId)
                .Include(t => t.CuentaOrigen)
                .Include(t => t.CuentaDestino)
                .OrderByDescending(t => t.Fecha)
                .Select(t => new TransferenciaDto
                {
                    Id = t.Id,
                    CuentaOrigenId = t.CuentaOrigenId,
                    CuentaOrigenNombre = t.CuentaOrigen!.Nombre,
                    CuentaDestinoId = t.CuentaDestinoId,
                    CuentaDestinoNombre = t.CuentaDestino!.Nombre,
                    Monto = t.Monto,
                    Fecha = t.Fecha,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();

            return Ok(transferencias);
        }

        // POST: api/Transferencias
        [HttpPost]
        public async Task<ActionResult<TransferenciaDto>> PostTransferencia(TransferenciaCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validar que las cuentas existan y pertenezcan al usuario
            var cuentaOrigen = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == dto.CuentaOrigenId && c.UserId == userId);

            var cuentaDestino = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == dto.CuentaDestinoId && c.UserId == userId);

            if (cuentaOrigen == null || cuentaDestino == null)
                return BadRequest("Cuentas no encontradas");

            if (dto.CuentaOrigenId == dto.CuentaDestinoId)
                return BadRequest("No puedes transferir a la misma cuenta");

            if (dto.Monto <= 0)
                return BadRequest("El monto debe ser mayor a 0");

            // Validar balance suficiente (opcional, depende de si permites saldo negativo)
            // if (cuentaOrigen.BalanceActual < dto.Monto)
            //     return BadRequest("Balance insuficiente");

            // Crear transferencia
            var transferencia = new Transferencia
            {
                UserId = userId!,
                CuentaOrigenId = dto.CuentaOrigenId,
                CuentaDestinoId = dto.CuentaDestinoId,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };

            // Actualizar balances
            cuentaOrigen.BalanceActual -= dto.Monto;
            cuentaDestino.BalanceActual += dto.Monto;

            _context.Transferencias.Add(transferencia);
            await _context.SaveChangesAsync();

            var result = new TransferenciaDto
            {
                Id = transferencia.Id,
                CuentaOrigenId = transferencia.CuentaOrigenId,
                CuentaOrigenNombre = cuentaOrigen.Nombre,
                CuentaDestinoId = transferencia.CuentaDestinoId,
                CuentaDestinoNombre = cuentaDestino.Nombre,
                Monto = transferencia.Monto,
                Fecha = transferencia.Fecha,
                Descripcion = transferencia.Descripcion
            };

            return CreatedAtAction(nameof(GetTransferencias), new { id = transferencia.Id }, result);
        }
    }
}
