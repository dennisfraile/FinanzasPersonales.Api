using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "El token de Google es requerido.")]
        public string IdToken { get; set; } = string.Empty;
    }
}
