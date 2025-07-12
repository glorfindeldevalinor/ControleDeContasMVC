using ControleDeContasMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleDeContasMVC.Services
{
    public interface IContaService
    {
        Task<IEnumerable<Conta>> GetContasDoMesAsync(int mes, int ano, string userId);
        FileContentResult GerarTemplateCsv();
        Task<(bool Sucesso, string Mensagem)> ProcessarUploadCsvAsync(IFormFile csvFile, string userId);        
        Task CriarNovoMesAsync(int mes, int ano, string userId);
    }
}