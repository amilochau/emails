using Milochau.Emails.DataAccess;
using Milochau.Emails.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Milochau.Emails.DataAccess.Implementations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Milochau.Emails.Options;

namespace Milochau.Emails.Tests.DataAccess
{
    [TestClass]
    public class EmailsSendGridClientTests
    {
        private Mock<ISendGridClient> sendGridClient = null!;
        private Mock<IStorageDataAccess> storageDataAccess = null!;
        private Mock<CosmosClient> cosmosClient = null!;
        private Mock<IOptions<DatabaseOptions>> databaseOptions = null!;
        private Mock<ILogger<EmailsSendGridClient>> logger = null!;

        private EmailsSendGridClient emailsSendGridClient = null!;

        [TestInitialize]
        public void Intiialize()
        {
            sendGridClient = new Mock<ISendGridClient>();
            var sendGridResponse = new Response(System.Net.HttpStatusCode.OK, null, null);
            sendGridClient.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), default)).ReturnsAsync(sendGridResponse);

            storageDataAccess = new Mock<IStorageDataAccess>();
            cosmosClient = new Mock<CosmosClient>();
            databaseOptions = new Mock<IOptions<DatabaseOptions>>();
            logger = new Mock<ILogger<EmailsSendGridClient>>();

            var container = new Mock<Container>();
            var response = new Mock<ItemResponse<Emails.DataAccess.Entities.EmailTracking>>();
            response.SetupGet(x => x.Diagnostics).Returns(new Mock<CosmosDiagnostics>().Object);
            container.Setup(x => x.CreateItemAsync(It.IsAny<Emails.DataAccess.Entities.EmailTracking>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), default)).ReturnsAsync(response.Object);
            cosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(container.Object);
            databaseOptions.SetupGet(x => x.Value).Returns(new DatabaseOptions());

            emailsSendGridClient = new EmailsSendGridClient(
                sendGridClient.Object,
                storageDataAccess.Object,
                cosmosClient.Object,
                databaseOptions.Object,
                logger.Object);
        }

        [TestMethod]
        public async Task SendEmailAsync_Should_CallSendGridClient_When_CalledWithEmptyEmail()
        {
            // Arrange
            var email = new Email();

            // Act
            await emailsSendGridClient.SendEmailAsync(email, CancellationToken.None);

            // Assert
            sendGridClient.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SendEmailAsync_Should_ContainProperRecipient_When_Called()
        {
            // Arrange
            var recipient = "bill.gates@microsoft.com";
            var email = new Email
            {
                Tos = new List<Emails.Sdk.Models.EmailAddress>
                {
                    new Emails.Sdk.Models.EmailAddress { Email = recipient }
                }
            };

            // Act
            await emailsSendGridClient.SendEmailAsync(email, CancellationToken.None);

            // Assert
            sendGridClient.Verify(x => x.SendEmailAsync(It.Is<SendGridMessage>(x => x.Personalizations.Any(p => p.Tos != null && p.Tos.Any(t => t.Email == recipient))), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SendEmailAsync_Should_ContainProperReplyTo_When_Called()
        {
            // Arrange
            var sender = "bill.gates@microsoft.com";
            var email = new Email
            {
                ReplyTo = new Emails.Sdk.Models.EmailAddress { Email = sender }
            };

            // Act
            await emailsSendGridClient.SendEmailAsync(email, CancellationToken.None);

            // Assert
            sendGridClient.Verify(x => x.SendEmailAsync(It.Is<SendGridMessage>(x => x.ReplyTo.Email == sender), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod("SetImportance - all cases")]
        [DataRow(ImportanceType.High, "high")]
        [DataRow(ImportanceType.Low, "low")]
        public void SetImportance_Should_AddHeaders_When_Called(ImportanceType emailImportance, string sendGridHeaderValue)
        {
            // Arrange
            var sendGridMessage = new SendGridMessage();
            var email = new Email { Importance = emailImportance };

            // Act
            emailsSendGridClient.SetImportance(sendGridMessage, email);

            // Assert
            Assert.AreEqual(sendGridHeaderValue, sendGridMessage.Personalizations[0].Headers["Importance"]);
        }

        [TestMethod("SetPriority - all cases")]
        [DataRow(PriorityType.Urgent, "urgent")]
        [DataRow(PriorityType.NonUrgent, "non-urgent")]
        public void SetPriority_Should_AddHeaders_When_Called(PriorityType emailPriority, string sendGridHeaderValue)
        {
            // Arrange
            var sendGridMessage = new SendGridMessage();
            var email = new Email { Priority = emailPriority };

            // Act
            emailsSendGridClient.SetPriority(sendGridMessage, email);

            // Assert
            Assert.AreEqual(sendGridHeaderValue, sendGridMessage.Personalizations[0].Headers["Priority"]);
        }
    }
}
