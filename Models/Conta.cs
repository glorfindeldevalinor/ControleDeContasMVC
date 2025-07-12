using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ControleDeContasMVC.Models
{
    public enum StatusConta
    {
        Pendente,
        Pago
    }

    public enum TipoConta
    {
        Despesa,
        Provento, // (Receita / Ganho)
        Lembrete
    }


    public class Conta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        public decimal Valor { get; set; }

        public TipoConta Tipo { get; set; }

        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public DateTime? DataAgendamento { get; set; }
        public string? Observacao { get; set; }
        public StatusConta Status { get; set; }
        public bool DebitoAutomatico { get; set; }
        public bool ValorFixo { get; set; }
        public bool Ativo { get; set; } = true;
        public int Ordem { get; set; } // Lembre-se que adicionamos este campo

        public string? UserId { get; set; } // <--- ADICIONE A INTERROGAÇÃO AQUI
        public virtual IdentityUser? User { get; set; } // <--- E AQUI TAMBÉM
                                                        // *************************************************
    }
}
