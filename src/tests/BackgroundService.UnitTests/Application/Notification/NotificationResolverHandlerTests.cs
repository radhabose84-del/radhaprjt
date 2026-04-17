using BackgroundService.Application.DTO;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Application.Notification;
using Microsoft.Extensions.Logging;

namespace BackgroundService.UnitTests.Application.Notification
{
    public sealed class NotificationResolverHandlerTests
    {
        private readonly Mock<INotificationUserResolver> _mockResolver = new(MockBehavior.Strict);
        private readonly Mock<ILogger<NotificationResolverHandler>> _mockLogger = new(MockBehavior.Loose);

        private NotificationResolverHandler CreateSut() =>
            new(_mockResolver.Object, _mockLogger.Object);

        // --- ResolveNotificationChannelsAsync ---

        [Fact]
        public async Task ResolveNotificationChannels_EmptyTargets_ReturnsEmptyList()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>());

            var result = await CreateSut().ResolveNotificationChannelsAsync(
                1, "M", 2, "a@b.com", "", "");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ResolveNotificationChannels_NormalizesChannels()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>
                {
                    new() { ChannelName = "Email" },
                    new() { ChannelName = "Sms" },
                    new() { ChannelName = "In-App" },
                    new() { ChannelName = "whatsapp" }
                });

            var result = await CreateSut().ResolveNotificationChannelsAsync(
                1, "M", 2, "", "", "");

            result.Should().Contain(new[] { "Email", "SMS", "InApp", "WhatsApp" });
        }

        [Fact]
        public async Task ResolveNotificationChannels_Deduplicates_CaseInsensitive()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>
                {
                    new() { ChannelName = "email" },
                    new() { ChannelName = "Email" },
                    new() { ChannelName = "EMAIL" }
                });

            var result = await CreateSut().ResolveNotificationChannelsAsync(
                1, "M", 2, "", "", "");

            result.Should().ContainSingle().Which.Should().Be("Email");
        }

        // --- ResolveNotificationTemplatesAsync ---

        [Fact]
        public async Task ResolveNotificationTemplates_EmptyTargets_ReturnsDefaults()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>());

            var result = await CreateSut().ResolveNotificationTemplatesAsync(
                1, "M", 2, "", "", "", "p1", "p2", DateTimeOffset.UtcNow,
                null, null, null, null, null, null, null);

            result.To.Should().BeEmpty();
            result.Sms.Should().BeEmpty();
            result.InApp.Should().BeEmpty();
            result.Subject.Should().Be(string.Empty);
            result.Body.Should().Be(string.Empty);
            result.TemplateId.Should().Be(0);
            result.IsTable.Should().BeFalse();
            result.Lang.Should().Be("en");
        }

        [Fact]
        public async Task ResolveNotificationTemplates_Splits_EmailTargetIds()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>
                {
                    new()
                    {
                        ChannelName = "Email",
                        TargetEmailIds = "a@x.com, b@x.com",
                        TargetCcEmails = "cc@x.com",
                        TargetBccEmails = "bcc@x.com",
                        SubjectTemplate = "Subj",
                        BodyTemplate = "Body",
                        HeaderTemplate = "Hdr",
                        FooterTemplate = "Ftr",
                        TemplateId = 42,
                        LanguageCode = "en"
                    }
                });

            var result = await CreateSut().ResolveNotificationTemplatesAsync(
                1, "M", 2, "", "", "", "p1", "p2", DateTimeOffset.UtcNow,
                null, null, null, null, null, null, null);

            result.To.Should().Contain(new[] { "a@x.com", "b@x.com" });
            result.Cc.Should().ContainSingle("cc@x.com");
            result.Bcc.Should().ContainSingle("bcc@x.com");
            result.Subject.Should().Be("Subj");
            result.Body.Should().Be("Body");
            result.TemplateId.Should().Be(42);
            result.Lang.Should().Be("en");
        }

        [Fact]
        public async Task ResolveNotificationTemplates_SmsFallback_WhenNoExplicitSmsChannel()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>
                {
                    new()
                    {
                        ChannelName = "Email",
                        TargetEmailIds = "a@x.com",
                        TargetMobileNumbers = "9999999999"
                    }
                });

            var result = await CreateSut().ResolveNotificationTemplatesAsync(
                1, "M", 2, "", "", "", "p1", "p2", DateTimeOffset.UtcNow,
                null, null, null, null, null, null, null);

            result.Sms.Should().ContainSingle("9999999999");
        }

        [Fact]
        public async Task ResolveNotificationTemplates_ParsesInApp_UserIds()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>
                {
                    new()
                    {
                        ChannelName = "InApp",
                        TargetUserIds = "10, 20, 30"
                    }
                });

            var result = await CreateSut().ResolveNotificationTemplatesAsync(
                1, "M", 2, "", "", "", "p1", "p2", DateTimeOffset.UtcNow,
                null, null, null, null, null, null, null);

            result.InApp.Should().BeEquivalentTo(new[] { 10, 20, 30 });
        }

        [Fact]
        public async Task ResolveNotificationTemplates_ApiToken_Preference_WhatsApp_Wins()
        {
            _mockResolver
                .Setup(r => r.GetNotificationTargetsAsync(1, "M", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<NotificationTargetDto>
                {
                    new() { ChannelName = "Email", ApiToken = "email-token" },
                    new() { ChannelName = "SMS", ApiToken = "sms-token" },
                    new() { ChannelName = "WhatsApp", ApiToken = "wa-token" }
                });

            var result = await CreateSut().ResolveNotificationTemplatesAsync(
                1, "M", 2, "", "", "", "p1", "p2", DateTimeOffset.UtcNow,
                null, null, null, null, null, null, null);

            result.ApiToken.Should().Be("wa-token");
        }
    }
}
