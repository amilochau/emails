using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Milochau.Core.Abstractions.Exceptions;
using Milochau.Emails.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Milochau.Emails.DataAccess.Helpers
{
    public static class CosmosClientExtensions
    {
        public const string EntityNotFoundExceptionMessage = "Entity is not found.";

        public async static Task CreateItemAsync<TItem>(this CosmosClient cosmosClient, string databaseName, string containerName, TItem item, string partitionKey, ILogger logger, CancellationToken cancellationToken)
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var response = await container.CreateItemAsync(item, new PartitionKey(partitionKey), null, cancellationToken);

            logger.LogResponse(response, "create");
        }

        public async static Task<TItem> ReadPointItemAsync<TItem>(this CosmosClient cosmosClient, string databaseName, string containerName, string id, string partitionKey, ILogger logger, CancellationToken cancellationToken)
            where TItem : IEntity
        {
            try
            {
                var container = cosmosClient.GetContainer(databaseName, containerName);
                var response = await container.ReadItemAsync<TItem>(id, new PartitionKey(partitionKey), null, cancellationToken);

                logger.LogResponse(response, "read point");

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new NotFoundException(EntityNotFoundExceptionMessage);
            }
        }

        public static IOrderedQueryable<TItem> QueryItems<TItem>(this CosmosClient cosmosClient, string databaseName, string containerName, string? partitionKey)
            where TItem : IEntity
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            return container.GetItemLinqQueryable<TItem>(false, null, new QueryRequestOptions
            {
                PartitionKey = partitionKey != null ? new PartitionKey(partitionKey) : null,
            });
        }

        public async static Task<TItem> GetSingleItemAsync<TItem>(this IQueryable<TItem> query, ILogger logger, CancellationToken cancellationToken)
        {
            using var feedIterator = query.ToFeedIterator();

            // Only read one item, so we don't need to loop with the 'feedIterator.HasMoreResults'
            var response = await feedIterator.ReadNextAsync(cancellationToken);

            logger.LogResponse(response, "get single");

            if (response.Count != 1)
            {
                throw new NotFoundException(EntityNotFoundExceptionMessage);
            }

            return response.First();
        }

        public async static IAsyncEnumerable<TItem> ListItemsAsync<TItem>(this IQueryable<TItem> query, ILogger logger, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var feedIterator = query.ToFeedIterator();

            // Iterate query result pages
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                logger.LogResponse(response, "list");

                foreach (var item in response)
                {
                    yield return item;
                }
            }
        }

        public async static Task PatchItemAsync<TItem>(this CosmosClient cosmosClient, string databaseName, string containerName, string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, ILogger logger, CancellationToken cancellationToken)
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var response = await container.PatchItemAsync<TItem>(id, new PartitionKey(partitionKey), patchOperations, null, cancellationToken);

            logger.LogResponse(response, "patch");
        }

        public async static Task RemoveItemAsync<TItem>(this CosmosClient cosmosClient, string databaseName, string containerName, string id, string partitionKey, ILogger logger, CancellationToken cancellationToken)
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var response = await container.DeleteItemAsync<TItem>(id, new PartitionKey(partitionKey), null, cancellationToken);

            logger.LogResponse(response, "remove");
        }

        private static void LogResponse<TItem>(this ILogger logger, ItemResponse<TItem> response, string operationType)
        {
            logger.LogInformation($"{typeof(TItem)} - {operationType} - RequestCharge: {response.RequestCharge}");
            logger.LogInformation(response.Diagnostics.ToString());
        }
        private static void LogResponse<TItem>(this ILogger logger, FeedResponse<TItem> response, string operationType)
        {
            logger.LogInformation($"{typeof(TItem)} - {operationType} - RequestCharge: {response.RequestCharge}");
            logger.LogInformation(response.Diagnostics.ToString());
        }
    }
}
