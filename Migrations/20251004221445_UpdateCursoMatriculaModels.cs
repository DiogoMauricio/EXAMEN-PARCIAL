using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace examen_parcial.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCursoMatriculaModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaRegistro",
                table: "Matriculas",
                newName: "FechaMatricula");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Cursos",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Cursos");

            migrationBuilder.RenameColumn(
                name: "FechaMatricula",
                table: "Matriculas",
                newName: "FechaRegistro");
        }
    }
}
