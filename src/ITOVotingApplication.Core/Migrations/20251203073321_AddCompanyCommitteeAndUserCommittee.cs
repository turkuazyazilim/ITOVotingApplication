using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCommitteeAndUserCommittee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommitteeId",
                table: "cdCompany",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prUserCommittee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommitteeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prUserCommittee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prUserCommittee_cdCommittee_CommitteeId",
                        column: x => x.CommitteeId,
                        principalTable: "cdCommittee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prUserCommittee_cdUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "cdUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cdCompany_CommitteeId",
                table: "cdCompany",
                column: "CommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_prUserCommittee_CommitteeId",
                table: "prUserCommittee",
                column: "CommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_prUserCommittee_UserId_CommitteeId",
                table: "prUserCommittee",
                columns: new[] { "UserId", "CommitteeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cdCompany_cdCommittee_CommitteeId",
                table: "cdCompany",
                column: "CommitteeId",
                principalTable: "cdCommittee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cdCompany_cdCommittee_CommitteeId",
                table: "cdCompany");

            migrationBuilder.DropTable(
                name: "prUserCommittee");

            migrationBuilder.DropIndex(
                name: "IX_cdCompany_CommitteeId",
                table: "cdCompany");

            migrationBuilder.DropColumn(
                name: "CommitteeId",
                table: "cdCompany");
        }
    }
}
