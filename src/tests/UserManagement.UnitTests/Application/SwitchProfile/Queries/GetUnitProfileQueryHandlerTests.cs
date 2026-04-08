using AutoMapper;
using Contracts.Interfaces;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IProfile;
using UserManagement.Application.SwitchProfile.Queries.GetUnitProfile;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.SwitchProfile.Queries
{
    public sealed class GetUnitProfileQueryHandlerTests
    {
        private readonly Mock<IProfileQuery> _mockProfileQuery = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUnitProfileQueryHandler CreateSut() =>
            new(_mockProfileQuery.Object, _mockMapper.Object, _mockIpService.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(42);

            var entities = new List<UserManagement.Domain.Entities.Unit> { new() { Id = 1 } };
            var dtoList = new List<GetUnitProfileDTO> { new() { UnitId = 1 } };

            _mockProfileQuery
                .Setup(r => r.GetUnit(42))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetUnitProfileDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitProfileQuery(), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoResults_ThrowsValidationException()
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(42);

            _mockProfileQuery
                .Setup(r => r.GetUnit(42))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Unit>());

            Func<Task> act = () => CreateSut().Handle(
                new GetUnitProfileQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
