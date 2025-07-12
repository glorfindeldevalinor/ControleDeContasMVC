using ControleDeContasMVC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleDeContasMVC.Repositories
{
    public interface IContaRepository
    {
    
        Task<IEnumerable<Conta>> GetByMonthYearAsync(int mes, int ano, string userId);
        Task<Conta?> GetByIdAsync(int id, string userId); // Adicionar userId aqui também por segurança
        Task AddAsync(Conta conta);
        Task AddRangeAsync(IEnumerable<Conta> contas);
        void Update(Conta conta);
        void Delete(Conta conta);
        Task<int> SaveChangesAsync();

    }
}