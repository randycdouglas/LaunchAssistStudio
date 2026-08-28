using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaunchAssistStudio.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIntakeFieldsAndLaunchDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EcommercePaymentProvider",
                table: "Leads",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EcommerceTaxes",
                table: "Leads",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoftwareMigrationNeeds",
                table: "Leads",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TargetLaunchDate",
                table: "Leads",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EcommercePaymentProvider",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EcommerceTaxes",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SoftwareMigrationNeeds",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TargetLaunchDate",
                table: "Leads");
        }
    }
}
