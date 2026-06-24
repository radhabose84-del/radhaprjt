using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.PartyMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.PartyMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterCommandRepository CreateRepo(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        // Seeds the ApprovalStatus MiscType and a "Pending" MiscMaster, plus a generic registration type misc.
        // Returns the registration type id (used as RegistrationTypeId FK on PartyMaster).
        private async Task<int> EnsureMiscSeedAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var approvalType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "ApprovalStatus");
            if (approvalType == null)
            {
                approvalType = new PartyManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus",
                    Description = "Approval Status",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(approvalType);
                await ctx.SaveChangesAsync();
            }

            var pending = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "Pending" && m.MiscTypeId == approvalType.Id);
            if (pending == null)
            {
                pending = new PartyManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = approvalType.Id,
                    Code = "Pending",
                    Description = "Pending",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(pending);
                await ctx.SaveChangesAsync();
            }

            var regType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "RegistrationType");
            if (regType == null)
            {
                regType = new PartyManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "RegistrationType",
                    Description = "Registration Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(regType);
                await ctx.SaveChangesAsync();
            }

            var registered = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "Registered" && m.MiscTypeId == regType.Id);
            if (registered == null)
            {
                registered = new PartyManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = regType.Id,
                    Code = "Registered",
                    Description = "Registered",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(registered);
                await ctx.SaveChangesAsync();
            }

            return registered.Id;
        }

        private async Task<PartyManagement.Domain.Entities.PartyMaster> BuildPartyAsync(string code, string name)
        {
            var regId = await EnsureMiscSeedAsync();
            return new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = code,
                PartyName = name,
                RegistrationTypeId = regId,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };
        }

        private async Task ClearPartyAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PCMD1", "Cmd Party 1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_BrokerConfig_Child()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var party = await BuildPartyAsync("PBRK1", "Broker Party");
            party.BrokerConfigs = new List<PartyManagement.Domain.Entities.BrokerConfig>
            {
                new()
                {
                    SettlementCycleId = null,
                    TdsApplicable = 0,
                    BrokerPayableControlGl = "GL99",
                    TargetAmount = 5000m,
                    Status = 1
                }
            };

            var id = await CreateRepo(ctx).CreateAsync(party);

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var saved = await verifyCtx.BrokerConfig.Where(b => b.PartyId == id).ToListAsync();
            saved.Should().HaveCount(1);
            saved[0].BrokerPayableControlGl.Should().Be("GL99");
            saved[0].TargetAmount.Should().Be(5000m);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Address_Location_And_Station()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var party = await BuildPartyAsync("PADR1", "Addr LocStn");
            party.PartyAddressTypes = new List<PartyManagement.Domain.Entities.PartyAddress>
            {
                new()
                {
                    AddressType = "Primary",
                    AddressLine1 = "Mill Road",
                    CityId = 1, StateId = 1, CountryId = 1,
                    PostalCode = "641012",
                    LocationId = 45, StationId = 12
                }
            };

            var id = await CreateRepo(ctx).CreateAsync(party);

            await using var verify = _fixture.CreateFreshDbContext();
            var addr = await verify.PartyAddress.FirstOrDefaultAsync(a => a.PartyId == id);
            addr.Should().NotBeNull();
            addr!.LocationId.Should().Be(45);
            addr.StationId.Should().Be(12);
        }

        [Fact]
        public async Task UpdateAsync_Should_Remove_AgentConfig_When_EmptyListSent()
        {
            await ClearPartyAsync();

            // Create a party carrying one agent config.
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var party = await BuildPartyAsync("PACR1", "Agent Cfg Remove");
                party.AgentConfigs = new List<PartyManagement.Domain.Entities.AgentConfig>
                {
                    new() { SettlementCycleId = null, TdsApplicable = 0, Status = 1 }
                };
                id = await CreateRepo(ctx).CreateAsync(party);
            }

            await using (var verify1 = _fixture.CreateFreshDbContext())
            {
                (await verify1.AgentConfig.CountAsync(a => a.PartyId == id)).Should().Be(1);
            }

            // Update with an empty agent-config list → payload-authoritative removal deletes it.
            // RegistrationTypeId is a NOT NULL FK that UpdateAsync copies from the payload, so the
            // update entity must carry it (a real update echoes the full header back).
            var regId = await EnsureMiscSeedAsync();
            await using (var ctx2 = _fixture.CreateFreshDbContext())
            {
                var updateEntity = new PartyManagement.Domain.Entities.PartyMaster
                {
                    Id = id,
                    PartyName = "Agent Cfg Remove",
                    RegistrationTypeId = regId,
                    AgentConfigs = new List<PartyManagement.Domain.Entities.AgentConfig>()
                };
                await CreateRepo(ctx2).UpdateAsync(id, updateEntity);
            }

            await using (var verify2 = _fixture.CreateFreshDbContext())
            {
                (await verify2.AgentConfig.CountAsync(a => a.PartyId == id)).Should().Be(0);
            }
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StatusId_To_Pending_MiscMasterId()
        {
            await ClearPartyAsync();
            await EnsureMiscSeedAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var pendingId = await ctx.MiscMaster
                .Where(m => m.Code == "Pending")
                .Select(m => m.Id)
                .FirstAsync();

            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PCMD2", "Cmd Party 2"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyMaster.FirstAsync(p => p.Id == id);
            saved.StatusId.Should().Be(pendingId);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_PartyName_And_PartyCode()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PCMD3", "FieldsCheck"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyMaster.FirstAsync(p => p.Id == id);
            saved.PartyCode.Should().Be("PCMD3");
            saved.PartyName.Should().Be("FieldsCheck");
        }

        // --- DeleteAsync (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PDEL1", "Del 1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildPartyAsync("PDEL1", "Del 1");
            entity.IsDeleted = IsDelete.Deleted;

            var result = await CreateRepo(ctx).DeleteAsync(id, entity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Flag_IsDeleted()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PDEL2", "Del 2"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildPartyAsync("PDEL2", "Del 2");
            entity.IsDeleted = IsDelete.Deleted;
            await CreateRepo(ctx).DeleteAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.PartyMaster.FirstAsync(p => p.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildPartyAsync("GH", "Ghost");
            entity.IsDeleted = IsDelete.Deleted;

            var result = await CreateRepo(ctx).DeleteAsync(9999999, entity);

            result.Should().BeFalse();
        }

        // --- GetNextPartyCodeAsync ---

        [Fact]
        public async Task GetNextPartyCodeAsync_Should_Return_P0001_When_Empty()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetNextPartyCodeAsync();

            result.Should().Be("P0001");
        }

        [Fact]
        public async Task GetNextPartyCodeAsync_Should_Increment_Last()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("P0007", "Seven"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetNextPartyCodeAsync();

            result.Should().Be("P0008");
        }

        // --- ExistsAsync (case-insensitive name check) ---

        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_Name_Exists()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PEX1", "ExistsName"));

            var result = await CreateRepo(ctx).ExistsAsync("existsname");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_When_Name_Missing()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsAsync("nonexistent");

            result.Should().BeFalse();
        }

        // --- ExistsForUpdateAsync ---

        [Fact]
        public async Task ExistsForUpdateAsync_Should_Exclude_Self()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PEX2", "SelfName"));

            var result = await CreateRepo(ctx).ExistsForUpdateAsync("SelfName", id);

            result.Should().BeFalse();
        }

        // --- GstNumberExistsAsync ---

        [Fact]
        public async Task GstNumberExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var party = await BuildPartyAsync("PGST1", "GstParty");
            party.GSTNumber = "22ABCDE1234F1Z5";
            await CreateRepo(ctx).CreateAsync(party);

            var result = await CreateRepo(ctx).GstNumberExistsAsync("22ABCDE1234F1Z5");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GstNumberExistsAsync_Should_Exclude_Self_Via_ExcludePartyId()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var party = await BuildPartyAsync("PGST2", "GstSelf");
            party.GSTNumber = "33SELFG5678H1Z9";
            var id = await CreateRepo(ctx).CreateAsync(party);

            var result = await CreateRepo(ctx).GstNumberExistsAsync("33SELFG5678H1Z9", excludePartyId: id);

            result.Should().BeFalse();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PGID1", "GetById"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByIdAsync(id);

            result.Should().NotBeNull();
            result.PartyName.Should().Be("GetById");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        // --- RollbackStatusAsync ---

        [Fact]
        public async Task RollbackStatusAsync_Should_Set_Status_To_Pending()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PRB1", "RollbackParty"));
            ctx.ChangeTracker.Clear();

            // Seed another ApprovalStatus value so we can switch the StatusId off "Pending"
            var approvalTypeId = await ctx.MiscTypeMaster
                .Where(t => t.MiscTypeCode == "ApprovalStatus").Select(t => t.Id).FirstAsync();
            var approved = new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = approvalTypeId,
                Code = "Approved",
                Description = "Approved",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(approved);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var entity = await ctx.PartyMaster.FirstAsync(p => p.Id == id);
            entity.StatusId = approved.Id;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).RollbackStatusAsync(id);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var pendingId = await ctx.MiscMaster.Where(m => m.Code == "Pending").Select(m => m.Id).FirstAsync();
            var reloaded = await ctx.PartyMaster.FirstAsync(p => p.Id == id);
            reloaded.StatusId.Should().Be(pendingId);
        }

        [Fact]
        public async Task RollbackStatusAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).RollbackStatusAsync(9999999);

            result.Should().BeFalse();
        }

        // --- FinalizePartyStatus ---

        [Fact]
        public async Task FinalizePartyStatus_Should_Update_PartyStatus_And_StatusId()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PF1", "FinalizeParty"));
            ctx.ChangeTracker.Clear();

            // Seed a valid "Approved" status under ApprovalStatus type
            var approvalTypeId = await ctx.MiscTypeMaster
                .Where(t => t.MiscTypeCode == "ApprovalStatus").Select(t => t.Id).FirstAsync();
            var approved = new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = approvalTypeId,
                Code = "Approved_F",
                Description = "Approved (Finalize)",
                SortOrder = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(approved);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var entity = new PartyManagement.Domain.Entities.PartyMaster
            {
                Id = id,
                PartyStatus = "Approved",
                StatusId = approved.Id
            };
            var result = await CreateRepo(ctx).FinalizePartyStatus(entity);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.PartyMaster.FirstAsync(p => p.Id == id);
            reloaded.PartyStatus.Should().Be("Approved");
            reloaded.StatusId.Should().Be(approved.Id);
        }

        [Fact]
        public async Task FinalizePartyStatus_Should_Return_False_When_NotFound()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var pendingId = await ctx.MiscMaster.Where(m => m.Code == "Pending").Select(m => m.Id).FirstOrDefaultAsync();
            if (pendingId == 0)
            {
                pendingId = await EnsureMiscSeedAsync();
                pendingId = await ctx.MiscMaster.Where(m => m.Code == "Pending").Select(m => m.Id).FirstAsync();
            }

            var result = await CreateRepo(ctx).FinalizePartyStatus(new PartyManagement.Domain.Entities.PartyMaster
            {
                Id = 9999999,
                PartyStatus = "Approved",
                StatusId = pendingId
            });

            result.Should().BeFalse();
        }

        // --- LogChange ---

        [Fact]
        public async Task LogChange_Should_Insert_PartyActivityLog_Row()
        {
            await ClearPartyAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildPartyAsync("PLOG1", "LogParty"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).LogChange(id, "PartyMaster", "PartyName", "Old", "New", "Update");
            await ctx.SaveChangesAsync();

            var rows = await ctx.PartyActivityLog.Where(l => l.PartyId == id).ToListAsync();
            rows.Should().HaveCount(1);
            rows[0].ColumnName.Should().Be("PartyName");
            rows[0].ActionType.Should().Be("Update");
        }
    }
}
