using AutoMapper;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMasterById;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static BackgroundService.Domain.Entities.Notification.MiscMaster ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeId = 1,
                Code = "TEST001",
                Description = "Test Description"
            };

        private static GetMiscMasterDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeId = 1,
                Code = "TEST001",
                Description = "Test Description"
            };

        [Fact]
        public async Task Handle_ExistingId_ReturnsMappedDto()
        {
            var entity = ValidEntity(1);
            var dto = ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("TEST001");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(ValidEntity(1));

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()))
                .Returns(ValidDto(1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetById"),
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
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()))
                .Returns(ValidDto(5));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(5), Times.Once);
        }
    }
}
