using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomFieldById;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Application.CustomFields.Queries
{
    public sealed class GetCustomFieldByIdQueryHandlerTests
    {
        private readonly Mock<ICustomFieldQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCustomFieldByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static CustomField ValidEntity(int id = 1) =>
            new() { Id = id, LabelName = "Test Label" };

        private static CustomFieldByIdDTO ValidDto(int id = 1) =>
            new() { Id = id, LabelName = "Test Label", IsActive = true };

        private (dynamic, IList<dynamic>, IList<dynamic>, IList<dynamic>) BuildTuple(
            CustomField entity,
            IList<dynamic>? menu = null,
            IList<dynamic>? unit = null,
            IList<dynamic>? options = null) =>
            (entity,
             menu ?? new List<dynamic>(),
             unit ?? new List<dynamic>(),
             options ?? new List<dynamic>());

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetCustomFieldByIdQuery { Id = 1 };
            var tuple = BuildTuple(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(query.Id))
                .ReturnsAsync(tuple);

            _mockMapper
                .Setup(m => m.Map<CustomFieldByIdDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMapper
                .Setup(m => m.Map<List<CustomFieldMenuDto>>(It.IsAny<object>()))
                .Returns(new List<CustomFieldMenuDto>());

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.LabelName.Should().Be("Test Label");
        }

        [Fact]
        public async Task Handle_ExistingId_CallsRepositoryOnce()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetCustomFieldByIdQuery { Id = 1 };
            var tuple = BuildTuple(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(query.Id))
                .ReturnsAsync(tuple);

            _mockMapper
                .Setup(m => m.Map<CustomFieldByIdDTO>(It.IsAny<object>()))
                .Returns(dto);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(query.Id), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullSubLists_StillReturnsDto()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetCustomFieldByIdQuery { Id = 1 };
            var tuple = ((dynamic)entity, (IList<dynamic>?)null!, (IList<dynamic>?)null!, (IList<dynamic>?)null!);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(query.Id))
                .ReturnsAsync(tuple);

            _mockMapper
                .Setup(m => m.Map<CustomFieldByIdDTO>(It.IsAny<object>()))
                .Returns(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_MapsDtoFromEntity()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetCustomFieldByIdQuery { Id = 1 };
            var tuple = BuildTuple(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(query.Id))
                .ReturnsAsync(tuple);

            _mockMapper
                .Setup(m => m.Map<CustomFieldByIdDTO>(It.IsAny<object>()))
                .Returns(dto);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<CustomFieldByIdDTO>(It.IsAny<object>()),
                Times.Once);
        }
    }
}
