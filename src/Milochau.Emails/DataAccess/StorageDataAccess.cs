using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milochau.Core.Abstractions;
using Milochau.Emails.Models.Options;
using Milochau.Emails.Sdk.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Milochau.Emails.DataAccess
{
    internal class StorageDataAccess : IStorageDataAccess
    {
        private readonly IOptions<CoreHostOptions> hostOptions;
        private readonly EmailsOptions options;
        private readonly ILogger<StorageDataAccess> logger;

        private const string defaultContainerName = "default";

        public StorageDataAccess(IOptions<CoreHostOptions> hostOptions,
            EmailsOptions options,
            ILogger<StorageDataAccess> logger)
        {
            this.hostOptions = hostOptions;
            this.options = options;
            this.logger = logger;
        }


        public async Task<Stream> ReadToStreamAsync(EmailAttachment attachment, CancellationToken cancellationToken)
        {
            var credential = new DefaultAzureCredential(hostOptions?.Value.Credential);
            var blobServiceClient = new BlobServiceClient(options.StorageAccountUri, credential);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(defaultContainerName);
            var blobClient = blobContainerClient.GetBlobClient(attachment.FileName);

            logger.LogDebug("Opening a blob from Storage Account...");
            return await blobClient.OpenReadAsync(new BlobOpenReadOptions(allowModifications: false), cancellationToken);
        }
    }
}
