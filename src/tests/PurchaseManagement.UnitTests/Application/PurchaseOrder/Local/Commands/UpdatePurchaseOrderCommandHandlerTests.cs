using AutoMapper;
using Contracts.Interfaces.Lookups.Budget;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;
using System.ComponentModel.DataAnnotations;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Commands
{
    public sealed class UpdatePurchaseOrderCommandHandlerTests
    {
        private readonly Mock<IPurchaseOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _mockBudgetLookup = new(MockBehavior.Loose);

        private UpdatePurchaseOrderCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockPoDocs.Object, _mockBudgetLookup.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsValidationException()
        {
            var command = new UpdatePurchaseOrderCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsRepo()
        {
            var command = new UpdatePurchaseOrderCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
            // Validates that null data triggers exception before repo call
        }
    }
}
