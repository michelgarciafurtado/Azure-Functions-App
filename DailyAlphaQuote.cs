using ALPHA_QUOTE.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

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

        //ResourceGroup: DefaultResourceGroup-CQ
        public DailyAlphaQuote(ILogger<DailyAlphaQuote> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("DailyAlphaQuote")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "DailyAlphaQuote/{symbol}")] HttpRequest req, string symbol)
        {
            _logger.LogInformation($"C# Http trigger function executed at: {DateTime.Now}");
            
           
            var url = $"{alphaUrl}?function={daily_function}&symbol={symbol}&apikey={alphaApiKey}";

            try
            {
                _logger.LogInformation($"Fetching data for {symbol} from {url}");
                
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
               
                using JsonDocument doc = JsonDocument.Parse(content);
                JsonElement root = doc.RootElement;
                JsonElement timeSeries = root.GetProperty("Time Series (Daily)");

                var ListValues = new List<StockValues>();


                foreach (JsonProperty dia in timeSeries.EnumerateObject())
                {
                    string dataDoRegistro = dia.Name; 
                    string precoFechamento = dia.Value.GetProperty("4. close").GetString()!;

                    ListValues.Add(new
                    StockValues(
                        dataDoRegistro,
                        precoFechamento
                    ));
                }

                var historico = new Stock(symbol, ListValues);

                return new OkObjectResult(historico);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch data for {symbol}: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao buscar dados: {ex.Message}");
            }
        }
    }
}
