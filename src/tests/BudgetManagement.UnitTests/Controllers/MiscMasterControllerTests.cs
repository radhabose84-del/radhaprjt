using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using BudgetManagement.Presentation.Controllers;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.UnitTests.Controllers
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateMiscMasterCommand>> _mockCreateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UpdateMiscMasterCommand>> _mockUpdateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<DeleteMiscMasterCommand>> _mockDeleteValidator = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private MiscMasterController CreateSut() => new(
            _mockMediator.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockCommandRepo.Object,
            _mockDeleteValidator.Object);

        private void SetupValidCreateValidation()
        {
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private void SetupValidUpdateValidation()
        {
            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMiscMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscMasterDto>(),
                    TotalCount = 0
                });

            await CreateSut().GetAllMiscMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetMiscMasterAutoCompleteDto>
                {
                    MiscMasterBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetMiscMaster("test", "ALLOC");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ValidCommand_ReturnsOkResult()
        {
            SetupValidCreateValidation();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(MiscMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_InvalidCommand_ReturnsBadRequest()
        {
            var failures = new List<FluentValidation.Results.ValidationFailure>
            {
                new("Code", "Code is required.")
            };
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var invalidCommand = MiscMasterBuilders.ValidCreateCommand(code: null);
            var result = await CreateSut().CreateAsync(invalidCommand);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            SetupValidUpdateValidation();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(MiscMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

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
