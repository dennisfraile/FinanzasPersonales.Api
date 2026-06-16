using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Hash SHA-256 (base64) del token. NUNCA se guarda el valor en claro.</summary>
        [Required]
        [StringLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresUtc { get; set; }
        public DateTime? RevokedUtc { get; set; }

        /// <summary>Hash del token que reemplazó a éste (para detección de reuso).</summary>
        [StringLength(64)]
        public string? ReplacedByTokenHash { get; set; }

        public bool IsActive => RevokedUtc == null && DateTime.UtcNow < ExpiresUtc;
    }
}
