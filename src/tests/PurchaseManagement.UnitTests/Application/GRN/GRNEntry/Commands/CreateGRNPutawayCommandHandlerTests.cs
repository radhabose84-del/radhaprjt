using System.Data;
using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Commands
{
    public sealed class CreateGRNPutawayCommandHandlerTests
    {
        private readonly Mock<IGRNEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IGRNEntryQueryRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IStockLedgerLookup> _mockStockLedger = new(MockBehavior.Loose);
        private readonly Mock<IDbConnection> _mockDbConnection = new(MockBehavior.Loose);

        private CreateGRNPutawayCommandHandler CreateSut() =>
            new(
                _mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQryRepo.Object, _mockIp.Object, _mockStockLedger.Object,
                _mockDbConnection.Object);

        [Fact]
        public async Task Handle_EmptyPutawayList_ThrowsArgumentException()
        {
            var command = new CreateGRNPutawayCommand
            {
                PutawayList = new List<CreateGRNPutawayDto>()
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Putaway list cannot be empty*");
        }

        [Fact]
        public async Task Handle_NullPutawayList_ThrowsArgumentException()
        {
            var command = new CreateGRNPutawayCommand { PutawayList = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Putaway list cannot be empty*");
        }
    }
}
