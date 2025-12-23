using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de gastos recurrentes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GastosRecurrentesController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public GastosRecurrentesController(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los gastos recurrentes del usuario
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GastoRecurrenteDto>>> GetGastosRecurrentes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrentes = await _context.GastosRecurrentes
                .Include(gr => gr.Categoria)
                .Include(gr => gr.Cuenta)
                .Where(gr => gr.UserId == userId)
                .OrderBy(gr => gr.ProximaFecha)
                .Select(gr => new GastoRecurrenteDto
                {
                    Id = gr.Id,
                    Descripcion = gr.Descripcion,
                    CategoriaId = gr.CategoriaId,
                    CategoriaNombre = gr.Categoria != null ? gr.Categoria.Nombre : null,
                    Monto = gr.Monto,
                    CuentaId = gr.CuentaId,
                    CuentaNombre = gr.Cuenta != null ? gr.Cuenta.Nombre : null,
                    Frecuencia = gr.Frecuencia,
                    DiaDePago = gr.DiaDePago,
                    ProximaFecha = gr.ProximaFecha,
                    UltimaGeneracion = gr.UltimaGeneracion,
                    Activo = gr.Activo,
                    FechaCreacion = gr.FechaCreacion
                })
                .ToListAsync();

            return Ok(recurrentes);
        }

        /// <summary>
        /// Obtiene un gasto recurrente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GastoRecurrenteDto>> GetGastoRecurrente(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrente = await _context.GastosRecurrentes
                .Include(gr => gr.Categoria)
                .Include(gr => gr.Cuenta)
                .FirstOrDefaultAsync(gr => gr.Id == id && gr.UserId == userId);

            if (recurrente == null)
                return NotFound();

            var dto = new GastoRecurrenteDto
            {
                Id = recurrente.Id,
                Descripcion = recurrente.Descripcion,
                CategoriaId = recurrente.CategoriaId,
                CategoriaNombre = recurrente.Categoria?.Nombre,
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                CuentaNombre = recurrente.Cuenta?.Nombre,
                Frecuencia = recurrente.Frecuencia,
                DiaDePago = recurrente.DiaDePago,
                ProximaFecha = recurrente.ProximaFecha,
                UltimaGeneracion = recurrente.UltimaGeneracion,
                Activo = recurrente.Activo,
                FechaCreacion = recurrente.FechaCreacion
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo gasto recurrente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GastoRecurrenteDto>> PostGastoRecurrente(CreateGastoRecurrenteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Validar categoría
            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return BadRequest("Categoría no válida");

            // Validar cuenta si se especifica
            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return BadRequest("Cuenta no válida");
            }

            // Calcular próxima fecha
            var proximaFecha = CalcularProximaFecha(dto.Frecuencia, dto.DiaDePago);

            var recurrente = new GastoRecurrente
            {
                UserId = userId,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                Monto = dto.Monto,
                CuentaId = dto.CuentaId,
                Frecuencia = dto.Frecuencia,
                DiaDePago = dto.DiaDePago,
                ProximaFecha = proximaFecha,
                Activo = true
            };

            _context.GastosRecurrentes.Add(recurrente);
            await _context.SaveChangesAsync();

            // Cargar datos relacionados
            await _context.Entry(recurrente).Reference(gr => gr.Categoria).LoadAsync();
            if (recurrente.CuentaId.HasValue)
                await _context.Entry(recurrente).Reference(gr => gr.Cuenta).LoadAsync();

            var result = new GastoRecurrenteDto
            {
                Id = recurrente.Id,
                Descripcion = recurrente.Descripcion,
                CategoriaId = recurrente.CategoriaId,
                CategoriaNombre = recurrente.Categoria?.Nombre,
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                CuentaNombre = recurrente.Cuenta?.Nombre,
                Frecuencia = recurrente.Frecuencia,
                DiaDePago = recurrente.DiaDePago,
                ProximaFecha = recurrente.ProximaFecha,
                UltimaGeneracion = recurrente.UltimaGeneracion,
                Activo = recurrente.Activo,
                FechaCreacion = recurrente.FechaCreacion
            };

            return CreatedAtAction(nameof(GetGastoRecurrente), new { id = recurrente.Id }, result);
        }

        /// <summary>
        /// Actualiza un gasto recurrente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGastoRecurrente(int id, UpdateGastoRecurrenteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrente = await _context.GastosRecurrentes.FindAsync(id);
            if (recurrente == null || recurrente.UserId != userId)
                return NotFound();

            // Validar categoría
            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return BadRequest("Categoría no válida");

            // Validar cuenta si se especifica
            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return BadRequest("Cuenta no válida");
            }

            recurrente.Descripcion = dto.Descripcion;
            recurrente.CategoriaId = dto.CategoriaId;
            recurrente.Monto = dto.Monto;
            recurrente.CuentaId = dto.CuentaId;
            recurrente.Frecuencia = dto.Frecuencia;
            recurrente.DiaDePago = dto.DiaDePago;
            recurrente.Activo = dto.Activo;

            // Recalcular próxima fecha si cambió frecuencia o día
            recurrente.ProximaFecha = CalcularProximaFecha(dto.Frecuencia, dto.DiaDePago);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Elimina un gasto recurrente
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGastoRecurrente(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrente = await _context.GastosRecurrentes.FindAsync(id);
            if (recurrente == null || recurrente.UserId != userId)
                return NotFound();

            _context.GastosRecurrentes.Remove(recurrente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Genera un gasto ahora desde un recurrente específico
        /// </summary>
        [HttpPost("{id}/generar")]
        public async Task<ActionResult<Models.Gasto>> GenerarGasto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrente = await _context.GastosRecurrentes
                .Include(gr => gr.Cuenta)
                .FirstOrDefaultAsync(gr => gr.Id == id && gr.UserId == userId);

            if (recurrente == null)
                return NotFound();

            if (!recurrente.Activo)
                return BadRequest("El gasto recurrente está inactivo");

            // Generar el gasto
            var gasto = new Models.Gasto
            {
                UserId = userId,
                CategoriaId = recurrente.CategoriaId,
                Descripcion = recurrente.Descripcion + " (Recurrente)",
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                Fecha = DateTime.UtcNow,
                Tipo = "Fijo"
            };

            _context.Gastos.Add(gasto);

            // Actualizar balance si tiene cuenta
            if (recurrente.CuentaId.HasValue && recurrente.Cuenta != null)
            {
                recurrente.Cuenta.BalanceActual -= recurrente.Monto;
            }

            // Actualizar fechas del recurrente
            recurrente.UltimaGeneracion = DateTime.UtcNow;
            recurrente.ProximaFecha = CalcularProximaFechaDesde(recurrente.ProximaFecha, recurrente.Frecuencia);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGasto", "Gastos", new { id = gasto.Id }, gasto);
        }

        /// <summary>
        /// Genera todos los gastos recurrentes pendientes (para cron job)
        /// </summary>
        [HttpPost("generar-pendientes")]
        public async Task<ActionResult<object>> GenerarPendientes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var pendientes = await _context.GastosRecurrentes
                .Include(gr => gr.Cuenta)
                .Where(gr => gr.UserId == userId && gr.Activo && gr.ProximaFecha <= DateTime.UtcNow)
                .ToListAsync();

            int generados = 0;

            foreach (var recurrente in pendientes)
            {
                var gasto = new Models.Gasto
                {
                    UserId = userId,
                    CategoriaId = recurrente.CategoriaId,
                    Descripcion = recurrente.Descripcion + " (Recurrente)",
                    Monto = recurrente.Monto,
                    CuentaId = recurrente.CuentaId,
                    Fecha = recurrente.ProximaFecha,
                    Tipo = "Fijo"
                };

                _context.Gastos.Add(gasto);

                // Actualizar balance
                if (recurrente.CuentaId.HasValue && recurrente.Cuenta != null)
                {
                    recurrente.Cuenta.BalanceActual -= recurrente.Monto;
                }

                // Actualizar fechas
                recurrente.UltimaGeneracion = DateTime.UtcNow;
                recurrente.ProximaFecha = CalcularProximaFechaDesde(recurrente.ProximaFecha, recurrente.Frecuencia);

                generados++;
            }

            await _context.SaveChangesAsync();

            return Ok(new { generados, mensaje = $"Se generaron {generados} gastos recurrentes" });
        }

        private DateTime CalcularProximaFecha(string frecuencia, int dia)
        {
            var ahora = DateTime.UtcNow;

            return frecuencia switch
            {
                "Semanal" => ahora.AddDays((7 + (dia - (int)ahora.DayOfWeek)) % 7),
                "Quincenal" => new DateTime(ahora.Year, ahora.Month, Math.Min(dia, DateTime.DaysInMonth(ahora.Year, ahora.Month))).AddDays(15),
                "Mensual" => new DateTime(ahora.Year, ahora.Month, Math.Min(dia, DateTime.DaysInMonth(ahora.Year, ahora.Month))).AddMonths(1),
                "Anual" => new DateTime(ahora.Year, ahora.Month, Math.Min(dia, DateTime.DaysInMonth(ahora.Year, ahora.Month))).AddYears(1),
                _ => ahora.AddMonths(1)
            };
        }

        private DateTime CalcularProximaFechaDesde(DateTime desde, string frecuencia)
        {
            return frecuencia switch
            {
                "Semanal" => desde.AddDays(7),
                "Quincenal" => desde.AddDays(15),
                "Mensual" => desde.AddMonths(1),
                "Anual" => desde.AddYears(1),
                _ => desde.AddMonths(1)
            };
        }
    }
}
