using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupAutoComplete;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupById;
using BudgetManagement.Presentation.Controllers;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.UnitTests.Controllers
{
    public sealed class BudgetGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private BudgetGroupController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<BudgetGroupListItemDto>>
                {
                    IsSuccess = true,
                    Data = new List<BudgetGroupListItemDto>(),
                    TotalCount = 0
                });

            var result = await CreateSut().GetBudgetGroups(new GetBudgetGroupQuery { PageNumber = 1, PageSize = 10 });

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<BudgetGroupDto>
                {
                    IsSuccess = true,
                    Data = BudgetGroupBuilders.ValidDto()
                });

            var result = await CreateSut().GetBudgetGroupById(1);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBudgetGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BudgetGroupAutoCompleteDto>());

            var result = await CreateSut().GetBudgetGroupAutoComplete(
                new GetBudgetGroupAutoCompleteQuery { SearchPattern = "test" });

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBudgetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateBudgetGroup(BudgetGroupBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            var command = BudgetGroupBuilders.ValidUpdateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateBudgetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateBudgetGroup(command.Id, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var command = BudgetGroupBuilders.ValidUpdateCommand(id: 1);

            var result = await CreateSut().UpdateBudgetGroup(99, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBudgetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteBudgetGroup(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBudgetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().DeleteBudgetGroup(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
