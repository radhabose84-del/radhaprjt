using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetComplaintAutoComplete;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetComplaintAutoCompleteQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetComplaintAutoCompleteQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsResults()
    {
        var lookups = new List<ComplaintLookupDto>
        {
            new ComplaintLookupDto { Id = 1, ComplaintNumber = "CMP001" }
        } as IReadOnlyList<ComplaintLookupDto>;

        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync("CMP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookups);

        var result = await CreateSut().Handle(
            new GetComplaintAutoCompleteQuery("CMP"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyTerm_PassesEmptyString()
    {
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ComplaintLookupDto>() as IReadOnlyList<ComplaintLookupDto>);

        var result = await CreateSut().Handle(
            new GetComplaintAutoCompleteQuery(null!), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
