using Newtonsoft.Json.Linq;
using ordreChange.Services.Implementations;

namespace ordreChange.Services.Helpers
{
    public class CurrencyExchangeService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.exchangeratesapi.io/latest";
        //private readonly string _apiKey = "VOTRE_CLE_API"; 
        private readonly string _apiKey = "8ba802474c35c5ffe0b3b0258699d4ba";

        public CurrencyExchangeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<double> CurrencyConversion(double montant, string deviseSource, string deviseCible)
        {
            //var taux = await GetExchangeRateAPIAsync(deviseSource, deviseCible); // Prod + quota
            var taux = GetExchangeRateMatrixAsync(deviseSource, deviseCible); // DEV mode

            if (taux == null)
            {
                throw new Exception("Erreur lors de la récuperation du taux de change par l'API externe");
            }

            return montant * (double)taux;
        }
        private async Task<decimal?> GetExchangeRateAPIAsync(string fromCurrency, string toCurrency)
        {
            var requestUrl = $"{ApiBaseUrl}?base={fromCurrency}&symbols={toCurrency}&access_key={_apiKey}";

            // SEND REQUEST
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonData = JObject.Parse(jsonString);

            // EXTRACT EXCHANGE RATE
            decimal exchangeRate = jsonData["rates"]?[toCurrency]?.Value<decimal>() ?? 0;
            return exchangeRate;
        }
        private float GetExchangeRateMatrixAsync(string deviseSource, string deviseCible)
        {
            return (float)MatrixExchangeRate.Instance.GetTaux(deviseSource, deviseCible);
        }

    }
}
