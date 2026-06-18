using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetStructure;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class GetStructureQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public GetStructureQueryHandlerTests()
        {
            // CompanyId + DivisionId now resolved from the token.
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1001);
            _mockIp.Setup(x => x.GetDivisionId()).Returns(7);
        }

        private GetStructureQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingStructure_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetStructureAsync(1001, 7))
                .ReturnsAsync(new ScheduleIIIHeaderDto { Id = 1, CompanyId = 1001, DivisionId = 7, StatusId = 22 });

            var result = await CreateSut().Handle(new GetStructureQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.StatusId.Should().Be(22);
        }

        [Fact]
        public async Task Handle_MissingStructure_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetStructureAsync(1001, 7)).ReturnsAsync((ScheduleIIIHeaderDto?)null);

            var result = await CreateSut().Handle(new GetStructureQuery(), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_MissingStructure_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetStructureAsync(1001, 7)).ReturnsAsync((ScheduleIIIHeaderDto?)null);

            await CreateSut().Handle(new GetStructureQuery(), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
