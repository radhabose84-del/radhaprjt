using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanForReceipt;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries;

public sealed class GetDeliveryChallanForReceiptQueryHandlerTests
{
    private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetDeliveryChallanForReceiptQueryHandler CreateSut()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetDeliveryChallanForReceiptQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMatchingChallans()
    {
        IReadOnlyList<DeliveryChallanLookupDto> data = new List<DeliveryChallanLookupDto>
        {
            new() { Id = 1, DeliveryNumber = "DC-001", DeliveryDate = new DateOnly(2026, 1, 1) },
            new() { Id = 2, DeliveryNumber = "DC-002", DeliveryDate = new DateOnly(2026, 1, 2) }
        };
        _mockQueryRepo
            .Setup(r => r.GetForReceiptAsync("DC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(new GetDeliveryChallanForReceiptQuery("DC"), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeSameAs(data);
    }

    [Fact]
    public async Task Handle_NullTerm_PassesEmptyStringToRepository()
    {
        IReadOnlyList<DeliveryChallanLookupDto> data = new List<DeliveryChallanLookupDto>();
        _mockQueryRepo
            .Setup(r => r.GetForReceiptAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        await CreateSut().Handle(new GetDeliveryChallanForReceiptQuery(null!), CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetForReceiptAsync(string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        IReadOnlyList<DeliveryChallanLookupDto> data = new List<DeliveryChallanLookupDto>();
        _mockQueryRepo
            .Setup(r => r.GetForReceiptAsync("none", It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(new GetDeliveryChallanForReceiptQuery("none"), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
