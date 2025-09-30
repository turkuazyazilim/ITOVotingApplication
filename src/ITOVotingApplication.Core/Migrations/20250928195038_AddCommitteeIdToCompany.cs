using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitteeIdToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommitteeId",
                table: "cdCompany",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdCompany_CommitteeId",
                table: "cdCompany",
                column: "CommitteeId");

            migrationBuilder.AddForeignKey(
                name: "FK_cdCompany_cdCommittee_CommitteeId",
                table: "cdCompany",
                column: "CommitteeId",
                principalTable: "cdCommittee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cdCompany_cdCommittee_CommitteeId",
                table: "cdCompany");

            migrationBuilder.DropIndex(
                name: "IX_cdCompany_CommitteeId",
                table: "cdCompany");

            migrationBuilder.DropColumn(
                name: "CommitteeId",
                table: "cdCompany");
        }
    }
}
