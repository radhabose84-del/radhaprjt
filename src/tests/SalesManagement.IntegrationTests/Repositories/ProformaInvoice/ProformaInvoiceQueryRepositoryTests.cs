using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ProformaInvoice;
using SalesManagement.IntegrationTests.Common;
using System.Data.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ProformaInvoice
{
    [Collection("DatabaseCollection")]
    public sealed class ProformaInvoiceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ProformaInvoiceQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ProformaInvoiceQueryRepository CreateRepo(Mock<IPartyLookup>? party = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>
                    {
                        new() { Id = 100, PartyName = "Party 100" }
                    });
                party.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((int id, CancellationToken _) =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id });
            }
            var unitDetailLookup = new Mock<IUnitDetailLookup>(MockBehavior.Loose);
            var companyDetailLookup = new Mock<ICompanyDetailLookup>(MockBehavior.Loose);
            var partyDetailLookup = new Mock<IPartyDetailLookup>(MockBehavior.Loose);
            var partyBankLookup = new Mock<IPartyBankLookup>(MockBehavior.Loose);
            var itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
            var stateLookup = new Mock<IStateLookup>(MockBehavior.Loose);
            var cityLookup = new Mock<ICityLookup>(MockBehavior.Loose);

            return new ProformaInvoiceQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object,
                unitDetailLookup.Object,
                companyDetailLookup.Object,
                partyDetailLookup.Object,
                partyBankLookup.Object,
                itemLookup.Object,
                stateLookup.Object,
                cityLookup.Object);
        }

        private async Task<(int soId, int partyId, int statusMiscId)> EnsureSalesOrderAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "PIQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "PIQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "PIQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "PIQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "PIQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "PIQ_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            var statusType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ProformaInvStatus");
            if (statusType == null)
            {
                statusType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ProformaInvStatus", Description = "PI Status",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(statusType);
                await ctx.SaveChangesAsync();
            }
            var draft = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == statusType.Id && x.Code == "Draft");
            if (draft == null)
            {
                draft = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = statusType.Id, Code = "Draft", Description = "Draft",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(draft);
                await ctx.SaveChangesAsync();
            }

            // Freight/Enquiry shared MiscType
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PIQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PIQ_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var freight = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PIQ_FT");
            if (freight == null)
            {
                freight = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "PIQ_FT", Description = "Freight",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(freight);
                await ctx.SaveChangesAsync();
            }
            var enq = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PIQ_ENQ");
            if (enq == null)
            {
                enq = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "PIQ_ENQ", Description = "Unit",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(enq);
                await ctx.SaveChangesAsync();
            }

            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "PIQ_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "PIQ_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = enq.Id,
                    UnitId = 1,
                    PartyId = 100,
                    FreightTypeId = freight.Id,
                    FinalAmount = 1000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }
            return (so.Id, 100, draft.Id);
        }

        private async Task<int> SeedProformaAsync(string number = "PI_Q1", decimal amount = 500m, decimal paid = 0m, IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active, int? statusId = null)
        {
            var (soId, partyId, defaultStatusId) = await EnsureSalesOrderAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var p = new SalesManagement.Domain.Entities.ProformaInvoice
            {
                ProformaNumber = number,
                ProformaDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                SalesOrderId = soId,
                PartyId = partyId,
                ProformaAmount = amount,
                SOBalance = amount,
                PaymentReceivedAmount = paid,
                StatusId = statusId ?? defaultStatusId,
                Remarks = "q test",
                IsActive = active, IsDeleted = deleted
            };
            await ctx.ProformaInvoice.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CustomerName_From_Lookup()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_A2");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows[0].CustomerName.Should().Be("Party 100");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_A3", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedProformaAsync("UNIQ_FILT_A");
            await SeedProformaAsync("OTHER_B");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQ_FILT");

            rows.Should().HaveCount(1);
            rows[0].ProformaNumber.Should().Be("UNIQ_FILT_A");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedProformaAsync("PI_B1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_AC_MATCH");

            var result = await CreateRepo().AutocompleteAsync("PI_AC_MATCH", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetBySalesOrderIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_SO_A");
            await SeedProformaAsync("PI_SO_B");
            var (soId, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().GetBySalesOrderIdAsync(soId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var id = await SeedProformaAsync("PI_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_True_For_ProformaInvStatus_Type()
        {
            var (_, _, statusId) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().StatusExistsAsync(statusId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().StatusExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetProformaAmountAsync_Should_Return_Amount()
        {
            await ClearAsync();
            var id = await SeedProformaAsync("PI_AMT", amount: 750m);

            var result = await CreateRepo().GetProformaAmountAsync(id);

            result.Should().Be(750m);
        }

        [Fact]
        public async Task GetProformaAmountAsync_Should_Return_Zero_When_NotFound()
        {
            var result = await CreateRepo().GetProformaAmountAsync(9999999);
            result.Should().Be(0);
        }

        [Fact]
        public async Task IsDraftStatusAsync_Should_Return_True_When_Status_Is_Draft()
        {
            await ClearAsync();
            var id = await SeedProformaAsync("PI_DRAFT");

            var result = await CreateRepo().IsDraftStatusAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasReceivedAdvancePaymentAsync_Should_Return_True_When_PaymentGt0()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_ADV", paid: 100m);
            var (soId, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().HasReceivedAdvancePaymentAsync(soId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasReceivedAdvancePaymentAsync_Should_Return_False_When_Payment_Zero()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_NOPAY", paid: 0m);
            var (soId, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().HasReceivedAdvancePaymentAsync(soId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetSalesOrderBalanceAsync_Should_Return_FinalAmount_Minus_Proformas()
        {
            await ClearAsync();
            await SeedProformaAsync("PI_BAL1", amount: 300m);
            var (soId, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().GetSalesOrderBalanceAsync(soId);

            result.Should().Be(1000m - 300m);
        }
    }
}
