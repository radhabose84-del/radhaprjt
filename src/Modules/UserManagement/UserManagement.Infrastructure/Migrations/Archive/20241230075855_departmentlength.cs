using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class departmentlength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AlterColumn<string>(
    name: "ShortName",
    schema: "AppData",
    table: "Department",
    type: "varchar(10)",
    maxLength: 10,
    nullable: false,
    oldClrType: typeof(string),
    oldType: "varchar(max)");

migrationBuilder.AlterColumn<string>(
    name: "DeptName",
    schema: "AppData",
    table: "Department",
    type: "varchar(50)",
    maxLength: 50,
    nullable: false,
    oldClrType: typeof(string),
    oldType: "nvarchar(max)");

     migrationBuilder.AlterColumn<bool>(
    name: "IsActive",
    schema: "AppData",
    table: "Department",
    type: "bit",
    nullable: false,
    oldClrType: typeof(int),
    oldType: "tinyint"); 
           

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          
        }
    }
}
