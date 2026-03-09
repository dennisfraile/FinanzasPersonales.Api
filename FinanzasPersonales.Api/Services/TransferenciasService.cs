using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class TransferenciasService : ITransferenciasService
    {
        private readonly FinanzasDbContext _context;

        public TransferenciasService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransferenciaDto>> GetTransferenciasAsync(string userId)
        {
            return await _context.Transferencias
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
        }

        public async Task<(TransferenciaDto? result, string? error)> CreateTransferenciaAsync(string userId, TransferenciaCreateDto dto)
        {
            var cuentaOrigen = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == dto.CuentaOrigenId && c.UserId == userId);

            var cuentaDestino = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == dto.CuentaDestinoId && c.UserId == userId);

            if (cuentaOrigen == null || cuentaDestino == null)
                return (null, "Cuentas no encontradas");

            if (dto.CuentaOrigenId == dto.CuentaDestinoId)
                return (null, "No puedes transferir a la misma cuenta");

            if (dto.Monto <= 0)
                return (null, "El monto debe ser mayor a 0");

            var transferencia = new Transferencia
            {
                UserId = userId,
                CuentaOrigenId = dto.CuentaOrigenId,
                CuentaDestinoId = dto.CuentaDestinoId,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };

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

            return (result, null);
        }
    }
}
