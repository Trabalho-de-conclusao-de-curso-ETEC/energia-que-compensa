namespace energia_que_compensa.Models
{
    public class SolarRecommendation
    {
        public bool IsViable { get; set; }
        public int PanelsCount { get; set; }
        public double SystemSizeKwp { get; set; }
        public double TotalAreaSqM { get; set; }
        public double EstimatedCost { get; set; }
        public double MonthlySavings { get; set; }
        public double PaybackYears { get; set; }
        public double LifetimeSavings { get; set; } // 25 years
        public double AnnualCo2SavedKg { get; set; }
        public int TreeEquivalent { get; set; }
        public string RecommendationMessage { get; set; } = string.Empty;
    }
}
