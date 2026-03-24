using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using System.Globalization;

namespace FinanzasPersonales.Api.Services
{
    public class PresupuestosService : IPresupuestosService
    {
        private readonly FinanzasDbContext _context;

        public PresupuestosService(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calcula el total comprometido (gastos programados pendientes) para una categoría en un rango.
        /// </summary>
        private async Task<decimal> CalcularComprometidoAsync(string userId, int categoriaId, DateTime inicio, DateTime fin)
        {
            return await _context.GastosProgramados
                .Where(gp => gp.UserId == userId
                    && gp.CategoriaId == categoriaId
                    && gp.Estado == "Pendiente"
                    && gp.FechaVencimiento >= inicio
                    && gp.FechaVencimiento <= fin)
                .SumAsync(gp => (decimal?)gp.Monto) ?? 0;
        }

        /// <summary>
        /// Calcula el rango de fechas para un presupuesto según su periodo.
        /// </summary>
        public static (DateTime inicio, DateTime fin) CalcularRangoFechas(Presupuesto presupuesto)
        {
            DateTime inicio, fin;

            switch (presupuesto.Periodo)
            {
                case "Semanal":
                    // Usar semana ISO: SemanaAplicable indica el número de semana del año
                    var semana = presupuesto.SemanaAplicable ?? 1;
                    var primerDiaAno = new DateTime(presupuesto.AnoAplicable, 1, 1);
                    // Encontrar el primer lunes del año ISO
                    var diaSemana = (int)primerDiaAno.DayOfWeek;
                    var offsetLunes = diaSemana == 0 ? -6 : 1 - diaSemana; // DayOfWeek: 0=Dom, 1=Lun
                    var primerLunes = primerDiaAno.AddDays(offsetLunes);
                    inicio = primerLunes.AddDays((semana - 1) * 7);
                    fin = inicio.AddDays(6); // Domingo
                    break;

                case "Quincenal":
                    if (presupuesto.MesAplicable > 0)
                    {
                        var dia = DateTime.UtcNow.Day;
                        if (dia <= 15)
                        {
                            inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                            fin = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 15);
                        }
                        else
                        {
                            inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 16);
                            fin = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable,
                                DateTime.DaysInMonth(presupuesto.AnoAplicable, presupuesto.MesAplicable));
                        }
                    }
                    else
                    {
                        inicio = new DateTime(presupuesto.AnoAplicable, 1, 1);
                        fin = new DateTime(presupuesto.AnoAplicable, 1, 15);
                    }
                    break;

                case "Mensual":
                    inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                    fin = inicio.AddMonths(1).AddDays(-1);
                    break;

                case "Trimestral":
                    // Trimestres: Q1=1-3, Q2=4-6, Q3=7-9, Q4=10-12
                    var mesInicioTrim = ((presupuesto.MesAplicable - 1) / 3) * 3 + 1;
                    inicio = new DateTime(presupuesto.AnoAplicable, mesInicioTrim, 1);
                    fin = inicio.AddMonths(3).AddDays(-1);
                    break;

                case "Semestral":
                    var mesInicioSem = presupuesto.MesAplicable <= 6 ? 1 : 7;
                    inicio = new DateTime(presupuesto.AnoAplicable, mesInicioSem, 1);
                    fin = inicio.AddMonths(6).AddDays(-1);
                    break;

                case "Anual":
                    inicio = new DateTime(presupuesto.AnoAplicable, 1, 1);
                    fin = new DateTime(presupuesto.AnoAplicable, 12, 31);
                    break;

                default:
                    inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                    fin = inicio.AddMonths(1).AddDays(-1);
                    break;
            }

            inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
            // Usar el final del último día (23:59:59) para incluir gastos de todo el día
            fin = DateTime.SpecifyKind(fin, DateTimeKind.Utc).Date.AddDays(1).AddTicks(-1);

