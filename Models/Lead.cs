namespace energia_que_compensa.Models
{
    /// <summary>
    /// Representa um lead: usuário que preencheu o formulário de contato
    /// pedindo orçamento para instalação solar.
    /// </summary>
    public class Lead
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Cep { get; set; }

        // consumo estimado informado pelo usuário no simulador
        public double? EstimatedMonthlyKwh { get; set; }

        // mensagem adicional do formulário de contato
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
