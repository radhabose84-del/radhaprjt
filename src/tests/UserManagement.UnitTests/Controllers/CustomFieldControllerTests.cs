using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Application.CustomFields.Commands.DeleteCustomField;
using UserManagement.Application.CustomFields.Commands.UpdateCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomFieldById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class CustomFieldControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CustomFieldController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllCustomFieldsAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCustomFieldQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CustomFieldDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CustomFieldDTO> { new CustomFieldDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllCustomFieldsAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCustomFieldsAsync_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCustomFieldQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CustomFieldDTO>>
                {
                    IsSuccess = true,
                    Data = new List<CustomFieldDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            await sut.GetAllCustomFieldsAsync(1, 10);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCustomFieldQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCustomFieldByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomFieldByIdDTO());

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateCustomFieldCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateCustomFieldCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Update(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            await sut.Delete(1);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteCustomFieldCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
