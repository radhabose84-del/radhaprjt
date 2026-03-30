using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;

namespace UserManagement.UnitTests.Application.UserRoleAllocation.Queries
{
    public sealed class GetUserRoleAllocationQueryHandlerTests
    {
        private readonly Mock<IUserRoleAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetUserRoleAllocationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_WhenAllocationsExist_ReturnsMappedDtos()
        {
            var allocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>
            {
                new() { UserId = 1, UserRoleId = 10 },
                new() { UserId = 2, UserRoleId = 20 }
            };
            var dtos = new List<CreateUserRoleAllocationDto>
            {
                new() { UserId = 1, RoleIds = new List<int> { 10 } }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(allocations);

            _mockMapper
                .Setup(m => m.Map<List<CreateUserRoleAllocationDto>>(allocations))
                .Returns(dtos);

            var result = await CreateSut().Handle(new GetUserRoleAllocationQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WhenNoAllocations_ReturnsEmptyList()
        {
            var allocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>();

            _mockQueryRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(allocations);

            _mockMapper
                .Setup(m => m.Map<List<CreateUserRoleAllocationDto>>(allocations))
                .Returns(new List<CreateUserRoleAllocationDto>());

            var result = await CreateSut().Handle(new GetUserRoleAllocationQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_AnyQuery_CallsGetAllOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UserManagement.Domain.Entities.UserRoleAllocation>());

            _mockMapper
                .Setup(m => m.Map<List<CreateUserRoleAllocationDto>>(It.IsAny<object>()))
                .Returns(new List<CreateUserRoleAllocationDto>());

            await CreateSut().Handle(new GetUserRoleAllocationQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_AnyQuery_MapsRepositoryResult()
        {
            var allocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>();

            _mockQueryRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(allocations);

            _mockMapper
                .Setup(m => m.Map<List<CreateUserRoleAllocationDto>>(allocations))
                .Returns(new List<CreateUserRoleAllocationDto>());

            await CreateSut().Handle(new GetUserRoleAllocationQuery(), CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<List<CreateUserRoleAllocationDto>>(allocations),
                Times.Once);
        }
    }
}
