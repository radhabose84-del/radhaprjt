using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Application.GlAccountMaster.Queries.GetCoaConsistencyReport;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Queries
{
    public sealed class GetCoaConsistencyReportQueryHandlerTests
    {
        private readonly Mock<IGlAccountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCoaConsistencyReportQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCompanyLookup.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_FlagsSingleEntityAccounts_WithCompanyName()
        {
            _mockIp.Setup(s => s.GetEntityId()).Returns(5);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "Spinning", EntityId = 5 },
                new() { CompanyId = 2, CompanyName = "Processing", EntityId = 5 },
                new() { CompanyId = 9, CompanyName = "OtherEntity", EntityId = 99 },
            });
            _mockQueryRepo
                .Setup(r => r.GetSingleEntityAccountsAsync(It.IsAny<IReadOnlyCollection<int>>()))
                .ReturnsAsync(new List<CoaConsistencyReportItemDto>
                {
                    new() { AccountCode = "5005", AccountName = "Dye Cost", CompanyId = 2 }
                });

            var result = await CreateSut().Handle(new GetCoaConsistencyReportQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].CompanyName.Should().Be("Processing");
            result.Data![0].Flag.Should().Be("in Processing only");
        }

        [Fact]
        public async Task Handle_OnlyConsidersCompaniesInSessionEntity()
        {
            _mockIp.Setup(s => s.GetEntityId()).Returns(5);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "Spinning", EntityId = 5 },
                new() { CompanyId = 9, CompanyName = "OtherEntity", EntityId = 99 },
            });
            List<int>? captured = null;
            _mockQueryRepo
                .Setup(r => r.GetSingleEntityAccountsAsync(It.IsAny<IReadOnlyCollection<int>>()))
                .Callback<IReadOnlyCollection<int>>(ids => captured = ids.ToList())
                .ReturnsAsync(new List<CoaConsistencyReportItemDto>());

            await CreateSut().Handle(new GetCoaConsistencyReportQuery(), CancellationToken.None);

            captured.Should().BeEquivalentTo(new[] { 1 });
        }
    }
}
