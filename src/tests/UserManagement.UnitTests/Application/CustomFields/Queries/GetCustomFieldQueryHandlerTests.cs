using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomField;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Application.CustomFields.Queries
{
    public sealed class GetCustomFieldQueryHandlerTests
    {
        private readonly Mock<ICustomFieldQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCustomFieldQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<CustomField> ValidEntityList() =>
            new()
            {
                new() { Id = 1, LabelName = "Field One" },
                new() { Id = 2, LabelName = "Field Two" }
            };

        private static List<CustomFieldDTO> ValidDtoList() =>
            new()
            {
                new() { Id = 1, LabelName = "Field One" },
                new() { Id = 2, LabelName = "Field Two" }
            };

        [Fact]
        public async Task Handle_WithData_ReturnsSuccess()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetCustomFieldQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo
                .Setup(r => r.GetAllCustomFieldsAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((entities, 2));

            _mockMapper
                .Setup(m => m.Map<List<CustomFieldDTO>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsPaginationMetadata()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetCustomFieldQuery { PageNumber = 2, PageSize = 5 };

            _mockQueryRepo
                .Setup(r => r.GetAllCustomFieldsAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((entities, 20));

            _mockMapper
                .Setup(m => m.Map<List<CustomFieldDTO>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            var query = new GetCustomFieldQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo
                .Setup(r => r.GetAllCustomFieldsAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((new List<CustomField>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<CustomFieldDTO>>(It.IsAny<List<CustomField>>()))
                .Returns(new List<CustomFieldDTO>());

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithData_CallsRepositoryOnce()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetCustomFieldQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo
                .Setup(r => r.GetAllCustomFieldsAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((entities, 2));

            _mockMapper
                .Setup(m => m.Map<List<CustomFieldDTO>>(entities))
                .Returns(dtos);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllCustomFieldsAsync(query.PageNumber, query.PageSize, query.SearchTerm),
                Times.Once);
        }
    }
}
