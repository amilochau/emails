using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace Milochau.Emails.Tests.Functions
{
    public abstract class BaseFunctionsTests
    {
        protected static FunctionContext CreateFunctionsContext()
        {
            var serviceCollection = new ServiceCollection();

            var workerOptions = new Mock<IOptions<WorkerOptions>>();
            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            workerOptions.SetupGet(x => x.Value).Returns(new WorkerOptions
            {
                Serializer = new JsonObjectSerializer(jsonSerializerOptions)
            });

            serviceCollection.AddSingleton(workerOptions.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new Mock<FunctionContext>();
            context.SetupProperty(c => c.InstanceServices, serviceProvider);

            return context.Object;
        }
    }
}
