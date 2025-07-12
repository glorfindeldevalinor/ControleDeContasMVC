using ControleDeContasMVC.Models;
using ControleDeContasMVC.Repositories;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace ControleDeContasMVC.Services
{
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;

        public ContaService(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        public async Task<IEnumerable<Conta>> GetContasDoMesAsync(int mes, int ano, string userId)
        {
            return await _contaRepository.GetByMonthYearAsync(mes, ano, userId); // PASSA O ID
        }

        public FileContentResult GerarTemplateCsv()
        {
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

            return new FileContentResult(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv")
            {
                FileDownloadName = "template_contas.csv"
            };
        }

        public async Task<(bool Sucesso, string Mensagem)> ProcessarUploadCsvAsync(IFormFile csvFile, string userId)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return (false, "Por favor, selecione um arquivo CSV.");
            }

            var novasContas = new List<Conta>();
            try
            {
                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<Conta>().ToList();
                    foreach (var record in records)
                    {
                        record.Ativo = true;
                        novasContas.Add(record);
                    }
                }

                await _contaRepository.AddRangeAsync(novasContas);
                await _contaRepository.SaveChangesAsync();

                return (true, $"{novasContas.Count} contas foram importadas com sucesso!");
            }
            catch (Exception)
            {
                return (false, "Ocorreu um erro ao processar o arquivo. Verifique se o formato está correto.");
            }
        }

        public async Task CriarNovoMesAsync(int mes, int ano, string userId)
        {
            var dataBase = new DateTime(ano, mes, 1);
            var proximoMes = dataBase.AddMonths(1);

            var contasParaReplicar = await _contaRepository.GetByMonthYearAsync(dataBase.Month, dataBase.Year, userId);
            var novasContas = new List<Conta>();

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
                novasContas.Add(novaConta);
            }

            if (novasContas.Any())
            {
                await _contaRepository.AddRangeAsync(novasContas);
                await _contaRepository.SaveChangesAsync();
            }
        }
    }
}