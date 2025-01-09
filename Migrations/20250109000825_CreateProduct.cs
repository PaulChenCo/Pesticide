using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pesticide.Migrations
{
    /// <inheritdoc />
    public partial class CreateProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Barcode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Seq = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Spec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Volume = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDT = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Barcode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Product");
        }
    }
}
