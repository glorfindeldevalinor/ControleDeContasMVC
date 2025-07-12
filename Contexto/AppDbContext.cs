using ControleDeContasMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControleDeContasMVC.Contexto
{
    // Mude de 'DbContext' para 'IdentityDbContext<IdentityUser>'
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Conta> Contas { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}