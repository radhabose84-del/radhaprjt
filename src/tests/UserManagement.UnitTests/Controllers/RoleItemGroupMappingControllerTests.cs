using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetAllRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingById;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingByRoleId;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class RoleItemGroupMappingControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private RoleItemGroupMappingController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAllAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllRoleItemGroupMappingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RoleItemGroupMappingDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<RoleItemGroupMappingDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllRoleItemGroupMappingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RoleItemGroupMappingDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<RoleItemGroupMappingDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAllRoleItemGroupMappingQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleItemGroupMappingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoleItemGroupMappingDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetByRoleIdAsync_ValidRoleId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleItemGroupMappingByRoleIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RoleItemGroupMappingLookupDto>());

            var result = await CreateSut().GetByRoleIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByRoleIdAsync_InvalidRoleId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByRoleIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateRoleItemGroupMappingCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateRoleItemGroupMappingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoleItemGroupMappingDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var command = new UpdateRoleItemGroupMappingCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateRoleItemGroupMappingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoleItemGroupMappingDto());

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteRoleItemGroupMappingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
