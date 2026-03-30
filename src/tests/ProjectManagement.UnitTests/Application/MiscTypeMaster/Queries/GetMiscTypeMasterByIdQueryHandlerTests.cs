using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            var dto = MiscTypeMasterBuilders.ValidDto();

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ProjectManagement.Domain.Entities.MiscTypeMaster)null!);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            var dto = MiscTypeMasterBuilders.ValidDto();

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);

            await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
