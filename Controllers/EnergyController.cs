using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using energia_que_compensa.Models;
using energia_que_compensa.Services;

namespace energia_que_compensa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergyController : ControllerBase
    {
        private readonly IEnergyService _energyService;

        public EnergyController(IEnergyService energyService)
        {
            _energyService = energyService;
        }

        [HttpGet("default-appliances")]
        public ActionResult<IEnumerable<Appliance>> GetDefaultAppliances()
        {
            var appliances = _energyService.GetDefaultAppliances();
            return Ok(appliances);
        }

        [HttpPost("simulate")]
        public ActionResult<SimulationResult> Simulate([FromBody] SimulationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Corpo da requisição inválido.");
            }

            if (request.MonthlyKwh < 0)
            {
                return BadRequest("O consumo mensal não pode ser negativo.");
            }

            if (request.TariffRate <= 0)
            {
                request.TariffRate = 0.85; // fallback
            }

            var result = _energyService.RunSimulation(request.MonthlyKwh, request.TariffRate);
            return Ok(result);
        }

        [HttpPost("simulate-appliances")]
        public ActionResult<SimulationResult> SimulateAppliances([FromBody] ApplianceSimulationRequest request)
        {
            if (request == null || request.Appliances == null)
            {
                return BadRequest("Lista de aparelhos inválida.");
            }

            if (request.TariffRate <= 0)
            {
                request.TariffRate = 0.85;
            }

            var result = _energyService.RunSimulationFromAppliances(request.Appliances, request.TariffRate);
            return Ok(result);
        }
    }

    public class SimulationRequest
    {
        public double MonthlyKwh { get; set; }
        public double TariffRate { get; set; }
    }

    public class ApplianceSimulationRequest
    {
        public List<Appliance> Appliances { get; set; } = new();
        public double TariffRate { get; set; }
    }
}
