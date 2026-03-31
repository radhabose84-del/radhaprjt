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

namespace UserManagement.UnitTests.Presentation.MiscMaster
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateMiscMasterCommand>> _createValidator = new();
        private readonly Mock<IValidator<UpdateMiscMasterCommand>> _updateValidator = new();
        private readonly Mock<IValidator<DeleteMiscMasterCommand>> _deleteValidator = new();
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new();

        private MiscMasterController CreateSut() =>
            new(_mockMediator.Object, _createValidator.Object, _updateValidator.Object,
                _mockCommandRepo.Object, _deleteValidator.Object);

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
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMiscMaster_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetMiscMasterAutoCompleteDto>());

            var result = await CreateSut().GetMiscMaster("test", "TypeCode");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto());

            var result = await CreateSut().CreateAsync(new CreateMiscMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateMiscMasterCommand());

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
