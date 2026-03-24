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
    [Route("api/dashboard-compartido")]
    [ApiController]
    [Authorize]
    public class DashboardCompartidoController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public DashboardCompartidoController(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un link compartido para el dashboard
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CrearLink(CreateDashboardCompartidoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_").Replace("+", "-").TrimEnd('=');

            var compartido = new DashboardCompartido
            {
                UserId = userId,
                Token = token,
                NombreDestinatario = dto.NombreDestinatario,
                FechaExpiracion = dto.DiasExpiracion.HasValue
                    ? DateTime.UtcNow.AddDays(dto.DiasExpiracion.Value) : null,
                SeccionesPermitidas = System.Text.Json.JsonSerializer.Serialize(
                    dto.Secciones ?? new[] { "dashboard", "gastos", "presupuestos" })
            };

            _context.DashboardsCompartidos.Add(compartido);
            await _context.SaveChangesAsync();

            return Ok(new { token, expira = compartido.FechaExpiracion });
        }

        /// <summary>
        /// Lista los links compartidos del usuario actual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> MisLinks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var links = await _context.DashboardsCompartidos
                .Where(d => d.UserId == userId && d.Activo)
                .OrderByDescending(d => d.FechaCreacion)
                .Select(d => new { d.Id, d.Token, d.NombreDestinatario, d.FechaCreacion, d.FechaExpiracion, d.SeccionesPermitidas })
                .ToListAsync();
            return Ok(links);
        }

        /// <summary>
        /// Revoca (desactiva) un link compartido
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> RevocarLink(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var link = await _context.DashboardsCompartidos
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (link == null) return NotFound();

            link.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Visualiza el dashboard compartido (acceso anónimo con token)
        /// </summary>
        [HttpGet("view/{token}")]
        [AllowAnonymous]
        public async Task<ActionResult> VerDashboard(string token)
        {
            var compartido = await _context.DashboardsCompartidos
                .FirstOrDefaultAsync(d => d.Token == token && d.Activo
                    && (d.FechaExpiracion == null || d.FechaExpiracion > DateTime.UtcNow));

            if (compartido == null)
                return NotFound(new { error = "Link no válido o expirado" });

            var secciones = System.Text.Json.JsonSerializer.Deserialize<string[]>(compartido.SeccionesPermitidas) ?? Array.Empty<string>();
            var resultado = new Dictionary<string, object>();

            if (secciones.Contains("dashboard"))
            {
                var dashService = HttpContext.RequestServices.GetRequiredService<IDashboardService>();
                resultado["dashboard"] = await dashService.GetMetricsAsync(compartido.UserId);
            }

            if (secciones.Contains("gastos"))
            {
                var gastos = await _context.Gastos
                    .Where(g => g.UserId == compartido.UserId)
                    .OrderByDescending(g => g.Fecha)
                    .Take(50)
                    .Select(g => new { g.Descripcion, g.Monto, g.Fecha, Categoria = g.Categoria!.Nombre })
                    .ToListAsync();
                resultado["gastos"] = gastos;
            }

            if (secciones.Contains("presupuestos"))
            {
                var presService = HttpContext.RequestServices.GetRequiredService<IPresupuestosService>();
                resultado["presupuestos"] = await presService.GetPresupuestosAsync(compartido.UserId);
            }

            if (secciones.Contains("metas"))
            {
                var metas = await _context.Metas
                    .Where(m => m.UserId == compartido.UserId)
                    .Select(m => new { m.Metas, m.MontoTotal, m.AhorroActual, m.MontoRestante })
                    .ToListAsync();
                resultado["metas"] = metas;
            }

            if (secciones.Contains("deudas"))
            {
                var deudas = await _context.Deudas
                    .Where(d => d.UserId == compartido.UserId && d.Activa)
                    .Select(d => new { d.Nombre, d.MontoOriginal, d.SaldoActual, TotalPagado = d.MontoOriginal - d.SaldoActual, PorcentajePagado = d.MontoOriginal == 0 ? 0 : Math.Round((d.MontoOriginal - d.SaldoActual) / d.MontoOriginal * 100, 2) })
                    .ToListAsync();
                resultado["deudas"] = deudas;
            }

            if (secciones.Contains("ingresos"))
            {
                var ingresos = await _context.Ingresos
                    .Where(i => i.UserId == compartido.UserId)
                    .OrderByDescending(i => i.Fecha)
                    .Take(50)
                    .Select(i => new { i.Descripcion, i.Monto, i.Fecha, Categoria = i.Categoria!.Nombre })
                    .ToListAsync();
                resultado["ingresos"] = ingresos;
            }

            return Ok(new { destinatario = compartido.NombreDestinatario, secciones, data = resultado });
        }
    }
}
