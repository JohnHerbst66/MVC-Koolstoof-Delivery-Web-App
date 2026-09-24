using System.Text;
using System.Text.Json;
using Koolstoof_App_1.Models;

namespace Koolstoof_App_1.Services
{
    // Sends plain-text WhatsApp messages via Meta's WhatsApp Cloud API
    // (https://graph.facebook.com/.../messages). In test mode this only works for
    // phone numbers added as verified recipients in the Meta developer console.
    public class WhatsAppNotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppNotificationService> _logger;
        private readonly WhatsAppCloudSettings _settings;

        public WhatsAppNotificationService(IHttpClientFactory httpClientFactory, ILogger<WhatsAppNotificationService> logger, Microsoft.Extensions.Options.IOptions<WhatsAppCloudSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings.Value;
        }

        // Fires off one message per recipient; never throws — a notification
        // failure should never block an order from going through.
        public async Task SendToAllAsync(IEnumerable<string> recipients, string message)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning("WhatsApp Cloud API is not configured; skipping order notifications.");
                return;
            }

            foreach (var number in recipients.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
            {
                try
                {
                    await SendAsync(number, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send WhatsApp order notification to {Number}", number);
                }
            }
        }

        private async Task SendAsync(string toNumber, string message)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.AccessToken);

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toNumber,
                type = "text",
                text = new { body = message }
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
    }
}
