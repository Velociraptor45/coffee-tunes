using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeTunes.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Franchises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hipsters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hipsters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    MaxIngredientsPerHipster = table.Column<long>(type: "bigint", nullable: false),
                    HasSupplyLeft = table.Column<bool>(type: "boolean", nullable: false),
                    FranchiseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bars_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HipstersInFranchises",
                columns: table => new
                {
                    FranchiseId = table.Column<Guid>(type: "uuid", nullable: false),
                    HipsterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HipstersInFranchises", x => new { x.FranchiseId, x.HipsterId });
                    table.ForeignKey(
                        name: "FK_HipstersInFranchises_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HipstersInFranchises_Hipsters_HipsterId",
                        column: x => x.HipsterId,
                        principalTable: "Hipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarId = table.Column<Guid>(type: "uuid", nullable: false),
                    FranchiseId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientsBrewedCount = table.Column<int>(type: "integer", nullable: false),
                    BeansCastCount = table.Column<int>(type: "integer", nullable: false),
                    CorrectCastsCount = table.Column<int>(type: "integer", nullable: false),
                    TotalHipstersContributedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarStatistics_Bars_BarId",
                        column: x => x.BarId,
                        principalTable: "Bars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarStatistics_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: false),
                    Selected = table.Column<bool>(type: "boolean", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false),
                    Revealed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_Bars_BarId",
                        column: x => x.BarId,
                        principalTable: "Bars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarHipsterStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarStatisticsId = table.Column<Guid>(type: "uuid", nullable: false),
                    BarId = table.Column<Guid>(type: "uuid", nullable: false),
                    HipsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectGuesses = table.Column<int>(type: "integer", nullable: false),
                    TotalGuesses = table.Column<int>(type: "integer", nullable: false),
                    IngredientsSubmitted = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarHipsterStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarHipsterStatistics_BarStatistics_BarStatisticsId",
                        column: x => x.BarStatisticsId,
                        principalTable: "BarStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarHipsterStatistics_Bars_BarId",
                        column: x => x.BarId,
                        principalTable: "Bars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarHipsterStatistics_Hipsters_HipsterId",
                        column: x => x.HipsterId,
                        principalTable: "Hipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CastFromId = table.Column<Guid>(type: "uuid", nullable: false),
                    CastToId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beans_Hipsters_CastFromId",
                        column: x => x.CastFromId,
                        principalTable: "Hipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Beans_Hipsters_CastToId",
                        column: x => x.CastToId,
                        principalTable: "Hipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Beans_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HipstersCastIngredientBeans",
                columns: table => new
                {
                    HipsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HipstersCastIngredientBeans", x => new { x.HipsterId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_HipstersCastIngredientBeans_Hipsters_HipsterId",
                        column: x => x.HipsterId,
                        principalTable: "Hipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HipstersCastIngredientBeans_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HipstersSubmittedIngredients",
                columns: table => new
                {
                    HipsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HipstersSubmittedIngredients", x => new { x.HipsterId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_HipstersSubmittedIngredients_Hipsters_HipsterId",
                        column: x => x.HipsterId,
                        principalTable: "Hipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HipstersSubmittedIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarHipsterStatistics_BarId",
                table: "BarHipsterStatistics",
                column: "BarId");

            migrationBuilder.CreateIndex(
                name: "IX_BarHipsterStatistics_BarStatisticsId",
                table: "BarHipsterStatistics",
                column: "BarStatisticsId");

            migrationBuilder.CreateIndex(
                name: "IX_BarHipsterStatistics_HipsterId",
                table: "BarHipsterStatistics",
                column: "HipsterId");

            migrationBuilder.CreateIndex(
                name: "IX_Bars_FranchiseId",
                table: "Bars",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_BarStatistics_BarId",
                table: "BarStatistics",
                column: "BarId");

            migrationBuilder.CreateIndex(
                name: "IX_BarStatistics_FranchiseId",
                table: "BarStatistics",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Beans_CastFromId",
                table: "Beans",
                column: "CastFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Beans_CastToId",
                table: "Beans",
                column: "CastToId");

            migrationBuilder.CreateIndex(
                name: "IX_Beans_IngredientId",
                table: "Beans",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Franchises_Name",
                table: "Franchises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HipstersCastIngredientBeans_IngredientId",
                table: "HipstersCastIngredientBeans",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_HipstersInFranchises_HipsterId",
                table: "HipstersInFranchises",
                column: "HipsterId");

            migrationBuilder.CreateIndex(
                name: "IX_HipstersSubmittedIngredients_IngredientId",
                table: "HipstersSubmittedIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_BarId",
                table: "Ingredients",
                column: "BarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarHipsterStatistics");

            migrationBuilder.DropTable(
                name: "Beans");

            migrationBuilder.DropTable(
                name: "HipstersCastIngredientBeans");

            migrationBuilder.DropTable(
                name: "HipstersInFranchises");

            migrationBuilder.DropTable(
                name: "HipstersSubmittedIngredients");

            migrationBuilder.DropTable(
                name: "BarStatistics");

            migrationBuilder.DropTable(
                name: "Hipsters");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Bars");

            migrationBuilder.DropTable(
                name: "Franchises");
        }
    }
}
