using ControleDeContasMVC.Models;
using ControleDeContasMVC.Services;
using ControleDeContasMVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ControleDeContasMVC.Controllers
{
    [Authorize] // Garante que só usuários logados acessem qualquer ação deste controller
    public class ContasController : Controller
    {
        private readonly IContaService _contaService;
        private readonly IContaRepository _contaRepository;

        public ContasController(IContaService contaService, IContaRepository contaRepository)
        {
            _contaService = contaService;
            _contaRepository = contaRepository;
        }

        // Helper privado para pegar o ID do usuário logado de forma limpa
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: /Contas/
        public async Task<IActionResult> Index(int? mes, int? ano)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var dataFiltro = DateTime.Now;
            if (mes.HasValue && ano.HasValue)
            {
                dataFiltro = new DateTime(ano.Value, mes.Value, 1);
            }
            ViewBag.DataFiltro = dataFiltro;

            var contas = await _contaService.GetContasDoMesAsync(dataFiltro.Month, dataFiltro.Year, userId);

            ViewBag.TotalDespesas = contas.Where(c => c.Tipo == TipoConta.Despesa).Sum(c => c.Valor);
            ViewBag.TotalProventos = contas.Where(c => c.Tipo == TipoConta.Provento).Sum(c => c.Valor);
            ViewBag.Balanco = ViewBag.TotalProventos - ViewBag.TotalDespesas;

            return View(contas);
        }

        // GET: Contas/Criar
        public IActionResult Criar()
        {
            var conta = new Conta { DataVencimento = DateTime.Now };
            return View(conta);
        }

        // POST: Contas/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Nome,Valor,Tipo,DataVencimento,Status,Observacao,DebitoAutomatico,ValorFixo,JaNoCartao")] Conta conta)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (ModelState.IsValid)
            {
                conta.UserId = userId; // Atribui o "dono" da conta
                conta.Ativo = true;

                await _contaRepository.AddAsync(conta);
                await _contaRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(conta);
        }

        // GET: Contas/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var conta = await _contaRepository.GetByIdAsync(id.Value, userId);

            if (conta == null) return NotFound(); // Se a conta não existe ou não pertence ao usuário

            return View(conta);
        }

        // POST: Contas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Valor,Tipo,DataVencimento,DataPagamento,DataAgendamento,Status,Observacao,DebitoAutomatico,ValorFixo,Ativo,Ordem,JaNoCartao")] Conta conta)
        {
            if (id != conta.Id) return NotFound();

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Garante que o UserId não seja perdido no processo de bind
            conta.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    _contaRepository.Update(conta);
                    await _contaRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _contaRepository.GetByIdAsync(conta.Id, userId) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(conta);
        }

        // POST: Contas/Deletar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletar(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var conta = await _contaRepository.GetByIdAsync(id, userId);
            if (conta != null)
            {
                _contaRepository.Delete(conta);
                await _contaRepository.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Ações de Lógica de Negócio (via Service) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarNovoMes(int mes, int ano)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _contaService.CriarNovoMesAsync(mes, ano, userId);
            var proximoMes = (new DateTime(ano, mes, 1)).AddMonths(1);
            return RedirectToAction(nameof(Index), new { mes = proximoMes.Month, ano = proximoMes.Year });
        }

        public IActionResult DownloadTemplate()
        {
            return _contaService.GerarTemplateCsv();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var (sucesso, mensagem) = await _contaService.ProcessarUploadCsvAsync(csvFile, userId);
            if (sucesso)
            {
                TempData["SuccessMessage"] = mensagem;
            }
            else
            {
                TempData["ErrorMessage"] = mensagem;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}