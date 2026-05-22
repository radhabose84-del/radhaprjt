using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyAutoComplete;
using UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyById;
using UserManagement.Application.AccessPolicy.Queries.GetAllAccessPolicy;
using UserManagement.Application.AccessPolicy.Queries.GetRoleAccessPolicies;
using UserManagement.Presentation.Controllers;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class AccessPolicyControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private AccessPolicyController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllAccessPolicyAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllAccessPolicyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccessPolicyDto>>
                {
                    IsSuccess = true, Message = "Success",
                    Data = new List<AccessPolicyDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            var result = await CreateSut().GetAllAccessPolicyAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAccessPolicyAsync_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllAccessPolicyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccessPolicyDto>>
                {
                    IsSuccess = true, Message = "Success",
                    Data = new List<AccessPolicyDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            await CreateSut().GetAllAccessPolicyAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllAccessPolicyQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAccessPolicyByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccessPolicyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccessPolicyBuilders.ValidDto());

            var result = await CreateSut().GetAccessPolicyByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAccessPolicyAutoCompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccessPolicyAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccessPolicyDto>().AsReadOnly());

            var result = await CreateSut().GetAccessPolicyAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRoleAccessPoliciesAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleAccessPoliciesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RoleAccessPolicyDto>());

            var result = await CreateSut().GetRoleAccessPoliciesAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAccessPolicy_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAccessPolicyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccessPolicyBuilders.ValidCreateResponse());

            var result = await CreateSut().CreateAccessPolicy(AccessPolicyBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAccessPolicy_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAccessPolicyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccessPolicyBuilders.ValidUpdateResponse());

            var result = await CreateSut().UpdateAccessPolicy(AccessPolicyBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAccessPolicy_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAccessPolicyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAccessPolicy(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAccessPolicy_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAccessPolicyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAccessPolicy(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAccessPolicyCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignRoleAccessPolicy_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AssignRoleAccessPolicyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccessPolicyBuilders.ValidCreateResponse());

            var result = await CreateSut().AssignRoleAccessPolicy(AccessPolicyBuilders.ValidAssignCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RemoveRoleAccessPolicy_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<RemoveRoleAccessPolicyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().RemoveRoleAccessPolicy(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
