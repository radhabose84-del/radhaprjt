using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetExMillRateByPaymentTerm;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Queries;

public sealed class GetExMillRateByPaymentTermQueryHandlerTests
{
    private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetExMillRateByPaymentTermQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var data = new List<ExMillRateDto> { new() { Id = 1, ExMillRate = 100m } };

        _mockQueryRepo
            .Setup(r => r.GetExMillRateByPaymentTermAsync(1, 1, null))
            .ReturnsAsync(data);

        _mockMapper
            .Setup(m => m.Map<List<ExMillRateDto>>(It.IsAny<List<ExMillRateDto>>()))
            .Returns(data);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetExMillRateByPaymentTermQuery { PaymentTermId = 1, ItemId = 1 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetExMillRateByPaymentTermAsync(1, 1, null))
            .ReturnsAsync(new List<ExMillRateDto>());

        _mockMapper
            .Setup(m => m.Map<List<ExMillRateDto>>(It.IsAny<List<ExMillRateDto>>()))
            .Returns(new List<ExMillRateDto>());

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetExMillRateByPaymentTermQuery { PaymentTermId = 1, ItemId = 1 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
