using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.GlAccountMaster.Queries.GetSelectableCompanies;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Queries
{
    public sealed class GetSelectableCompaniesQueryHandlerTests
    {
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSelectableCompaniesQueryHandler CreateSut() =>
            new(_mockCompanyLookup.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsOnlyUserAssignedCompanies()
        {
            _mockIp.Setup(s => s.GetUserId()).Returns(7);
            _mockCompanyLookup.Setup(c => c.GetUserCompaniesAsync(7)).ReturnsAsync(new List<CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "Spinning" },
                new() { CompanyId = 2, CompanyName = "Processing" },
            });

            var result = await CreateSut().Handle(new GetSelectableCompaniesQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data!.Select(o => o.CompanyName).Should().Contain(new[] { "Spinning", "Processing" });
            _mockCompanyLookup.Verify(c => c.GetUserCompaniesAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_NoAssignedCompanies_ReturnsEmpty()
        {
            _mockIp.Setup(s => s.GetUserId()).Returns(7);
            _mockCompanyLookup.Setup(c => c.GetUserCompaniesAsync(7)).ReturnsAsync(new List<CompanyLookupDto>());

            var result = await CreateSut().Handle(new GetSelectableCompaniesQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
