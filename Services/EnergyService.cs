using System;
using System.Collections.Generic;
using System.Linq;
using energia_que_compensa.Models;

namespace energia_que_compensa.Services
{
    public class EnergyService : IEnergyService
    {
        private const double KWH_GENERATION_PER_PANEL = 50.0; // average monthly generation of a 400W solar panel in Brazil
        private const double PANEL_AREA_SQM = 2.0;            // size of a typical 400W panel
        private const double PANEL_CAPACITY_KWP = 0.40;       // 400W = 0.40 kWp
        private const double CO2_EMISSION_FACTOR = 0.10;      // 0.10 kg CO2 per kWh in the Brazilian electricity grid (SIN)
        private const double CO2_ABSORPTION_PER_TREE_YEAR = 15.0; // a typical young tree absorbs 15kg CO2/year

        public SimulationResult RunSimulation(double monthlyKwh, double tariffRate)
        {
            var result = new SimulationResult
            {
                TotalKwh = Math.Round(monthlyKwh, 2),
                TotalCost = Math.Round(monthlyKwh * tariffRate, 2),
                TariffRate = tariffRate
            };


            result.CarbonReductionKg = Math.Round(monthlyKwh * 12 * CO2_EMISSION_FACTOR, 2);

            result.EfficiencyScore = CalculateEfficiencyScore(monthlyKwh);
            result.EfficiencyCategory = GetEfficiencyCategory(monthlyKwh);

            result.SolarRecommendation = CalculateSolarRecommendation(monthlyKwh, tariffRate);

            return result;
        }

        public SimulationResult RunSimulationFromAppliances(List<Appliance> appliances, double tariffRate)
        {
            double totalKwh = appliances.Sum(a => a.CalculateMonthlyKwh());
            var result = RunSimulation(totalKwh, tariffRate);
            result.Appliances = appliances;
            return result;
        }

        public List<Appliance> GetDefaultAppliances()
        {
            return new List<Appliance>
            {
                new Appliance { Name = "Geladeira Frost Free", PowerWatts = 50, Quantity = 1, DailyHours = 24 }, // Continuous average
                new Appliance { Name = "Chuveiro Elétrico", PowerWatts = 5500, Quantity = 1, DailyHours = 1 },   // 1 hour total per day
                new Appliance { Name = "Ar Condicionado", PowerWatts = 1200, Quantity = 1, DailyHours = 6 },
                new Appliance { Name = "Televisão LED", PowerWatts = 90, Quantity = 2, DailyHours = 5 },
                new Appliance { Name = "Computador / Desktop", PowerWatts = 150, Quantity = 1, DailyHours = 5 },
                new Appliance { Name = "Máquina de Lavar Roupa", PowerWatts = 500, Quantity = 1, DailyHours = 1 },
                new Appliance { Name = "Forno Micro-ondas", PowerWatts = 1200, Quantity = 1, DailyHours = 0.3 }, // 18 minutes
                new Appliance { Name = "Iluminação Residencial (LED)", PowerWatts = 100, Quantity = 1, DailyHours = 6 }
            };
        }

        private int CalculateEfficiencyScore(double monthlyKwh)
        {
            if (monthlyKwh <= 100) return 95;
            if (monthlyKwh <= 180) return 80;
            if (monthlyKwh <= 300) return 60;
            if (monthlyKwh <= 500) return 40;
            return 20;
        }

        private string GetEfficiencyCategory(double monthlyKwh)
        {
            if (monthlyKwh <= 120) return "Baixo / Eficiente";
            if (monthlyKwh <= 220) return "Moderado";
            if (monthlyKwh <= 400) return "Alto";
            return "Crítico / Muito Alto";
        }

        private SolarRecommendation CalculateSolarRecommendation(double monthlyKwh, double tariffRate)
        {
            var rec = new SolarRecommendation();

            rec.IsViable = monthlyKwh >= 100;

            if (monthlyKwh <= 0)
            {
                rec.RecommendationMessage = "Nenhum consumo registrado. Adicione aparelhos ou digite seu consumo.";
                return rec;
            }

            rec.PanelsCount = (int)Math.Ceiling(monthlyKwh / KWH_GENERATION_PER_PANEL);
            rec.SystemSizeKwp = Math.Round(rec.PanelsCount * PANEL_CAPACITY_KWP, 2);
            rec.TotalAreaSqM = Math.Round(rec.PanelsCount * PANEL_AREA_SQM, 2);

            rec.EstimatedCost = Math.Round((rec.PanelsCount * 2200.0) + 3500.0, 2);

            double currentMonthlyCost = monthlyKwh * tariffRate;
            rec.MonthlySavings = Math.Round(currentMonthlyCost * 0.95, 2);

            if (rec.MonthlySavings > 0)
            {
                rec.PaybackYears = Math.Round(rec.EstimatedCost / (rec.MonthlySavings * 12.0), 1);
            }

            rec.LifetimeSavings = Math.Round((rec.MonthlySavings * 12.0 * 25.0) - rec.EstimatedCost, 2);

            rec.AnnualCo2SavedKg = Math.Round(monthlyKwh * 12.0 * CO2_EMISSION_FACTOR, 1);
            rec.TreeEquivalent = (int)Math.Ceiling(rec.AnnualCo2SavedKg / CO2_ABSORPTION_PER_TREE_YEAR);
            if (rec.IsViable)
            {
                rec.RecommendationMessage = $"Recomendado: Sistema Solar Fotovoltaico de {rec.SystemSizeKwp} kWp. " +
                    $"Com {rec.PanelsCount} painéis solares, você gerará energia limpa suficiente para reduzir sua conta " +
                    $"em até 95%. O retorno do seu investimento ocorrerá em aproximadamente {rec.PaybackYears} anos, " +
                    $"gerando mais de R$ {rec.LifetimeSavings:N2} em economia ao longo de 25 anos!";
            }
            else
            {
                rec.RecommendationMessage = "Para faturas abaixo de 100 kWh/mês, sistemas solares demoram muito para se pagar " +
                    "devido ao custo mínimo de disponibilidade cobrado pelas distribuidoras. " +
                    "Recomendamos focar em hábitos de eficiência energética e substituição de aparelhos antigos por modelos com selo Procel A.";
            }

            return rec;
        }
    }
}
