using System;

namespace pTyping.Web;

public static class Helpers {
    public static string GetUntil(this string text, string stopAt) {
        if (!string.IsNullOrWhiteSpace(text)) {
            int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

            if (charLocation > 0)
                return text[..charLocation];
        }

        return string.Empty;
    }
}
