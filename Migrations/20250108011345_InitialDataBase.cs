using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pesticide.Migrations
{
    /// <inheritdoc />
    public partial class InitialDataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pesticide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermitNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChineseName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PesticideCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChemicalComposition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForeignMaker = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    formCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpireDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FRAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HRAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IRAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevocationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevocationDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScopeOfUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GetFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GetBrcodeXML = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GetGropty = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesticide", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Register",
                columns: table => new
                {
                    RegtID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Permit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Register", x => x.RegtID);
                });

            migrationBuilder.CreateTable(
                name: "ThirdParty",
                columns: table => new
                {
                    DataStatus = table.Column<bool>(type: "bit", nullable: false),
                    DataTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pesticide");

            migrationBuilder.DropTable(
                name: "Register");

            migrationBuilder.DropTable(
                name: "ThirdParty");
        }
    }
}
