using ControleDeContasMVC.Contexto;
using ControleDeContasMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeContasMVC.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly AppDbContext _context;

        public ContaRepository(AppDbContext context)
        {
            _context = context;
        }

        // Em ContaRepository.cs
        public async Task<IEnumerable<Conta>> GetByMonthYearAsync(int mes, int ano, string userId)
        {
            return await _context.Contas
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.Ativo && c.DataVencimento.Month == mes && c.DataVencimento.Year == ano) // FILTRO AQUI
                .OrderBy(c => c.DataVencimento)
                .ToListAsync();
        }

        public async Task<Conta?> GetByIdAsync(int id, string userId)
        {
            return await _context.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId); // FILTRO AQUI
        }

        public async Task AddAsync(Conta conta)
        {
            await _context.Contas.AddAsync(conta);
        }

        public async Task AddRangeAsync(IEnumerable<Conta> contas)
        {
            await _context.Contas.AddRangeAsync(contas);
        }

        public void Update(Conta conta)
        {
            _context.Contas.Update(conta);
        }

        public void Delete(Conta conta)
        {
            conta.Ativo = false;
            _context.Contas.Update(conta);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}