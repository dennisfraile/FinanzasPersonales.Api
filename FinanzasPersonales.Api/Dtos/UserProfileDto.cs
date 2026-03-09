namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para respuesta con informacion del perfil del usuario
    /// </summary>
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? FotoUrl { get; set; }
    }

    /// <summary>
    /// DTO para actualizar el perfil del usuario
    /// </summary>
    public class UpdateUserProfileDto
    {
        public string? UserName { get; set; }
    }
}
