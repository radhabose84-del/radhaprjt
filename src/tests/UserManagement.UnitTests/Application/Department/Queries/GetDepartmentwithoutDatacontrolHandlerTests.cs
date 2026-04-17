using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentwithoutDatacontrolHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetDepartmentwithoutDatacontrolHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetDepartmentwithoutDatacontrolHandler CreateSut() =>
            new(
                _mockQueryRepo.Object,
                _mockMapper.Object,
                _mockMediator.Object,
                _mockLogger.Object,
                _mockIpService.Object);

        [Fact]
        public async Task Handle_ReturnsMappedDepartments()
        {
            var entities = new List<UserManagement.Domain.Entities.Department>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };
            var dtoList = new List<DepartmentAutocompleteDto>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            _mockQueryRepo
                .Setup(r => r.GetDepartment_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(entities))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetDepartmentwithoutDataControl { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Success");
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_CallsSuperAdminRepositoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetDepartment_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Department>());

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(It.IsAny<List<UserManagement.Domain.Entities.Department>>()))
                .Returns(new List<DepartmentAutocompleteDto>());

            await CreateSut().Handle(
                new GetDepartmentwithoutDataControl { SearchPattern = "abc" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetDepartment_SuperAdmin("abc"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetDepartment_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Department>());

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(It.IsAny<List<UserManagement.Domain.Entities.Department>>()))
                .Returns(new List<DepartmentAutocompleteDto>());

            var result = await CreateSut().Handle(
                new GetDepartmentwithoutDataControl { SearchPattern = "nomatch" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullSearchPattern_StillInvokesRepository()
        {
            _mockQueryRepo
                .Setup(r => r.GetDepartment_SuperAdmin(null))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Department>());

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(It.IsAny<List<UserManagement.Domain.Entities.Department>>()))
                .Returns(new List<DepartmentAutocompleteDto>());

            var result = await CreateSut().Handle(
                new GetDepartmentwithoutDataControl { SearchPattern = null },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockQueryRepo.Verify(r => r.GetDepartment_SuperAdmin(null), Times.Once);
        }
    }
}
