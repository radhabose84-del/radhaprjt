using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountSearch;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Queries
{
    public sealed class GetGlAccountSearchQueryHandlerTests
    {
        private readonly Mock<IGlAccountMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IGlAccountUserPrefStore> _mockStore = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGlAccountSearchQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockStore.Object, _mockIp.Object, _mockMediator.Object);

        private static AccountSearchResultDto Row(int id, string code) =>
            new() { Id = id, AccountCode = code, AccountName = code, IsActive = true };

        private static GlAccountRecentUseItem Recent(int id) =>
            new(id, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), 1);

        private void Session(int companyId = 1, int userId = 396)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(companyId);
            _mockIp.Setup(s => s.GetUserId()).Returns(userId);
        }

        [Fact]
        public async Task Handle_TermSearch_RanksFavouritesThenRecentThenRest()
        {
            Session();
            // SQL returns 1,2,3 in code order; user favourited 3, recently used 2.
            _mockRepo.Setup(r => r.SearchAsync("110", 1, null, null, false, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountSearchResultDto> { Row(1, "110"), Row(2, "111"), Row(3, "112") });
            _mockStore.Setup(s => s.GetFavouriteAccountIdsAsync(396, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 3 });
            _mockStore.Setup(s => s.GetRecentAsync(396, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GlAccountRecentUseItem> { Recent(2) });

            var result = await CreateSut().Handle(new GetGlAccountSearchQuery { Term = "110" }, CancellationToken.None);

            result.Select(r => r.Id).Should().ContainInOrder(3, 2, 1);   // favourite → recent → rest
            result.Single(r => r.Id == 3).IsFavourite.Should().BeTrue();
            result.Single(r => r.Id == 2).IsRecent.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyTerm_WithShortcuts_ReturnsFavouritesAndRecentByIds()
        {
            Session();
            _mockStore.Setup(s => s.GetFavouriteAccountIdsAsync(396, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 5 });
            _mockStore.Setup(s => s.GetRecentAsync(396, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GlAccountRecentUseItem> { Recent(7) });
            _mockRepo.Setup(r => r.GetByIdsForSearchAsync(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(5) && ids.Contains(7)), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountSearchResultDto> { Row(7, "200"), Row(5, "100") });

            var result = await CreateSut().Handle(new GetGlAccountSearchQuery { Term = "" }, CancellationToken.None);

            result.Select(r => r.Id).Should().ContainInOrder(5, 7);   // favourite first, then recent
        }

        [Fact]
        public async Task Handle_EmptyTerm_NoShortcuts_FallsBackToGeneralSearch()
        {
            Session();
            _mockStore.Setup(s => s.GetFavouriteAccountIdsAsync(396, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int>());
            _mockStore.Setup(s => s.GetRecentAsync(396, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GlAccountRecentUseItem>());
            _mockRepo.Setup(r => r.SearchAsync(null, 1, null, null, false, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountSearchResultDto> { Row(1, "100") });

            var result = await CreateSut().Handle(new GetGlAccountSearchQuery { Term = null }, CancellationToken.None);

            result.Should().ContainSingle();
            _mockRepo.Verify(r => r.SearchAsync(null, 1, null, null, false, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ClampsTakeToMax50()
        {
            Session();
            _mockStore.Setup(s => s.GetFavouriteAccountIdsAsync(396, 1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<int>());
            _mockStore.Setup(s => s.GetRecentAsync(396, 1, 50, It.IsAny<CancellationToken>())).ReturnsAsync(new List<GlAccountRecentUseItem>());
            _mockRepo.Setup(r => r.SearchAsync("x", 1, null, null, false, 50, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountSearchResultDto>());

            await CreateSut().Handle(new GetGlAccountSearchQuery { Term = "x", Take = 999 }, CancellationToken.None);

            _mockRepo.Verify(r => r.SearchAsync("x", 1, null, null, false, 50, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
