using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Infrastructure.Migrations
{
    public partial class AddAlmocoHorario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AlmocoFim",
                table: "HorariosBarbeiro",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AlmocoInicio",
                table: "HorariosBarbeiro",
                type: "time",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AlmocoFim", table: "HorariosBarbeiro");
            migrationBuilder.DropColumn(name: "AlmocoInicio", table: "HorariosBarbeiro");
        }
    }
}
