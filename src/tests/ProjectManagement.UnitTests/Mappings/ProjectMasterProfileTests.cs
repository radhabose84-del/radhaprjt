using AutoMapper;
using ProjectManagement.Application.Common.Mappings;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.UnitTests.Mappings
{
    public sealed class ProjectMasterProfileTests
    {
        private readonly IMapper _mapper;

        public ProjectMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ProjectMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Entity_To_ProjectMasterDto_MapsId()
        {
            var entity = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                Id = 42,
                ProjectCode = "PRJ001",
                ProjectName = "Test Project",
                ProjectDocuments = new List<ProjectDocument>()
            };

            var dto = _mapper.Map<ProjectMasterDto>(entity);

            dto.Id.Should().Be(42);
        }

        [Fact]
        public void Entity_To_ProjectMasterDto_MapsEmptyDocuments()
        {
            var entity = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                Id = 1,
                ProjectDocuments = new List<ProjectDocument>()
            };

            var dto = _mapper.Map<ProjectMasterDto>(entity);

            dto.Documents.Should().NotBeNull();
            dto.Documents.Should().BeEmpty();
        }

        [Fact]
        public void GetProjectMasterDto_To_ProjectMasterDto_MapsCorrectly()
        {
            var src = new GetProjectMasterDto { Id = 10, ProjectCode = "PRJ010" };

            var dto = _mapper.Map<ProjectMasterDto>(src);

            dto.Id.Should().Be(10);
        }
    }
}
