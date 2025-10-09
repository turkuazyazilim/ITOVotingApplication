using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCommitteeIdFromContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cdContacts_cdCommittees_CommitteeId",
                table: "cdContacts");

            migrationBuilder.DropIndex(
                name: "IX_cdContacts_CommitteeId",
                table: "cdContacts");

            migrationBuilder.DropColumn(
                name: "CommitteeId",
                table: "cdContacts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommitteeId",
                table: "cdContacts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdContacts_CommitteeId",
                table: "cdContacts",
                column: "CommitteeId");

            migrationBuilder.AddForeignKey(
                name: "FK_cdContacts_cdCommittees_CommitteeId",
                table: "cdContacts",
                column: "CommitteeId",
                principalTable: "cdCommittees",
                principalColumn: "Id");
        }
    }
}
