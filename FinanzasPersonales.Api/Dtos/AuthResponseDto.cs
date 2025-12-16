namespace FinanzasPersonales.Api.Dtos
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; } // El '?' permite que sea nulo (ej. si falla el login)
    }
}
