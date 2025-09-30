using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInvitationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trUserInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trUserInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trUserInvitations_cdUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "cdUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trUserInvitations_cdUsers_UsedByUserId",
                        column: x => x.UsedByUserId,
                        principalTable: "cdUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_CreatedByUserId",
                table: "trUserInvitations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_CreatedDate",
                table: "trUserInvitations",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_ExpiryDate",
                table: "trUserInvitations",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_Token",
                table: "trUserInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trUserInvitations_UsedByUserId",
                table: "trUserInvitations",
                column: "UsedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trUserInvitations");
        }
    }
}
