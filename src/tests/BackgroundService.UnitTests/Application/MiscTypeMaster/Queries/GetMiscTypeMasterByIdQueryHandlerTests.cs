using AutoMapper;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static BackgroundService.Domain.Entities.Notification.MiscTypeMaster ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeCode = "TYPE1",
                Description = "Type One"
            };

        private static GetMiscTypeMasterDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeCode = "TYPE1",
                Description = "Type One"
            };

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(ValidEntity(1));

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .Returns(ValidDto(1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((BackgroundService.Domain.Entities.Notification.MiscTypeMaster?)null);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((BackgroundService.Domain.Entities.Notification.MiscTypeMaster?)null);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 999 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(ValidEntity(1));

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .Returns(ValidDto(1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdAsyncOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(ValidEntity(5));

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .Returns(ValidDto(5));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(5), Times.Once);
        }
    }
}
