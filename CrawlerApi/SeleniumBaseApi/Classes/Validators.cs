using System.Text.RegularExpressions;

namespace SeleniumBaseApi.Classes
{
    public class Validators
    {
        public bool IsURLValid(string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        public bool IsWordValid(string input)
        {
            string pattern = @"^(?=.*[a-zA-Z])[a-zA-Z0-9#.]+$";
            return Regex.IsMatch(input, pattern) && input.Length > 2;
        }
    }
}
