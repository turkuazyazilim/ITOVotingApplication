using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedContactToDocumentTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedContactId",
                table: "trCompanyDocumentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trCompanyDocumentTransactions_AssignedContactId",
                table: "trCompanyDocumentTransactions",
                column: "AssignedContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_trCompanyDocumentTransactions_cdContacts_AssignedContactId",
                table: "trCompanyDocumentTransactions",
                column: "AssignedContactId",
                principalTable: "cdContacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trCompanyDocumentTransactions_cdContacts_AssignedContactId",
                table: "trCompanyDocumentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_trCompanyDocumentTransactions_AssignedContactId",
                table: "trCompanyDocumentTransactions");

            migrationBuilder.DropColumn(
                name: "AssignedContactId",
                table: "trCompanyDocumentTransactions");
        }
    }
}
