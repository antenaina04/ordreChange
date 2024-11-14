namespace ordreChange.Services.Implementations
{
    public class TauxChangeMatrix
    {
        private static readonly Lazy<TauxChangeMatrix> _instance = new(() => new TauxChangeMatrix());
        private readonly Dictionary<string, int> _deviseIndices;
        private readonly double[,] _tauxChange;

        private TauxChangeMatrix()
        {
            string[] devises = { "USD", "EUR", "CAD", "GBP", "MGA" };
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
                    _tauxChange[i, j] = (i == j) ? 1.0 : GetRealisticExchangeRate(devises[i], devises[j]);
                }
            }
        }

        private double GetRealisticExchangeRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == "USD" && toCurrency == "EUR") return 0.92;
            if (fromCurrency == "USD" && toCurrency == "CAD") return 1.25;
            if (fromCurrency == "USD" && toCurrency == "GBP") return 0.73;
            if (fromCurrency == "USD" && toCurrency == "MGA") return 4000.00;
            if (fromCurrency == "EUR" && toCurrency == "USD") return 1.09;
            if (fromCurrency == "EUR" && toCurrency == "CAD") return 1.36;
            if (fromCurrency == "EUR" && toCurrency == "GBP") return 0.79;
            if (fromCurrency == "EUR" && toCurrency == "MGA") return 4350.00;
            if (fromCurrency == "CAD" && toCurrency == "USD") return 0.80;
            if (fromCurrency == "CAD" && toCurrency == "EUR") return 0.74;
            if (fromCurrency == "CAD" && toCurrency == "GBP") return 0.58;
            if (fromCurrency == "CAD" && toCurrency == "MGA") return 3200.00;
            if (fromCurrency == "GBP" && toCurrency == "USD") return 1.37;
            if (fromCurrency == "GBP" && toCurrency == "EUR") return 1.27;
            if (fromCurrency == "GBP" && toCurrency == "CAD") return 1.72;
            if (fromCurrency == "GBP" && toCurrency == "MGA") return 5500.00;
            if (fromCurrency == "MGA" && toCurrency == "USD") return 0.00025;
            if (fromCurrency == "MGA" && toCurrency == "EUR") return 0.00023;
            if (fromCurrency == "MGA" && toCurrency == "CAD") return 0.00031;
            if (fromCurrency == "MGA" && toCurrency == "GBP") return 0.00018;

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
