using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para actualizar el perfil del usuario.
    /// </summary>
    public class ActualizarPerfilDto
    {
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string? NombreCompleto { get; set; }
    }
}
