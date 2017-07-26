using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace Azure.Functions.UserIp
{
    public static class DownloadGeoLiteDb
    {
        [FunctionName("DownloadGeoLiteDb")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info("C# HTTP trigger function processed a request.");
            HttpClient httpClient = new HttpClient();

            var stream = await httpClient.GetStreamAsync("http://geolite.maxmind.com/download/geoip/database/GeoLite2-City.tar.gz");
            
            DirectoryInfo targetDirectory = new DirectoryInfo(context.FunctionDirectory).Parent;
            if (!targetDirectory.Exists)
            {
                targetDirectory.Create();
            }
            using (Stream sourceStream = new GZipInputStream(stream))
            {
                using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(sourceStream, TarBuffer.DefaultBlockFactor))
                {
                    tarArchive.ExtractContents(targetDirectory.FullName);
                }
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}