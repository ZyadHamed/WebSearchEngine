using SeleniumBaseApi.Models;
using System.Collections.Generic;

namespace SeleniumBaseApi.Services;

public static class BypassUrlService
{
    static List<BypassUrl> BypassUrls { get; }
    static int nextId = 2;

    static BypassUrlService()
    {
        // this initial value is for testing purposes
        BypassUrls = new List<BypassUrl>
        {
            new BypassUrl {Id = 1, Url = "https://gitlab.com/users/sign_in", HtmlContent = null}
        };
    }

    public static List<BypassUrl> GetAll() => BypassUrls;

    public static BypassUrl? Get(int id) => BypassUrls.FirstOrDefault(url => url.Id == id);

    public static void Add(BypassUrl bypassUrl)
    {
        bypassUrl.Id = nextId++;
        BypassUrls.Add(bypassUrl);
    }

    public static void Delete(int id)
    {
        var bypassUrl = Get(id);
        if (bypassUrl is null)
            return;

        BypassUrls.Remove(bypassUrl);
    }
    
    public static void Update(BypassUrl bypassUrl)
    {
        var index = BypassUrls.FindIndex(url => url.Id == bypassUrl.Id);
        if (index == -1)
            return;

        BypassUrls[index] = bypassUrl;
    }
}