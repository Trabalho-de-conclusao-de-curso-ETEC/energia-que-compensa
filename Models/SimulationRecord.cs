namespace energia_que_compensa.Models
{
    /// <summary>
    /// Representa uma simulação salva no banco de dados para fins de auditoria (ODS 7)
    /// e geração de relatórios de impacto ambiental.
    /// </summary>
    public class SimulationRecord
    {
        public int Id { get; set; }

        // --- Dados de entrada ---
        public double MonthlyKwh { get; set; }
        public double TariffRate { get; set; }
        public string? Cep { get; set; }
        public string? Uf { get; set; }

        // --- Resultados do cálculo de consumo ---
        public double TotalCost { get; set; }
        public double CarbonReductionKg { get; set; }
        public int EfficiencyScore { get; set; }
        public string EfficiencyCategory { get; set; } = string.Empty;

        // --- Resultados da recomendação solar ---
        public bool SolarIsViable { get; set; }
        public int PanelsCount { get; set; }
        public double SystemSizeKwp { get; set; }
        public double TotalAreaSqM { get; set; }
        public double EstimatedCost { get; set; }
        public double MonthlySavings { get; set; }
        public double PaybackYears { get; set; }
        public double LifetimeSavings { get; set; }
        public double AnnualCo2SavedKg { get; set; }
        public int TreeEquivalent { get; set; }

        // --- Metadados ---
        public DateTime CreatedAt { get; set; }

        // Chave estrangeira opcional → AspNetUsers (usuário logado que fez a simulação)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
