using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ordreChange.Services.Helpers;

namespace ordreChange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly CurrencyExchangeService _currencyExchangeService;

        public ExchangeController(CurrencyExchangeService currencyExchangeService)
        {
            _currencyExchangeService = currencyExchangeService;
        }

        // Endpoint GET_TAUX_DE_CHANGE_PAR_API_EXTERNE
        //[HttpGet("rate")]
        //public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency)
        //{
        //    if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
        //    {
        //        return BadRequest("Les codes devises sont requis");
        //    }

        //    var rate = await _currencyExchangeService.GetExchangeRateAsync(fromCurrency, toCurrency);
        //    if (rate == null)
        //    {
        //        return NotFound("Taux de change introuvable ou erreur de l'API externe");
        //    }

        //    return Ok(new { FromCurrency = fromCurrency, ToCurrency = toCurrency, ExchangeRate = rate });
        //}
    }
}
