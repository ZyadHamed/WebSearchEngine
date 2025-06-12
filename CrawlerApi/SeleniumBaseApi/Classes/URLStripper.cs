namespace SeleniumBaseApi.Classes
{
    public class URLStripper
    {
        public string stripURL(string URL)
        {
            //string firstRedundancy = "http://www.";
            //bool HTTPFlag = true;
            //bool HTTPsFlag = true;
            //bool wwwFlag = true;

            string returnedURL = "";
            //for (int i = 0; i < 4; i++)
            //{
            //    if (URL[i] != firstRedundancy[i])
            //    {
            //        HTTPFlag = false;
            //        break;
            //    }
            //}
            //if (HTTPFlag == true && URL[4] == 's')
            //{
            //    HTTPFlag = false;
            //    HTTPsFlag = true;
            //}
            //else
            //{
            //    HTTPsFlag = false;
            //}

            //if (HTTPFlag == true)
            //{
            //    if (URL.Length < 12 || URL.Substring(7, 4) != "www.")
            //    {
            //        wwwFlag = false;
            //    }
            //}
            //else if (HTTPsFlag == true)
            //{
            //    if (URL.Length < 13 || URL.Substring(8, 4) != "www.")
            //    {
            //        wwwFlag = false;
            //    }
            //}
            //else
            //{
            //    if (URL.Length < 5 || URL.Substring(0, 4) != "www.")
            //    {
            //        wwwFlag = false;
            //    }
            //};

            //if(HTTPFlag == true)
            //{
            //    if(wwwFlag == true)
            //    {
            //        returnedURL = URL.Substring(11);
            //    }
            //    else
            //    {
            //        returnedURL = URL.Substring(7);
            //    }
            //}
            //else if(HTTPsFlag == true)
            //{
            //    if (wwwFlag == true)
            //    {
            //        returnedURL = URL.Substring(12);
            //    }
            //    else
            //    {
            //        returnedURL = URL.Substring(8);
            //    }
            //}
            //else if(wwwFlag == true)
            //{
            //    returnedURL = URL.Substring(4);
            //}
            //else
            //{
                returnedURL = URL;
            //}

            int indexOfParameters = returnedURL.IndexOf("?"); 
            if (indexOfParameters != -1)
            {
                returnedURL = returnedURL.Substring(0, indexOfParameters);
            }
            return returnedURL;
        }
        public string GetBaseURL(string URL)
        {
            UriBuilder uriBuilder = new UriBuilder(URL);
            return uriBuilder.Host;
        }
    }
}
