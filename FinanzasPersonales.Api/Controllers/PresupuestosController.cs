using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de presupuestos por categoría.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PresupuestosController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public PresupuestosController(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los presupuestos del usuario con información de progreso.
        /// </summary>
        /// <param name="mes">Filtro opcional por mes (1-12)</param>
        /// <param name="ano">Filtro opcional por año</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<PresupuestoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PresupuestoDto>>> GetPresupuestos(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Build the query
            IQueryable<Presupuesto> query = _context.Presupuestos
                .Where(p => p.UserId == userId);

            if (mes.HasValue)
                query = query.Where(p => p.MesAplicable == mes.Value);

            if (ano.HasValue)
                query = query.Where(p => p.AnoAplicable == ano.Value);

            var presupuestos = await query.Include(p => p.Categoria).ToListAsync();

            var resultado = new List<PresupuestoDto>();

            foreach (var presupuesto in presupuestos)
            {
                var gastadoActual = await CalcularGastadoActual(userId, presupuesto);

                resultado.Add(new PresupuestoDto
                {
                    Id = presupuesto.Id,
                    CategoriaId = presupuesto.CategoriaId,
                    CategoriaNombre = presupuesto.Categoria!.Nombre,
                    MontoLimite = presupuesto.MontoLimite,
                    Periodo = presupuesto.Periodo,
                    MesAplicable = presupuesto.MesAplicable,
                    AnoAplicable = presupuesto.AnoAplicable,
                    GastadoActual = gastadoActual,
                    Disponible = presupuesto.MontoLimite - gastadoActual,
                    PorcentajeUtilizado = presupuesto.MontoLimite > 0
                        ? (gastadoActual / presupuesto.MontoLimite) * 100
                        : 0
                });
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene un presupuesto específico por ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PresupuestoDto>> GetPresupuesto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var presupuesto = await _context.Presupuestos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return NotFound();

            var gastadoActual = await CalcularGastadoActual(userId, presupuesto);

            var resultado = new PresupuestoDto
            {
                Id = presupuesto.Id,
                CategoriaId = presupuesto.CategoriaId,
                CategoriaNombre = presupuesto.Categoria!.Nombre,
                MontoLimite = presupuesto.MontoLimite,
                Periodo = presupuesto.Periodo,
                MesAplicable = presupuesto.MesAplicable,
                AnoAplicable = presupuesto.AnoAplicable,
                GastadoActual = gastadoActual,
                Disponible = presupuesto.MontoLimite - gastadoActual,
                PorcentajeUtilizado = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100
                    : 0
            };

            return Ok(resultado);
        }

        /// <summary>
        /// Crea un nuevo presupuesto.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PresupuestoDto>> PostPresupuesto(PresupuestoCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validar que la categoría existe y pertenece al usuario
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);

            if (!categoriaExiste)
                return BadRequest("La categoría no existe o no pertenece al usuario.");

            // Validar que no exista un presupuesto para la misma categoría, mes y año
            var presupuestoExiste = await _context.Presupuestos
                .AnyAsync(p => p.UserId == userId &&
                              p.CategoriaId == dto.CategoriaId &&
                              p.MesAplicable == dto.MesAplicable &&
                              p.AnoAplicable == dto.AnoAplicable &&
                              p.Periodo == dto.Periodo);

            if (presupuestoExiste)
                return BadRequest("Ya existe un presupuesto para esta categoría en el período especificado.");

            var presupuesto = new Presupuesto
            {
                CategoriaId = dto.CategoriaId,
                MontoLimite = dto.MontoLimite,
                Periodo = dto.Periodo,
                MesAplicable = dto.MesAplicable,
                AnoAplicable = dto.AnoAplicable,
                UserId = userId
            };

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPresupuesto), new { id = presupuesto.Id },
                await GetPresupuesto(presupuesto.Id));
        }

        /// <summary>
        /// Actualiza un presupuesto existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPresupuesto(int id, PresupuestoUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return NotFound();

            presupuesto.MontoLimite = dto.MontoLimite;
            presupuesto.Periodo = dto.Periodo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Presupuestos.AnyAsync(p => p.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un presupuesto.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePresupuesto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return NotFound();

            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Obtiene presupuestos que están cerca del límite (>80%).
        /// </summary>
        [HttpGet("alertas")]
        [ProducesResponseType(typeof(List<PresupuestoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PresupuestoDto>>> GetAlertasPresupuesto()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mesActual = DateTime.Now.Month;
            var anoActual = DateTime.Now.Year;

            var presupuestos = await _context.Presupuestos
                .Where(p => p.UserId == userId &&
                           p.MesAplicable == mesActual &&
                           p.AnoAplicable == anoActual)
                .Include(p => p.Categoria)
                .ToListAsync();

            var resultado = new List<PresupuestoDto>();

            foreach (var presupuesto in presupuestos)
            {
                var gastadoActual = await CalcularGastadoActual(userId, presupuesto);
                var porcentaje = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100
                    : 0;

                // Solo incluir si está por encima del 80%
                if (porcentaje >= 80)
                {
                    resultado.Add(new PresupuestoDto
                    {
                        Id = presupuesto.Id,
                        CategoriaId = presupuesto.CategoriaId,
                        CategoriaNombre = presupuesto.Categoria!.Nombre,
                        MontoLimite = presupuesto.MontoLimite,
                        Periodo = presupuesto.Periodo,
                        MesAplicable = presupuesto.MesAplicable,
                        AnoAplicable = presupuesto.AnoAplicable,
                        GastadoActual = gastadoActual,
                        Disponible = presupuesto.MontoLimite - gastadoActual,
                        PorcentajeUtilizado = porcentaje
                    });
                }
            }

            return Ok(resultado.OrderByDescending(p => p.PorcentajeUtilizado).ToList());
        }

        // Método privado para calcular lo gastado en el período del presupuesto
        private async Task<decimal> CalcularGastadoActual(string userId, Presupuesto presupuesto)
        {
            DateTime inicio, fin;

            if (presupuesto.Periodo == "Mensual")
            {
                inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                fin = inicio.AddMonths(1).AddDays(-1);
            }
            else // Quincenal
            {
                inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                fin = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 15);
            }

            // Convertir a UTC para compatibilidad con PostgreSQL
            inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
            fin = DateTime.SpecifyKind(fin, DateTimeKind.Utc);

            var gastado = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           g.CategoriaId == presupuesto.CategoriaId &&
                           g.Fecha >= inicio &&
                           g.Fecha <= fin)
                .SumAsync(g => g.Monto);

            return gastado;
        }
    }
}
