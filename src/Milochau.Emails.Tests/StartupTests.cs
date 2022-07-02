using Milochau.Emails.DataAccess;
using Milochau.Emails.Options;
using Milochau.Emails.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SendGrid;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace Milochau.Emails.Tests
{
    [TestClass]
    public class StartupTests
    {
        private readonly ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

        [TestMethod]
        public void ConfigureServices_Should_RegisterAllServices_When_Called()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "SendGrid:Key", "Key" },
                { "Database:ConnectionString", "AccountEndpoint=https://example.com;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" },
                { "Database:DatabaseName", "databasename" }
            });

            var configuration = configurationBuilder.Build();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton(Mock.Of<IHostEnvironment>());
            serviceCollection.AddSingleton(Mock.Of<HtmlEncoder>());
            serviceCollection.AddLogging();

            var startup = TestableStartup.Create<Startup>(configuration);

            // Act
            startup.ConfigureServices(serviceCollection);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.IsNotNull(serviceProvider.GetService<IOptions<EmailsOptions>>());
            Assert.IsNotNull(serviceProvider.GetService<IOptions<SendGridOptions>>());

            Assert.IsNotNull(serviceProvider.GetService<IEmailsService>());

            Assert.IsNotNull(serviceProvider.GetService<IEmailsDataAccess>());
            Assert.IsNotNull(serviceProvider.GetService<ISendGridClient>());
        }

        public class TestableStartup : Startup
        {
        }
    }
}
