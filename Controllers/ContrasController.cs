using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using ControleDeContasMVC.Contexto;
using ControleDeContasMVC.Models;
using Microsoft.AspNetCore.Authorization;

// ---------------------------------------------------------------------------------
// ARQUIVO: Controllers/ContasController.cs
// ---------------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Text;


namespace ControleDeContasMVC.Controllers;

[Authorize]
public class ContasController : Controller
{
    private readonly AppDbContext _context;

    public ContasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Contas/Index ou /Contas
    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        var dataFiltro = DateTime.Now;
        if (mes.HasValue && ano.HasValue)
        {
            // Garante que o dia seja 1 para evitar erros com meses de 30/31 dias
            dataFiltro = new DateTime(ano.Value, mes.Value, 1);
        }

        ViewBag.DataFiltro = dataFiltro;

        var contas = await _context.Contas
            .Where(c => c.Ativo && c.DataVencimento.Month == dataFiltro.Month && c.DataVencimento.Year == dataFiltro.Year)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();

        // Cálculos para o Balanço
        ViewBag.TotalDespesas = contas.Where(c => c.Tipo == TipoConta.Despesa).Sum(c => c.Valor);
        ViewBag.TotalProventos = contas.Where(c => c.Tipo == TipoConta.Provento).Sum(c => c.Valor);
        ViewBag.Balanco = ViewBag.TotalProventos - ViewBag.TotalDespesas;

        return View(contas);
    }

    // GET: /Contas/Criar
    public IActionResult Criar()
    {
        // Define valores padrão para o formulário
        var conta = new Conta
        {
            DataVencimento = DateTime.Now
        };
        return View(conta);
    }

    // POST: /Contas/Criar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar([Bind("Nome,Valor,Tipo,DataVencimento,Status,Observacao,DebitoAutomatico,ValorFixo")] Conta conta)
    {
        if (ModelState.IsValid)
        {
            conta.Ativo = true;
            if (conta.Status == StatusConta.Pago)
            {
                conta.DataPagamento = DateTime.Now;
            }
            _context.Add(conta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(conta);
    }

    // GET: /Contas/Editar/5
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null) return NotFound();
        var conta = await _context.Contas.FindAsync(id);
        if (conta == null) return NotFound();
        return View(conta);
    }

    // POST: /Contas/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Valor,Tipo,DataVencimento,DataPagamento,Status,Observacao,DebitoAutomatico,ValorFixo,Ativo")] Conta conta)
    {
        if (id != conta.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (conta.Status == StatusConta.Pago && !conta.DataPagamento.HasValue)
                {
                    conta.DataPagamento = DateTime.Now;
                }
                else if (conta.Status == StatusConta.Pendente)
                {
                    conta.DataPagamento = null;
                }
                _context.Update(conta);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Contas.Any(e => e.Id == conta.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(conta);
    }

    // POST: /Contas/Deletar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deletar(int id)
    {
        var conta = await _context.Contas.FindAsync(id);
        if (conta != null)
        {
            conta.Ativo = false; // Soft delete
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: /Contas/CriarNovoMes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarNovoMes(int mes, int ano)
    {
        var dataBase = new DateTime(ano, mes, 1);
        var proximoMes = dataBase.AddMonths(1);

        var contasParaReplicar = await _context.Contas
            .AsNoTracking()
            .Where(c => c.Ativo && c.DataVencimento.Month == dataBase.Month && c.DataVencimento.Year == dataBase.Year)
            .ToListAsync();

        foreach (var conta in contasParaReplicar)
        {
            var novaConta = new Conta
            {
                Nome = conta.Nome,
                Tipo = conta.Tipo,
                Valor = conta.ValorFixo ? conta.Valor : 0,
                DataVencimento = new DateTime(proximoMes.Year, proximoMes.Month, conta.DataVencimento.Day),
                Observacao = conta.Observacao,
                DebitoAutomatico = conta.DebitoAutomatico,
                ValorFixo = conta.ValorFixo,
                Status = StatusConta.Pendente,
                Ativo = true
            };
            _context.Contas.Add(novaConta);
        }
        await _context.SaveChangesAsync();

        // Redireciona para o novo mês criado
        return RedirectToAction(nameof(Index), new { mes = proximoMes.Month, ano = proximoMes.Year });
    }

    // Ação para baixar o arquivo de template CSV
    public IActionResult DownloadTemplate()
    {
        // Criando uma lista de contas de exemplo
        var exemplos = new List<Conta>
    {
        new Conta { Nome = "Aluguel", Valor = 2500.50M, Tipo = TipoConta.Despesa, DataVencimento = DateTime.Now.AddDays(5), Status = StatusConta.Pendente, Observacao = "Exemplo", DebitoAutomatico = false, ValorFixo = true },
        new Conta { Nome = "Salário", Valor = 8500.00M, Tipo = TipoConta.Provento, DataVencimento = DateTime.Now, Status = StatusConta.Pago, Observacao = "Exemplo", DebitoAutomatico = false, ValorFixo = true }
    };

        var builder = new StringBuilder();
        using (var writer = new StringWriter(builder))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(exemplos);
        }

        return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "template_contas.csv");
    }


    // Ação para processar o upload do arquivo CSV
    [HttpPost]
    public async Task<IActionResult> UploadCsv(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Por favor, selecione um arquivo CSV.";
            return RedirectToAction(nameof(Index));
        }

        var novasContas = new List<Conta>();

        try
        {
            using (var reader = new StreamReader(csvFile.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // O CsvHelper lê os registros e os mapeia para a classe Conta automaticamente
                var records = csv.GetRecords<Conta>().ToList();

                // A lógica da 'Ordem' foi removida daqui

                foreach (var record in records)
                {
                    // Definimos propriedades padrão que não vêm do CSV
                    record.Ativo = true;

                    // Poderíamos adicionar outras validações aqui se necessário
                    novasContas.Add(record);
                }
            }

            await _context.Contas.AddRangeAsync(novasContas);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{novasContas.Count} contas foram importadas com sucesso!";
        }
        catch (Exception ex)
        {
            // Em um app real, logaríamos o erro 'ex'
            TempData["ErrorMessage"] = "Ocorreu um erro ao processar o arquivo. Verifique se o formato está correto.";
        }

        return RedirectToAction(nameof(Index));
    }
}
