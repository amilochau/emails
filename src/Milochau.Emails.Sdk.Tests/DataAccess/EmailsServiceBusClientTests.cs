using Milochau.Emails.Sdk.DataAccess;
using Milochau.Emails.Sdk.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Milochau.Emails.Sdk.Helpers;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;

namespace Milochau.Emails.Sdk.UnitTests.DataAccess
{
    [TestClass]
    public class EmailsServiceBusClientTests
    {
        private Mock<ServiceBusClient> serviceBusClient;
        private Mock<ServiceBusSender> serviceBusSender;
        private Mock<IEmailsValidationHelper> emailsValidationHelper;
        private Mock<ILogger<EmailsServiceBusClient>> logger;

        private EmailsServiceBusClient emailsServiceBusClient;

        [TestInitialize]
        public void Initialize()
        {
            serviceBusClient = new Mock<ServiceBusClient>();
            serviceBusSender = new Mock<ServiceBusSender>();
            emailsValidationHelper = new Mock<IEmailsValidationHelper>();
            logger = new Mock<ILogger<EmailsServiceBusClient>>();

            serviceBusClient.Setup(x => x.CreateSender("emails")).Returns(serviceBusSender.Object);

            emailsServiceBusClient = new EmailsServiceBusClient(serviceBusClient.Object, emailsValidationHelper.Object, logger.Object);
        }

        [TestMethod]
        public async Task SendEmailAsync_Should_CallQueueClient_When_Called()
        {
            // Arrange
            var email = new Email();

            // Act
            await emailsServiceBusClient.SendEmailAsync(email, CancellationToken.None);

            // Assert
            serviceBusSender.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()));
        }
    }
}
