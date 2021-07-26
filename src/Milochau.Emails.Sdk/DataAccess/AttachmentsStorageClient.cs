using Azure.Storage.Blobs;
using Milochau.Emails.Sdk.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Milochau.Emails.Sdk.DataAccess
{
    internal class AttachmentsStorageClient : IAttachmentsClient
    {
        private const string defaultContainerName = "default";

        /// <summary>Write an attachment into a stream</summary>
        /// <param name="attachment">Attachment content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The document URI</returns>
        public async Task<Uri> WriteFromStreamAsync(EmailAttachmentContent attachment, CancellationToken cancellationToken)
        {
            var blobServiceClient = new BlobServiceClient(options.DefaultConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(defaultContainerName);
            var fileName = Guid.NewGuid().ToString();
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(attachment.Content, overwrite: false, cancellationToken);

            return blobClient.Uri;
        }
    }
}
