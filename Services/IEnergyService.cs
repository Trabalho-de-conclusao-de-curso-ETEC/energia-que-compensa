using System.Collections.Generic;
using energia_que_compensa.Models;

namespace energia_que_compensa.Services
{
    public interface IEnergyService
    {
        SimulationResult RunSimulation(double monthlyKwh, double tariffRate);
        SimulationResult RunSimulationFromAppliances(List<Appliance> appliances, double tariffRate);
        List<Appliance> GetDefaultAppliances();
    }
}
