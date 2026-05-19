using Contracts.Common;
using UserManagement.Application.RoleEntitlements.Commands.DeleteRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Commands
{
    public sealed class DeleteRoleEntitlementCommandHandlerTests
    {
        private static DeleteRoleEntitlementCommandHandler CreateSut() => new();

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

            result.Message.Should().Be("RoleEntitlement deletion is not supported.");
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
