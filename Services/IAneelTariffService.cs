namespace energia_que_compensa.Services
{
    public interface IAneelTariffService
    {
        /// <summary>
        /// Retorna a tarifa média residencial (R$/kWh) da distribuidora responsável
        /// pelo CEP informado, consultando a API ViaCEP para resolver o estado.
        /// </summary>
        Task<double> GetTariffByCepAsync(string cep);

        /// <summary>
        /// Retorna a tarifa residencial média (R$/kWh) pelo estado (UF).
        /// </summary>
        Task<double> GetTariffByStateAsync(string uf);
    }
}
