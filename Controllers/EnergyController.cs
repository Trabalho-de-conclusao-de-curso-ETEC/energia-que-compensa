using Microsoft.AspNetCore.Mvc;
using energia_que_compensa.Data;
using energia_que_compensa.Models;
using energia_que_compensa.Services;

namespace energia_que_compensa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyController : ControllerBase
    {
        private readonly IEnergyService _energyService;
        private readonly IAneelTariffService _tariffService;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EnergyController> _logger;

        public EnergyController(
            IEnergyService energyService,
            IAneelTariffService tariffService,
            ApplicationDbContext db,
            ILogger<EnergyController> logger)
        {
            _energyService = energyService;
            _tariffService = tariffService;
            _db = db;
            _logger = logger;
        }

        // aparelhos pré-cadastrados para simulação rápida (sem necessidade de informar consumo manualmente)

        [HttpGet("default-appliances")]
        public ActionResult<IEnumerable<Appliance>> GetDefaultAppliances()
        {
            return Ok(_energyService.GetDefaultAppliances());
        }

        // tarifas por CEP ou UF (busca automática na ANEEL, sem necessidade de informar a tarifa manualmente)

        [HttpGet("tariff/{cep}")]
        public async Task<ActionResult<TariffResponse>> GetTariffByCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep) || cep.Replace("-", "").Length != 8)
                return BadRequest("CEP inválido. Informe 8 dígitos numéricos.");

            var tariff = await _tariffService.GetTariffByCepAsync(cep);
            return Ok(new TariffResponse { Cep = cep, TariffKwh = tariff });
        }

        [HttpGet("tariff/state/{uf}")]
        public async Task<ActionResult<TariffResponse>> GetTariffByState(string uf)
        {
            if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
                return BadRequest("UF inválida. Informe a sigla com 2 letras (ex: SP, MG, PR).");

            var tariff = await _tariffService.GetTariffByStateAsync(uf);
            return Ok(new TariffResponse { Uf = uf.ToUpperInvariant(), TariffKwh = tariff });
        }

        // simulação básica por consumo mensal e tarifa (sem CEP, sem lista de aparelhos)

        [HttpPost("simulate")]
        public async Task<ActionResult<SimulationResult>> Simulate([FromBody] SimulationRequest request)
        {
            if (request == null)
                return BadRequest("Corpo da requisição inválido.");

            if (request.MonthlyKwh < 0)
                return BadRequest("O consumo mensal não pode ser negativo.");

            if (request.TariffRate <= 0)
                request.TariffRate = 0.85;

            var result = _energyService.RunSimulation(request.MonthlyKwh, request.TariffRate);

            await SaveSimulationRecordAsync(result, cep: null, uf: null);

            return Ok(result);
        }

        // simulação avançada por consumo mensal + CEP ou UF (busca automática da tarifa na ANEEL, sem necessidade de informar a tarifa manualmente)

        [HttpPost("simulate-with-cep")]
        public async Task<ActionResult<SimulationResult>> SimulateWithCep([FromBody] SimulationWithCepRequest request)
        {
            if (request == null)
                return BadRequest("Corpo da requisição inválido.");

            if (request.MonthlyKwh < 0)
                return BadRequest("O consumo mensal não pode ser negativo.");

            string? uf = null;
            double tariff;

            if (!string.IsNullOrWhiteSpace(request.Cep))
            {
                tariff = await _tariffService.GetTariffByCepAsync(request.Cep);
            }
            else if (!string.IsNullOrWhiteSpace(request.Uf))
            {
                uf = request.Uf.ToUpperInvariant();
                tariff = await _tariffService.GetTariffByStateAsync(uf);
            }
            else
            {
                tariff = request.TariffRate > 0 ? request.TariffRate : 0.85;
            }

            var result = _energyService.RunSimulation(request.MonthlyKwh, tariff);

            await SaveSimulationRecordAsync(result, cep: request.Cep, uf: uf);

            return Ok(result);
        }

        // simulação por lista de aparelhos (entrada detalhada, sem necessidade de informar consumo mensal nem tarifa manualmente)

        [HttpPost("simulate-appliances")]
        public async Task<ActionResult<SimulationResult>> SimulateAppliances([FromBody] ApplianceSimulationRequest request)
        {
            if (request == null || request.Appliances == null)
                return BadRequest("Lista de aparelhos inválida.");

            if (request.TariffRate <= 0)
                request.TariffRate = 0.85;

            var result = _energyService.RunSimulationFromAppliances(request.Appliances, request.TariffRate);

            await SaveSimulationRecordAsync(result, cep: null, uf: null);

            return Ok(result);
        }

        // histórico de simulações (para futuras funcionalidades de dashboard, perfil do usuário, etc)

        /// <summary>
        /// Salva o resultado da simulação na tabela SimulationRecords.
        /// Nunca lança exceção — se o banco falhar, a simulação ainda é retornada ao usuário.
        /// </summary>
        private async Task SaveSimulationRecordAsync(SimulationResult result, string? cep, string? uf)
        {
            try
            {
                var solar = result.SolarRecommendation;

                var record = new SimulationRecord
                {
                    // Entrada
                    MonthlyKwh       = result.TotalKwh,
                    TariffRate       = result.TariffRate,
                    Cep              = cep,
                    Uf               = uf,

                    // Resultado de consumo
                    TotalCost           = result.TotalCost,
                    CarbonReductionKg   = result.CarbonReductionKg,
                    EfficiencyScore     = result.EfficiencyScore,
                    EfficiencyCategory  = result.EfficiencyCategory,

                    // Resultado solar
                    SolarIsViable    = solar.IsViable,
                    PanelsCount      = solar.PanelsCount,
                    SystemSizeKwp    = solar.SystemSizeKwp,
                    TotalAreaSqM     = solar.TotalAreaSqM,
                    EstimatedCost    = solar.EstimatedCost,
                    MonthlySavings   = solar.MonthlySavings,
                    PaybackYears     = solar.PaybackYears,
                    LifetimeSavings  = solar.LifetimeSavings,
                    AnnualCo2SavedKg = solar.AnnualCo2SavedKg,
                    TreeEquivalent   = solar.TreeEquivalent,

                    // UserId fica null por enquanto (sem autenticação ativa ainda)
                    UserId = null
                };

                _db.SimulationRecords.Add(record);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loga o erro mas não quebra a resposta ao usuário
                _logger.LogError(ex, "Erro ao salvar SimulationRecord no banco.");
            }
        }
    }

    // DTOs 

    public class SimulationRequest
    {
        public double MonthlyKwh { get; set; }
        public double TariffRate { get; set; }
    }

    public class SimulationWithCepRequest
    {
        public double MonthlyKwh { get; set; }
        public string? Cep { get; set; }
        public string? Uf { get; set; }
        public double TariffRate { get; set; }
    }

    public class ApplianceSimulationRequest
    {
        public List<Appliance> Appliances { get; set; } = new();
        public double TariffRate { get; set; }
    }

    public class TariffResponse
    {
        public string? Cep { get; set; }
        public string? Uf { get; set; }
        public double TariffKwh { get; set; }
        public string Source { get; set; } = "Resoluções Homologatórias ANEEL 2024-2025";
    }
}
