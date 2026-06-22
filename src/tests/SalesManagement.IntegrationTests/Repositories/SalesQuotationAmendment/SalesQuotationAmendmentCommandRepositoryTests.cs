using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesQuotationAmendment;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.SalesQuotationAmendment
{
    [Collection("DatabaseCollection")]
    public sealed class SalesQuotationAmendmentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesQuotationAmendmentCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesQuotationAmendmentCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose).Object);

        [Fact]
        public async Task GetByIdEntityAsync_Should_Return_Null_For_Unknown()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdEntityAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSalesQuotationEntityAsync_Should_Return_Null_For_Unknown()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetSalesQuotationEntityAsync(9999999);
            result.Should().BeNull();
        }
    }
}
