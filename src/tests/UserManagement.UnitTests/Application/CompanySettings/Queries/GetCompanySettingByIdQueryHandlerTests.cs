using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettingsById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.CompanySettings.Queries
{
    public sealed class GetCompanySettingByIdQueryHandlerTests
    {
        private readonly Mock<ICompanyQuerySettings> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCompanySettingByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.CompanySettings ValidEntity() =>
            new() { Id = 1, CompanyId = 1, SessionTimeout = 30 };

        private static CompanySettingsDTO ValidDto() =>
            new() { Id = 1, CompanyId = 1, SessionTimeout = 30 };

        [Fact]
        public async Task Handle_ExistingSettings_ReturnsSuccess()
        {
            var entity = ValidEntity();
            var dto = ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetAsync())
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CompanySettingsDTO>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCompanySettingByIdQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NullEntity_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetAsync())
                .ReturnsAsync((UserManagement.Domain.Entities.CompanySettings?)null!);

            var result = await CreateSut().Handle(new GetCompanySettingByIdQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_ExistingSettings_PublishesAuditEvent()
        {
            var entity = ValidEntity();
            var dto = ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetAsync())
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CompanySettingsDTO>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetCompanySettingByIdQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "Company Setting"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingSettings_CallsGetAsyncOnce()
        {
            var entity = ValidEntity();
            var dto = ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetAsync())
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CompanySettingsDTO>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetCompanySettingByIdQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAsync(), Times.Once);
        }
    }
}
