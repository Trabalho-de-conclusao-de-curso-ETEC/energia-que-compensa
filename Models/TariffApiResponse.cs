namespace energia_que_compensa.Models
{
    /// <summary>
    /// Resposta da API ViaCEP (https://viacep.com.br).
    /// Usada pelo AneelTariffService para resolver CEP → UF.
    /// </summary>
    public class ViaCepResponse
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public string? Ibge { get; set; }
        public bool Erro { get; set; }
    }
}
