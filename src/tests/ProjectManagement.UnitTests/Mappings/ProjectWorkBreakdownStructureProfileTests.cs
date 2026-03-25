using AutoMapper;
using ProjectManagement.Application.Common.Mappings;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;

namespace ProjectManagement.UnitTests.Mappings
{
    public sealed class ProjectWorkBreakdownStructureProfileTests
    {
        private readonly IMapper _mapper;

        public ProjectWorkBreakdownStructureProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ProjectWorkBreakdownStructureProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsWorkBreakdownStructureName()
        {
            var cmd = new CreateProjectWorkBreakdownStructureCommand
            {
                ProjectId = 1,
                WorkBreakdownStructureName = "Phase 1"
            };

            var entity = _mapper.Map<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>(cmd);

            entity.WorkBreakdownStructureName.Should().Be("Phase 1");
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsProjectId()
        {
            var cmd = new CreateProjectWorkBreakdownStructureCommand
            {
                ProjectId = 5,
                WorkBreakdownStructureName = "Phase 1"
            };

            var entity = _mapper.Map<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>(cmd);

            entity.ProjectId.Should().Be(5);
        }
    }
}
