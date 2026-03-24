using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api.Jobs
{
    /// <summary>
    /// Job de Hangfire que verifica alertas de presupuestos y metas diariamente.
    /// </summary>
    public class NotificacionesJob
    {
        private readonly FinanzasDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificacionService _notificacionService;
        private readonly IGastosProgramadosService _gastosProgramadosService;
        private readonly ILogger<NotificacionesJob> _logger;

        public NotificacionesJob(
            FinanzasDbContext context,
            IEmailService emailService,
            INotificacionService notificacionService,
            IGastosProgramadosService gastosProgramadosService,
            ILogger<NotificacionesJob> logger)
        {
            _context = context;
            _emailService = emailService;
            _notificacionService = notificacionService;
            _gastosProgramadosService = gastosProgramadosService;
            _logger = logger;
        }

        /// <summary>
        /// Umbrales progresivos para alertas de presupuesto
        /// </summary>
        private static readonly int[] UmbralesProgresivos = { 50, 80, 95, 100 };

        /// <summary>
        /// Verifica presupuestos cercanos al límite con alertas progresivas (50%, 80%, 95%, 100%)
        /// e incluye gastos programados pendientes (comprometidos) en el cálculo.
        /// </summary>
        public async Task VerificarAlertasPresupuestosAsync()
        {
            _logger.LogInformation("Iniciando verificación de alertas de presupuestos (con umbrales progresivos)...");

            var mesActual = DateTime.Now.Month;
            var anoActual = DateTime.Now.Year;

            var presupuestos = await _context.Presupuestos
                .Include(p => p.Categoria)
                .Include(p => p.User)
                .Where(p => p.MesAplicable == mesActual && p.AnoAplicable == anoActual)
                .ToListAsync();

            foreach (var presupuesto in presupuestos)
            {
                var (inicio, fin) = PresupuestosService.CalcularRangoFechas(presupuesto);

                // Calcular gasto actual (ya registrado)
                var gastadoActual = await _context.Gastos
                    .Where(g => g.UserId == presupuesto.UserId
                               && g.CategoriaId == presupuesto.CategoriaId
                               && g.Fecha >= inicio && g.Fecha <= fin)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

                // Calcular gasto comprometido (programados pendientes en el rango)
                var comprometido = await _gastosProgramadosService.GetTotalComprometidoAsync(
                    presupuesto.UserId, presupuesto.CategoriaId, inicio, fin);

                var totalProyectado = gastadoActual + comprometido;
                var porcentajeReal = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100 : 0;
                var porcentajeProyectado = presupuesto.MontoLimite > 0
                    ? (totalProyectado / presupuesto.MontoLimite) * 100 : 0;

                // Determinar el umbral más alto alcanzado
                var umbralAlcanzado = UmbralesProgresivos
                    .Where(u => porcentajeProyectado >= u)
                    .DefaultIfEmpty(0)
                    .Max();

                if (umbralAlcanzado == 0)
                    continue;

                // Verificar si ya se envió notificación para este umbral hoy
                var hoy = DateTime.UtcNow.Date;
                var yaNotificado = await _context.Notificaciones
                    .AnyAsync(n => n.UserId == presupuesto.UserId
                        && n.Tipo == "PresupuestoAlerta"
                        && n.ReferenciaId == presupuesto.Id
                        && n.FechaCreacion >= hoy
                        && n.DatosAdicionales != null
                        && n.DatosAdicionales.Contains($"\"umbral\":{umbralAlcanzado}"));

                if (yaNotificado)
                    continue;

                var categoriaNombre = presupuesto.Categoria?.Nombre ?? "Categoría";
                var nivelAlerta = umbralAlcanzado switch
                {
                    >= 100 => "Límite alcanzado",
                    >= 95 => "Casi agotado",
                    >= 80 => "Cuidado",
                    _ => "Aviso"
                };

                var mensaje = $"{nivelAlerta}: Has utilizado {porcentajeReal:N1}% de tu presupuesto de {categoriaNombre} ({gastadoActual:C} de {presupuesto.MontoLimite:C}).";
                if (comprometido > 0)
                    mensaje += $" Además, tienes {comprometido:C} comprometidos en gastos programados pendientes (total proyectado: {porcentajeProyectado:N1}%).";

                await _notificacionService.CrearNotificacionAsync(
                    presupuesto.UserId,
                    "PresupuestoAlerta",
                    $"Presupuesto {categoriaNombre}: {nivelAlerta} ({porcentajeProyectado:N0}%)",
                    mensaje,
                    presupuesto.Id,
                    $"{{\"umbral\":{umbralAlcanzado},\"porcentajeReal\":{porcentajeReal:N1},\"porcentajeProyectado\":{porcentajeProyectado:N1},\"comprometido\":{comprometido}}}"
                );

                // Enviar email solo en umbrales altos (80%+)
                if (umbralAlcanzado >= 80)
                {
                    var email = presupuesto.User?.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        await _emailService.SendAlertaPresupuestoAsync(
                            email,
                            categoriaNombre,
                            gastadoActual,
                            presupuesto.MontoLimite,
                            porcentajeProyectado
                        );
                    }
                }

                _logger.LogInformation("Alerta presupuesto {Id} ({Categoria}): {Nivel} - real {PReal:N1}%, proyectado {PProy:N1}%",
                    presupuesto.Id, categoriaNombre, nivelAlerta, porcentajeReal, porcentajeProyectado);
            }

            _logger.LogInformation("Verificación de presupuestos completada.");
        }

        /// <summary>
        /// Verifica metas próximas a vencer y envía recordatorios
        /// </summary>
        public async Task VerificarAlertasMetasAsync()
        {
            _logger.LogInformation("Iniciando verificación de alertas de metas...");

            var hoy = DateTime.Now.Date;
            var dentroDeUnaSemana = hoy.AddDays(7);

            // No hay FechaObjetivo en el modelo Meta actual, usando una alternativa
            // En una implementación completa, se agregaría esta propiedad al modelo
            _logger.LogInformation("Verificación de metas completada (modelo actual no tiene FechaObjetivo).");
        }

        /// <summary>
        /// Verifica gastos inusuales comparando contra el promedio por categoría
        /// </summary>
        public async Task VerificarGastosInusualesAsync()
        {
            _logger.LogInformation("Verificando gastos inusuales...");

            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.GastosInusuales)
                .ToListAsync();

            foreach (var config in configs)
            {
                var hace30Dias = DateTime.UtcNow.AddDays(-30);
                var hoy = DateTime.UtcNow.Date;

                // Promedios por categoría del último mes
                var promedios = await _context.Gastos
                    .Where(g => g.UserId == config.UserId && g.Fecha >= hace30Dias && g.Fecha < hoy)
                    .GroupBy(g => g.CategoriaId)
                    .Select(g => new { CategoriaId = g.Key, Promedio = g.Average(x => x.Monto) })
                    .ToListAsync();

                // Gastos de hoy
                var gastosHoy = await _context.Gastos
                    .Where(g => g.UserId == config.UserId && g.Fecha.Date == hoy)
                    .Include(g => g.Categoria)
                    .ToListAsync();

                foreach (var gasto in gastosHoy)
                {
                    var promedio = promedios.FirstOrDefault(p => p.CategoriaId == gasto.CategoriaId)?.Promedio ?? 0;
                    if (promedio > 0 && gasto.Monto > promedio * config.FactorGastoInusual)
                    {
                        await _notificacionService.CrearNotificacionAsync(
                            config.UserId,
                            "GastoInusual",
                            $"Gasto inusual en {gasto.Categoria?.Nombre}",
                            $"Registraste un gasto de {gasto.Monto:C} que es {(gasto.Monto / promedio):N1}x mayor al promedio ({promedio:C})."
                        );
                    }
                }
            }

            _logger.LogInformation("Verificación de gastos inusuales completada.");
        }

        /// <summary>
        /// Verifica pagos recurrentes próximos y envía recordatorios
        /// </summary>
        public async Task VerificarPagosRecurrentesAsync()
        {
            _logger.LogInformation("Verificando pagos recurrentes próximos...");

            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.AlertaPagoRecurrente)
                .ToListAsync();

            foreach (var config in configs)
            {
                var limite = DateTime.UtcNow.AddDays(config.DiasAntesPagoRecurrente);

                var gastosProximos = await _context.GastosRecurrentes
                    .Where(gr => gr.UserId == config.UserId && gr.Activo && gr.ProximaFecha <= limite && gr.ProximaFecha >= DateTime.UtcNow)
                    .Include(gr => gr.Categoria)
                    .ToListAsync();

                foreach (var gasto in gastosProximos)
                {
                    await _notificacionService.CrearNotificacionAsync(
                        config.UserId,
                        "Informativa",
                        $"Pago próximo: {gasto.Descripcion}",
                        $"Tu gasto recurrente de {gasto.Monto:C} ({gasto.Categoria?.Nombre}) vence el {gasto.ProximaFecha:dd/MM/yyyy}."
                    );
                }
            }

            _logger.LogInformation("Verificación de pagos recurrentes completada.");
        }

        /// <summary>
        /// Verifica si el balance de alguna cuenta está por debajo del umbral configurado
        /// </summary>
        public async Task VerificarBalanceBajoAsync()
        {
            _logger.LogInformation("Verificando balances bajos...");

            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.AlertaBalanceBajo && c.UmbralBalanceBajo > 0)
                .ToListAsync();

            foreach (var config in configs)
            {
                var cuentasBajas = await _context.Cuentas
                    .Where(c => c.UserId == config.UserId && c.Activa && c.BalanceActual < config.UmbralBalanceBajo)
                    .ToListAsync();

                foreach (var cuenta in cuentasBajas)
                {
                    await _notificacionService.CrearNotificacionAsync(
                        config.UserId,
                        "Informativa",
                        $"Balance bajo: {cuenta.Nombre}",
                        $"Tu cuenta '{cuenta.Nombre}' tiene un balance de {cuenta.BalanceActual:C}, por debajo del umbral de {config.UmbralBalanceBajo:C}."
                    );
                }
            }

            _logger.LogInformation("Verificación de balances bajos completada.");
        }

        /// <summary>
        /// Verifica surplus quincenal y notifica al usuario para que lo asigne
        /// </summary>
        public async Task VerificarSurplusQuincenalAsync()
        {
            var hoy = DateTime.UtcNow;

            // Solo ejecutar el día 16 (fin Q1) y el día 1 (fin Q2 del mes anterior)
            if (hoy.Day != 1 && hoy.Day != 16)
                return;

            _logger.LogInformation("Verificando surplus quincenal...");

            // Determinar la quincena que acaba de terminar
            DateTime inicioQuincena, finQuincena;
            string periodo;

            if (hoy.Day == 16)
            {
                // Acaba de terminar Q1 (días 1-15 del mes actual)
                inicioQuincena = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                finQuincena = new DateTime(hoy.Year, hoy.Month, 15, 23, 59, 59, DateTimeKind.Utc);
                periodo = $"Q1-{hoy.Year}-{hoy.Month:D2}";
            }
            else
            {
                // Día 1: acaba de terminar Q2 del mes anterior
                var mesAnterior = hoy.AddMonths(-1);
                inicioQuincena = new DateTime(mesAnterior.Year, mesAnterior.Month, 16, 0, 0, 0, DateTimeKind.Utc);
                finQuincena = new DateTime(mesAnterior.Year, mesAnterior.Month,
                    DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month), 23, 59, 59, DateTimeKind.Utc);
                periodo = $"Q2-{mesAnterior.Year}-{mesAnterior.Month:D2}";
            }

            // Obtener cuentas activas con ingresos recurrentes
            var cuentasConRecurrentes = await _context.IngresosRecurrentes
                .Where(ir => ir.Activo && ir.CuentaId.HasValue)
                .Select(ir => new { ir.UserId, CuentaId = ir.CuentaId!.Value })
                .Distinct()
                .ToListAsync();

            foreach (var item in cuentasConRecurrentes)
            {
                // Verificar si ya se envió notificación para este periodo y cuenta
                var yaNotificado = await _context.Notificaciones
                    .AnyAsync(n => n.UserId == item.UserId
                        && n.Tipo == "SurplusQuincenal"
                        && n.ReferenciaId == item.CuentaId
                        && n.DatosAdicionales != null && n.DatosAdicionales.Contains(periodo));

                if (yaNotificado)
                    continue;

                // Calcular surplus
                var ingresos = await _context.Ingresos
                    .Where(i => i.UserId == item.UserId && i.CuentaId == item.CuentaId
                        && i.Fecha >= inicioQuincena && i.Fecha <= finQuincena)
                    .SumAsync(i => (decimal?)i.Monto) ?? 0;

                var gastos = await _context.Gastos
                    .Where(g => g.UserId == item.UserId && g.CuentaId == item.CuentaId
                        && g.Fecha >= inicioQuincena && g.Fecha <= finQuincena)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

                var surplus = ingresos - gastos;

                if (surplus > 0)
                {
                    var cuenta = await _context.Cuentas.FindAsync(item.CuentaId);
                    var nombreCuenta = cuenta?.Nombre ?? "tu cuenta";

                    await _notificacionService.CrearNotificacionAsync(
                        item.UserId,
                        "SurplusQuincenal",
                        $"Sobrante disponible en {nombreCuenta}",
                        $"Tu quincena terminó con un sobrante de ${surplus:F2}. Decide si quieres ahorrarlo o asignarlo a una meta.",
                        item.CuentaId,
                        $"{{\"periodo\":\"{periodo}\",\"cuentaId\":{item.CuentaId},\"surplus\":{surplus}}}"
                    );

                    _logger.LogInformation("Notificación de surplus enviada: usuario {UserId}, cuenta {CuentaId}, surplus {Surplus}",
                        item.UserId, item.CuentaId, surplus);
                }
            }

            _logger.LogInformation("Verificación de surplus quincenal completada.");
        }

        /// <summary>
        /// Verifica gastos programados próximos a vencer y envía recordatorios.
        /// También alerta sobre gastos de monto variable que requieren confirmación manual.
        /// </summary>
        public async Task VerificarGastosProgramadosProximosAsync()
        {
            _logger.LogInformation("Verificando gastos programados próximos a vencer...");

            // Buscar todos los gastos programados pendientes en los próximos 3 días (default)
            // para TODOS los usuarios, con o sin configuración
            var defaultDias = 3;

            // Usuarios con configuración personalizada
            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.AlertaPagoRecurrente)
                .ToDictionaryAsync(c => c.UserId, c => c.DiasAntesPagoRecurrente);

            // Todos los gastos programados pendientes próximos a vencer
            var limiteMaximo = DateTime.UtcNow.AddDays(Math.Max(defaultDias, configs.Values.Any() ? configs.Values.Max() : defaultDias));

            var todosProximos = await _context.GastosProgramados
                .Where(gp => gp.Estado == "Pendiente"
                    && gp.FechaVencimiento <= limiteMaximo
                    && gp.FechaVencimiento >= DateTime.UtcNow)
                .Include(gp => gp.Categoria)
                .Include(gp => gp.Cuenta)
                .ToListAsync();

            // Agrupar por usuario
            var porUsuario = todosProximos.GroupBy(gp => gp.UserId);

            foreach (var grupo in porUsuario)
            {
                var userId = grupo.Key;
                var diasAntes = configs.GetValueOrDefault(userId, defaultDias);
                var limite = DateTime.UtcNow.AddDays(diasAntes);

                var programadosProximos = grupo
                    .Where(gp => gp.FechaVencimiento <= limite)
                    .ToList();

                foreach (var gp in programadosProximos)
                {
                    var diasRestantes = (int)(gp.FechaVencimiento.Date - DateTime.UtcNow.Date).TotalDays;

                    // Verificar si ya se envió notificación hoy para este gasto programado
                    var hoy = DateTime.UtcNow.Date;
                    var yaNotificado = await _context.Notificaciones
                        .AnyAsync(n => n.UserId == userId
                            && n.Tipo == "Informativa"
                            && n.ReferenciaId == gp.Id
                            && n.FechaCreacion >= hoy
                            && n.DatosAdicionales != null
                            && n.DatosAdicionales.Contains("\"tipo\":\"gastoProgramadoProximo\""));

                    if (yaNotificado)
                        continue;

                    var tipoMonto = gp.EsMontoVariable ? " (monto variable, requiere confirmación)" : "";
                    var cuentaInfo = gp.Cuenta != null ? $" - Cuenta: {gp.Cuenta.Nombre}" : "";

                    await _notificacionService.CrearNotificacionAsync(
                        userId,
                        "Informativa",
                        $"Pago programado próximo: {gp.Descripcion}",
                        $"Tu gasto de {gp.Monto:C} ({gp.Categoria?.Nombre}) vence en {diasRestantes} día(s) ({gp.FechaVencimiento:dd/MM/yyyy}){tipoMonto}{cuentaInfo}.",
                        gp.Id,
                        $"{{\"tipo\":\"gastoProgramadoProximo\",\"gastoProgramadoId\":{gp.Id},\"diasRestantes\":{diasRestantes},\"esVariable\":{gp.EsMontoVariable.ToString().ToLower()}}}"
                    );
                }
            }

            _logger.LogInformation("Verificación de gastos programados próximos completada.");
        }

        /// <summary>
        /// Marca gastos programados vencidos y procesa cobros automáticos de gastos fijos.
        /// </summary>
        public async Task ProcesarGastosProgramadosAsync()
        {
            _logger.LogInformation("Procesando gastos programados (vencidos y cobros automáticos)...");

            // 1. Procesar cobros automáticos (gastos fijos con cuenta, cuya fecha es hoy)
            var cobrosAutomaticos = await _gastosProgramadosService.ProcesarCobrosAutomaticosAsync();
            if (cobrosAutomaticos > 0)
                _logger.LogInformation("Se procesaron {Count} cobros automáticos", cobrosAutomaticos);

            // 2. Marcar vencidos (gastos cuya fecha ya pasó y siguen pendientes)
            var vencidos = await _gastosProgramadosService.MarcarVencidosAsync();
            if (vencidos > 0)
            {
                _logger.LogInformation("Se marcaron {Count} gastos como vencidos", vencidos);

                // Notificar sobre gastos vencidos
                var gastosVencidosHoy = await _context.GastosProgramados
                    .Where(gp => gp.Estado == "Vencido")
                    .Include(gp => gp.Categoria)
                    .ToListAsync();

                // Solo notificar los recién vencidos (sin notificación previa de vencimiento)
                foreach (var gp in gastosVencidosHoy)
                {
                    var yaNotificadoVencimiento = await _context.Notificaciones
                        .AnyAsync(n => n.UserId == gp.UserId
                            && n.Tipo == "Informativa"
                            && n.ReferenciaId == gp.Id
                            && n.DatosAdicionales != null
                            && n.DatosAdicionales.Contains("\"tipo\":\"gastoProgramadoVencido\""));

                    if (yaNotificadoVencimiento)
                        continue;

                    await _notificacionService.CrearNotificacionAsync(
                        gp.UserId,
                        "Informativa",
                        $"Gasto vencido: {gp.Descripcion}",
                        $"El pago de {gp.Monto:C} ({gp.Categoria?.Nombre}) venció el {gp.FechaVencimiento:dd/MM/yyyy}. Puedes registrar el pago manualmente o cancelarlo.",
                        gp.Id,
                        $"{{\"tipo\":\"gastoProgramadoVencido\",\"gastoProgramadoId\":{gp.Id}}}"
                    );
                }
            }

            _logger.LogInformation("Procesamiento de gastos programados completado.");
        }

        /// <summary>
        /// Método principal que ejecuta todas las verificaciones
        /// </summary>
        public async Task EjecutarVerificacionesAsync()
        {
            _logger.LogInformation("=== Iniciando job de notificaciones ===");

            await VerificarAlertasPresupuestosAsync();
            await VerificarAlertasMetasAsync();
            await VerificarGastosInusualesAsync();
            await VerificarPagosRecurrentesAsync();
            await VerificarBalanceBajoAsync();
            await VerificarSurplusQuincenalAsync();
            await VerificarGastosProgramadosProximosAsync();
            await ProcesarGastosProgramadosAsync();

            _logger.LogInformation("=== Job de notificaciones completado ===");
        }
    }
}
