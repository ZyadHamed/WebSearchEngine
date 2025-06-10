namespace SeleniumBaseApi.Models;

public class BypassUrl
{
    // for caching purposes only == won't be sent back
    public int Id { get; set; }
    
    public string? Url { get; set; }

    // the content to be returned
    public string? HtmlContent { get; set; }

    // It is checked regularly to determene when to send the actual response
    // still to be used
    public bool? HasHtmlContent { get; set; }
}