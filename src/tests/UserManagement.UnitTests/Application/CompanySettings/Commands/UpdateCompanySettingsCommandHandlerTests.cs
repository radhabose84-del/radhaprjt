using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.CompanySettings.Commands.UpdateCompanySettings;

namespace UserManagement.UnitTests.Application.CompanySettings.Commands
{
    public sealed class UpdateCompanySettingsCommandHandlerTests
    {
        private readonly Mock<ICompanyCommandSettings> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateCompanySettingsCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateCompanySettingsCommand ValidCommand() =>
            new()
            {
                Id = 1,
                CompanyId = 1,
                PasswordHistoryCount = 5,
                SessionTimeout = 30,
                FailedLoginAttempts = 3,
                AutoReleaseTime = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 5,
                ForgotPasswordCodeExpiry = 60,
                CaptchaOnLogin = 1,
                Currency = 1,
                Language = 1,
                TimeZone = 1,
                FinancialYear = 2024,
                IsActive = 1
            };

        private static UserManagement.Domain.Entities.CompanySettings ValidEntity() =>
            new() { Id = 1 };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.CompanySettings>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsException()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not updated*");
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command),
                Times.Once);
        }
    }
}
