using PartyManagement.Application.PartyGroup.Command.CreatePartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.TestData
{
    public static class PartyGroupBuilders
    {
        public static CreatePartyGroupCommand ValidCreateCommand(
            string name = "Test Party Group",
            int groupTypeId = 1,
            int glCategoryId = 2) =>
            new CreatePartyGroupCommand
            {
                PartyGroupName = name,
                GroupTypeId = groupTypeId,
                GlCategoryId = glCategoryId,
                IsGroup = 1,
                Description = "Test description",
                Glcode = "GL001"
            };

        public static UpdatePartyGroupCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Party Group",
            int glCategoryId = 2,
            byte isActive = 1) =>
            new UpdatePartyGroupCommand
            {
                Id = id,
                PartyGroupName = name,
                GlCategoryId = glCategoryId,
                IsGroup = 1,
                IsActive = isActive
            };

        public static DeletePartyGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeletePartyGroupCommand { Id = id };

        public static PartyGroupDto ValidDto(int id = 1) =>
            new PartyGroupDto
            {
                Id = id,
                PartyGroupName = "Test Party Group",
                GroupTypeId = 1,
                GlCategoryId = 2,
                IsGroup = 1,
                IsActive = Status.Active
            };

        public static PartyGroupByIdDto ValidByIdDto(int id = 1) =>
            new PartyGroupByIdDto
            {
                Id = id,
                PartyGroupName = "Test Party Group",
                GroupTypeId = 1,
                GlCategoryId = 2,
                IsGroup = 1,
                IsActive = Status.Active
            };

        public static PartyManagement.Domain.Entities.PartyGroup ValidEntity(int id = 1) =>
            new PartyManagement.Domain.Entities.PartyGroup
            {
                Id = id,
                PartyGroupName = "Test Party Group",
                GroupTypeId = 1,
                GlCategoryId = 2,
                IsGroup = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
