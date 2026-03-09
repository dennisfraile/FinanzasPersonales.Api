using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class CuentasService : ICuentasService
    {
        private readonly FinanzasDbContext _context;

        public CuentasService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<CuentaDto>> GetCuentasAsync(string userId)
        {
            return await _context.Cuentas
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
        }

        public async Task<CuentaDto?> GetCuentaAsync(string userId, int id)
        {
            return await _context.Cuentas
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
        }

        public async Task<CuentaDto> CreateCuentaAsync(string userId, CuentaCreateDto dto)
        {
            var cuenta = new Cuenta
            {
                UserId = userId,
                Nombre = dto.Nombre,
                Tipo = Enum.Parse<TipoCuenta>(dto.Tipo),
                BalanceInicial = dto.BalanceInicial,
                BalanceActual = dto.BalanceInicial,
                Moneda = dto.Moneda,
                Color = dto.Color,
                Icono = dto.Icono,
                Activa = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();

            return new CuentaDto
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
        }

        public async Task<bool> UpdateCuentaAsync(string userId, int id, CuentaUpdateDto dto)
        {
            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cuenta == null)
                return false;

            cuenta.Nombre = dto.Nombre;
            cuenta.BalanceActual = dto.BalanceActual;
            cuenta.Color = dto.Color;
            cuenta.Icono = dto.Icono;
            cuenta.Activa = dto.Activa;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCuentaAsync(string userId, int id)
        {
            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cuenta == null)
                return false;

            cuenta.Activa = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetBalanceTotalAsync(string userId)
        {
            return await _context.Cuentas
                .Where(c => c.UserId == userId && c.Activa)
                .SumAsync(c => c.BalanceActual);
        }
    }
}
