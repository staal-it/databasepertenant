using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabasePerTenant.Data.Catalog.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(maxLength: 128, nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerName = table.Column<string>(maxLength: 128, nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "ElasticPools",
                columns: table => new
                {
                    ElasticPoolId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElasticPoolName = table.Column<string>(maxLength: 128, nullable: false),
                    ServerId = table.Column<int>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElasticPools", x => x.ElasticPoolId);
                    table.ForeignKey(
                        name: "FK_ElasticPools_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<int>(maxLength: 128, nullable: false),
                    HashedTenantId = table.Column<byte[]>(nullable: false),
                    TenantName = table.Column<string>(nullable: true),
                    DatabaseName = table.Column<string>(maxLength: 128, nullable: false),
                    ElasticPoolId = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_Tenants_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tenants_ElasticPools_ElasticPoolId",
                        column: x => x.ElasticPoolId,
                        principalTable: "ElasticPools",
                        principalColumn: "ElasticPoolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElasticPools_ServerId",
                table: "ElasticPools",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CustomerId",
                table: "Tenants",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ElasticPoolId",
                table: "Tenants",
                column: "ElasticPoolId");

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerName", "LastUpdated" },
                values: new object[] { "DemoCustomer", "2021-11-15" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ElasticPools");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
