using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ALPHA_QUOTE
{
    public class DailyAlphaQuote
    {
        private readonly ILogger<DailyAlphaQuote> _logger;
        private readonly HttpClient _httpClient;
        private readonly string? alphaApiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
        private readonly string? resendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY");
        private readonly string alphaUrl = "https://www.alphavantage.co/query";
        private readonly string daily_function = "TIME_SERIES_DAILY";

        // O HttpClient e o ILogger agora são injetados automaticamente pelo .NET 8
        public DailyAlphaQuote(ILogger<DailyAlphaQuote> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("DailyAlphaQuote")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation($"C# Http trigger function executed at: {DateTime.Now}");
            
            var petrobras = new { symbol = "PETR4.SA", name = "Petrobrás S.A." };
            var url = $"{alphaUrl}?function={daily_function}&symbol={petrobras.symbol}&apikey={alphaApiKey}";

            try
            {
                _logger.LogInformation($"Fetching data for {petrobras.name} from {url}");
                
                // Usando o HttpClient injetado corretamente com await
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response for {petrobras.name}: {content}");

                // Retorna o conteúdo da Alpha Vantage em caso de sucesso
                return new OkObjectResult(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch data for {petrobras.name}: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao buscar dados: {ex.Message}");
            }
        }
    }
}
