using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadastroCurriculos.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateEducationAndExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cursos",
                table: "Candidates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienciasProfissionais",
                table: "Candidates",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormacaoAcademica",
                table: "Candidates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cursos",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ExperienciasProfissionais",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "FormacaoAcademica",
                table: "Candidates");
        }
    }
}
