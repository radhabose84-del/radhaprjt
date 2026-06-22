using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesProjection;
using SalesManagement.Application.Reports.SalesProjection.Dto;
using SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection;

namespace SalesManagement.UnitTests.Application.SalesProjection;

public class GetSalesProjectionQueryHandlerTests
{
    private readonly Mock<ISalesProjectionRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesProjectionQueryHandler CreateSut()
    {
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesProjectionQueryHandler(_mockRepo.Object, _mockMediator.Object);
    }

    private void SetupRepo(SalesProjectionDto? dto = null)
    {
        _mockRepo
            .Setup(r => r.GetProjectionAsync(
                It.IsAny<ProjectionPeriodType>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto ?? new SalesProjectionDto
            {
                PeriodType = "Monthly",
                Periods = new List<SalesProjectionPeriodDto>(),
                Summary = new SalesProjectionSummaryDto()
            });
    }

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        SetupRepo();
        var result = await CreateSut().Handle(new GetSalesProjectionQuery { PeriodType = ProjectionPeriodType.Monthly }, CancellationToken.None);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsRepositoryData()
    {
        var dto = new SalesProjectionDto { PeriodType = "Yearly", Periods = new List<SalesProjectionPeriodDto>(), Summary = new SalesProjectionSummaryDto() };
        SetupRepo(dto);
        var result = await CreateSut().Handle(new GetSalesProjectionQuery { PeriodType = ProjectionPeriodType.Yearly }, CancellationToken.None);
        result.Data.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Handle_CallsRepository_Once()
    {
        SetupRepo();
        await CreateSut().Handle(new GetSalesProjectionQuery(), CancellationToken.None);
        _mockRepo.Verify(r => r.GetProjectionAsync(
            It.IsAny<ProjectionPeriodType>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
