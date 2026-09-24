namespace Koolstoof_App_1.Models
{
    // Bound from the "WhatsAppCloud" config section. Real values (access token,
    // phone number id) belong in user-secrets locally / real secret storage in
    // production — never committed to appsettings.json. See PayFastSettings for
    // the same pattern.
    public class WhatsAppCloudSettings
    {
        public string AccessToken { get; set; } = "";
        public string PhoneNumberId { get; set; } = "";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(PhoneNumberId);
    }
}
