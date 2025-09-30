using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cdBallotBox",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BallotBoxDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdBallotBox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cdCommittee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommitteeDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdCommittee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cdRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cdUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prUsersRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prUsersRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prUsersRoles_cdRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "cdRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prUsersRoles_cdUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "cdUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cdCompany",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TradeRegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProfessionalGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfficePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MobilePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActiveContactId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Has2022AuthorizationCertificate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdCompany", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cdContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AuthorizationType = table.Column<int>(type: "int", nullable: false),
                    CommitteeId = table.Column<int>(type: "int", nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdentityNum = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    EligibleToVote = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cdContacts_cdCommittee_CommitteeId",
                        column: x => x.CommitteeId,
                        principalTable: "cdCommittee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_cdContacts_cdCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "cdCompany",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trVoteTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    BallotBoxId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trVoteTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trVoteTransactions_cdBallotBox_BallotBoxId",
                        column: x => x.BallotBoxId,
                        principalTable: "cdBallotBox",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trVoteTransactions_cdCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "cdCompany",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trVoteTransactions_cdContacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "cdContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trVoteTransactions_cdUsers_CreatedUserId",
                        column: x => x.CreatedUserId,
                        principalTable: "cdUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cdCompany_ActiveContactId",
                table: "cdCompany",
                column: "ActiveContactId");

            migrationBuilder.CreateIndex(
                name: "IX_cdCompany_RegistrationNumber",
                table: "cdCompany",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdContacts_CommitteeId",
                table: "cdContacts",
                column: "CommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_cdContacts_CompanyId_IdentityNum",
                table: "cdContacts",
                columns: new[] { "CompanyId", "IdentityNum" });

            migrationBuilder.CreateIndex(
                name: "IX_cdContacts_IdentityNum",
                table: "cdContacts",
                column: "IdentityNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdUsers_Email",
                table: "cdUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdUsers_UserName",
                table: "cdUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prUsersRoles_RoleId",
                table: "prUsersRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_prUsersRoles_UserId_RoleId",
                table: "prUsersRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trVoteTransactions_BallotBoxId",
                table: "trVoteTransactions",
                column: "BallotBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_trVoteTransactions_CompanyId",
                table: "trVoteTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_trVoteTransactions_ContactId_BallotBoxId",
                table: "trVoteTransactions",
                columns: new[] { "ContactId", "BallotBoxId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trVoteTransactions_CreatedUserId",
                table: "trVoteTransactions",
                column: "CreatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_cdCompany_cdContacts_ActiveContactId",
                table: "cdCompany",
                column: "ActiveContactId",
                principalTable: "cdContacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cdCompany_cdContacts_ActiveContactId",
                table: "cdCompany");

            migrationBuilder.DropTable(
                name: "prUsersRoles");

            migrationBuilder.DropTable(
                name: "trVoteTransactions");

            migrationBuilder.DropTable(
                name: "cdRoles");

            migrationBuilder.DropTable(
                name: "cdBallotBox");

            migrationBuilder.DropTable(
                name: "cdUsers");

            migrationBuilder.DropTable(
                name: "cdContacts");

            migrationBuilder.DropTable(
                name: "cdCommittee");

            migrationBuilder.DropTable(
                name: "cdCompany");
        }
    }
}
