using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Application.ComplaintResolution.Queries.GetResolutionFormData;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintResolution.Queries;

public sealed class GetResolutionFormDataQueryHandlerTests
{
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetResolutionFormDataQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingComplaint_ReturnsSuccess()
    {
        var formData = new ComplaintResolutionFormDataDto
        {
            ComplaintHeaderId = 1,
            ComplaintNumber = "CMP001",
            CustomerName = "Test Customer"
        };

        _mockQueryRepo
            .Setup(r => r.GetFormDataByComplaintIdAsync(1))
            .ReturnsAsync(formData);

        var result = await CreateSut().Handle(
            new GetResolutionFormDataQuery { ComplaintHeaderId = 1 },
            CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ComplaintNumber.Should().Be("CMP001");
    }

    [Fact]
    public async Task Handle_NonExistentComplaint_ReturnsFailure()
    {
        _mockQueryRepo
            .Setup(r => r.GetFormDataByComplaintIdAsync(99))
            .ReturnsAsync((ComplaintResolutionFormDataDto?)null);

        var result = await CreateSut().Handle(
            new GetResolutionFormDataQuery { ComplaintHeaderId = 99 },
            CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingComplaint_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetFormDataByComplaintIdAsync(1))
            .ReturnsAsync(new ComplaintResolutionFormDataDto { ComplaintHeaderId = 1 });

        await CreateSut().Handle(
            new GetResolutionFormDataQuery { ComplaintHeaderId = 1 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "GetResolutionFormDataQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentComplaint_DoesNotPublishAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetFormDataByComplaintIdAsync(99))
            .ReturnsAsync((ComplaintResolutionFormDataDto?)null);

        await CreateSut().Handle(
            new GetResolutionFormDataQuery { ComplaintHeaderId = 99 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        _mockQueryRepo
            .Setup(r => r.GetFormDataByComplaintIdAsync(1))
            .ReturnsAsync(new ComplaintResolutionFormDataDto { ComplaintHeaderId = 1 });

        await CreateSut().Handle(
            new GetResolutionFormDataQuery { ComplaintHeaderId = 1 },
            CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetFormDataByComplaintIdAsync(1),
            Times.Once);
    }
}
