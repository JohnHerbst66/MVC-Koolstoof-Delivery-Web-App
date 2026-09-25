using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Koolstoof_App_1.Models;
using Microsoft.Extensions.Options;

namespace Koolstoof_App_1.Services
{
    // Sends the new-order alert to admins via Meta's WhatsApp Cloud API using an
    // approved message template (see WhatsAppCloudSettings). In test mode this only
    // reaches phone numbers added as verified recipients in the Meta developer console.
    public class WhatsAppNotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppNotificationService> _logger;
        private readonly WhatsAppCloudSettings _settings;

        public WhatsAppNotificationService(IHttpClientFactory httpClientFactory, ILogger<WhatsAppNotificationService> logger, IOptions<WhatsAppCloudSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings.Value;
        }

        // Sends one alert per recipient; never throws — a notification failure should
        // never block an order from going through.
        public async Task SendOrderAlertAsync(IEnumerable<string> recipients, string orderNumber, string customerName, string total, string deliveryArea)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning("WhatsApp Cloud API is not configured; skipping order notifications.");
                return;
            }

            var parameters = new[] { orderNumber, customerName, total, deliveryArea }.Select(Sanitize).ToArray();

            foreach (var number in recipients.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
            {
                try
                {
                    await SendTemplateAsync(number, parameters);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send WhatsApp order notification to {Number}", number);
                }
            }
        }

        private async Task SendTemplateAsync(string toNumber, string[] bodyParameters)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.AccessToken);

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toNumber,
                type = "template",
                template = new
                {
                    name = _settings.OrderAlertTemplateName,
                    language = new { code = _settings.TemplateLanguage },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = bodyParameters.Select(p => new { type = "text", text = p }).ToArray()
                        }
                    }
                }
            };

            var url = $"https://graph.facebook.com/v20.0/{_settings.PhoneNumberId}/messages";
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("WhatsApp Cloud API returned {Status} for {Number}: {Body}", response.StatusCode, toNumber, body);
            }
        }

        // Template variables can't contain newlines, tabs, or long runs of spaces, and
        // the customer name is free text typed on the public checkout form.
        private static string Sanitize(string value)
        {
            var cleaned = Regex.Replace(value ?? "", @"\s+", " ").Trim();
            if (cleaned.Length > 60)
            {
                cleaned = cleaned[..60];
            }
            return cleaned.Length == 0 ? "-" : cleaned;
        }
    }
}
