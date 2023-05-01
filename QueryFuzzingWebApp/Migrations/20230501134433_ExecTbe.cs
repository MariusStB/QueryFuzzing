using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueryFuzzingWebApp.Migrations
{
    /// <inheritdoc />
    public partial class ExecTbe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.CreateTable(
                name: "Executables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Executables_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            

            migrationBuilder.CreateIndex(
                name: "IX_Executables_ProjectId",
                table: "Executables",
                column: "ProjectId");
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Executables");
           
        }
    }
}
