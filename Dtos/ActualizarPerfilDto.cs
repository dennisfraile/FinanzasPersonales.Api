using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para actualizar el perfil del usuario.
    /// </summary>
    public class ActualizarPerfilDto
    {
        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public required string Email { get; set; }
    }
}
