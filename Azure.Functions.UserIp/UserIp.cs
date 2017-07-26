using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Web;
using MaxMind.GeoIP2;
using System.IO;

namespace Azure.Functions.UserIp
{
    public static class UserIp
    {
        [FunctionName("UserIp")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            
            log.Info("C# HTTP trigger function processed a request.");
            var ip = "83.249.92.71";
            if (req.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = req.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null)
                {
                    ip = ctx.Request.UserHostAddress;
                }
            }

            var dir = new DirectoryInfo(context.FunctionDirectory);
            log.Info($"looking in {dir.Parent.FullName}");
            var dbLOcation = Directory.EnumerateFiles(dir.Parent.FullName, "GeoLite2-City.mmdb", SearchOption.AllDirectories);
            using (var db = new DatabaseReader(dbLOcation.First(), MaxMind.Db.FileAccessMode.MemoryMapped))
            {
                //var info = db.AnonymousIP(ip);
                var city = db.City(ip);
                return req.CreateResponse<dynamic>(new
                {
                    City = city,
                    Ip = ip
                });
                //return req.CreateResponse(HttpStatusCode.OK, "Hello " + ip + " headers: " + string.Join(",", req.Headers.GetValues("X-Forwarded-For")));
            }

                
        }
    }
}