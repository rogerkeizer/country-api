using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace CountryApi.Models
{
    public class Translations
    {
        public string de { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
        public string ja { get; set; }
        public string it { get; set; }
    }

    public class Dropdown
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Dropdown(string n){
            Name = n;
            Value = Shared.Normalize(n);
        }
        public Dropdown(string n, string v) {
            Name = n;
            Value = v;
        }
    }

    public class Feedback
    {
        public string Message { get; set; }
        public string Type { get; set; }

        public Feedback(string m, string t)
        {
            Message = m;
            Type = t;
        }
    }

    public static class Shared
    {
        public static string Normalize(string orig){

            orig = orig.Trim();
            orig = orig.ToLower();
            orig = orig.Replace(" ", "-");
            orig = orig.Replace("&", "-");

            orig = RemoveDiacritics(orig);

            return orig;
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}