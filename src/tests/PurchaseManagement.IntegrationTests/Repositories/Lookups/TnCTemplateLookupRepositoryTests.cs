using Microsoft.Data.SqlClient;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class TnCTemplateLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TnCTemplateLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private TnCTemplateLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedTemplateAsync(
            string code,
            int transactionTypeId,
            string termsHtml,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted,
            Status childActive = Status.Active,
            IsDelete childDeleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = new TnCTemplateMaster
            {
                TemplateCode = code,
                TemplateName = code,
                ModuleId = 1,
                TermsHtml = termsHtml,
                IsActive = active,
                IsDeleted = deleted,
                Applicabilities = new List<TnCTemplateApplicability>
                {
                    new() { TransactionTypeId = transactionTypeId, IsActive = childActive, IsDeleted = childDeleted }
                }
            };
            await ctx.TnCTemplateMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByTransactionTypeAsync_Should_Return_TermsHtml_When_Matched()
        {
            await ClearAsync();
            await SeedTemplateAsync("TNC_LK1", transactionTypeId: 500, termsHtml: "<p>Terms A</p>");

            var result = await CreateRepo().GetByTransactionTypeAsync(500);

            result.Should().NotBeNull();
            result!.TermsHtml.Should().Be("<p>Terms A</p>");
            result.TemplateCode.Should().Be("TNC_LK1");
        }

        [Fact]
        public async Task GetByTransactionTypeAsync_Should_Return_Null_When_No_Match()
        {
            await ClearAsync();
            await SeedTemplateAsync("TNC_LK2", transactionTypeId: 501, termsHtml: "<p>Terms B</p>");

            var result = await CreateRepo().GetByTransactionTypeAsync(999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByTransactionTypeAsync_Should_Ignore_SoftDeleted_Template()
        {
            await ClearAsync();
            await SeedTemplateAsync("TNC_LK3", transactionTypeId: 502, termsHtml: "<p>Deleted</p>", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByTransactionTypeAsync(502);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByTransactionTypeAsync_Should_Ignore_Inactive_Template()
        {
            await ClearAsync();
            await SeedTemplateAsync("TNC_LK4", transactionTypeId: 503, termsHtml: "<p>Inactive</p>", active: Status.Inactive);

            var result = await CreateRepo().GetByTransactionTypeAsync(503);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByTransactionTypeAsync_Should_Ignore_Inactive_Applicability()
        {
            await ClearAsync();
            await SeedTemplateAsync("TNC_LK5", transactionTypeId: 504, termsHtml: "<p>X</p>", childActive: Status.Inactive);

            var result = await CreateRepo().GetByTransactionTypeAsync(504);

            result.Should().BeNull();
        }
    }
}
