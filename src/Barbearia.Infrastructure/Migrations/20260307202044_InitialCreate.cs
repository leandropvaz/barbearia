using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Barbearia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Barbeiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgendaAberta = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barbeiros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloquiosAgenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloquiosAgenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloquiosAgenda_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorariosBarbeiro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time", nullable: false),
                    Disponivel = table.Column<bool>(type: "bit", nullable: false),
                    IntervaloMinutos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosBarbeiro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosBarbeiro_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarbeiroServicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarbeiroServicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarbeiroServicos_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarbeiroServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarbeiroId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    ClienteNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClienteEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClienteTelefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CodigoConfirmacao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Barbeiros_BarbeiroId",
                        column: x => x.BarbeiroId,
                        principalTable: "Barbeiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Barbeiros",
                columns: new[] { "Id", "AgendaAberta", "Ativo", "DataCriacao", "Email", "FotoUrl", "Nome", "SenhaHash", "Telefone" },
                values: new object[] { 1, true, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@barbearia.com", null, "Admin", "$2a$11$pAygl7EL7khL69q5Hd8XDukQOKMO6mWqPsnTTTH1bmdFXMB1OQao2", "(11) 99999-0000" });

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Descricao", "DuracaoMinutos", "Nome", "Preco" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Corte masculino completo", 30, "Corte de Cabelo", 45.00m },
                    { 2, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aparação e modelagem de barba", 20, "Barba", 30.00m },
                    { 3, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Corte completo + barba", 50, "Corte + Barba", 65.00m },
                    { 4, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Design de sobrancelha masculina", 15, "Sobrancelha", 15.00m }
                });

            migrationBuilder.InsertData(
                table: "HorariosBarbeiro",
                columns: new[] { "Id", "BarbeiroId", "DiaSemana", "Disponivel", "HoraFim", "HoraInicio", "IntervaloMinutos" },
                values: new object[,]
                {
                    { 1, 1, 1, true, new TimeSpan(0, 19, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 30 },
                    { 2, 1, 2, true, new TimeSpan(0, 19, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 30 },
                    { 3, 1, 3, true, new TimeSpan(0, 19, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 30 },
                    { 4, 1, 4, true, new TimeSpan(0, 19, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 30 },
                    { 5, 1, 5, true, new TimeSpan(0, 19, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 30 },
                    { 6, 1, 6, true, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 30 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barbeiros_Email",
                table: "Barbeiros",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarbeiroServicos_BarbeiroId_ServicoId",
                table: "BarbeiroServicos",
                columns: new[] { "BarbeiroId", "ServicoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarbeiroServicos_ServicoId",
                table: "BarbeiroServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_BloquiosAgenda_BarbeiroId",
                table: "BloquiosAgenda",
                column: "BarbeiroId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosBarbeiro_BarbeiroId",
                table: "HorariosBarbeiro",
                column: "BarbeiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_BarbeiroId",
                table: "Reservas",
                column: "BarbeiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ServicoId",
                table: "Reservas",
                column: "ServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarbeiroServicos");

            migrationBuilder.DropTable(
                name: "BloquiosAgenda");

            migrationBuilder.DropTable(
                name: "HorariosBarbeiro");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Barbeiros");

            migrationBuilder.DropTable(
                name: "Servicos");
        }
    }
}
