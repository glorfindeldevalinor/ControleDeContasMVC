using ControleDeContasMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleDeContasMVC.Contexto
{
    public class AppDbContext : DbContext
    {
        // Mapeia a classe 'Conta' para uma tabela 'Contas' no banco de dados
        public DbSet<Conta> Contas { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

    }
}
