using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace slackbot.Controllers
{
    [Route("api/[controller]")]
    public class stockscontroller : Controller
    {
        private string yahooUrl = "http://download.finance.yahoo.com/d/quotes.csv?s={0}&f=nl1op";

        // POST api/values
        [HttpPost]
        public async Task<object> Post(SlackRequest request)
        {
            if (request.token != Important.token)
            {
                return new
                {
                    text = "Bad request. Token mismatch. Check config."
                };
            }

            using (var client = new HttpClient())
            {
                var parsed = request.text.Trim().Substring(request.text.IndexOf(':'));

                if (parsed.Length < 3)
                {
                    return new
                    {
                        text = "Unable to find a stock symbol in your request."
                    };
                }

                var sendstring = string.Format(yahooUrl, parsed);
                var result = await client.GetAsync(sendstring);
                var response = "";

                switch (result.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        response = await ProcessResult(result, request.text);
                        break;
                    default:
                        response = string.Format("A network error occurred. Status code {0} from Yahoo.", result.StatusCode);
                        break;
                }

                var objtosend = new
                {
                    text = response
                };

                return objtosend;
            }
        }

        public async Task<string> ProcessResult(HttpResponseMessage thing, string symbol)
        {
            var csv = await thing.Content.ReadAsStringAsync();

            using (var client = new HttpClient())
            {
                if (csv.Contains("N/A"))
                {
                    return string.Format("Your search for symbol {0} returned no results. This is most likely because your symbol could not be found.", symbol);
                }
                else
                {
                    var items = csv.Replace("\"", "").Replace("\n", "").Split(',');
                    return string.Format("The current stock price for {0} (symbol {2}) is ${1}", items[0], items[1], symbol);
                }
            }
        }
    }
}
