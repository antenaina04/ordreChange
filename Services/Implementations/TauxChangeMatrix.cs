namespace ordreChange.Services.Implementations
{
    public class TauxChangeMatrix
    {
        private static readonly Lazy<TauxChangeMatrix> _instance = new(() => new TauxChangeMatrix());
        private readonly Dictionary<string, int> _deviseIndices;
        private readonly double[,] _tauxChange;

        private TauxChangeMatrix()
        {
            string[] devises = { "USD", "EUR", "JPY", "GBP" };
            _deviseIndices = new Dictionary<string, int>();
            for (int i = 0; i < devises.Length; i++)
                _deviseIndices[devises[i]] = i;

            _tauxChange = new double[devises.Length, devises.Length];
            InitializeMatrice(devises);
        }

        public static TauxChangeMatrix Instance => _instance.Value;

        private void InitializeMatrice(string[] devises)
        {
            for (int i = 0; i < devises.Length; i++)
            {
                for (int j = 0; j < devises.Length; j++)
                {
                    _tauxChange[i, j] = (i == j) ? 1.0 : GetFakeExchangeRate(devises[i], devises[j]);
                }
            }
        }

        private double GetFakeExchangeRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == "USD" && toCurrency == "EUR") return 0.85;
            if (fromCurrency == "USD" && toCurrency == "JPY") return 110.53;
            if (fromCurrency == "USD" && toCurrency == "GBP") return 0.75;
            if (fromCurrency == "EUR" && toCurrency == "USD") return 1.18;
            if (fromCurrency == "EUR" && toCurrency == "JPY") return 129.53;
            if (fromCurrency == "EUR" && toCurrency == "GBP") return 0.88;
            if (fromCurrency == "JPY" && toCurrency == "USD") return 0.009;
            if (fromCurrency == "JPY" && toCurrency == "EUR") return 0.0077;
            if (fromCurrency == "JPY" && toCurrency == "GBP") return 0.0068;
            if (fromCurrency == "GBP" && toCurrency == "USD") return 1.33;
            if (fromCurrency == "GBP" && toCurrency == "EUR") return 1.14;
            if (fromCurrency == "GBP" && toCurrency == "JPY") return 150.33;

            return 1.0;
        }

        public double GetTaux(string deviseSource, string deviseCible)
        {
            int indexSource = _deviseIndices[deviseSource];
            int indexCible = _deviseIndices[deviseCible];
            return _tauxChange[indexSource, indexCible];
        }
    }
}
