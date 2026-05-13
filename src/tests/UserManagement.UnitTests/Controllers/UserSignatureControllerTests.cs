using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Application.UserSignature.Queries.GetAllUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureByUserId;
using Contracts.Common;
using UserManagement.Presentation.Controllers;
using UserManagement.Presentation.Requests.UserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UserSignatureControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private UserSignatureController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task GetAllUserSignatureAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllUserSignatureQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllUserSignatureDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAllUserSignatureDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUserSignatureAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllUserSignatureAsync_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllUserSignatureQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllUserSignatureDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found",
                    Data = new List<GetAllUserSignatureDto>()
                });

            var result = await CreateSut().GetAllUserSignatureAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserSignatureByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserSignatureByIdDto { Id = 1 });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserSignatureByUserIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserSignatureByIdDto { UserId = 10 });

            var result = await CreateSut().GetByUserIdAsync(10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var request = new CreateUserSignatureRequest { UserId = 1, File = UserSignatureBuilders.BuildFormFile() };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserSignatureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(7);

            var result = await CreateSut().CreateAsync(request);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSendOnce()
        {
            var request = new CreateUserSignatureRequest { UserId = 1, File = UserSignatureBuilders.BuildFormFile() };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserSignatureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(request);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateUserSignatureCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var request = new UpdateUserSignatureRequest { File = UserSignatureBuilders.BuildFormFile(), IsActive = 1 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUserSignatureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(id: 1, request: request);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUserSignatureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUserSignatureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteUserSignatureCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
