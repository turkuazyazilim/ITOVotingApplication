using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyDocumentTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trCompanyDocumentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: true),
                    DeliveryStatusDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<int>(type: "int", nullable: true),
                    RejectionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WillParticipateInElection = table.Column<bool>(type: "bit", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trCompanyDocumentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trCompanyDocumentTransactions_cdCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "cdCompany",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_trCompanyDocumentTransactions_cdUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "cdUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trCompanyDocumentTransactions_CompanyId",
                table: "trCompanyDocumentTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_trCompanyDocumentTransactions_CompanyId_DocumentType",
                table: "trCompanyDocumentTransactions",
                columns: new[] { "CompanyId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_trCompanyDocumentTransactions_UploadedByUserId",
                table: "trCompanyDocumentTransactions",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trCompanyDocumentTransactions");
        }
    }
}
