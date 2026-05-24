using System.Net.Http.Json;

namespace energia_que_compensa.Services
{
    /// <summary>
    /// Serviço de tarifas residenciais por estado (UF).
    ///
    /// Estratégia:
    ///   1. Recebe um CEP → chama ViaCEP (gratuita) para descobrir a UF
    ///   2. Mapeia UF → tarifa residencial B1 vigente (fonte: resoluções homologatórias ANEEL 2024/2025)
    ///   3. Fallback de R$ 0,97/kWh (média nacional) caso a UF não seja reconhecida
    ///
    /// Os valores são atualizados anualmente conforme os reajustes homologatórios da ANEEL.
    /// Última atualização desta tabela: maio/2025 (ciclo tarifário 2024-2025).
    /// </summary>
    public class AneelTariffService : IAneelTariffService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AneelTariffService> _logger;

        // Média nacional residencial B1 como fallback (R$/kWh)
        private const double DEFAULT_TARIFF_KWH = 0.97;

        /// <summary>
        /// Tarifas residenciais B1 por UF (R$/kWh) — TE + TUSD + encargos, com impostos médios incluídos.
        /// Fonte: Resoluções Homologatórias ANEEL, ciclo 2024-2025.
        /// Referência: https://www.gov.br/aneel/pt-br/assuntos/tarifas
        /// </summary>
        private static readonly Dictionary<string, double> _tarifasPorUf = new(StringComparer.OrdinalIgnoreCase)
        {
            { "AC", 0.97 },  // Energisa Acre
            { "AL", 0.89 },  // Equatorial Alagoas
            { "AM", 0.74 },  // Amazonas Energia (subsidiada)
            { "AP", 0.68 },  // CEA (subsidiada, região isolada)
            { "BA", 0.91 },  // Neoenergia Coelba
            { "CE", 0.94 },  // Enel Ceará
            { "DF", 0.83 },  // CEB Distribuição
            { "ES", 0.96 },  // EDP Espírito Santo
            { "GO", 0.88 },  // Enel Goiás
            { "MA", 0.99 },  // Equatorial Maranhão
            { "MG", 0.92 },  // CEMIG-D
            { "MS", 1.01 },  // Energisa MS
            { "MT", 1.03 },  // Energisa MT
            { "PA", 0.87 },  // Equatorial Pará
            { "PB", 0.98 },  // Energisa Paraíba
            { "PE", 0.93 },  // Neoenergia Pernambuco (Celpe)
            { "PI", 0.95 },  // Equatorial Piauí
            { "PR", 0.82 },  // Copel (uma das mais baratas do Brasil)
            { "RJ", 1.05 },  // Light / Enel RJ (uma das mais caras)
            { "RN", 0.90 },  // Neoenergia Cosern
            { "RO", 0.96 },  // Energisa Rondônia
            { "RR", 0.72 },  // Roraima Energia (subsidiada)
            { "RS", 0.98 },  // CEEE / RGE
            { "SC", 0.86 },  // Celesc
            { "SE", 0.97 },  // Energisa Sergipe
            { "SP", 1.00 },  // Enel SP / CPFL / Elektro (média das distribuidoras)
            { "TO", 1.02 },  // Energisa Tocantins
        };

        public AneelTariffService(HttpClient httpClient, ILogger<AneelTariffService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── Por CEP ──────────────────────────────────────────────────────────────────

        public async Task<double> GetTariffByCepAsync(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return DEFAULT_TARIFF_KWH;

            var uf = await GetUfByCepAsync(cep);

            if (string.IsNullOrWhiteSpace(uf))
            {
                _logger.LogWarning("Não foi possível resolver UF para o CEP {Cep}. Usando tarifa padrão.", cep);
                return DEFAULT_TARIFF_KWH;
            }

            return await GetTariffByStateAsync(uf);
        }

        // ── Por UF ───────────────────────────────────────────────────────────────────

        public Task<double> GetTariffByStateAsync(string uf)
        {
            if (string.IsNullOrWhiteSpace(uf))
                return Task.FromResult(DEFAULT_TARIFF_KWH);

            uf = uf.ToUpperInvariant().Trim();

            if (_tarifasPorUf.TryGetValue(uf, out var tarifa))
            {
                _logger.LogInformation("Tarifa para UF {UF}: R$ {Tarifa}/kWh", uf, tarifa);
                return Task.FromResult(tarifa);
            }

            _logger.LogWarning("UF {UF} não encontrada na tabela. Usando tarifa padrão.", uf);
            return Task.FromResult(DEFAULT_TARIFF_KWH);
        }

        // ── Helper: ViaCEP ───────────────────────────────────────────────────────────

        private async Task<string?> GetUfByCepAsync(string cep)
        {
            var cleanCep = cep.Replace("-", "").Trim();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ViaCepResponse>(
                    $"https://viacep.com.br/ws/{cleanCep}/json/");

                if (response?.Erro == true)
                {
                    _logger.LogWarning("ViaCEP retornou erro para o CEP {Cep}.", cep);
                    return null;
                }

                return response?.Uf;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao consultar ViaCEP para o CEP {Cep}.", cep);
                return null;
            }
        }

        private class ViaCepResponse
        {
            public string? Uf { get; set; }
            public string? Localidade { get; set; }
            public bool Erro { get; set; }
        }
    }
}
