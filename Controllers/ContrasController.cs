using ControleDeContasMVC.Contexto;
using ControleDeContasMVC.Models;
// ---------------------------------------------------------------------------------
// ARQUIVO: Controllers/ContasController.cs
// ---------------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ControleDeContasMVC.Controllers;

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
}
