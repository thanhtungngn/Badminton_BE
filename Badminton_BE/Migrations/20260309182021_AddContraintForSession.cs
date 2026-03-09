using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Badminton_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddContraintForSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Sessions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Sessions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxPlayerPerCourt",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfCourts",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Sessions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Sessions",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "MaxPlayerPerCourt",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "NumberOfCourts",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Sessions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
