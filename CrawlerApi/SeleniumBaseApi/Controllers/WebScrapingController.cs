using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.Net;
using SeleniumBaseApi.Objects;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using SeleniumBaseApi.Classes;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using SeleniumBaseApi.Services;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SeleniumBaseApi.Models;


namespace SeleniumBaseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebScrapingController : ControllerBase
    {

        #region Variables
        volatile Dictionary<string, List<string>> WebsitesReferencingEachSite = new Dictionary<string, List<string>>();
        readonly object WebsitesReferencingEachSiteLock = new object();

        List<PageData> AllScrappedPagesData = new List<PageData>();
        readonly object AllScrappedPagesDataLock = new object();

        volatile Queue<string> WebsitesQueue = new Queue<string>();
        readonly object WebsitesQueueLock = new object();

        Thread CancellationMonitoringThread;
        Thread UpdatingThread;

        CancellationTokenSource cancellationTokenSource;
        readonly object CancellationTokenSourceLock = new object();

        HashSet<string> currentWebsitesSoFar = new HashSet<string>();
        readonly object currentWebsitesSoFarLock = new object();

        HttpClient httpclient = new HttpClient();

        #endregion


        async Task SendDataRegularly()
        {
            while (true)
            {
                await Task.Delay(200);
                string APIURL = "http://localhost:4000/api/index";
                List<PageData> finalValues = await UpdateDatabaseIndexing();
                Console.WriteLine($"Current executing a new queue. Queue value: {WebsitesQueue.Count}");
                using StringContent jsonContent = new(
                JsonSerializer.Serialize(finalValues),
                Encoding.UTF8,
                "application/json");

                using HttpResponseMessage response = await httpclient.PostAsync(
                    APIURL,
                    jsonContent);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseString);
            }
        }

        async Task<List<PageData>> UpdateDatabaseIndexing()
        {
            AllScrappedPagesData.Clear();
            
            List<string> WebsitesToScrap = await SQLiteManagerService.RetriveWebsitesBatch();
            lock (WebsitesQueueLock)
            {
                foreach(string link in WebsitesToScrap)
                {
                    WebsitesQueue.Enqueue(link);
                }
            }
            int maxThreads = 10;
            var tasks = new Task[maxThreads];
            cancellationTokenSource = new CancellationTokenSource(15 * 1000 + 10);
            for (int i = 0; i < maxThreads; i++)
            {
                tasks[i] = RetrivePagesData(cancellationTokenSource);

            }

            await Task.WhenAll(tasks);
            URLStripper urlStripper = new URLStripper();
            for (int i = 0; i < AllScrappedPagesData.Count; i++)
            {
                string pageURLHost = urlStripper.GetBaseURL(AllScrappedPagesData[i].PageURL);
                if (WebsitesReferencingEachSite.ContainsKey(pageURLHost))
                {
                    AllScrappedPagesData[i].PagesReferencingThisPage = WebsitesReferencingEachSite[pageURLHost];
                }
            }


            string combinedQuery = "INSERT INTO WebPages (URL) VALUES ";
            Console.WriteLine($"The queue so far has {WebsitesQueue.Count}");
            while (WebsitesQueue.Count > 1)
            {
                string website = WebsitesQueue.Dequeue();
                if (!currentWebsitesSoFar.Contains(website))
                {
                    combinedQuery += $"('{website}'), ";
                    currentWebsitesSoFar.Add(website);
                    Console.WriteLine("A new unique page!!!");
                }
            }
            string finalwebsite = WebsitesQueue.Dequeue();
            combinedQuery += $"('{finalwebsite}');";
            Console.WriteLine($"Query Length {combinedQuery.Length}");
            await SQLiteManagerService.InsertWebPages(combinedQuery);
            return AllScrappedPagesData;
        }

        [HttpPost]
        public async Task<List<PageData>> ScrapURLs(List<string> URLs, int secondsToScrap)
        {
            foreach(string URL in URLs)
            {
                WebsitesQueue.Enqueue(URL);
            }

            cancellationTokenSource = new CancellationTokenSource(secondsToScrap * 1000 + 10);

            int maxThreads = 10;
            var tasks = new Task[maxThreads];
            for(int i = 0; i < maxThreads; i++)
            {
                tasks[i] = RetrivePagesData(cancellationTokenSource);

            } 

            await Task.WhenAll(tasks);
            URLStripper urlStripper = new URLStripper();
            for(int i = 0; i < AllScrappedPagesData.Count; i++)
            {
                string pageURLHost = urlStripper.GetBaseURL(AllScrappedPagesData[i].PageURL);
                if (WebsitesReferencingEachSite.ContainsKey(pageURLHost))
                {
                    AllScrappedPagesData[i].PagesReferencingThisPage = WebsitesReferencingEachSite[pageURLHost];
                }
            }


            string combinedQuery = "INSERT INTO WebPages (URL) VALUES ";
            while (WebsitesQueue.Count > 1)
            {
                string website = WebsitesQueue.Dequeue();
                if (!currentWebsitesSoFar.Contains(website))
                {
                    combinedQuery += $"('{website}'), ";
                    currentWebsitesSoFar.Add(website);
                }
            }
            string finalwebsite = WebsitesQueue.Dequeue();
            combinedQuery += $"('{finalwebsite}');";
            await SQLiteManagerService.InsertWebPages(combinedQuery);
            //Task.Run(SendDataRegularly);

            return AllScrappedPagesData;
        }

        async Task RetrivePagesData(CancellationTokenSource tokenSource)
        {
            while (!tokenSource.IsCancellationRequested)
            {
                PageData retrivedData = await ScrapPage();
                lock (AllScrappedPagesDataLock)
                {
                    if(retrivedData != null)
                    {
                        AllScrappedPagesData.Add(retrivedData);
                    }
                }
                if (tokenSource.IsCancellationRequested)
                {
                    break;
                }
            }
            
        }

        async Task<PageData> ScrapPage()
        {
            //Initialize Helper classes for each thread scraping
            WordStemmer stemmer = new WordStemmer();
            RedundantWordsChecker redundantWordsChecker = new RedundantWordsChecker();
            Validators validadtors = new Validators();
            URLStripper urlStripper = new URLStripper();

            string url;
            lock (WebsitesQueueLock)
            {
                if(WebsitesQueue.Count == 0)
                {
                    return null;
                }
                url = WebsitesQueue.Dequeue();
            }

            string strippedURL = urlStripper.stripURL(url);
            if (validadtors.IsURLValid(strippedURL))
            {
                //Initialize the browser and retrive the page within the given link
                ScrapingBrowser browser = new ScrapingBrowser();
                browser.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                WebPage page;
                try
                {
                    page = await browser.NavigateToPageAsync(new Uri(strippedURL));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }

                //Initialize PageData object for storing useful information of the site
                PageData pageData = new PageData();
                pageData.PageURL = strippedURL;

                HtmlNode TitleNode = page.Html.SelectSingleNode("//title");
                string pageTitle = "";
                if(TitleNode != null)
                {
                    pageTitle = TitleNode.InnerText.Trim();
                }
                pageData.PageTitle = pageTitle;

                HtmlNode DescriptionNode = page.Html.SelectSingleNode("//meta[@name=\"description\"]");
                string pageDescription = "";
                if (DescriptionNode != null)
                {
                    pageDescription = DescriptionNode.InnerText.Trim();
                }
                pageData.Description = pageDescription;


                List<string> links = new List<string>();

                lock (currentWebsitesSoFarLock)
                {
                    //If that's the first time we have seen this webpage, store it in the hashmap of visited webpages
                    if (currentWebsitesSoFar.Contains(strippedURL))
                    {
                        return null;
                    }
                    currentWebsitesSoFar.Add(strippedURL);
                }


                //most important links are contained in <a></a> HTML elements
                var linksContainers = page.Html.CssSelect("a");
                foreach (HtmlNode linkContainer in linksContainers)
                {
                    //The actual link is stored in the href attribute (<a href="facebook.com"></a>)
                    string link = linkContainer.GetAttributeValue("href");
                    //If the URL is a valid one, we can proceed. Otherwise return null
                    if (validadtors.IsURLValid(link))
                    {
                        //Strip the URL from any parameters 
                        link = urlStripper.stripURL(link);
                        //Add the link to the list of links embedded in the webpage
                        links.Add(link);

                        lock (WebsitesQueueLock)
                        {
                            //If we have visited the website before, no need to enqueue it again
                            if (!currentWebsitesSoFar.Contains(link))
                            {
                                WebsitesQueue.Enqueue(link);
                            }
                        }

                        //Map each website host (www.wikipedia.org for example regardless of what webpage we're in) as a website that references whatever websites are embedded in it. 
                        lock (WebsitesReferencingEachSiteLock)
                        {
                            string linkHost = urlStripper.GetBaseURL(link);
                            string strippedURLHost = urlStripper.GetBaseURL(strippedURL);
                            if (WebsitesReferencingEachSite.ContainsKey(linkHost) && strippedURLHost != linkHost)
                            {
                                if (!WebsitesReferencingEachSite[linkHost].Contains(strippedURLHost))
                                {
                                    WebsitesReferencingEachSite[linkHost].Add(strippedURLHost);
                                }
                            }
                            else
                            {
                                WebsitesReferencingEachSite[linkHost] = new List<string>();
                                WebsitesReferencingEachSite[linkHost].Add(strippedURLHost);
                            }
                        }
                    }
                }

                //Once we are done with collecting links in a list, add them to the pageData object
                pageData.EmbeddedURLs = links;

                // Use CSS selectors to find all elements
                var nodes = page.Html.CssSelect(".");

                Dictionary<string, int> AllWordsOccurances = new Dictionary<string, int>();
                int totalValidWordCount = 0;
                foreach (var node in nodes)
                {
                    //Split the chunks of text by spaces, apostrophe, and new lines to get individual words
                    string[] text = node.InnerText.Trim().Split(new char[] { ' ', '\'', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string word in text)
                    {
                        //Filter out garbage text and redundant words like the, I, you, etc
                        if (validadtors.IsWordValid(word) && !redundantWordsChecker.IsRedundant(word))
                        {
                            //For each valid word, stem it (reduce it to its root) and update its count whenever we find it again
                            string stemmedWord = stemmer.stem(word);
                            if (AllWordsOccurances.ContainsKey(stemmedWord))
                            {
                                AllWordsOccurances[stemmedWord] += 1;
                            }
                            else
                            {
                                AllWordsOccurances[stemmedWord] = 1;
                            }
                            //The total valid words count in the page to calculate the frequency of each word 
                            totalValidWordCount++;
                        }
                    }
                }

                //After all words are collected with their number of occurances, create a WordData object for each word
                Dictionary<string, WordData> wordsData = new Dictionary<string, WordData>();
                foreach (KeyValuePair<string, int> keyValuePair in AllWordsOccurances)
                {
                    WordData currentWordData = new WordData()
                    {
                        Word = keyValuePair.Key,
                        Count = keyValuePair.Value,
                        Frequency = 1.0 * keyValuePair.Value / totalValidWordCount
                    };
                    wordsData[keyValuePair.Key] = currentWordData;
                }

                //Add this WordsData to the PageData
                pageData.WordsData = wordsData;
                return pageData;
            }
            return null;
        }

        async Task<string> BypassComplexURL(string url)
        {

            var (pythonOutput, pythonError, pythonExitCode) = await PythonRunner.RunPythonScriptAsync(url);

            if (pythonExitCode != 0)
                Console.WriteLine($"Python script failed to update HTML. Error: {pythonError}");

            return pythonOutput;
        }

    }
}
