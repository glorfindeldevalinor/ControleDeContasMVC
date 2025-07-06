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
        Provento // (Receita / Ganho)
    }

    public class Conta
    {
        public int Id { get; set; } // Chave primária

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        public decimal Valor { get; set; }

        public TipoConta Tipo { get; set; } // Para diferenciar despesas de proventos

        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; } // Nullable, pois pode não ter sido pago ainda

        public DateTime? DataAgendamento { get; set; } // Nullable

        public string? Observacao { get; set; }

        public StatusConta Status { get; set; }

        public bool DebitoAutomatico { get; set; }

        public bool ValorFixo { get; set; } // Para a funcionalidade de "Criar Mês"

        public bool Ativo { get; set; } = true; // Para o soft delete. True = visível, False = oculto
    }
}
