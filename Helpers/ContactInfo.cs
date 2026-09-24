using System.Net;

namespace Koolstoof_App_1.Helpers
{
    public static class ContactInfo
    {
        // TODO: replace with the restaurant's own WhatsApp number once one is set up.
        public const string WhatsAppNumber = "27604944665";

        public static string WhatsAppLink(string message) =>
            $"https://wa.me/{WhatsAppNumber}?text={WebUtility.UrlEncode(message)}";
    }
}
