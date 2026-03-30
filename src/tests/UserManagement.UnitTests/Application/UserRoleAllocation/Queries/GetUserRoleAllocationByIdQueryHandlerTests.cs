using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById;

namespace UserManagement.UnitTests.Application.UserRoleAllocation.Queries
{
    public sealed class GetUserRoleAllocationByIdQueryHandlerTests
    {
        private readonly Mock<IUserRoleAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetUserRoleAllocationByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidUserId_ReturnsDtoWithRoleIds()
        {
            var allocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>
            {
                new() { UserId = 3, UserRoleId = 10 },
                new() { UserId = 3, UserRoleId = 20 }
            };

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(3))
                .ReturnsAsync(allocations);

            var result = await CreateSut().Handle(new GetUserRoleAllocationByIdQuery(3), CancellationToken.None);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(3);
            result.RoleIds.Should().BeEquivalentTo(new List<int> { 10, 20 });
        }

        [Fact]
        public async Task Handle_UserWithNoRoles_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.UserRoleAllocation>());

            var result = await CreateSut().Handle(new GetUserRoleAllocationByIdQuery(5), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_InvalidUserId_ThrowsArgumentException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new GetUserRoleAllocationByIdQuery(0), CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid User ID*");
        }

        [Fact]
        public async Task Handle_ValidUserId_CallsGetByUserIdOnce()
        {
            var allocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>
            {
                new() { UserId = 7, UserRoleId = 30 }
            };

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(7))
                .ReturnsAsync(allocations);

            await CreateSut().Handle(new GetUserRoleAllocationByIdQuery(7), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByUserIdAsync(7), Times.Once);
        }
    }
}
