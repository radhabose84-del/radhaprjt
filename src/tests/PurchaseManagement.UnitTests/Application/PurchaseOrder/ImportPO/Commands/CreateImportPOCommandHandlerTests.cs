using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Commands
{
    public sealed class CreateImportPOCommandHandlerTests
    {
        private readonly Mock<IImportPOCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateImportPOCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseOrderCommandRepository> _mockPoRepo = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);
        private readonly Mock<IImportPOQueryRepository> _mockImportQuery = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _mockBudgetLookup = new(MockBehavior.Loose);

        private CreateImportPOCommandHandler CreateSut() =>
            new(
                _mockRepo.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object,
                _mockLogger.Object, _mockMisc.Object, _mockPoRepo.Object,
                _mockPoDocs.Object, _mockImportQuery.Object, _mockUnitLookup.Object,
                _mockBudgetLookup.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsNullReferenceException()
        {
            var command = new CreateImportPOCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public void Constructor_NullUnitLookup_ThrowsArgumentNullException()
        {
            var act = () => new CreateImportPOCommandHandler(
                _mockRepo.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object,
                _mockLogger.Object, _mockMisc.Object, _mockPoRepo.Object,
                _mockPoDocs.Object, _mockImportQuery.Object, null!, _mockBudgetLookup.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("unitLookup");
        }

        [Fact]
        public void Constructor_NullBudgetLookup_ThrowsArgumentNullException()
        {
            var act = () => new CreateImportPOCommandHandler(
                _mockRepo.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object,
                _mockLogger.Object, _mockMisc.Object, _mockPoRepo.Object,
                _mockPoDocs.Object, _mockImportQuery.Object, _mockUnitLookup.Object, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("budgetAllocationLookup");
        }
    }
}
