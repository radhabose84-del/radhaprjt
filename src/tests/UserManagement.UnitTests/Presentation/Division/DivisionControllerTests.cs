using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Divisions.Queries.GetDivisionById;
using UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete;
using UserManagement.Presentation.Controllers;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Presentation.Division
{
    public sealed class DivisionControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private DivisionController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllDivisions_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DivisionDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<DivisionDTO> { DivisionBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDivisionsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDivisions_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DivisionDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<DivisionDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllDivisionsAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetDivisionQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DivisionBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var command = DivisionBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DivisionBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            var command = DivisionBuilders.ValidUpdateCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DivisionBuilders.ValidDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_DivisionNotFound_ReturnsNotFound()
        {
            var command = DivisionBuilders.ValidUpdateCommand(id: 999);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DivisionDTO?)null);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteDivisionCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