            return (inicio, fin);
        }

        /// <summary>
        /// Obtiene el número de semana ISO 8601 para una fecha.
        /// </summary>
        private static int GetISOWeekNumber(DateTime date)
        {
            var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                date = date.AddDays(3);
            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public async Task<List<PresupuestoDto>> GetPresupuestosAsync(string userId, int? mes = null, int? ano = null)
        {
            IQueryable<Presupuesto> query = _context.Presupuestos
                .Where(p => p.UserId == userId);

            if (mes.HasValue)
                query = query.Where(p => p.MesAplicable == mes.Value);

            if (ano.HasValue)
                query = query.Where(p => p.AnoAplicable == ano.Value);

            var presupuestos = await query.Include(p => p.Categoria).ToListAsync();

            if (!presupuestos.Any())
                return new List<PresupuestoDto>();

            // Auto-actualizar presupuestos semanales a la semana actual
            var ahora = DateTime.UtcNow;
            var semanaActual = GetISOWeekNumber(ahora);
            var semanalesActualizados = false;
            foreach (var p in presupuestos.Where(p => p.Periodo == "Semanal"))
            {
                if (p.SemanaAplicable != semanaActual || p.AnoAplicable != ahora.Year)
                {
                    p.SemanaAplicable = semanaActual;
                    p.AnoAplicable = ahora.Year;
                    p.MesAplicable = ahora.Month;
                    semanalesActualizados = true;
                }
            }
            if (semanalesActualizados)
                await _context.SaveChangesAsync();

            var gastadoPorPresupuesto = await CalcularGastadoBatchAsync(userId, presupuestos);

            // Obtener transferencias del usuario para el rango de todos los presupuestos
            var categoriaIds = presupuestos.Select(p => p.CategoriaId).Distinct().ToList();
            var todasTransferencias = await _context.TransferenciasGasto
                .Where(t => t.UserId == userId &&
                    (categoriaIds.Contains(t.CategoriaOrigenId) || categoriaIds.Contains(t.CategoriaDestinoId)))
                .ToListAsync();

            // Map categoriaId -> nombre para las transferencias
            var categoriaNombres = presupuestos
                .Where(p => p.Categoria != null)
                .ToDictionary(p => p.CategoriaId, p => p.Categoria!.Nombre);

            var resultado = new List<PresupuestoDto>();

            foreach (var presupuesto in presupuestos)
            {
                var gastadoActual = gastadoPorPresupuesto.GetValueOrDefault(presupuesto.Id, 0m);
                var (inicio, fin) = CalcularRangoFechas(presupuesto);
                var comprometido = await CalcularComprometidoAsync(userId, presupuesto.CategoriaId, inicio, fin);
                var rollover = await CalcularRolloverAsync(userId, presupuesto);
                var limiteEfectivo = presupuesto.MontoLimite + rollover;
                var totalProyectado = gastadoActual + comprometido;

                // Transferencias que afectan esta categoría en el periodo
                var transferencias = todasTransferencias
                    .Where(t => t.Fecha >= inicio && t.Fecha <= fin &&
                        (t.CategoriaOrigenId == presupuesto.CategoriaId || t.CategoriaDestinoId == presupuesto.CategoriaId))
                    .Select(t => new TransferenciaGastoResumenDto
                    {
                        Id = t.Id,
                        Monto = t.Monto,
                        CategoriaOrigenNombre = categoriaNombres.GetValueOrDefault(t.CategoriaOrigenId, "Otra"),
                        CategoriaDestinoNombre = categoriaNombres.GetValueOrDefault(t.CategoriaDestinoId, "Otra"),
                        Direccion = t.CategoriaOrigenId == presupuesto.CategoriaId ? "salida" : "entrada",
                        Fecha = t.Fecha
                    })
                    .OrderByDescending(t => t.Fecha)
                    .ToList();

                resultado.Add(new PresupuestoDto
                {
                    Id = presupuesto.Id,
                    CategoriaId = presupuesto.CategoriaId,
                    CategoriaNombre = presupuesto.Categoria!.Nombre,
                    MontoLimite = presupuesto.MontoLimite,
                    Periodo = presupuesto.Periodo,
                    MesAplicable = presupuesto.MesAplicable,
                    AnoAplicable = presupuesto.AnoAplicable,
                    SemanaAplicable = presupuesto.SemanaAplicable,
                    GastadoActual = gastadoActual,
                    Disponible = limiteEfectivo - gastadoActual,
                    PorcentajeUtilizado = limiteEfectivo > 0
                        ? (gastadoActual / limiteEfectivo) * 100
                        : 0,
                    FechaInicio = inicio,
                    FechaFin = fin,
                    Transferencias = transferencias,
                    Comprometido = comprometido,
                    TotalProyectado = totalProyectado,
                    PorcentajeProyectado = limiteEfectivo > 0
                        ? (totalProyectado / limiteEfectivo) * 100
                        : 0,
                    PermiteRollover = presupuesto.PermiteRollover,
                    Rollover = rollover,
                    LimiteEfectivo = limiteEfectivo
                });
            }

            return resultado;
        }

        public async Task<PresupuestoDto?> GetPresupuestoAsync(string userId, int id)
        {
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return null;

            var gastadoActual = await CalcularGastadoActual(userId, presupuesto);
            var (inicio, fin) = CalcularRangoFechas(presupuesto);
            var comprometido = await CalcularComprometidoAsync(userId, presupuesto.CategoriaId, inicio, fin);
            var totalProyectado = gastadoActual + comprometido;

            return new PresupuestoDto
            {
                Id = presupuesto.Id,
                CategoriaId = presupuesto.CategoriaId,
                CategoriaNombre = presupuesto.Categoria!.Nombre,
                MontoLimite = presupuesto.MontoLimite,
                Periodo = presupuesto.Periodo,
                MesAplicable = presupuesto.MesAplicable,
                AnoAplicable = presupuesto.AnoAplicable,
                SemanaAplicable = presupuesto.SemanaAplicable,
                GastadoActual = gastadoActual,
                Disponible = presupuesto.MontoLimite - gastadoActual,
                PorcentajeUtilizado = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100
                    : 0,
                FechaInicio = inicio,
                FechaFin = fin,
                Comprometido = comprometido,
                TotalProyectado = totalProyectado,
                PorcentajeProyectado = presupuesto.MontoLimite > 0
                    ? (totalProyectado / presupuesto.MontoLimite) * 100
                    : 0
            };
        }

        public async Task<(PresupuestoDto? result, string? error)> CreatePresupuestoAsync(string userId, PresupuestoCreateDto dto)
        {
            // Validar que Semanal requiere SemanaAplicable
            if (dto.Periodo == "Semanal" && !dto.SemanaAplicable.HasValue)
                return (null, "El período Semanal requiere indicar la semana aplicable.");

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);

            if (!categoriaExiste)
                return (null, "Recurso no encontrado o acceso denegado.");

            // Verificar duplicados según periodo
            IQueryable<Presupuesto> duplicadoQuery = _context.Presupuestos
                .Where(p => p.UserId == userId &&
                           p.CategoriaId == dto.CategoriaId &&
                           p.AnoAplicable == dto.AnoAplicable &&
                           p.Periodo == dto.Periodo);

            if (dto.Periodo == "Semanal")
                duplicadoQuery = duplicadoQuery.Where(p => p.SemanaAplicable == dto.SemanaAplicable);
            else if (dto.Periodo != "Anual")
                duplicadoQuery = duplicadoQuery.Where(p => p.MesAplicable == dto.MesAplicable);

            if (await duplicadoQuery.AnyAsync())
                return (null, "Ya existe un presupuesto para esta categoría en el período especificado.");

            var presupuesto = new Presupuesto
            {
                CategoriaId = dto.CategoriaId,
                MontoLimite = dto.MontoLimite,
                Periodo = dto.Periodo,
                MesAplicable = dto.MesAplicable,
                AnoAplicable = dto.AnoAplicable,
                SemanaAplicable = dto.SemanaAplicable,
                PermiteRollover = dto.PermiteRollover,
                UserId = userId
            };

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            var result = await GetPresupuestoAsync(userId, presupuesto.Id);
            return (result, null);
        }

        public async Task<bool> UpdatePresupuestoAsync(string userId, int id, PresupuestoUpdateDto dto)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return false;

            presupuesto.MontoLimite = dto.MontoLimite;
            presupuesto.Periodo = dto.Periodo;
            presupuesto.PermiteRollover = dto.PermiteRollover;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Presupuestos.AnyAsync(p => p.Id == id))
                    return false;
                else
                    throw;
            }

            return true;
        }

        public async Task<bool> DeletePresupuestoAsync(string userId, int id)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return false;

            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<PresupuestoDto>> GetAlertasAsync(string userId)
        {
            var mesActual = DateTime.UtcNow.Month;
            var anoActual = DateTime.UtcNow.Year;

            var presupuestos = await _context.Presupuestos
                .Where(p => p.UserId == userId &&
                           p.MesAplicable == mesActual &&
                           p.AnoAplicable == anoActual)
                .Include(p => p.Categoria)
                .ToListAsync();

            if (!presupuestos.Any())
                return new List<PresupuestoDto>();

            var gastadoPorPresupuesto = await CalcularGastadoBatchAsync(userId, presupuestos);

            var resultado = new List<PresupuestoDto>();

            foreach (var presupuesto in presupuestos)
            {
                var gastadoActual = gastadoPorPresupuesto.GetValueOrDefault(presupuesto.Id, 0m);
                var porcentaje = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100
                    : 0;

                if (porcentaje >= 80)
                {
                    var (inicio, fin) = CalcularRangoFechas(presupuesto);
                    resultado.Add(new PresupuestoDto
                    {
                        Id = presupuesto.Id,
                        CategoriaId = presupuesto.CategoriaId,
                        CategoriaNombre = presupuesto.Categoria!.Nombre,
                        MontoLimite = presupuesto.MontoLimite,
                        Periodo = presupuesto.Periodo,
                        MesAplicable = presupuesto.MesAplicable,
                        AnoAplicable = presupuesto.AnoAplicable,
                        SemanaAplicable = presupuesto.SemanaAplicable,
                        GastadoActual = gastadoActual,
                        Disponible = presupuesto.MontoLimite - gastadoActual,
                        PorcentajeUtilizado = porcentaje,
                        FechaInicio = inicio,
                        FechaFin = fin
                    });
                }
            }

            return resultado.OrderByDescending(p => p.PorcentajeUtilizado).ToList();
        }

        public async Task<PresupuestoDashboardDto> GetDashboardAsync(string userId, string periodo)
        {
            var hoy = DateTime.UtcNow;

            // Buscar presupuestos activos del periodo solicitado
            var presupuestos = await _context.Presupuestos
                .Where(p => p.UserId == userId && p.Periodo == periodo && p.AnoAplicable == hoy.Year)
                .Include(p => p.Categoria)
                .ToListAsync();

            // Filtrar solo los que aplican al periodo actual
            var presupuestosActuales = presupuestos.Where(p =>
            {
                var (inicio, fin) = CalcularRangoFechas(p);
                return hoy >= inicio && hoy <= fin;
            }).ToList();

            // Si no hay del periodo actual, buscar los del mes actual como fallback
            if (!presupuestosActuales.Any() && periodo != "Anual")
            {
                presupuestosActuales = presupuestos
                    .Where(p => p.MesAplicable == hoy.Month)
                    .ToList();
            }

            // Calcular gastado
            var gastadoPorPresupuesto = presupuestosActuales.Any()
                ? await CalcularGastadoBatchAsync(userId, presupuestosActuales)
                : new Dictionary<int, decimal>();

            // Determinar rango del periodo actual para el label
            DateTime fechaInicio, fechaFin;
            string periodoLabel;

            if (presupuestosActuales.Any())
            {
                var (i, f) = CalcularRangoFechas(presupuestosActuales.First());
                fechaInicio = i;
                fechaFin = f;
            }
            else
            {
                // Fallback: calcular rango genérico
                fechaInicio = DateTime.SpecifyKind(new DateTime(hoy.Year, hoy.Month, 1), DateTimeKind.Utc);
                fechaFin = DateTime.SpecifyKind(fechaInicio.AddMonths(1).AddDays(-1), DateTimeKind.Utc);
            }

            periodoLabel = periodo switch
            {
                "Semanal" => $"Semana del {fechaInicio:dd MMM} al {fechaFin:dd MMM yyyy}",
                "Quincenal" => hoy.Day <= 15
                    ? $"Q1 {hoy:MMM yyyy} (1-15)"
                    : $"Q2 {hoy:MMM yyyy} (16-{DateTime.DaysInMonth(hoy.Year, hoy.Month)})",
                "Mensual" => $"{hoy:MMMM yyyy}",
                "Trimestral" => $"Trimestre {((hoy.Month - 1) / 3) + 1} - {hoy.Year}",
                "Semestral" => hoy.Month <= 6 ? $"1er Semestre {hoy.Year}" : $"2do Semestre {hoy.Year}",
                "Anual" => $"Año {hoy.Year}",
                _ => periodo
            };

            var comparaciones = presupuestosActuales.Select(p =>
            {
                var gastado = gastadoPorPresupuesto.GetValueOrDefault(p.Id, 0m);
                return new PresupuestoComparacionDto
                {
                    PresupuestoId = p.Id,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria!.Nombre,
                    MontoLimite = p.MontoLimite,
                    GastadoActual = gastado,
                    Disponible = p.MontoLimite - gastado,
                    PorcentajeUtilizado = p.MontoLimite > 0 ? (gastado / p.MontoLimite) * 100 : 0
                };
            }).OrderByDescending(c => c.PorcentajeUtilizado).ToList();

            return new PresupuestoDashboardDto
            {
                Periodo = periodo,
                PeriodoLabel = periodoLabel,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalPresupuestado = comparaciones.Sum(c => c.MontoLimite),
                TotalGastado = comparaciones.Sum(c => c.GastadoActual),
                TotalDisponible = comparaciones.Sum(c => c.MontoLimite) - comparaciones.Sum(c => c.GastadoActual),
                Comparaciones = comparaciones
            };
        }

        /// <summary>
        /// Calcula el rollover (sobrante del periodo anterior) para un presupuesto.
        /// Retorna el monto no gastado del periodo inmediatamente anterior.
        /// </summary>
        private async Task<decimal> CalcularRolloverAsync(string userId, Presupuesto presupuesto)
        {
            if (!presupuesto.PermiteRollover)
                return 0;

            // Crear un presupuesto "virtual" del periodo anterior para calcular su rango
            var anterior = new Presupuesto
            {
                CategoriaId = presupuesto.CategoriaId,
                MontoLimite = presupuesto.MontoLimite,
                Periodo = presupuesto.Periodo,
                MesAplicable = presupuesto.MesAplicable,
                AnoAplicable = presupuesto.AnoAplicable,
                SemanaAplicable = presupuesto.SemanaAplicable,
                UserId = userId
            };

            // Retroceder un periodo
            switch (presupuesto.Periodo)
            {
                case "Semanal":
                    var semana = (anterior.SemanaAplicable ?? 1) - 1;
                    if (semana < 1)
                    {
                        anterior.AnoAplicable--;
                        semana = GetISOWeekNumber(new DateTime(anterior.AnoAplicable, 12, 28));
                    }
                    anterior.SemanaAplicable = semana;
                    break;
                case "Quincenal":
                    // No retroceder, la quincena actual se determina por el día actual
                    // Invertir: si estamos en primera quincena, el anterior es la segunda del mes pasado
                    var dia = DateTime.UtcNow.Day;
                    if (dia <= 15)
                    {
                        // Anterior = segunda quincena del mes pasado
                        if (anterior.MesAplicable == 1)
                        {
                            anterior.MesAplicable = 12;
                            anterior.AnoAplicable--;
                        }
                        else
                        {
                            anterior.MesAplicable--;
                        }
                    }
                    // Si dia > 15, anterior = primera quincena del mismo mes (se calcula automáticamente)
                    break;
                case "Mensual":
                    if (anterior.MesAplicable == 1)
                    {
                        anterior.MesAplicable = 12;
                        anterior.AnoAplicable--;
                    }
                    else
                    {
                        anterior.MesAplicable--;
                    }
                    break;
                case "Trimestral":
                    anterior.MesAplicable -= 3;
                    if (anterior.MesAplicable < 1)
                    {
                        anterior.MesAplicable += 12;
                        anterior.AnoAplicable--;
                    }
                    break;
                case "Semestral":
                    anterior.MesAplicable -= 6;
                    if (anterior.MesAplicable < 1)
                    {
                        anterior.MesAplicable += 12;
                        anterior.AnoAplicable--;
                    }
                    break;
                case "Anual":
                    anterior.AnoAplicable--;
                    break;
            }

            var (inicioAnterior, finAnterior) = CalcularRangoFechas(anterior);

            var gastadoAnterior = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           g.CategoriaId == presupuesto.CategoriaId &&
                           g.Fecha >= inicioAnterior &&
                           g.Fecha <= finAnterior)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var sobrante = presupuesto.MontoLimite - gastadoAnterior;
            return sobrante > 0 ? sobrante : 0;
        }

        /// <summary>
        /// Calcula el gastado para un solo presupuesto
        /// </summary>
        public async Task<decimal> CalcularGastadoActual(string userId, Presupuesto presupuesto)
        {
            var (inicio, fin) = CalcularRangoFechas(presupuesto);

            var gastado = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           g.CategoriaId == presupuesto.CategoriaId &&
                           g.Fecha >= inicio &&
                           g.Fecha <= fin)
                .SumAsync(g => g.Monto);

            return gastado;
        }

        /// <summary>
        /// Calcula el gastado en batch para múltiples presupuestos (evita N+1)
        /// </summary>
        private async Task<Dictionary<int, decimal>> CalcularGastadoBatchAsync(
            string userId, List<Presupuesto> presupuestos)
        {
            // Calcular rangos para cada presupuesto
            var rangos = presupuestos.Select(p => new { p.Id, p.CategoriaId, Rango = CalcularRangoFechas(p) }).ToList();

            var categoriaIds = presupuestos.Select(p => p.CategoriaId).Distinct().ToList();
            var fechaMinima = rangos.Min(r => r.Rango.inicio);
            var fechaMaxima = rangos.Max(r => r.Rango.fin);

            var gastos = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           categoriaIds.Contains(g.CategoriaId) &&
                           g.Fecha >= fechaMinima && g.Fecha <= fechaMaxima)
                .Select(g => new { g.CategoriaId, g.Fecha, g.Monto })
                .ToListAsync();

            var result = new Dictionary<int, decimal>();

            foreach (var rango in rangos)
            {
                result[rango.Id] = gastos
                    .Where(g => g.CategoriaId == rango.CategoriaId &&
                               g.Fecha >= rango.Rango.inicio &&
                               g.Fecha <= rango.Rango.fin)
                    .Sum(g => g.Monto);
            }

            return result;
        }
    }
}
