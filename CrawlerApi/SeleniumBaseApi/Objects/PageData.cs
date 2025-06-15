namespace SeleniumBaseApi.Objects
{
    public class PageData
    {
        public string PageURL { get; set; }
        public string PageTitle { get; set; }
        public List<string> PagesReferencingThisPage { get; set; }
        public Dictionary<string, WordData> WordsData { get; set; }
        public List<string> EmbeddedURLs { get; set; }

        public string Description { get; set; } = "";
    }
}
