using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;

namespace UserManagement.UnitTests.Application.UserRoleAllocation.Commands
{
    public sealed class CreateUserRoleAllocationCommandHandlerTests
    {
        private readonly Mock<IUserRoleAllocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateUserRoleAllocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private static CreateUserRoleAllocationCommand BuildCommand(int userId = 1, List<int>? roleIds = null) =>
            new(new CreateUserRoleAllocationDto
            {
                UserId = userId,
                RoleIds = roleIds ?? new List<int> { 1, 2 }
            });

        [Fact]
        public async Task Handle_ValidCommand_CallsAddRangeOnce()
        {
            _mockCommandRepo
                .Setup(r => r.AddRangeAsync(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<List<UserRoleAllocationResponseDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(new List<UserRoleAllocationResponseDto>());

            await CreateSut().Handle(BuildCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.AddRangeAsync(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResponseList()
        {
            var responseDtos = new List<UserRoleAllocationResponseDto>
            {
                new() { UserId = 1, UserRoleId = 1 },
                new() { UserId = 1, UserRoleId = 2 }
            };

            _mockCommandRepo
                .Setup(r => r.AddRangeAsync(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<List<UserRoleAllocationResponseDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(responseDtos);

            var result = await CreateSut().Handle(BuildCommand(roleIds: new List<int> { 1, 2 }), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_BuildsCorrectAllocations_PerRoleId()
        {
            List<UserManagement.Domain.Entities.UserRoleAllocation>? captured = null;

            _mockCommandRepo
                .Setup(r => r.AddRangeAsync(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Callback<List<UserManagement.Domain.Entities.UserRoleAllocation>>(a => captured = a)
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<List<UserRoleAllocationResponseDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(new List<UserRoleAllocationResponseDto>());

            await CreateSut().Handle(BuildCommand(userId: 5, roleIds: new List<int> { 10, 20, 30 }), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Should().HaveCount(3);
            captured.Should().AllSatisfy(a => a.UserId.Should().Be(5));
            captured.Select(a => a.UserRoleId).Should().BeEquivalentTo(new[] { 10, 20, 30 });
        }

        [Fact]
        public async Task Handle_EmptyRoleIds_CallsAddRangeWithEmptyList()
        {
            List<UserManagement.Domain.Entities.UserRoleAllocation>? captured = null;

            _mockCommandRepo
                .Setup(r => r.AddRangeAsync(It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Callback<List<UserManagement.Domain.Entities.UserRoleAllocation>>(a => captured = a)
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<List<UserRoleAllocationResponseDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.UserRoleAllocation>>()))
                .Returns(new List<UserRoleAllocationResponseDto>());

            await CreateSut().Handle(BuildCommand(roleIds: new List<int>()), CancellationToken.None);

            captured.Should().BeEmpty();
        }
    }
}
