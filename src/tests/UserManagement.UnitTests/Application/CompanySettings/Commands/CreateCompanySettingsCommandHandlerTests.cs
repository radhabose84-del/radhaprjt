using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.CompanySettings.Commands.CreateCompanySettings;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.CompanySettings.Commands
{
    public sealed class CreateCompanySettingsCommandHandlerTests
    {
        private readonly Mock<ICompanyCommandSettings> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateCompanySettingsCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateCompanySettingsCommand ValidCommand() =>
            new()
            {
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
                FinancialYear = 2024
            };

        private static UserManagement.Domain.Entities.CompanySettings ValidEntity() =>
            new() { Id = 1 };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.CompanySettings>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Company Setting"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.CompanySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.CompanySettings>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not created*");
        }
    }
}
