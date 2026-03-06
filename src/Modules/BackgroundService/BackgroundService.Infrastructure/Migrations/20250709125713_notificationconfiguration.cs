using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class notificationconfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AppData");

            migrationBuilder.EnsureSchema(
                name: "AppNotification");

            migrationBuilder.CreateTable(
                name: "MiscTypeMaster",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MiscTypeCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscTypeMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationGroup",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "Varchar(250)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiscMaster",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MiscTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    description = table.Column<string>(type: "varchar(250)", nullable: false),
                    sortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MiscMaster_MiscTypeMaster_MiscTypeId",
                        column: x => x.MiscTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationGroupMembers",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationGroupMembers_NotificationGroup_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "AppNotification",
                        principalTable: "NotificationGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationConfig",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleName = table.Column<string>(type: "Varchar(250)", nullable: false),
                    NotificationEventTypeId = table.Column<int>(type: "int", nullable: false),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationConfig_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationConfig_MiscMaster_NotificationEventTypeId",
                        column: x => x.NotificationEventTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false),
                    ModuleName = table.Column<string>(type: "Varchar(250)", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "NVarchar(Max)", nullable: false),
                    BodyTemplate = table.Column<string>(type: "NVarchar(Max)", nullable: false),
                    LanguageCode = table.Column<string>(type: "Varchar(50)", nullable: false),
                    FooterTemplate = table.Column<string>(type: "NVarchar(Max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplate_MiscMaster_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationEventRule",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationGroupMemberId = table.Column<int>(type: "int", nullable: false),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false),
                    NotificationStatusId = table.Column<int>(type: "int", nullable: false),
                    NotificationConfigId = table.Column<int>(type: "int", nullable: false),
                    NotificationGroupId = table.Column<int>(type: "int", nullable: false),
                    RecipientTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationEventRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationEventRule_MiscMaster_NotificationStatusId",
                        column: x => x.NotificationStatusId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationEventRule_MiscMaster_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationEventRule_MiscMaster_RecipientTypeId",
                        column: x => x.RecipientTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationEventRule_NotificationConfig_NotificationConfigId",
                        column: x => x.NotificationConfigId,
                        principalSchema: "AppNotification",
                        principalTable: "NotificationConfig",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                        column: x => x.NotificationGroupId,
                        principalSchema: "AppNotification",
                        principalTable: "NotificationGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLevelHierarchy",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationConfigId = table.Column<int>(type: "int", nullable: false),
                    TargetTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ApprovalModeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "Varchar(Max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLevelHierarchy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLevelHierarchy_MiscMaster_ApprovalModeId",
                        column: x => x.ApprovalModeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationLevelHierarchy_MiscMaster_TargetTypeId",
                        column: x => x.TargetTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationLevelHierarchy_NotificationConfig_NotificationConfigId",
                        column: x => x.NotificationConfigId,
                        principalSchema: "AppNotification",
                        principalTable: "NotificationConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationEventLog",
                schema: "AppNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationLevelHierarchyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "Varchar(250)", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    MessageText = table.Column<string>(type: "Varchar(Max)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationEventLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationEventLog_MiscMaster_ChannelId",
                        column: x => x.ChannelId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationEventLog_NotificationLevelHierarchy_NotificationLevelHierarchyId",
                        column: x => x.NotificationLevelHierarchyId,
                        principalSchema: "AppNotification",
                        principalTable: "NotificationLevelHierarchy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MiscMaster_MiscTypeId",
                schema: "AppData",
                table: "MiscMaster",
                column: "MiscTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationConfig_MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationConfig_NotificationEventTypeId",
                schema: "AppNotification",
                table: "NotificationConfig",
                column: "NotificationEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventLog_ChannelId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventLog_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationLevelHierarchyId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationTypeId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_RecipientTypeId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "RecipientTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationGroupMembers_GroupId",
                schema: "AppNotification",
                table: "NotificationGroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLevelHierarchy_ApprovalModeId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                column: "ApprovalModeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLevelHierarchy_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                column: "NotificationConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLevelHierarchy_TargetTypeId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                column: "TargetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_NotificationTypeId",
                schema: "AppNotification",
                table: "NotificationTemplate",
                column: "NotificationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationEventLog",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "NotificationEventRule",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "NotificationGroupMembers",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "NotificationTemplate",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "NotificationLevelHierarchy",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "NotificationGroup",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "NotificationConfig",
                schema: "AppNotification");

            migrationBuilder.DropTable(
                name: "MiscMaster",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "MiscTypeMaster",
                schema: "AppData");
        }
    }
}
