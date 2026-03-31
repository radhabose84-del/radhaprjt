using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Application.CustomFields.Commands.DeleteCustomField;
using UserManagement.Application.CustomFields.Commands.UpdateCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomFieldById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.CustomField
{
    public sealed class CustomFieldControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CustomFieldController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
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

            var result = await CreateSut().GetAllCustomFieldsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCustomFieldByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomFieldByIdDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateCustomFieldCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateCustomFieldCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCustomFieldCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
