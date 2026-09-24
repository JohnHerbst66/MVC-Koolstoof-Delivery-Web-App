namespace Koolstoof_App_1.Models
{
    // Bound from the "PayFast" section of appsettings.json. The defaults here are
    // PayFast's own published sandbox test credentials (not a secret — anyone
    // integrating with PayFast's sandbox uses these same values) so the payment
    // flow can be built and tested end-to-end before the restaurant has a real
    // PayFast merchant account. Swap SandboxMode to false and fill in the real
    // MerchantId/MerchantKey/Passphrase to go live.
    public class PayFastSettings
    {
        public string MerchantId { get; set; } = "10000100";
        public string MerchantKey { get; set; } = "46f0cd694581a";
        public string Passphrase { get; set; } = "";
        public bool SandboxMode { get; set; } = true;

        public string ProcessUrl => SandboxMode
            ? "https://sandbox.payfast.co.za/eng/process"
            : "https://www.payfast.co.za/eng/process";
    }
}
