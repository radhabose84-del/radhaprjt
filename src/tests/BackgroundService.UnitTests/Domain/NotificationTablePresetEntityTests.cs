using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationTablePresetEntityTests
    {
        [Fact]
        public void NotificationTablePreset_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var rowVersion = new byte[] { 1, 2, 3, 4 };
            var entity = new NotificationTablePreset
            {
                Id = 1,
                TemplateId = 10,
                PresetKey = "email-preset",
                ColumnsJson = "{\"col1\":\"value1\"}",
                Version = 3,
                IsActive = true,
                CreatedAt = now,
                RowVersion = rowVersion
            };

            entity.Id.Should().Be(1);
            entity.TemplateId.Should().Be(10);
            entity.PresetKey.Should().Be("email-preset");
            entity.ColumnsJson.Should().Be("{\"col1\":\"value1\"}");
            entity.Version.Should().Be(3);
            entity.IsActive.Should().BeTrue();
            entity.CreatedAt.Should().Be(now);
            entity.RowVersion.Should().BeEquivalentTo(rowVersion);
        }

        [Fact]
        public void NotificationTablePreset_DefaultValues_ShouldBeCorrect()
        {
            var entity = new NotificationTablePreset();

            entity.PresetKey.Should().Be(string.Empty);
            entity.ColumnsJson.Should().Be(string.Empty);
            entity.IsActive.Should().BeTrue();
        }

        [Fact]
        public void NotificationTablePreset_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationTablePreset
            {
                Version = null,
                RowVersion = null,
                Template = null
            };

            entity.Version.Should().BeNull();
            entity.RowVersion.Should().BeNull();
            entity.Template.Should().BeNull();
        }

        [Fact]
        public void NotificationTablePreset_NavigationProperty_ShouldBeAssignable()
        {
            var template = new NotificationTemplate { Id = 10 };
            var entity = new NotificationTablePreset
            {
                TemplateId = 10,
                Template = template
            };

            entity.Template.Should().NotBeNull();
            entity.Template!.Id.Should().Be(10);
        }

        [Fact]
        public void NotificationTablePreset_Id_DefaultShouldBeZero()
        {
            var entity = new NotificationTablePreset();
            entity.Id.Should().Be(0);
        }
    }
}
