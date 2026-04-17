using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackingList;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DispatchAdvice.Queries;

public sealed class GetDispatchAdvicePackingListQueryHandlerTests
{
    private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetDispatchAdvicePackingListQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingDispatchAdvice_ReturnsPackingList()
    {
        var dto = new DispatchAdvicePackingListDto
        {
            DispatchAdviceId = 1,
            DispatchNo = "DA001",
            PartyName = "Test Party"
        };

        _mockQueryRepo
            .Setup(r => r.GetPackingListAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateSut().Handle(
            new GetDispatchAdvicePackingListQuery { DispatchAdviceId = 1 },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.DispatchNo.Should().Be("DA001");
    }

    [Fact]
    public async Task Handle_NonExistentDispatchAdvice_ReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetPackingListAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DispatchAdvicePackingListDto?)null);

        var result = await CreateSut().Handle(
            new GetDispatchAdvicePackingListQuery { DispatchAdviceId = 99 },
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        _mockQueryRepo
            .Setup(r => r.GetPackingListAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DispatchAdvicePackingListDto { DispatchAdviceId = 1 });

        await CreateSut().Handle(
            new GetDispatchAdvicePackingListQuery { DispatchAdviceId = 1 },
            CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetPackingListAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetPackingListAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DispatchAdvicePackingListDto { DispatchAdviceId = 1 });

        await CreateSut().Handle(
            new GetDispatchAdvicePackingListQuery { DispatchAdviceId = 1 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetDispatchAdvicePackingListQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
