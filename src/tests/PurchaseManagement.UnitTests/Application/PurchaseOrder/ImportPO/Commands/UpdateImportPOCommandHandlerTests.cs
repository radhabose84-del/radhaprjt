using AutoMapper;
using Contracts.Interfaces.Lookups.Budget;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Update;
using System.ComponentModel.DataAnnotations;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Commands
{
    public sealed class UpdateImportPOCommandHandlerTests
    {
        private readonly Mock<IImportPOCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _mockBudgetLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateImportPOCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateImportPOCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockPoDocs.Object,
                _mockBudgetLookup.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsValidationException()
        {
            var command = new UpdateImportPOCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Body required*");
        }

        [Fact]
        public void Constructor_NullBudgetLookup_ThrowsArgumentNullException()
        {
            var act = () => new UpdateImportPOCommandHandler(
                _mockRepo.Object, _mockMapper.Object, _mockPoDocs.Object,
                null!, _mockLogger.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("budgetAllocationLookup");
        }
    }
}
