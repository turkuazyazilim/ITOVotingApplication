using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAssignedContactWithAssignedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First try to drop FK if exists (may have been dropped in failed migration)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_trCompanyDocumentTransactions_cdContacts_AssignedContactId')
                BEGIN
                    ALTER TABLE [trCompanyDocumentTransactions] DROP CONSTRAINT [FK_trCompanyDocumentTransactions_cdContacts_AssignedContactId];
                END
            ");

            // Check if column needs to be renamed (from failed migration)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('trCompanyDocumentTransactions') AND name = 'AssignedContactId')
                BEGIN
                    EXEC sp_rename N'[trCompanyDocumentTransactions].[AssignedContactId]', N'AssignedUserId', 'COLUMN';
                END
            ");

            // Check if index needs to be renamed
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_trCompanyDocumentTransactions_AssignedContactId')
                BEGIN
                    EXEC sp_rename N'[trCompanyDocumentTransactions].[IX_trCompanyDocumentTransactions_AssignedContactId]', N'IX_trCompanyDocumentTransactions_AssignedUserId', 'INDEX';
                END
            ");

            // Clear existing values (Contact IDs are not valid User IDs)
            migrationBuilder.Sql("UPDATE [trCompanyDocumentTransactions] SET [AssignedUserId] = NULL");

            // Add FK to Users table
            migrationBuilder.AddForeignKey(
                name: "FK_trCompanyDocumentTransactions_cdUsers_AssignedUserId",
                table: "trCompanyDocumentTransactions",
                column: "AssignedUserId",
                principalTable: "cdUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trCompanyDocumentTransactions_cdUsers_AssignedUserId",
                table: "trCompanyDocumentTransactions");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                table: "trCompanyDocumentTransactions",
                newName: "AssignedContactId");

            migrationBuilder.RenameIndex(
                name: "IX_trCompanyDocumentTransactions_AssignedUserId",
                table: "trCompanyDocumentTransactions",
                newName: "IX_trCompanyDocumentTransactions_AssignedContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_trCompanyDocumentTransactions_cdContacts_AssignedContactId",
                table: "trCompanyDocumentTransactions",
                column: "AssignedContactId",
                principalTable: "cdContacts",
                principalColumn: "Id");
        }
    }
}
