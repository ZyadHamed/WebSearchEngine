using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace SeleniumBaseApi.Classes
{
    public class CaptchaDetector
    {
        public readonly List<string> CaptchaKeywords = new List<string>
        {
            // from most common to least common
            "iframe[src*='recaptcha/api2']",
            ".g-recaptcha",
            "iframe[src*='hcaptcha.com/captcha.html']",
            ".h-captcha",
            "iframe[src*='challenges.cloudflare.com/cdn-cgi/challenge-platform/']",
            ".cf-turnstile",
            "#challenge-form"
        };

        public readonly List<string> ImageCaptcha = new List<string>
        {
            // sorted as all img elements then all input element
            "img[src*='captcha']",
            "img[id*='captcha']",
            "img[class*='captcha']",
            "input[type='text'][name*='captcha']",
            "input[type='text'][id*='captcha']"
        };

        // update this yourself
        public int ImgElementCount = 3;

        public async Task<bool?> DetectCaptcha(WebPage page)
        {
            string htmlContent = page.Content;

            if (string.IsNullOrWhiteSpace(htmlContent) || htmlContent.Length < 200)
            {
                return true;
            }
                

            if (htmlContent.Contains("Checking your browser") || htmlContent.Contains("Please wait..."))
            {
                return true;
            }


            var htmlDoc = page.Html;
            
            foreach (string keyword in CaptchaKeywords)
            {
                if (htmlDoc.CssSelect(keyword).Any())
                    return true;
            }

            // Check img selectors
            bool hasCaptchaImage = ImageCaptcha.Take(ImgElementCount).Any(s => htmlDoc.CssSelect(s).Any());
            // Check input selectors
            bool hasCaptchaInput = ImageCaptcha.Skip(ImgElementCount).Any(s => htmlDoc.CssSelect(s).Any()); 

            if (hasCaptchaImage && hasCaptchaInput)
            {
                return null;
            }

            return false;
        }
    }
}
