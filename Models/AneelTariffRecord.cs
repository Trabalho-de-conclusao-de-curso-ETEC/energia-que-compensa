namespace energia_que_compensa.Models
{
    /// <summary>
    /// Representa uma linha do CSV de tarifas homologadas da ANEEL.
    /// URL do CSV: https://dadosabertos.aneel.gov.br/dataset/5a583f3e-.../tarifas-homologadas-distribuidoras-energia-eletrica.csv

    public class AneelTariffRecord
    {
        public string SigAgente { get; set; } = string.Empty;           // Sigla da distribuidora (ex: CEMIG-D, COPEL-DIS, LIGHT)
        public DateTime DatInicioVigencia { get; set; }
        public DateTime DatFimVigencia { get; set; }
        public string DscBaseTarifa { get; set; } = string.Empty;       // "Tarifa de Aplicação" ou "Base Econômica"
        public string DscSubgrupo { get; set; } = string.Empty;         // B1, B2, B3, B4a... (B1 = residencial)
        public string DscModalidadeTarifaria { get; set; } = string.Empty; // convencional, branca, etc.
        public string DscClasse { get; set; } = string.Empty;           // residencial, rural, industrial, etc.
        public string DscUnidade { get; set; } = string.Empty;          // R$/MWh ou R$/kW
        public double VlrTusd { get; set; }                             // Tarifa de Uso do Sistema de Distribuição
        public double VlrTe { get; set; }                               // Tarifa de Energia
    }
}
