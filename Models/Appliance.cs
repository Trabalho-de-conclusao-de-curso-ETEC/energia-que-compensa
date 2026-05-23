using System.ComponentModel.DataAnnotations;

namespace energia_que_compensa.Models
{
    public class Appliance
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "O nome do aparelho é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 100000, ErrorMessage = "A potência deve ser maior que 0 Watts.")]
        public double PowerWatts { get; set; }

        [Range(1, 100, ErrorMessage = "A quantidade deve ser de pelo menos 1.")]
        public int Quantity { get; set; } = 1;

        [Range(0, 24, ErrorMessage = "O uso diário deve estar entre 0 e 24 horas.")]
        public double DailyHours { get; set; }

        public double CalculateMonthlyKwh()
        {
            // Formula: (Power (W) * Qty * Hours/Day * 30 days) / 1000
            return (PowerWatts * Quantity * DailyHours * 30) / 1000.0;
        }

        public double CalculateMonthlyCost(double tariffRate)
        {
            return CalculateMonthlyKwh() * tariffRate;
        }
    }
}
