using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Milochau.Emails.Sdk.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Milochau.Emails.DataAccess
{
    public class StorageDataAccess : IStorageDataAccess
    {
        public async Task<Stream> ReadToStreamAsync(EmailAttachment attachment, CancellationToken cancellationToken)
        {
            var blobServiceClient = new BlobServiceClient(options.DefaultConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(attachment.ContainerName);
            var blobClient = blobContainerClient.GetBlobClient(attachment.FileName);
            
            return await blobClient.OpenReadAsync(new BlobOpenReadOptions(allowModifications: false), cancellationToken);
        }
    }
}
