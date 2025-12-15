using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendPatientProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNumber",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceProvider",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicalNotes",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Patients",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceProvider",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "MedicalNotes",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Patients");
        }
    }
}
