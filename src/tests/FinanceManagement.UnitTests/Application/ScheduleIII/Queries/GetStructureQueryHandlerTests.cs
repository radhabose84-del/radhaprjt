using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetStructure;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class GetStructureQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStructureQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingStructure_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetStructureAsync(1001, 7))
                .ReturnsAsync(new ScheduleIIIStructureDto { Id = 1, CompanyId = 1001, DivisionId = 7, VersionNo = 5 });

            var result = await CreateSut().Handle(
                new GetStructureQuery { CompanyId = 1001, DivisionId = 7 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.VersionNo.Should().Be(5);
        }

        [Fact]
        public async Task Handle_MissingStructure_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetStructureAsync(1001, 9)).ReturnsAsync((ScheduleIIIStructureDto?)null);

            var result = await CreateSut().Handle(
                new GetStructureQuery { CompanyId = 1001, DivisionId = 9 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_MissingStructure_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetStructureAsync(1001, 9)).ReturnsAsync((ScheduleIIIStructureDto?)null);

            await CreateSut().Handle(new GetStructureQuery { CompanyId = 1001, DivisionId = 9 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
