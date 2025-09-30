using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldReferenceEntitiesFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FieldReferenceCategoryId",
                table: "prUsersRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldReferenceSubCategoryId",
                table: "prUsersRoles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prFieldReferenceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prFieldReferenceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prFieldReferenceSubCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prFieldReferenceSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prFieldReferenceSubCategories_prFieldReferenceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "prFieldReferenceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prUsersRoles_FieldReferenceCategoryId",
                table: "prUsersRoles",
                column: "FieldReferenceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_prUsersRoles_FieldReferenceSubCategoryId",
                table: "prUsersRoles",
                column: "FieldReferenceSubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldReferenceCategory_CategoryName",
                table: "prFieldReferenceCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldReferenceSubCategory_CategoryId_SubCategoryName",
                table: "prFieldReferenceSubCategories",
                columns: new[] { "CategoryId", "SubCategoryName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_prUsersRoles_prFieldReferenceCategories_FieldReferenceCategoryId",
                table: "prUsersRoles",
                column: "FieldReferenceCategoryId",
                principalTable: "prFieldReferenceCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_prUsersRoles_prFieldReferenceSubCategories_FieldReferenceSubCategoryId",
                table: "prUsersRoles",
                column: "FieldReferenceSubCategoryId",
                principalTable: "prFieldReferenceSubCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prUsersRoles_prFieldReferenceCategories_FieldReferenceCategoryId",
                table: "prUsersRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_prUsersRoles_prFieldReferenceSubCategories_FieldReferenceSubCategoryId",
                table: "prUsersRoles");

            migrationBuilder.DropTable(
                name: "prFieldReferenceSubCategories");

            migrationBuilder.DropTable(
                name: "prFieldReferenceCategories");

            migrationBuilder.DropIndex(
                name: "IX_prUsersRoles_FieldReferenceCategoryId",
                table: "prUsersRoles");

            migrationBuilder.DropIndex(
                name: "IX_prUsersRoles_FieldReferenceSubCategoryId",
                table: "prUsersRoles");

            migrationBuilder.DropColumn(
                name: "FieldReferenceCategoryId",
                table: "prUsersRoles");

            migrationBuilder.DropColumn(
                name: "FieldReferenceSubCategoryId",
                table: "prUsersRoles");
        }
    }
}
