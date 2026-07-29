using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public DailyAlphaQuote(ILogger<DailyAlphaQuote> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("DailyAlphaQuote")]
        public async Task<IActionResult> Run(
            [TimerTrigger("* */2 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            var stock = new { symbol = "PETR4.SA", name = "Petrobrás S.A." };
            var url = $"{alphaUrl}?function={daily_function}&symbol={stock.symbol}&apikey={alphaApiKey}";

            try
            {
                _logger.LogInformation($"Fetching data for {stock.name} from {url}");
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response for {stock.name}: {json}");

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var TimeSeries = root.GetProperty("Time Series (Daily)");
                var latest = TimeSeries.EnumerateObject().First();
                var date = latest.Name;
                var close = latest.Value.GetProperty("4. close").GetString();

                 var html = BuildEmailHtml(stock, date, close = "");
                 await SendMailAsync("michelgarciafurtado@gmail.com", $"Cotação {stock.name} - {date}", html, _logger);   
                // Retorna o conteúdo da Alpha Vantage em caso de sucesso
                return new OkObjectResult(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch data for {stock.name}: {ex.Message}");
                return new BadRequestObjectResult($"Erro ao buscar dados: {ex.Message}");
            }
        }

        private string BuildEmailHtml(dynamic stock, string date, string close)
        {
            return $@"
                <html>
                    <body>
                        <h1> Cotação {stock.name} - {date}</h1>
                        <p>Último fechamento: R$ {close}</p>
                    </body>
                </html>
                ";
        }

        private async Task SendMailAsync(string to, string subject, string html, ILogger log)
        {
                var url = "https://api.resend.com/emails";
                var content = new StringContent(JsonSerializer.Serialize(new
                {
                    from = "onboarding@resend.dev",
                    to = to,
                    subject = subject,
                    html = html
                }), System.Text.Encoding.UTF8, "application/json");
               using var request = new HttpRequestMessage(HttpMethod.Post, url);
               request.Content = content;
               request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resendApiKey);
               
               log.LogInformation($"Sending email to {to} with subject: '{subject}'");
               
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                log.LogInformation($"Email sent successfully ro {to}");
        }
    }
}
