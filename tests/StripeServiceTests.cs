using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DotnetStripePaymentDemo.Services;
using Stripe;

namespace DotnetStripePaymentDemo.Tests
{
    /// <summary>
    /// Unit tests for StripeService
    /// </summary>
    public class StripeServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<StripeService>> _mockLogger;

        public StripeServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<StripeService>>();

            // Setup default configuration
            _mockConfiguration
                .Setup(c => c["Stripe:SecretKey"])
                .Returns("sk_test_fake_key_123456");

            _mockConfiguration
                .Setup(c => c["Stripe:PublishableKey"])
                .Returns("pk_test_fake_key_123456");

            _mockConfiguration
                .Setup(c => c["Stripe:WebhookSecret"])
                .Returns("whsec_test_fake_secret");
        }

        [Fact]
        public void Constructor_WithValidConfig_ShouldInitialize()
        {
            // Arrange & Act
            var service = new StripeService(_mockConfiguration.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithoutSecretKey_ShouldThrowException()
        {
            // Arrange
            _mockConfiguration
                .Setup(c => c["Stripe:SecretKey"])
                .Returns((string)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                new StripeService(_mockConfiguration.Object, _mockLogger.Object));
        }

        [Fact]
        public void GetPublishableKey_ShouldReturnKey()
        {
            // Arrange
            var service = new StripeService(_mockConfiguration.Object, _mockLogger.Object);
            var expectedKey = "pk_test_fake_key_123456";

            // Act
            var result = service.GetPublishableKey();

            // Assert
            Assert.Equal(expectedKey, result);
        }

        [Fact]
        public void GetWebhookSecret_ShouldReturnSecret()
        {
            // Arrange
            var service = new StripeService(_mockConfiguration.Object, _mockLogger.Object);
            var expectedSecret = "whsec_test_fake_secret";

            // Act
            var result = service.GetWebhookSecret();

            // Assert
            Assert.Equal(expectedSecret, result);
        }

        [Fact]
        public void GetPublishableKey_WhenNotConfigured_ShouldThrowException()
        {
            // Arrange
            _mockConfiguration
                .Setup(c => c["Stripe:PublishableKey"])
                .Returns((string)null);

            var service = new StripeService(_mockConfiguration.Object, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.GetPublishableKey());
        }

        [Fact]
        public void GetWebhookSecret_WhenNotConfigured_ShouldThrowException()
        {
            // Arrange
            _mockConfiguration
                .Setup(c => c["Stripe:WebhookSecret"])
                .Returns((string)null);

            var service = new StripeService(_mockConfiguration.Object, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.GetWebhookSecret());
        }
    }
}
