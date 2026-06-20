using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.MapScheduleIIILine;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupTree;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupById;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupAutoComplete;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupParents;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupApprovalChain;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupMovePending;
using FinanceManagement.Application.AccountGroup.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class AccountGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private AccountGroupController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetTree_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupTreeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccountGroupTreeDto>>
                {
                    IsSuccess = true,
                    Data = new List<AccountGroupTreeDto>(),
                    TotalCount = 0
                });

            var result = await CreateSut().GetAccountGroupTreeAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountGroupDetailDto());

            var result = await CreateSut().GetAccountGroupByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<AccountGroupLookupDto>)new List<AccountGroupLookupDto>());

            var result = await CreateSut().GetAccountGroupAutoCompleteAsync("inv");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetParents_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupParentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<AccountGroupLookupDto>)new List<AccountGroupLookupDto>());

            var result = await CreateSut().GetAccountGroupParentsAsync(2);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task LeafGroups_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupLeavesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<AccountGroupLookupDto>)new List<AccountGroupLookupDto>());

            var result = await CreateSut().GetAccountGroupLeavesAsync(3);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetApprovalChain_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupApprovalChainQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<AccountGroupApprovalChainDto>)new List<AccountGroupApprovalChainDto>());

            var result = await CreateSut().GetAccountGroupApprovalChainAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMovePending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountGroupMovePendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccountGroupMovePendingDto>>
                {
                    IsSuccess = true,
                    Data = new List<AccountGroupMovePendingDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAccountGroupMovePendingAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAccountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateAccountGroup(new CreateAccountGroupCommand
            {
                GroupCode = "A-CA-INV-FF",
                GroupName = "Finished Goods — Fabric",
                ParentAccountGroupId = 5
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAccountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateAccountGroup(new UpdateAccountGroupCommand
            {
                Id = 1,
                GroupName = "Inventories",
                IsActive = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Move_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<MoveAccountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Moved", Data = 10 });

            var result = await CreateSut().MoveAccountGroup(new MoveAccountGroupCommand
            {
                Id = 10,
                NewParentAccountGroupId = 5,
                Justification = "Restructure for FY2026 reporting"
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task MapScheduleIIILine_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<MapScheduleIIILineCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Mapped", Data = 3 });

            var result = await CreateSut().MapScheduleIIILine(new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 10 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAccountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAccountGroup(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAccountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAccountGroup(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAccountGroupCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
