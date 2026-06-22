using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementById;

namespace SalesManagement.UnitTests.Application.SalesAgreement.Queries;

public class GetSalesAgreementByIdQueryHandlerTests
{
    private readonly Mock<ISalesAgreementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesAgreementByIdQueryHandler CreateSut()
    {
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesAgreementByIdQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new SalesAgreementHeaderDto { Id = 1, AgreementNo = "SA001" };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetSalesAgreementByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.AgreementNo.Should().Be("SA001");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesAgreementHeaderDto?)null);

        var result = await CreateSut().Handle(new GetSalesAgreementByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }
}
