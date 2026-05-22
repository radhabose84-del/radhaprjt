using AutoMapper;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyById;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Queries
{
    public sealed class GetAccessPolicyByIdQueryHandlerTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper>                      _mockMapper    = new(MockBehavior.Loose);
        private readonly Mock<IMediator>                    _mockMediator  = new(MockBehavior.Loose);

        private GetAccessPolicyByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = AccessPolicyBuilders.ValidDto(id: 5);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetAccessPolicyByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.PolicyCode.Should().Be("AP001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AccessPolicyDto?)null);

            var result = await CreateSut().Handle(
                new GetAccessPolicyByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AccessPolicyDto?)null);

            await CreateSut().Handle(
                new GetAccessPolicyByIdQuery { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(AccessPolicyBuilders.ValidDto());

            await CreateSut().Handle(
                new GetAccessPolicyByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
