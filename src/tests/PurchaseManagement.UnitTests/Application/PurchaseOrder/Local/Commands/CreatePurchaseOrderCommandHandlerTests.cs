using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Commands
{
    public sealed class CreatePurchaseOrderCommandHandlerTests
    {
        private readonly Mock<IPurchaseOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreatePurchaseOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _mockBudgetLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);
        private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMisc = new(MockBehavior.Loose);

        private CreatePurchaseOrderCommandHandler CreateSut() =>
            new(
                _mockRepo.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object,
                _mockLogger.Object, _mockOutbox.Object, _mockPoDocs.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object, _mockBudgetLookup.Object,
                _mockFyLookup.Object, _mockAppDataMisc.Object);

        [Fact]
        public async Task Handle_NullData_ReturnsFailure()
        {
            var command = new CreatePurchaseOrderCommand { Data = null! };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsApiResponseDTO()
        {
            var command = new CreatePurchaseOrderCommand { Data = null! };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeOfType<ApiResponseDTO<int>>();
        }
    }
}
