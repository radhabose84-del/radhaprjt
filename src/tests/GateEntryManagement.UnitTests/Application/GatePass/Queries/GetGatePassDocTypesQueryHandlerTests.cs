using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassDocTypes;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GatePass.Queries
{
    public sealed class GetGatePassDocTypesQueryHandlerTests
    {
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGatePassDocTypesQueryHandler CreateSut() =>
            new(_mockDocSeq.Object, _mockFyLookup.Object, _mockIpService.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_NoFinancialYear_ReturnsEmpty()
        {
            _mockFyLookup.Setup(r => r.GetAllFinancialYearAsync()).ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.FinancialYearLookupDto>());
            _mockIpService.Setup(i => i.GetUnitId()).Returns(1);

            var result = await CreateSut().Handle(new GetGatePassDocTypesQuery(), CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
