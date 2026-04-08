using Contracts.Common;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Command.CreateMiscMaster;
using UserManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using UserManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using UserManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateMiscMasterCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateMiscMasterCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<DeleteMiscMasterCommand>> _mockDeleteValidator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private MiscMasterController CreateSut() =>
            new(_mockMediator.Object, _mockCreateValidator.Object, _mockUpdateValidator.Object,
                _mockCommandRepo.Object, _mockDeleteValidator.Object);

        // --- GetAllMiscMasterAsync ---

        [Fact]
        public async Task GetAllMiscMasterAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetMiscMasterDto> { new GetMiscMasterDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMiscMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetMiscMaster (AutoComplete) ---

        [Fact]
        public async Task GetMiscMaster_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetMiscMasterAutoCompleteDto>());

            var result = await CreateSut().GetMiscMaster("test", "TYPE001");

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateMiscMasterCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Update ---

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            var command = new UpdateMiscMasterCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
