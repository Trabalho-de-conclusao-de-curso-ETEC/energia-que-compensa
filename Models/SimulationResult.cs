using System.Collections.Generic;

namespace energia_que_compensa.Models
{
    public class SimulationResult
    {
        public double TotalKwh { get; set; }
        public double TotalCost { get; set; }
        public double TariffRate { get; set; }
        public double CarbonReductionKg { get; set; }
        public int EfficiencyScore { get; set; } // 0 to 100
        public string EfficiencyCategory { get; set; } = string.Empty; // Baixo, Moderado, Alto, Crítico
        public SolarRecommendation SolarRecommendation { get; set; } = new();
        public List<Appliance> Appliances { get; set; } = new();
    }
}
