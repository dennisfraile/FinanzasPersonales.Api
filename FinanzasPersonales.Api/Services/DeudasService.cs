using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IDeudasService
    {
        Task<List<DeudaDto>> GetDeudasAsync(string userId);
        Task<DeudaDto?> GetDeudaAsync(string userId, int id);
        Task<DeudaDto> CreateDeudaAsync(string userId, CreateDeudaDto dto);
        Task<bool> UpdateDeudaAsync(string userId, int id, UpdateDeudaDto dto);
        Task<bool> DeleteDeudaAsync(string userId, int id);
        Task<PagoDeudaDto> RegistrarPagoAsync(string userId, int deudaId, CreatePagoDeudaDto dto);
        Task<List<PagoDeudaDto>> GetPagosAsync(string userId, int deudaId);
        Task<List<ProyeccionPagoDto>> GetProyeccionAsync(string userId, int deudaId, decimal? pagoMensual = null);
    }

    public class DeudasService : IDeudasService
    {
        private readonly FinanzasDbContext _context;

        public DeudasService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeudaDto>> GetDeudasAsync(string userId)
        {
            return await _context.Deudas
                .Where(d => d.UserId == userId)
                .Include(d => d.Pagos)
                .OrderByDescending(d => d.Activa)
                .ThenByDescending(d => d.SaldoActual)
                .Select(d => MapToDto(d))
                .ToListAsync();
        }

        public async Task<DeudaDto?> GetDeudaAsync(string userId, int id)
        {
            var deuda = await _context.Deudas
                .Include(d => d.Pagos)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            return deuda == null ? null : MapToDto(deuda);
        }

        public async Task<DeudaDto> CreateDeudaAsync(string userId, CreateDeudaDto dto)
        {
            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == dto.CuentaId && c.UserId == userId);
                if (cuenta == null)
                    throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");
            }

            var deuda = new Deuda
            {
                UserId = userId,
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                MontoOriginal = dto.MontoOriginal,
                SaldoActual = dto.SaldoActual,
                TasaInteres = dto.TasaInteres,
                PagoMinimo = dto.PagoMinimo,
                DiaDePago = dto.DiaDePago,
                FechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc),
                FechaVencimiento = dto.FechaVencimiento.HasValue ? DateTime.SpecifyKind(dto.FechaVencimiento.Value, DateTimeKind.Utc) : null,
                CuentaId = dto.CuentaId,
                Notas = dto.Notas
            };

            _context.Deudas.Add(deuda);
            await _context.SaveChangesAsync();

            return MapToDto(deuda);
        }

        public async Task<bool> UpdateDeudaAsync(string userId, int id, UpdateDeudaDto dto)
        {
            var deuda = await _context.Deudas.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (deuda == null) return false;

            deuda.Nombre = dto.Nombre;
            deuda.Tipo = dto.Tipo;
            deuda.TasaInteres = dto.TasaInteres;
            deuda.PagoMinimo = dto.PagoMinimo;
            deuda.DiaDePago = dto.DiaDePago;
            deuda.FechaVencimiento = dto.FechaVencimiento.HasValue ? DateTime.SpecifyKind(dto.FechaVencimiento.Value, DateTimeKind.Utc) : null;
            deuda.CuentaId = dto.CuentaId;
            deuda.Activa = dto.Activa;
            deuda.Notas = dto.Notas;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDeudaAsync(string userId, int id)
        {
            var deuda = await _context.Deudas.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (deuda == null) return false;

            _context.Deudas.Remove(deuda);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagoDeudaDto> RegistrarPagoAsync(string userId, int deudaId, CreatePagoDeudaDto dto)
        {
            var deuda = await _context.Deudas.FirstOrDefaultAsync(d => d.Id == deudaId && d.UserId == userId);
            if (deuda == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            // Calcular distribución interés/capital
            var interesMes = deuda.SaldoActual * (deuda.TasaInteres / 100 / 12);
            var capitalPagado = dto.Monto - interesMes;
            if (capitalPagado < 0) capitalPagado = 0;

            var pago = new PagoDeuda
            {
                DeudaId = deudaId,
                UserId = userId,
                Monto = dto.Monto,
                MontoInteres = Math.Round(interesMes, 2),
                MontoCapital = Math.Round(capitalPagado, 2),
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                Descripcion = dto.Descripcion
            };

            deuda.SaldoActual -= capitalPagado;
            if (deuda.SaldoActual < 0) deuda.SaldoActual = 0;
            if (deuda.SaldoActual == 0) deuda.Activa = false;

            // Actualizar balance de cuenta si está vinculada
            if (deuda.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(deuda.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual -= dto.Monto;
                }
            }

            _context.PagosDeuda.Add(pago);
            await _context.SaveChangesAsync();

            return new PagoDeudaDto
            {
                Id = pago.Id,
                DeudaId = pago.DeudaId,
                Monto = pago.Monto,
                MontoInteres = pago.MontoInteres,
                MontoCapital = pago.MontoCapital,
                Fecha = pago.Fecha,
                Descripcion = pago.Descripcion
            };
        }

        public async Task<List<PagoDeudaDto>> GetPagosAsync(string userId, int deudaId)
        {
            var deudaExiste = await _context.Deudas.AnyAsync(d => d.Id == deudaId && d.UserId == userId);
            if (!deudaExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            return await _context.PagosDeuda
                .Where(p => p.DeudaId == deudaId && p.UserId == userId)
                .OrderByDescending(p => p.Fecha)
                .Select(p => new PagoDeudaDto
                {
                    Id = p.Id,
                    DeudaId = p.DeudaId,
                    Monto = p.Monto,
                    MontoInteres = p.MontoInteres,
                    MontoCapital = p.MontoCapital,
                    Fecha = p.Fecha,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();
        }

        public Task<List<ProyeccionPagoDto>> GetProyeccionAsync(string userId, int deudaId, decimal? pagoMensual = null)
        {
            return GetProyeccionInternalAsync(userId, deudaId, pagoMensual);
        }

        private async Task<List<ProyeccionPagoDto>> GetProyeccionInternalAsync(string userId, int deudaId, decimal? pagoMensual)
        {
            var deuda = await _context.Deudas.FirstOrDefaultAsync(d => d.Id == deudaId && d.UserId == userId);
            if (deuda == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var pago = pagoMensual ?? deuda.PagoMinimo ?? 0;
            if (pago <= 0)
                throw new InvalidOperationException("Debe proporcionar un monto de pago mensual.");

            var proyecciones = new List<ProyeccionPagoDto>();
            var saldo = deuda.SaldoActual;
            var tasaMensual = deuda.TasaInteres / 100 / 12;
            var fecha = DateTime.UtcNow;
            var mes = 1;
            var maxMeses = 360; // 30 años máximo

            while (saldo > 0 && mes <= maxMeses)
            {
                var interesMes = Math.Round(saldo * tasaMensual, 2);
                var pagoReal = Math.Min(pago, saldo + interesMes);
                var capitalMes = Math.Round(pagoReal - interesMes, 2);
                if (capitalMes < 0) capitalMes = 0;

                saldo -= capitalMes;
                if (saldo < 0.01m) saldo = 0;

                fecha = fecha.AddMonths(1);

                proyecciones.Add(new ProyeccionPagoDto
                {
                    Mes = mes,
                    FechaPago = fecha,
                    PagoMensual = pagoReal,
                    InteresDelMes = interesMes,
                    CapitalDelMes = capitalMes,
                    SaldoRestante = saldo
                });

                mes++;
            }

            return proyecciones;
        }

        private static DeudaDto MapToDto(Deuda d)
        {
            var totalPagado = d.Pagos?.Sum(p => p.MontoCapital ?? 0) ?? 0;
            return new DeudaDto
            {
                Id = d.Id,
                Nombre = d.Nombre,
                Tipo = d.Tipo,
                MontoOriginal = d.MontoOriginal,
                SaldoActual = d.SaldoActual,
                TasaInteres = d.TasaInteres,
                PagoMinimo = d.PagoMinimo,
                DiaDePago = d.DiaDePago,
                FechaInicio = d.FechaInicio,
                FechaVencimiento = d.FechaVencimiento,
                CuentaId = d.CuentaId,
                Activa = d.Activa,
                Notas = d.Notas,
                TotalPagado = totalPagado,
                PorcentajePagado = d.MontoOriginal > 0 ? Math.Round(totalPagado / d.MontoOriginal * 100, 2) : 0
            };
        }
    }
}
