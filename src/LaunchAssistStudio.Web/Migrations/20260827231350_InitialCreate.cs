using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaunchAssistStudio.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServicesRequested = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentWebsite = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BusinessDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    EcommerceSellType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EcommerceProductCount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EcommerceExistingPlatform = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EcommerceInventoryNeeds = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EcommerceShipping = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EcommerceSubscriptions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EcommerceIntegrations = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    EcommerceMigration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareApplicationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareNewOrExisting = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareCurrentTechnology = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    SoftwareLoginRequirements = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareIntegrations = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    SoftwareDataMigration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoftwareBusinessProblem = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Budget = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timeline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreferredContact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastContactedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConvertedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadNotes_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadStatusHistory_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeadNotes_LeadId",
                table: "LeadNotes",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Email",
                table: "Leads",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PublicId",
                table: "Leads",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Status",
                table: "Leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_SubmittedAtUtc",
                table: "Leads",
                column: "SubmittedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStatusHistory_LeadId",
                table: "LeadStatusHistory",
                column: "LeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadNotes");

            migrationBuilder.DropTable(
                name: "LeadStatusHistory");

            migrationBuilder.DropTable(
                name: "Leads");
        }
    }
}
