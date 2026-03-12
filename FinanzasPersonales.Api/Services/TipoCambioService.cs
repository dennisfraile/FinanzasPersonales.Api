using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface ITipoCambioService
    {
        Task<TipoCambioDto?> GetTasaActualAsync(string monedaOrigen, string monedaDestino);
        Task<TipoCambioDto> CreateTipoCambioAsync(CreateTipoCambioDto dto);
        Task<ConversionDto> ConvertirAsync(decimal monto, string monedaOrigen, string monedaDestino);
        Task<List<TipoCambioDto>> GetHistorialAsync(string monedaOrigen, string monedaDestino, int limite = 30);
    }

    public class TipoCambioService : ITipoCambioService
    {
        private readonly FinanzasDbContext _context;

        public TipoCambioService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<TipoCambioDto?> GetTasaActualAsync(string monedaOrigen, string monedaDestino)
        {
            var tipo = await _context.TiposCambio
                .Where(t => t.MonedaOrigen == monedaOrigen && t.MonedaDestino == monedaDestino)
                .OrderByDescending(t => t.Fecha)
                .FirstOrDefaultAsync();

            if (tipo == null) return null;

            return new TipoCambioDto
            {
                Id = tipo.Id,
                MonedaOrigen = tipo.MonedaOrigen,
                MonedaDestino = tipo.MonedaDestino,
                Tasa = tipo.Tasa,
                Fecha = tipo.Fecha,
                Fuente = tipo.Fuente
            };
        }

        public async Task<TipoCambioDto> CreateTipoCambioAsync(CreateTipoCambioDto dto)
        {
            var tipo = new TipoCambio
            {
                MonedaOrigen = dto.MonedaOrigen.ToUpper(),
                MonedaDestino = dto.MonedaDestino.ToUpper(),
                Tasa = dto.Tasa,
                Fecha = DateTime.UtcNow,
                Fuente = "Manual"
            };

            _context.TiposCambio.Add(tipo);
            await _context.SaveChangesAsync();

            return new TipoCambioDto
            {
                Id = tipo.Id,
                MonedaOrigen = tipo.MonedaOrigen,
                MonedaDestino = tipo.MonedaDestino,
                Tasa = tipo.Tasa,
                Fecha = tipo.Fecha,
                Fuente = tipo.Fuente
            };
        }

        public async Task<ConversionDto> ConvertirAsync(decimal monto, string monedaOrigen, string monedaDestino)
        {
            var tasa = await _context.TiposCambio
                .Where(t => t.MonedaOrigen == monedaOrigen && t.MonedaDestino == monedaDestino)
                .OrderByDescending(t => t.Fecha)
                .FirstOrDefaultAsync();

            if (tasa == null)
                throw new InvalidOperationException($"No existe tasa de cambio de {monedaOrigen} a {monedaDestino}.");

            return new ConversionDto
            {
                MontoOriginal = monto,
                MonedaOrigen = monedaOrigen,
                MontoConvertido = Math.Round(monto * tasa.Tasa, 2),
                MonedaDestino = monedaDestino,
                TasaUsada = tasa.Tasa
            };
        }

        public async Task<List<TipoCambioDto>> GetHistorialAsync(string monedaOrigen, string monedaDestino, int limite = 30)
        {
            return await _context.TiposCambio
                .Where(t => t.MonedaOrigen == monedaOrigen && t.MonedaDestino == monedaDestino)
                .OrderByDescending(t => t.Fecha)
                .Take(limite)
                .Select(t => new TipoCambioDto
                {
                    Id = t.Id,
                    MonedaOrigen = t.MonedaOrigen,
                    MonedaDestino = t.MonedaDestino,
                    Tasa = t.Tasa,
                    Fecha = t.Fecha,
                    Fuente = t.Fuente
                })
                .ToListAsync();
        }
    }
}
