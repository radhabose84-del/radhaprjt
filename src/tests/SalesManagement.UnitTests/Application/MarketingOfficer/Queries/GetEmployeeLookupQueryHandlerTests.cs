using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Application.MarketingOfficer.Queries.GetEmployeeLookup;

namespace SalesManagement.UnitTests.Application.MarketingOfficer.Queries;

public sealed class GetEmployeeLookupQueryHandlerTests
{
    private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private GetEmployeeLookupQueryHandler CreateSut() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task Handle_ValidOldUnitId_ReturnsList()
    {
        var expected = new List<EmployeeLookupDto>
        {
            new() { Empcode = "E001", Empname = "John" }
        };

        _mockQueryRepo
            .Setup(r => r.GetEmployeeLookupAsync("UNIT1", "E001"))
            .ReturnsAsync(expected);

        var query = new GetEmployeeLookupQuery { OldUnitId = "UNIT1", EmpNo = "E001" };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Empcode.Should().Be("E001");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyOldUnitId_ThrowsValidationException(string? oldUnitId)
    {
        var query = new GetEmployeeLookupQuery { OldUnitId = oldUnitId };

        Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*OldUnitId*");
    }
}
