using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Logistics;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAdvice;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAdvice
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAdviceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DispatchAdviceQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAdviceQueryRepository CreateRepo(IPartyLookup? partyLookup = null)
        {
            IPartyLookup party;
            if (partyLookup != null)
            {
                party = partyLookup;
            }
            else
            {
                var partyMock = new Mock<IPartyLookup>(MockBehavior.Loose);
                partyMock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                            new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
                partyMock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((int id, CancellationToken _) =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id });
                party = partyMock.Object;
            }

            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());

            var hsn = new Mock<IHSNLookup>(MockBehavior.Loose);
            hsn.Setup(h => h.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<HSNLookupDto>)new List<HSNLookupDto>());

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUserId()).Returns(1);
            ip.Setup(x => x.GetUnitId()).Returns(1);

            var packType = new Mock<IPackTypeLookup>(MockBehavior.Loose);
            packType.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PackTypeLookupDto>)new List<PackTypeLookupDto>());

            var lot = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            lot.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());

            var freight = new Mock<IFreightMasterLookup>(MockBehavior.Loose);
            freight.Setup(f => f.GetAllFreightMasterAsync())
                .ReturnsAsync(new List<FreightMasterLookupDto>());

            var warehouse = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            warehouse.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<WarehouseLookupDto>)new List<WarehouseLookupDto>());

            var bin = new Mock<IBinLookup>(MockBehavior.Loose);
            bin.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BinLookupDto>)new List<BinLookupDto>());

            var uom = new Mock<IUOMLookup>(MockBehavior.Loose);
            uom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());

            var city = new Mock<ICityLookup>(MockBehavior.Loose);
            city.Setup(c => c.GetAllCityAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>());

            var state = new Mock<IStateLookup>(MockBehavior.Loose);
            state.Setup(s => s.GetAllStatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateLookupDto>());

            var country = new Mock<ICountryLookup>(MockBehavior.Loose);
            country.Setup(c => c.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryLookupDto>());

            return new DispatchAdviceQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party, item.Object, hsn.Object, ip.Object,
                packType.Object, lot.Object, freight.Object,
                warehouse.Object, bin.Object, uom.Object,
                city.Object, state.Object, country.Object);
        }

        private static IPartyLookup BuildPartyLookupWithAddresses(int partyId, params PartyAddressLookupDto[] addresses)
        {
            var dto = new PartyLookupDto
            {
                Id = partyId,
                PartyName = "Party " + partyId,
                Addresses = addresses.ToList()
            };
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        id == partyId ? dto : new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    id == partyId ? dto : new PartyLookupDto { Id = id, PartyName = "Party " + id });
            return mock.Object;
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<(int soId, int statusId, int dispatchTypeId)> EnsureSalesOrderAndMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DAQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DAQ_MT", Description = "DA Misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var statusId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_ST");
            var dispTypeId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_DT");
            var freightId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_FT");
            var enquiryId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_ENQ");

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "DAQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "DAQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "DAQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "DAQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "DAQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "DAQ_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "DAQ_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "DAQ_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = enquiryId,
                    UnitId = 1, PartyId = 100,
                    FreightTypeId = freightId,
                    FinalAmount = 5000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }
            return (so.Id, statusId, dispTypeId);
        }

        private async Task<int> SeedAsync(string dispatchNo = "DA_Q1", IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active)
        {
            var (soId, statusId, dispTypeId) = await EnsureSalesOrderAndMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var da = new SalesManagement.Domain.Entities.DispatchAdviceHeader
            {
                DispatchNo = dispatchNo,
                DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StatusId = statusId,
                SalesOrderId = soId,
                PartyId = 100,
                TotOrderQty = 100m,
                TotDispatchedQty = 50m,
                TotPendingQty = 50m,
                DispatchTypeId = dispTypeId,
                FreightId = 1,
                UnitId = 1,
                InvFlg = false,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.DispatchAdviceHeader.AddAsync(da);
            await ctx.SaveChangesAsync();
            return da.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("DA_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DA_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("UNIQ_DA_ZZ");
            await SeedAsync("OTHER_DA");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQ_DA_ZZ");

            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("DA_B1");

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
            await SeedAsync("DA_AC_MATCH");

            var result = await CreateRepo().AutocompleteAsync("DA_AC_MATCH", CancellationToken.None);

            result.Should().HaveCount(1);
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
            var id = await SeedAsync("DA_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesOrderExistsAsync_Should_Return_True()
        {
            var (soId, _, _) = await EnsureSalesOrderAndMiscAsync();

            var result = await CreateRepo().SalesOrderExistsAsync(soId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOrderExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().SalesOrderExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasPendingAmendmentAsync_Should_Return_False_When_None()
        {
            var (soId, _, _) = await EnsureSalesOrderAndMiscAsync();

            var result = await CreateRepo().HasPendingAmendmentAsync(soId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasInvoiceAsync_Should_Return_False_When_None()
        {
            await ClearAsync();
            var id = await SeedAsync("DA_INV");

            var result = await CreateRepo().HasInvoiceAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetSalesOrderUnitIdAsync_Should_Return_OrderUnitId()
        {
            var (soId, _, _) = await EnsureSalesOrderAndMiscAsync();

            var result = await CreateRepo().GetSalesOrderUnitIdAsync(soId);

            // OrderUnitId is nullable and not seeded in test fixture — returns 0 via ExecuteScalar<int>
            result.Should().Be(0);
        }

        // -----------------------------------------------------------------------------------------
        // DispatchAddress array shape — Direct-To-Party / Others / Unknown
        // -----------------------------------------------------------------------------------------

        private async Task<int> EnsureMiscWithDescriptionAsync(string code, string description)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DAQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DAQ_MT",
                    Description = "DA Misc",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == mt.Id && x.Code == code);
            if (existing != null)
            {
                if (existing.Description != description)
                {
                    existing.Description = description;
                    await ctx.SaveChangesAsync();
                }
                return existing.Id;
            }
            var m = new SalesManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = code,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedDispatchAddressMasterAsync(string name, string addr1, string addr2,
            int cityId, int stateId, int countryId, string pin)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new SalesManagement.Domain.Entities.DispatchAddressMaster
            {
                DispatchAddressName = name,
                AddressLine1 = addr1,
                AddressLine2 = addr2,
                CityId = cityId,
                StateId = stateId,
                CountryId = countryId,
                PinCode = pin,
                FreightId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.DispatchAddressMaster.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task<(int dispatchId, int partyId)> SeedAdviceWithDispatchTypeAsync(
            string dispatchNo,
            string dispatchTypeCode,
            string dispatchTypeDescription,
            int? dispatchAddressId = null,
            int partyId = 200)
        {
            var (soId, statusId, _) = await EnsureSalesOrderAndMiscAsync();
            var dispTypeId = await EnsureMiscWithDescriptionAsync(dispatchTypeCode, dispatchTypeDescription);

            await using var ctx = _fixture.CreateFreshDbContext();
            var da = new SalesManagement.Domain.Entities.DispatchAdviceHeader
            {
                DispatchNo = dispatchNo,
                DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StatusId = statusId,
                SalesOrderId = soId,
                PartyId = partyId,
                TotOrderQty = 100m,
                TotDispatchedQty = 50m,
                TotPendingQty = 50m,
                DispatchTypeId = dispTypeId,
                DispatchAddressId = dispatchAddressId,
                FreightId = 1,
                UnitId = 1,
                InvFlg = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.DispatchAdviceHeader.AddAsync(da);
            await ctx.SaveChangesAsync();
            return (da.Id, partyId);
        }

        [Fact]
        public async Task GetAllAsync_DirectToParty_PopulatesDispatchAddress_FromPartyShipping()
        {
            await ClearAsync();
            var (_, partyId) = await SeedAdviceWithDispatchTypeAsync("DA_D2P_1", "DAQ_D2P", "Direct-To-Party");

            var partyLookup = BuildPartyLookupWithAddresses(partyId, new PartyAddressLookupDto
            {
                Id = 901, PartyId = partyId, AddressType = "Shipping",
                AddressLine1 = "NO.32/50", AddressLine2 = "AVINASHI ROAD",
                CityId = 138, City = "Tirupur",
                StateId = 97, State = "Tamil Nadu",
                CountryId = 1145, Country = "India",
                PostalCode = "641602"
            });

            var (rows, _) = await CreateRepo(partyLookup).GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].DispatchAddress.Should().HaveCount(1);
            var addr = rows[0].DispatchAddress[0];
            addr.Source.Should().Be("Party");
            addr.Id.Should().Be(901);
            addr.DispatchAddressId.Should().BeNull();
            addr.DispatchAddressName.Should().BeNull();
            addr.AddressLine1.Should().Be("NO.32/50");
            addr.AddressLine2.Should().Be("AVINASHI ROAD");
            addr.CityId.Should().Be(138);
            addr.CityName.Should().Be("Tirupur");
            addr.StateId.Should().Be(97);
            addr.StateName.Should().Be("Tamil Nadu");
            addr.CountryId.Should().Be(1145);
            addr.CountryName.Should().Be("India");
            addr.PinCode.Should().Be("641602");
        }

        [Fact]
        public async Task GetAllAsync_DirectToParty_MultipleShippingRows_ReturnsAllAsArray()
        {
            await ClearAsync();
            var (_, partyId) = await SeedAdviceWithDispatchTypeAsync("DA_D2P_2", "DAQ_D2P", "Direct-To-Party");

            var partyLookup = BuildPartyLookupWithAddresses(partyId,
                new PartyAddressLookupDto
                {
                    Id = 1, PartyId = partyId, AddressType = "Shipping",
                    AddressLine1 = "Addr 1", CityId = 1, City = "C1",
                    StateId = 1, State = "S1", CountryId = 1, Country = "Co1", PostalCode = "100001"
                },
                new PartyAddressLookupDto
                {
                    Id = 2, PartyId = partyId, AddressType = "Shipping",
                    AddressLine1 = "Addr 2", CityId = 2, City = "C2",
                    StateId = 2, State = "S2", CountryId = 2, Country = "Co2", PostalCode = "100002"
                },
                new PartyAddressLookupDto
                {
                    Id = 3, PartyId = partyId, AddressType = "Billing",  // ← excluded
                    AddressLine1 = "Bill", CityId = 9, City = "B", PostalCode = "999999"
                });

            var (rows, _) = await CreateRepo(partyLookup).GetAllAsync(1, 10, null);

            rows[0].DispatchAddress.Should().HaveCount(2);
            rows[0].DispatchAddress.Select(a => a.Id).Should().BeEquivalentTo(new int?[] { 1, 2 });
            rows[0].DispatchAddress.Should().AllSatisfy(a => a.Source.Should().Be("Party"));
        }

        [Fact]
        public async Task GetAllAsync_Others_PopulatesDispatchAddress_FromDispatchAddressMaster()
        {
            await ClearAsync();
            var masterId = await SeedDispatchAddressMasterAsync("MAINPLANT", "Coimbatore", "Saravanampatti",
                cityId: 1210, stateId: 2129, countryId: 2190, pin: "630207");
            await SeedAdviceWithDispatchTypeAsync("DA_OTH_1", "DAQ_OTH", "Others", dispatchAddressId: masterId);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].DispatchAddress.Should().HaveCount(1);
            var addr = rows[0].DispatchAddress[0];
            addr.Source.Should().Be("Master");
            addr.Id.Should().Be(masterId);
            addr.DispatchAddressId.Should().Be(masterId);
            addr.DispatchAddressName.Should().Be("MAINPLANT");
            addr.AddressLine1.Should().Be("Coimbatore");
            addr.AddressLine2.Should().Be("Saravanampatti");
            addr.CityId.Should().Be(1210);
            addr.StateId.Should().Be(2129);
            addr.CountryId.Should().Be(2190);
            addr.PinCode.Should().Be("630207");
            // City/state/country names are null because the lookup mocks return empty lists — that is expected.
        }

        [Fact]
        public async Task GetAllAsync_UnknownDispatchType_ReturnsEmptyArray()
        {
            await ClearAsync();
            await SeedAdviceWithDispatchTypeAsync("DA_UNK_1", "DAQ_UNK", "SomeRandomType");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].DispatchAddress.Should().NotBeNull();
            rows[0].DispatchAddress.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_DirectToParty_PopulatesDispatchAddress_FromPartyShipping()
        {
            await ClearAsync();
            var (id, partyId) = await SeedAdviceWithDispatchTypeAsync("DA_D2P_3", "DAQ_D2P", "Direct-To-Party");

            var partyLookup = BuildPartyLookupWithAddresses(partyId, new PartyAddressLookupDto
            {
                Id = 555, PartyId = partyId, AddressType = "Shipping",
                AddressLine1 = "Line1", AddressLine2 = "Line2",
                CityId = 10, City = "X", StateId = 11, State = "Y",
                CountryId = 12, Country = "Z", PostalCode = "123456"
            });

            var result = await CreateRepo(partyLookup).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DispatchAddress.Should().HaveCount(1);
            result.DispatchAddress[0].Source.Should().Be("Party");
            result.DispatchAddress[0].Id.Should().Be(555);
            result.DispatchAddress[0].PinCode.Should().Be("123456");
        }

        [Fact]
        public async Task GetByIdAsync_Others_PopulatesDispatchAddress_FromDispatchAddressMaster()
        {
            await ClearAsync();
            var masterId = await SeedDispatchAddressMasterAsync("WHSE", "AddrA", "AddrB",
                cityId: 5, stateId: 6, countryId: 7, pin: "555555");
            var (id, _) = await SeedAdviceWithDispatchTypeAsync("DA_OTH_2", "DAQ_OTH", "Others", dispatchAddressId: masterId);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DispatchAddress.Should().HaveCount(1);
            var addr = result.DispatchAddress[0];
            addr.Source.Should().Be("Master");
            addr.DispatchAddressId.Should().Be(masterId);
            addr.DispatchAddressName.Should().Be("WHSE");
            addr.AddressLine1.Should().Be("AddrA");
            addr.AddressLine2.Should().Be("AddrB");
            addr.PinCode.Should().Be("555555");
        }

        [Fact]
        public async Task GetByIdAsync_UnknownDispatchType_ReturnsEmptyArray()
        {
            await ClearAsync();
            var (id, _) = await SeedAdviceWithDispatchTypeAsync("DA_UNK_2", "DAQ_UNK", "SomeRandomType");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DispatchAddress.Should().NotBeNull();
            result.DispatchAddress.Should().BeEmpty();
        }
    }
}
