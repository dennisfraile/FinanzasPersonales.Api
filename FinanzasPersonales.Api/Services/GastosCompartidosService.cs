using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IGastosCompartidosService
    {
        Task<List<GastoCompartidoDto>> GetGastosCompartidosAsync(string userId);
        Task<GastoCompartidoDto?> GetGastoCompartidoAsync(string userId, int id);
        Task<GastoCompartidoDto> CreateGastoCompartidoAsync(string userId, CreateGastoCompartidoDto dto);
        Task<bool> DeleteGastoCompartidoAsync(string userId, int id);
        Task<bool> LiquidarParticipanteAsync(string userId, int gastoCompartidoId, int participanteId, LiquidarParticipanteDto dto);
        Task<ResumenSplitDto> GetResumenAsync(string userId);
    }

    public class GastosCompartidosService : IGastosCompartidosService
    {
        private readonly FinanzasDbContext _context;

        public GastosCompartidosService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<GastoCompartidoDto>> GetGastosCompartidosAsync(string userId)
        {
            return await _context.GastosCompartidos
                .Where(g => g.UserId == userId)
                .Include(g => g.Participantes)
                .Include(g => g.Categoria)
                .OrderByDescending(g => g.Fecha)
                .Select(g => MapToDto(g))
                .ToListAsync();
        }

        public async Task<GastoCompartidoDto?> GetGastoCompartidoAsync(string userId, int id)
        {
            var gasto = await _context.GastosCompartidos
                .Include(g => g.Participantes)
                .Include(g => g.Categoria)
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            return gasto == null ? null : MapToDto(gasto);
        }

        public async Task<GastoCompartidoDto> CreateGastoCompartidoAsync(string userId, CreateGastoCompartidoDto dto)
        {
            if (dto.Participantes.Count == 0)
                throw new InvalidOperationException("Debe incluir al menos un participante.");

            if (dto.CategoriaId.HasValue)
            {
                var catExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
                if (!catExiste)
                    throw new InvalidOperationException("La categoría no existe o no pertenece al usuario.");
            }

            var gastoCompartido = new GastoCompartido
            {
                UserId = userId,
                Descripcion = dto.Descripcion,
                MontoTotal = dto.MontoTotal,
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                CategoriaId = dto.CategoriaId,
                MetodoDivision = dto.MetodoDivision
            };

            // Calcular montos por participante
            var participantes = CalcularMontos(dto);
            gastoCompartido.Participantes = participantes;

            _context.GastosCompartidos.Add(gastoCompartido);
            await _context.SaveChangesAsync();

            return MapToDto(gastoCompartido);
        }

        public async Task<bool> DeleteGastoCompartidoAsync(string userId, int id)
        {
            var gasto = await _context.GastosCompartidos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (gasto == null) return false;

            _context.GastosCompartidos.Remove(gasto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LiquidarParticipanteAsync(string userId, int gastoCompartidoId, int participanteId, LiquidarParticipanteDto dto)
        {
            var gasto = await _context.GastosCompartidos
                .Include(g => g.Participantes)
                .FirstOrDefaultAsync(g => g.Id == gastoCompartidoId && g.UserId == userId);

            if (gasto == null) return false;

            var participante = gasto.Participantes.FirstOrDefault(p => p.Id == participanteId);
            if (participante == null) return false;

            participante.MontoPagado += dto.Monto;
            if (participante.MontoPagado >= participante.MontoAsignado)
            {
                participante.Liquidado = true;
                participante.FechaLiquidacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResumenSplitDto> GetResumenAsync(string userId)
        {
            var gastos = await _context.GastosCompartidos
                .Where(g => g.UserId == userId)
                .Include(g => g.Participantes)
                .ToListAsync();

            var todosParticipantes = gastos.SelectMany(g => g.Participantes).ToList();

            var deudores = todosParticipantes
                .GroupBy(p => p.Nombre.ToLower())
                .Select(g => new DeudorResumenDto
                {
                    Nombre = g.First().Nombre,
                    TotalDeuda = g.Sum(p => p.MontoAsignado),
                    TotalPagado = g.Sum(p => p.MontoPagado),
                    Pendiente = g.Sum(p => p.MontoAsignado - p.MontoPagado)
                })
                .Where(d => d.Pendiente > 0)
                .OrderByDescending(d => d.Pendiente)
                .ToList();

            return new ResumenSplitDto
            {
                TotalPendientePorCobrar = deudores.Sum(d => d.Pendiente),
                TotalRecuperado = todosParticipantes.Sum(p => p.MontoPagado),
                Deudores = deudores
            };
        }

        private static List<ParticipanteGasto> CalcularMontos(CreateGastoCompartidoDto dto)
        {
            var participantes = new List<ParticipanteGasto>();
            var numParticipantes = dto.Participantes.Count;

            foreach (var p in dto.Participantes)
            {
                decimal montoAsignado = dto.MetodoDivision switch
                {
                    "Equitativo" => Math.Round(dto.MontoTotal / (numParticipantes + 1), 2), // +1 incluye al pagador
                    "Porcentaje" => Math.Round(dto.MontoTotal * ((p.Porcentaje ?? 0) / 100), 2),
                    "MontoFijo" => p.MontoAsignado ?? 0,
                    _ => Math.Round(dto.MontoTotal / (numParticipantes + 1), 2)
                };

                participantes.Add(new ParticipanteGasto
                {
                    Nombre = p.Nombre,
                    Email = p.Email,
                    MontoAsignado = montoAsignado
                });
            }

            return participantes;
        }

        private static GastoCompartidoDto MapToDto(GastoCompartido g)
        {
            var recuperado = g.Participantes.Sum(p => p.MontoPagado);
            var totalAsignado = g.Participantes.Sum(p => p.MontoAsignado);

            return new GastoCompartidoDto
            {
                Id = g.Id,
                Descripcion = g.Descripcion,
                MontoTotal = g.MontoTotal,
                Fecha = g.Fecha,
                CategoriaId = g.CategoriaId,
                CategoriaNombre = g.Categoria?.Nombre,
                MetodoDivision = g.MetodoDivision,
                MontoRecuperado = recuperado,
                MontoPendiente = totalAsignado - recuperado,
                Participantes = g.Participantes.Select(p => new ParticipanteGastoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Email = p.Email,
                    MontoAsignado = p.MontoAsignado,
                    MontoPagado = p.MontoPagado,
                    Liquidado = p.Liquidado,
                    FechaLiquidacion = p.FechaLiquidacion
                }).ToList()
            };
        }
    }
}
