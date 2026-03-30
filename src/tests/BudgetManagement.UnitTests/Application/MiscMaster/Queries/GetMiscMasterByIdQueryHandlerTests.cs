using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = MiscMasterBuilders.ValidEntity();
            var dto = MiscMasterBuilders.ValidDto();

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectFields()
        {
            var entity = MiscMasterBuilders.ValidEntity(42);
            var dto = MiscMasterBuilders.ValidDto(42, 1, "MSC042", "Found Master");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 42 }, CancellationToken.None);

            result.Code.Should().Be("MSC042");
            result.Description.Should().Be("Found Master");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = MiscMasterBuilders.ValidEntity();
            var dto = MiscMasterBuilders.ValidDto();

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);

            await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<BudgetManagement.Domain.Events.AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
