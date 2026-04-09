using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Commands.UpdateUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;

namespace UserManagement.UnitTests.Application.Role.Commands
{
    public sealed class UpdateRoleAllocationCommandHandlerTests
    {
        private readonly Mock<IUserRoleAllocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserRoleAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRoleAllocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_NoChanges_ReturnsResult()
        {
            var existingAllocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>
            {
                new() { UserRoleId = 1, UserId = 42 }
            };

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(42))
                .ReturnsAsync(existingAllocations);

            _mockCommandRepo
                .Setup(r => r.AddRangeAsync(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<List<UserRoleAllocationResponseDto>>(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(new List<UserRoleAllocationResponseDto> { new() { UserRoleId = 1, UserId = 42 } });

            var result = await CreateSut().Handle(
                new UpdateRoleAllocationCommand(42, new List<int> { 1 }),
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
