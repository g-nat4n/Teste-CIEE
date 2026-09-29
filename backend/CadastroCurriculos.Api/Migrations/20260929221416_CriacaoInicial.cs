using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadastroCurriculos.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AreaInteresse = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResumoProfissional = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FormacaoAcademica = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Cursos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExperienciasProfissionais = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidatos");
        }
    }
}
