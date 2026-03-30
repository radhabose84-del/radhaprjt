using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Commands.DeleteUserRoleAllocation;

namespace UserManagement.UnitTests.Application.UserRoleAllocation.Commands
{
    // The handler casts IUserRoleCommandRepository to IUserRoleAllocationCommandRepository at runtime.
    // This combined interface allows the mock to satisfy both sides of that cast.
    public interface ICombinedUserRoleRepository
        : IUserRoleCommandRepository, IUserRoleAllocationCommandRepository { }

    public sealed class DeleteRoleAllocationCommandHandlerTests
    {
        private readonly Mock<ICombinedUserRoleRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserRoleAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteRoleAllocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsOne()
        {
            var allocation = new UserManagement.Domain.Entities.UserRoleAllocation { UserId = 1, UserRoleId = 10 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(allocation);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(5))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteRoleAllocationCommand(5), CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFoundId_ReturnsZero()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.UserRoleAllocation?)null);

            var result = await CreateSut().Handle(new DeleteRoleAllocationCommand(999), CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var allocation = new UserManagement.Domain.Entities.UserRoleAllocation { UserId = 2, UserRoleId = 20 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(allocation);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(7))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteRoleAllocationCommand(7), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFoundId_DoesNotCallDelete()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(888))
                .ReturnsAsync((UserManagement.Domain.Entities.UserRoleAllocation?)null);

            await CreateSut().Handle(new DeleteRoleAllocationCommand(888), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
