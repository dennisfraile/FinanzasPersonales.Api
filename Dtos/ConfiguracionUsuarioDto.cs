using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para configuración del usuario.
    /// </summary>
    public class ConfiguracionUsuarioDto
    {
        [StringLength(10)]
        public string Moneda { get; set; } = "USD";

        [StringLength(5)]
        public string SimboloMoneda { get; set; } = "$";

        [StringLength(5)]
        public string Idioma { get; set; } = "es";

        [StringLength(20)]
        public string Tema { get; set; } = "light";

        [Range(1, 31)]
        public int DiaInicioMes { get; set; } = 1;

        public bool MostrarSaldoInicial { get; set; } = true;
    }
}
