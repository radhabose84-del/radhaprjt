using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.DeleteRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Commands
{
    public sealed class DeleteRoleEntitlementCommandHandlerTests
    {
        private readonly Mock<IRoleEntitlementCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleEntitlementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRoleEntitlementCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AnyRequest_ReturnsFailureResponse()
        {
            var command = new DeleteRoleEntitlementCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AnyRequest_ReturnsExpectedMessage()
        {
            var command = new DeleteRoleEntitlementCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Be("RoleEntitlement deletion failed.");
        }

        [Fact]
        public async Task Handle_AnyRequest_ReturnsApiResponseDtoType()
        {
            var command = new DeleteRoleEntitlementCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeOfType<ApiResponseDTO<RoleEntitlementDto>>();
        }

        [Fact]
        public async Task Handle_AnyRequest_DataIsNull()
        {
            var command = new DeleteRoleEntitlementCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().BeNull();
        }
    }
}
