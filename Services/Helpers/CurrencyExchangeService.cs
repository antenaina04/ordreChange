using Newtonsoft.Json.Linq;
using NLog;
using ordreChange.Services.Implementations;

namespace ordreChange.Services.Helpers
{
    public class CurrencyExchangeService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.exchangeratesapi.io/latest";
        //private readonly string _apiKey = "VOTRE_CLE_API"; 
        private readonly string _apiKey = "8ba802474c35c5ffe0b3b0258699d4ba";
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public CurrencyExchangeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<double> CurrencyConversion(double montant, string deviseSource, string deviseCible)
        {
            Logger.Info("Converting {Montant} from {DeviseSource} to {DeviseCible}", montant, deviseSource, deviseCible);
            //var taux = await GetExchangeRateAPIAsync(deviseSource, deviseCible); // Prod + quota
            var taux = GetExchangeRateMatrixAsync(deviseSource, deviseCible); // DEV mode

            if (taux == null)
            {
                Logger.Error("Failed to retrieve exchange rate from {DeviseSource} to {DeviseCible}", deviseSource, deviseCible);
                throw new Exception("Erreur lors de la récuperation du taux de change par l'API externe");
            }
            Logger.Info("Exchange rate from {DeviseSource} to {DeviseCible} is {Taux}", deviseSource, deviseCible, taux);
            return montant * (double)taux;
        }
        private async Task<decimal?> GetExchangeRateAPIAsync(string fromCurrency, string toCurrency)
        {
            Logger.Info("Fetching exchange rate from API for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            var requestUrl = $"{ApiBaseUrl}?base={fromCurrency}&symbols={toCurrency}&access_key={_apiKey}";

            // SEND REQUEST
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error("Failed to fetch exchange rate from API. Status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonData = JObject.Parse(jsonString);

            // EXTRACT EXCHANGE RATE
            decimal exchangeRate = jsonData["rates"]?[toCurrency]?.Value<decimal>() ?? 0;
            Logger.Info("Exchange rate from API for {FromCurrency} to {ToCurrency} is {ExchangeRate}", fromCurrency, toCurrency, exchangeRate);
            return exchangeRate;
        }
        private float GetExchangeRateMatrixAsync(string deviseSource, string deviseCible)
        {
            Logger.Info("Fetching exchange rate from matrix for {DeviseSource} to {DeviseCible}", deviseSource, deviseCible);
            return (float)MatrixExchangeRate.Instance.GetTaux(deviseSource, deviseCible);
        }

    }
}
