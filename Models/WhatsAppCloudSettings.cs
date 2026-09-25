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

        // Business-initiated WhatsApp messages must use a Meta-approved template
        // (free-form text only works inside a 24h window after the recipient last
        // messaged the business). The template's body takes 4 variables, in order:
        // order number, customer name, total, delivery area.
        public string OrderAlertTemplateName { get; set; } = "new_order_alert";
        public string TemplateLanguage { get; set; } = "en_US";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(PhoneNumberId);
    }
}
