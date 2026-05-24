using Microsoft.AspNetCore.Identity;

namespace energia_que_compensa.Models
{
    /// <summary>
    /// Usuário da aplicação. Herda de IdentityUser (que já traz Email, PasswordHash, etc.)
    /// e permite adicionar campos customizados no futuro (ex: plano, cidade, etc.).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
