using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para cambiar la contraseña del usuario.
    /// </summary>
    public class CambiarPasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida.")]
        public required string PasswordActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public required string PasswordNuevo { get; set; }

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [Compare("PasswordNuevo", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmarPassword { get; set; }
    }
}
