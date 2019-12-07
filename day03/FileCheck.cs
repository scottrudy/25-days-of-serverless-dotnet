using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;

namespace day03
{
    public static class FileCheck
    {
        [FunctionName("FileCheck")]
        public static async Task Run([BlobTrigger("mycontainer/{fileName}", Connection = "AzureWebJobsStorage")]Stream photo, string fileName)
        {
            string pngHeader = Encoding.ASCII.GetString(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
            var isPng = false;
            if (photo.Length > 0) {
                byte[] photoHeader = new byte[pngHeader.Length];
                await photo.ReadAsync(photoHeader, 0, photoHeader.Length);
                isPng = pngHeader.Equals(Encoding.ASCII.GetString(photoHeader));
            }

            if (!isPng) {
                var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("mycontainer");
                await container.CreateIfNotExistsAsync();
                await container.GetBlockBlobReference(fileName).DeleteIfExistsAsync();
            }
        }
    }
}
