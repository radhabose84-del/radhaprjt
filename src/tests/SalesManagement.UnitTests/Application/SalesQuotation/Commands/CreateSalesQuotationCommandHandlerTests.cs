using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesQuotation.Commands;

public sealed class CreateSalesQuotationCommandHandlerTests
{
    private readonly Mock<ISalesQuotationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CreateSalesQuotationCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockMapper.Object, _mockMediator.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesQuotationHeader>(It.IsAny<object>()))
            .Returns(new SalesQuotationHeader { CustomerId = 1 });

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10, Description = "Pending" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesQuotationHeader>()))
            .ReturnsAsync(newId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(42);
        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath(1);
        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "SALESQUOTATION_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
    {
        SetupHappyPath(0);
        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>();
    }
}
