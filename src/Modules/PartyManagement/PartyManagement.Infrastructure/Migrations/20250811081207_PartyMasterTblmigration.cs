using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyMasterTblmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartyMaster",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PartyCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PartyName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PartyZoneId = table.Column<int>(type: "int", nullable: true),
                    RegistrationTypeId = table.Column<int>(type: "int", nullable: false),
                    GSTNumber = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    GSTStateCode = table.Column<int>(type: "int", nullable: true),
                    PAN = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    TAN = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    TDSCategoryId = table.Column<int>(type: "int", nullable: true),
                    MSMETypeId = table.Column<int>(type: "int", nullable: true),
                    MSMENO = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    MSMEValidUpto = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsMsmeCompliant = table.Column<bool>(type: "bit", nullable: false),
                    IsTDSApplicable = table.Column<bool>(type: "bit", nullable: false),
                    IsTCSApplicable = table.Column<bool>(type: "bit", nullable: false),
                    IsGstReverseCharge = table.Column<bool>(type: "bit", nullable: false),
                    Is206AB206CCAApplicable = table.Column<bool>(type: "bit", nullable: false),
                    PayementModeId = table.Column<int>(type: "int", nullable: true),
                    FavourOf = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    PreferredCurrencyPurchase = table.Column<int>(type: "int", nullable: true),
                    CreditDays = table.Column<int>(type: "int", nullable: true),
                    DueDateTypeId = table.Column<int>(type: "int", nullable: true),
                    LeadTime = table.Column<int>(type: "int", nullable: true),
                    PreferredCurrencySale = table.Column<int>(type: "int", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    SellingPriceListId = table.Column<int>(type: "int", nullable: true),
                    CustomerTypeId = table.Column<int>(type: "int", nullable: true),
                    IsInternalSupplier = table.Column<bool>(type: "bit", nullable: false),
                    IsInternalCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsStopPayment = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_PartyMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyMaster_MiscMaster_CustomerTypeId",
                        column: x => x.CustomerTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyMaster_MiscMaster_DueDateTypeId",
                        column: x => x.DueDateTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyMaster_MiscMaster_MSMETypeId",
                        column: x => x.MSMETypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyMaster_MiscMaster_PartyZoneId",
                        column: x => x.PartyZoneId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyMaster_MiscMaster_PayementModeId",
                        column: x => x.PayementModeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyMaster_MiscMaster_RegistrationTypeId",
                        column: x => x.RegistrationTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartyAddress",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(25)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    AddressLine4 = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    AddressLine5 = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    AddressLine6 = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyAddress_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartyBank",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(25)", nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(25)", nullable: true),
                    IFSCCode = table.Column<string>(type: "nvarchar(25)", nullable: true),
                    SWIFTCode = table.Column<string>(type: "nvarchar(25)", nullable: true),
                    AccountTypeId = table.Column<int>(type: "int", nullable: true),
                    IsDefaultAccount = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryAccount = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyBank", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyBank_MiscMaster_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyBank_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartyContact",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    GSTNumber = table.Column<string>(type: "nvarchar(25)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(25)", nullable: true),
                    GenderId = table.Column<int>(type: "int", nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    EmailID = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PreferredChannelId = table.Column<int>(type: "int", nullable: true),
                    ContactTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyContact_MiscMaster_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyContact_MiscMaster_GenderId",
                        column: x => x.GenderId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyContact_MiscMaster_PreferredChannelId",
                        column: x => x.PreferredChannelId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyContact_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartyDocument",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    UploadedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyDocument_MiscMaster_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyDocument_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartyType",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    PartyTypeId = table.Column<int>(type: "int", nullable: false),
                    PartyGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyType_MiscMaster_PartyTypeId",
                        column: x => x.PartyTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyType_PartyGroup_PartyGroupId",
                        column: x => x.PartyGroupId,
                        principalSchema: "Party",
                        principalTable: "PartyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyType_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartyAddress_PartyId",
                schema: "Party",
                table: "PartyAddress",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyBank_AccountTypeId",
                schema: "Party",
                table: "PartyBank",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyBank_PartyId",
                schema: "Party",
                table: "PartyBank",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ContactTypeId",
                schema: "Party",
                table: "PartyContact",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_GenderId",
                schema: "Party",
                table: "PartyContact",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_PartyId",
                schema: "Party",
                table: "PartyContact",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_PreferredChannelId",
                schema: "Party",
                table: "PartyContact",
                column: "PreferredChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyDocument_DocumentId",
                schema: "Party",
                table: "PartyDocument",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyDocument_PartyId",
                schema: "Party",
                table: "PartyDocument",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_CustomerTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "CustomerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_DueDateTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "DueDateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_MSMETypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "MSMETypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_PartyZoneId",
                schema: "Party",
                table: "PartyMaster",
                column: "PartyZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_PayementModeId",
                schema: "Party",
                table: "PartyMaster",
                column: "PayementModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_RegistrationTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "RegistrationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyType_PartyGroupId",
                schema: "Party",
                table: "PartyType",
                column: "PartyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyType_PartyId",
                schema: "Party",
                table: "PartyType",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyType_PartyTypeId",
                schema: "Party",
                table: "PartyType",
                column: "PartyTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyAddress",
                schema: "Party");

            migrationBuilder.DropTable(
                name: "PartyBank",
                schema: "Party");

            migrationBuilder.DropTable(
                name: "PartyContact",
                schema: "Party");

            migrationBuilder.DropTable(
                name: "PartyDocument",
                schema: "Party");

            migrationBuilder.DropTable(
                name: "PartyType",
                schema: "Party");

            migrationBuilder.DropTable(
                name: "PartyMaster",
                schema: "Party");
        }
    }
}
