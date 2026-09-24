using System.Net;

namespace Koolstoof_App_1.Helpers
{
    public static class ContactInfo
    {
        // The WhatsApp number is admin-editable — see RestaurantSettings.WhatsAppNumber
        // (Settings tab). This just builds the link once you have that number.
        public static string WhatsAppLink(string number, string message) =>
            $"https://wa.me/{number}?text={WebUtility.UrlEncode(message)}";
    }
}
