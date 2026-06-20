using Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Application.GlAccountMaster.Commands.AddGlAccountFavourite;
using FinanceManagement.Application.GlAccountMaster.Commands.RemoveGlAccountFavourite;
using FinanceManagement.Application.GlAccountMaster.Commands.RecordGlAccountRecent;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountSearch;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountFavourites;

namespace FinanceManagement.UnitTests.Controllers
{
    // US-GL02-07 type-ahead endpoints (search + favourites + recent). Pre-existing CRUD is unchanged.
    public sealed class GlAccountMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GlAccountMasterController CreateSut() => new(_mockMediator.Object);

        private static IReadOnlyList<AccountSearchResultDto> EmptySearch() => new List<AccountSearchResultDto>();

        [Fact]
        public async Task Search_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetGlAccountSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptySearch());

            var result = await CreateSut().SearchAccountsAsync("110", null, null, false, 20);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFavourites_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetGlAccountFavouritesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptySearch());

            var result = await CreateSut().GetFavouritesAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AddFavourite_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<AddGlAccountFavouriteCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            var result = await CreateSut().AddFavourite(new AddGlAccountFavouriteCommand { GlAccountMasterId = 42 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RemoveFavourite_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<RemoveGlAccountFavouriteCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            var result = await CreateSut().RemoveFavourite(42);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RecordRecent_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<RecordGlAccountRecentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            var result = await CreateSut().RecordRecent(new RecordGlAccountRecentCommand { GlAccountMasterId = 42 });

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
