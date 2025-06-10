using Microsoft.AspNetCore.Mvc;
using ScrapySharp.Extensions;
using ScrapySharp.Html;
using ScrapySharp.Html.Forms;
using ScrapySharp.Network;
using SeleniumBaseApi.Models;
using SeleniumBaseApi.Services;

namespace SeleniumBaseApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebScrapingController : ControllerBase
    {
        public WebScrapingController() 
        { 

        }

        [HttpPost]
        public async Task<IEnumerable<string>> GetData(string _url)
        {
            ScrapingBrowser browser = new ScrapingBrowser();
            browser.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            WebPage page = await browser.NavigateToPageAsync(new Uri(_url));
            var links = page.Html.CssSelect("a");
            List<string> returnedList = new List<string>();
            foreach(var link in links)
            {
                string url = link.GetAttributeValue("href", "");
                returnedList.Add(url);
            }
            return returnedList;
        }

    }
}
