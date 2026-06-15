using Contracts.Common;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.AccessPolicy.Queries.GetRoleAccessPolicies;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class AccessPolicyBuilders
    {
        public static CreateAccessPolicyCommand ValidCreateCommand(
            string policyCode = "AP001",
            string policyName = "Test Policy",
            string entityName = "SalesOrder",
            string fieldName  = "SalesOrderTypeId") =>
            new CreateAccessPolicyCommand
            {
                PolicyCode = policyCode,
                PolicyName = policyName,
                EntityName = entityName,
                FieldName  = fieldName
            };

        public static UpdateAccessPolicyCommand ValidUpdateCommand(
            int    id         = 1,
            string policyName = "Updated Policy",
            string entityName = "SalesOrder",
            string fieldName  = "SalesOrderTypeId",
            int    isActive   = 1) =>
            new UpdateAccessPolicyCommand
            {
                Id         = id,
                PolicyName = policyName,
                EntityName = entityName,
                FieldName  = fieldName,
                IsActive   = isActive
            };

        public static DeleteAccessPolicyCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAccessPolicyCommand(id);

        public static AssignRoleAccessPolicyCommand ValidAssignCommand(
            int accessPolicyId = 1,
            int roleId         = 1,
            int valueId        = 1) =>
            new AssignRoleAccessPolicyCommand
            {
                AccessPolicyId = accessPolicyId,
                RoleId         = roleId,
                ValueId        = valueId
            };

        public static RemoveRoleAccessPolicyCommand ValidRemoveCommand(int id = 1) =>
            new RemoveRoleAccessPolicyCommand(id);

        public static AccessPolicyDto ValidDto(
            int    id         = 1,
            string policyCode = "AP001",
            string policyName = "Test Policy") =>
            new AccessPolicyDto
            {
                Id         = id,
                PolicyCode = policyCode,
                PolicyName = policyName,
                EntityName = "SalesOrder",
                FieldName  = "SalesOrderTypeId",
                IsActive   = true
            };

        public static RoleAccessPolicyDto ValidRoleAccessPolicyDto(
            int id             = 1,
            int accessPolicyId = 1,
            int roleId         = 1,
            int valueId        = 10) =>
            new RoleAccessPolicyDto
            {
                Id             = id,
                AccessPolicyId = accessPolicyId,
                RoleId         = roleId,
                ValueId        = valueId,
                PolicyCode     = "AP001",
                PolicyName     = "Test Policy",
                RoleName       = "Admin"
            };

        public static UserManagement.Domain.Entities.AccessPolicy ValidEntity(int id = 1) =>
            new UserManagement.Domain.Entities.AccessPolicy
            {
                Id         = id,
                PolicyCode = "AP001",
                PolicyName = "Test Policy",
                EntityName = "SalesOrder",
                FieldName  = "SalesOrderTypeId",
                IsActive   = Status.Active,
                IsDeleted  = IsDelete.NotDeleted
            };

        public static UserManagement.Domain.Entities.RoleAccessPolicy ValidRoleEntity(int id = 1) =>
            new UserManagement.Domain.Entities.RoleAccessPolicy
            {
                Id             = id,
                AccessPolicyId = 1,
                RoleId         = 1,
                ValueId        = 10
            };

        public static ApiResponseDTO<int> ValidCreateResponse(int id = 1) =>
            new ApiResponseDTO<int> { IsSuccess = true, Message = "Access Policy created successfully.", Data = id };

        public static ApiResponseDTO<int> ValidUpdateResponse(int id = 1) =>
            new ApiResponseDTO<int> { IsSuccess = true, Message = "Access Policy updated successfully.", Data = id };
    }
}
