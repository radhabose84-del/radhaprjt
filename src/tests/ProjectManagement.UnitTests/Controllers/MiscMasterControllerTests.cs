using Contracts.Common;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.CreateMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using ProjectManagement.Presentation.Controllers;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Controllers
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateMiscMasterCommand>> _mockCreateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UpdateMiscMasterCommand>> _mockUpdateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<DeleteMiscMasterCommand>> _mockDeleteValidator = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private MiscMasterController CreateSut() =>
            new(_mockMediator.Object, _mockCreateValidator.Object, _mockUpdateValidator.Object,
                _mockCommandRepo.Object, _mockDeleteValidator.Object);

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
        public async Task GetById_ReturnsOkResult()
        {
            var dto = MiscMasterBuilders.ValidDto();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ValidCommand_ReturnsOkResult()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            var dto = MiscMasterBuilders.ValidDto();

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().CreateAsync(command);
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
