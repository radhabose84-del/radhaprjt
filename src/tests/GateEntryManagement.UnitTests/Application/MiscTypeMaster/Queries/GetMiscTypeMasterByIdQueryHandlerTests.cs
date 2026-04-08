using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Dto;
using GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsDto()
        {
            var dto = new MiscTypeMasterDto { Id = 1, MiscTypeCode = "MT001", Description = "Test MiscType" };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.MiscTypeCode.Should().Be("MT001");
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MiscTypeMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WhenFound()
        {
            var dto = new MiscTypeMasterDto { Id = 1, MiscTypeCode = "MT001", Description = "Test" };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "GetMiscTypeMasterByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotPublishAuditEvent_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MiscTypeMasterDto?)null);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
