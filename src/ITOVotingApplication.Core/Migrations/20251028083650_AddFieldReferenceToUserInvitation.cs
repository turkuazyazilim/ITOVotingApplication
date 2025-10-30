using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldReferenceToUserInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FieldReferenceCategoryId",
                table: "trUserInvitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldReferenceSubCategoryId",
                table: "trUserInvitations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_FieldReferenceCategoryId",
                table: "trUserInvitations",
                column: "FieldReferenceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_FieldReferenceSubCategoryId",
                table: "trUserInvitations",
                column: "FieldReferenceSubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_trUserInvitations_prFieldReferenceCategories_FieldReferenceCategoryId",
                table: "trUserInvitations",
                column: "FieldReferenceCategoryId",
                principalTable: "prFieldReferenceCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trUserInvitations_prFieldReferenceSubCategories_FieldReferenceSubCategoryId",
                table: "trUserInvitations",
                column: "FieldReferenceSubCategoryId",
                principalTable: "prFieldReferenceSubCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trUserInvitations_prFieldReferenceCategories_FieldReferenceCategoryId",
                table: "trUserInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_trUserInvitations_prFieldReferenceSubCategories_FieldReferenceSubCategoryId",
                table: "trUserInvitations");

            migrationBuilder.DropIndex(
                name: "IX_trUserInvitations_FieldReferenceCategoryId",
                table: "trUserInvitations");

            migrationBuilder.DropIndex(
                name: "IX_trUserInvitations_FieldReferenceSubCategoryId",
                table: "trUserInvitations");

            migrationBuilder.DropColumn(
                name: "FieldReferenceCategoryId",
                table: "trUserInvitations");

            migrationBuilder.DropColumn(
                name: "FieldReferenceSubCategoryId",
                table: "trUserInvitations");
        }
    }
}
