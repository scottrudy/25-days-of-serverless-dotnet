using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace day03
{
    public static class FileUpload
    {
        [FunctionName(nameof(HttpFileUpload))]
        public static async Task<IActionResult> HttpFileUpload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "photo/{fileName}")]
            HttpRequest req, ILogger log, string fileName,
            [Blob("mycontainer/{fileName}", FileAccess.Write)] Stream photo) {

            if (!fileName.EndsWith("png")) {
                return new BadRequestResult();
            }

            await req.Body.CopyToAsync(photo);

            return (ActionResult)new AcceptedResult();
        }
    }
}
