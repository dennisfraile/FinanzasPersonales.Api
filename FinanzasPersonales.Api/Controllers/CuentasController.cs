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
    public class CuentasController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public CuentasController(FinanzasDbContext context)
        {
            _context = context;
        }

        // GET: api/Cuentas
        [HttpGet]
        public async Task<ActionResult<List<CuentaDto>>> GetCuentas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuentas = await _context.Cuentas
                .Where(c => c.UserId == userId && c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new CuentaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo.ToString(),
                    BalanceActual = c.BalanceActual,
                    BalanceInicial = c.BalanceInicial,
                    Moneda = c.Moneda,
                    Color = c.Color,
                    Icono = c.Icono,
                    Activa = c.Activa,
                    FechaCreacion = c.FechaCreacion
                })
                .ToListAsync();

            return Ok(cuentas);
        }

        // GET: api/Cuentas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaDto>> GetCuenta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuenta = await _context.Cuentas
                .Where(c => c.Id == id && c.UserId == userId)
                .Select(c => new CuentaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo.ToString(),
                    BalanceActual = c.BalanceActual,
                    BalanceInicial = c.BalanceInicial,
                    Moneda = c.Moneda,
                    Color = c.Color,
                    Icono = c.Icono,
                    Activa = c.Activa,
                    FechaCreacion = c.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (cuenta == null)
                return NotFound();

            return Ok(cuenta);
        }

        // POST: api/Cuentas
        [HttpPost]
        public async Task<ActionResult<CuentaDto>> PostCuenta(CuentaCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuenta = new Cuenta
            {
                UserId = userId!,
                Nombre = dto.Nombre,
                Tipo = Enum.Parse<TipoCuenta>(dto.Tipo),
                BalanceInicial = dto.BalanceInicial,
                BalanceActual = dto.BalanceInicial,  // Igual al inicial
                Moneda = dto.Moneda,
                Color = dto.Color,
                Icono = dto.Icono,
                Activa = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();

            var result = new CuentaDto
            {
                Id = cuenta.Id,
                Nombre = cuenta.Nombre,
                Tipo = cuenta.Tipo.ToString(),
                BalanceActual = cuenta.BalanceActual,
                BalanceInicial = cuenta.BalanceInicial,
                Moneda = cuenta.Moneda,
                Color = cuenta.Color,
                Icono = cuenta.Icono,
                Activa = cuenta.Activa,
                FechaCreacion = cuenta.FechaCreacion
            };

            return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.Id }, result);
        }

        // PUT: api/Cuentas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuenta(int id, CuentaUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cuenta == null)
                return NotFound();

            cuenta.Nombre = dto.Nombre;
            cuenta.BalanceActual = dto.BalanceActual;
            cuenta.Color = dto.Color;
            cuenta.Icono = dto.Icono;
            cuenta.Activa = dto.Activa;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cuentas/{id} (Soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuenta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cuenta == null)
                return NotFound();

            cuenta.Activa = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Cuentas/balance-total
        [HttpGet("balance-total")]
        public async Task<ActionResult<decimal>> GetBalanceTotal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var balanceTotal = await _context.Cuentas
                .Where(c => c.UserId == userId && c.Activa)
                .SumAsync(c => c.BalanceActual);

            return Ok(balanceTotal);
        }
    }
}
