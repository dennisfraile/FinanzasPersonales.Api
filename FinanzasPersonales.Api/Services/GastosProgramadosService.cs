using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class GastosProgramadosService : IGastosProgramadosService
    {
        private readonly FinanzasDbContext _context;
        private readonly INotificacionService _notificacionService;
        private readonly ILogger<GastosProgramadosService> _logger;

        public GastosProgramadosService(
            FinanzasDbContext context,
            INotificacionService notificacionService,
            ILogger<GastosProgramadosService> logger)
        {
            _context = context;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        public async Task<List<GastoProgramadoDto>> GetGastosProgramadosAsync(string userId, string? estado = null)
        {
            var query = _context.GastosProgramados
                .Include(gp => gp.Categoria)
                .Include(gp => gp.Cuenta)
                .Where(gp => gp.UserId == userId);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(gp => gp.Estado == estado);

            return await query
                .OrderBy(gp => gp.FechaVencimiento)
                .Select(gp => MapToDto(gp))
                .ToListAsync();
        }

        public async Task<GastoProgramadoDto?> GetGastoProgramadoAsync(string userId, int id)
        {
            var gp = await _context.GastosProgramados
                .Include(g => g.Categoria)
                .Include(g => g.Cuenta)
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gp == null) return null;

            return MapToDto(gp);
        }

        public async Task<(GastoProgramadoDto? result, string? error)> CreateGastoProgramadoAsync(string userId, CreateGastoProgramadoDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return (null, "Categoría no válida");

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return (null, "Cuenta no válida");
            }

            var gastoProgramado = new GastoProgramado
            {
                UserId = userId,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                CuentaId = dto.CuentaId,
                Monto = dto.Monto,
                EsMontoVariable = dto.EsMontoVariable,
                FechaVencimiento = dto.FechaVencimiento,
                Estado = "Pendiente",
                Notas = dto.Notas
            };

            _context.GastosProgramados.Add(gastoProgramado);
            await _context.SaveChangesAsync();

            await _context.Entry(gastoProgramado).Reference(g => g.Categoria).LoadAsync();
            if (gastoProgramado.CuentaId.HasValue)
                await _context.Entry(gastoProgramado).Reference(g => g.Cuenta).LoadAsync();

            return (MapToDto(gastoProgramado), null);
        }

        public async Task<(bool success, string? error)> UpdateGastoProgramadoAsync(string userId, int id, UpdateGastoProgramadoDto dto)
        {
            var gp = await _context.GastosProgramados.FindAsync(id);
            if (gp == null || gp.UserId != userId)
                return (false, null);

            if (gp.Estado == "Pagado")
                return (false, "No se puede modificar un gasto que ya fue pagado");

            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return (false, "Categoría no válida");

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return (false, "Cuenta no válida");
            }

            gp.Descripcion = dto.Descripcion;
            gp.CategoriaId = dto.CategoriaId;
            gp.CuentaId = dto.CuentaId;
            gp.Monto = dto.Monto;
            gp.EsMontoVariable = dto.EsMontoVariable;
            gp.FechaVencimiento = dto.FechaVencimiento;
            gp.Notas = dto.Notas;

            // Si se cambió la fecha y estaba vencido, volver a pendiente
            if (gp.Estado == "Vencido" && dto.FechaVencimiento > DateTime.UtcNow)
                gp.Estado = "Pendiente";

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteGastoProgramadoAsync(string userId, int id)
        {
            var gp = await _context.GastosProgramados.FindAsync(id);
            if (gp == null || gp.UserId != userId)
                return false;

            _context.GastosProgramados.Remove(gp);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Registra el pago de un gasto programado: crea el Gasto real, descuenta la cuenta y notifica.
        /// </summary>
        public async Task<(GastoProgramadoDto? result, string? error)> PagarGastoProgramadoAsync(string userId, int id, PagarGastoProgramadoDto dto)
        {
            var gp = await _context.GastosProgramados
                .Include(g => g.Cuenta)
                .Include(g => g.Categoria)
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gp == null)
                return (null, null); // not found

            if (gp.Estado == "Pagado")
                return (null, "Este gasto ya fue pagado");

            if (gp.Estado == "Cancelado")
                return (null, "Este gasto fue cancelado");

            var montoPago = dto.MontoPagado ?? gp.Monto;
            var cuentaId = dto.CuentaId ?? gp.CuentaId;
            var fechaPago = dto.FechaPago ?? DateTime.UtcNow;

            // Validar cuenta si se especificó una diferente
            Cuenta? cuentaPago = null;
            if (cuentaId.HasValue)
            {
                cuentaPago = cuentaId == gp.CuentaId
                    ? gp.Cuenta
                    : await _context.Cuentas.FindAsync(cuentaId.Value);

                if (cuentaPago == null || cuentaPago.UserId != userId)
                    return (null, "Cuenta no válida");
            }

            // Crear el gasto real
            var gasto = new Gasto
            {
                UserId = userId,
                CategoriaId = gp.CategoriaId,
                Descripcion = gp.Descripcion,
                Monto = montoPago,
                CuentaId = cuentaId,
                Fecha = fechaPago,
                Tipo = gp.EsMontoVariable ? "Variable" : "Fijo",
                Notas = gp.Notas
            };

            _context.Gastos.Add(gasto);

            // Descontar de la cuenta
            if (cuentaPago != null)
            {
                cuentaPago.BalanceActual -= montoPago;
            }

            // Actualizar estado del gasto programado
            gp.Estado = "Pagado";
            gp.MontoPagado = montoPago;
            gp.FechaPago = fechaPago;
            if (cuentaId.HasValue)
                gp.CuentaId = cuentaId.Value;

            await _context.SaveChangesAsync();

            // Guardar referencia al gasto generado
            gp.GastoGeneradoId = gasto.Id;
            await _context.SaveChangesAsync();

            // Notificar al usuario que el cobro fue efectuado
            await _notificacionService.CrearNotificacionAsync(
                userId,
                "PagoEfectuado",
                $"Pago registrado: {gp.Descripcion}",
                $"Se registró el pago de {montoPago:C} para '{gp.Descripcion}' ({gp.Categoria?.Nombre}). " +
                (cuentaPago != null ? $"Cuenta: {cuentaPago.Nombre}, nuevo balance: {cuentaPago.BalanceActual:C}." : ""),
                gp.Id,
                $"{{\"gastoProgramadoId\":{gp.Id},\"gastoId\":{gasto.Id},\"monto\":{montoPago}}}"
            );

            // Recargar relaciones para el DTO
            await _context.Entry(gp).Reference(g => g.Categoria).LoadAsync();
            if (gp.CuentaId.HasValue)
                await _context.Entry(gp).Reference(g => g.Cuenta).LoadAsync();

            return (MapToDto(gp), null);
        }

        public async Task<(bool success, string? error)> CancelarGastoProgramadoAsync(string userId, int id)
        {
            var gp = await _context.GastosProgramados.FindAsync(id);
            if (gp == null || gp.UserId != userId)
                return (false, null);

            if (gp.Estado == "Pagado")
                return (false, "No se puede cancelar un gasto que ya fue pagado");

            gp.Estado = "Cancelado";
            await _context.SaveChangesAsync();
            return (true, null);
        }

        /// <summary>
        /// Marca como vencidos los gastos programados cuya fecha de vencimiento ya pasó.
        /// Llamado por el job de Hangfire.
        /// </summary>
        public async Task<int> MarcarVencidosAsync()
        {
            var ahora = DateTime.UtcNow;
            var vencidos = await _context.GastosProgramados
                .Where(gp => gp.Estado == "Pendiente" && gp.FechaVencimiento < ahora)
                .ToListAsync();

            foreach (var gp in vencidos)
            {
                gp.Estado = "Vencido";
            }

            await _context.SaveChangesAsync();
            return vencidos.Count;
        }

        /// <summary>
        /// Procesa cobros automáticos: gastos programados con fecha de vencimiento = hoy
        /// que tienen cuenta asignada y no son de monto variable (son fijos predecibles).
        /// Los de monto variable requieren confirmación manual del usuario.
        /// </summary>
        public async Task<int> ProcesarCobrosAutomaticosAsync()
        {
            var hoy = DateTime.UtcNow.Date;
            var manana = hoy.AddDays(1);

            var programados = await _context.GastosProgramados
                .Include(gp => gp.Cuenta)
                .Include(gp => gp.Categoria)
                .Where(gp => gp.Estado == "Pendiente"
                    && !gp.EsMontoVariable
                    && gp.CuentaId.HasValue
                    && gp.FechaVencimiento >= hoy
                    && gp.FechaVencimiento < manana)
                .ToListAsync();

            int procesados = 0;

            foreach (var gp in programados)
            {
                if (gp.Cuenta == null) continue;

                var gasto = new Gasto
                {
                    UserId = gp.UserId,
                    CategoriaId = gp.CategoriaId,
                    Descripcion = gp.Descripcion,
                    Monto = gp.Monto,
                    CuentaId = gp.CuentaId,
                    Fecha = DateTime.UtcNow,
                    Tipo = "Fijo",
                    Notas = gp.Notas
                };

                _context.Gastos.Add(gasto);
                gp.Cuenta.BalanceActual -= gp.Monto;

                gp.Estado = "Pagado";
                gp.MontoPagado = gp.Monto;
                gp.FechaPago = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                gp.GastoGeneradoId = gasto.Id;
                await _context.SaveChangesAsync();

                // Notificar cobro efectuado
                await _notificacionService.CrearNotificacionAsync(
                    gp.UserId,
                    "PagoEfectuado",
                    $"Cobro efectuado: {gp.Descripcion}",
                    $"Se procesó el cobro automático de {gp.Monto:C} para '{gp.Descripcion}' ({gp.Categoria?.Nombre}). " +
                    $"Cuenta: {gp.Cuenta.Nombre}, nuevo balance: {gp.Cuenta.BalanceActual:C}.",
                    gp.Id,
                    $"{{\"gastoProgramadoId\":{gp.Id},\"gastoId\":{gasto.Id},\"monto\":{gp.Monto},\"automatico\":true}}"
                );

                procesados++;
                _logger.LogInformation("Cobro automático procesado: GastoProgramado {Id}, monto {Monto}", gp.Id, gp.Monto);
            }

            return procesados;
        }

        /// <summary>
        /// Obtiene el total comprometido (gastos programados pendientes) para una categoría en un rango de fechas.
        /// Usado por el sistema de presupuestos para calcular gasto proyectado.
        /// </summary>
        public async Task<decimal> GetTotalComprometidoAsync(string userId, int categoriaId, DateTime inicio, DateTime fin)
        {
            return await _context.GastosProgramados
                .Where(gp => gp.UserId == userId
                    && gp.CategoriaId == categoriaId
                    && gp.Estado == "Pendiente"
                    && gp.FechaVencimiento >= inicio
                    && gp.FechaVencimiento <= fin)
                .SumAsync(gp => (decimal?)gp.Monto) ?? 0;
        }

        private static GastoProgramadoDto MapToDto(GastoProgramado gp)
        {
            return new GastoProgramadoDto
            {
                Id = gp.Id,
                Descripcion = gp.Descripcion,
                CategoriaId = gp.CategoriaId,
                CategoriaNombre = gp.Categoria?.Nombre,
                CuentaId = gp.CuentaId,
                CuentaNombre = gp.Cuenta?.Nombre,
                Monto = gp.Monto,
                MontoPagado = gp.MontoPagado,
                EsMontoVariable = gp.EsMontoVariable,
                FechaVencimiento = gp.FechaVencimiento,
                FechaPago = gp.FechaPago,
                Estado = gp.Estado,
                GastoRecurrenteId = gp.GastoRecurrenteId,
                GastoGeneradoId = gp.GastoGeneradoId,
                Notas = gp.Notas,
                FechaCreacion = gp.FechaCreacion,
                DiasParaVencimiento = (int)(gp.FechaVencimiento.Date - DateTime.UtcNow.Date).TotalDays
            };
        }
    }
}
