using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Validation_Management_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    AuthorCount = table.Column<int>(type: "integer", nullable: false),
                    ReviewerCount = table.Column<int>(type: "integer", nullable: false),
                    ApproverCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentListDependencies",
                columns: table => new
                {
                    DependsOnId = table.Column<int>(type: "integer", nullable: false),
                    DocumentListId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentListDependencies", x => new { x.DependsOnId, x.DocumentListId });
                    table.ForeignKey(
                        name: "FK_DocumentListDependencies_DocumentLists_DependsOnId",
                        column: x => x.DependsOnId,
                        principalTable: "DocumentLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentListDependencies_DocumentLists_DocumentListId",
                        column: x => x.DocumentListId,
                        principalTable: "DocumentLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValidationType = table.Column<int>(type: "integer", nullable: false),
                    ManagerId = table.Column<int>(type: "integer", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComputerSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComputerSystems_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComputerSystems_Employees_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComputerSystems_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    DocumentListId = table.Column<int>(type: "integer", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDocuments_DocumentLists_DocumentListId",
                        column: x => x.DocumentListId,
                        principalTable: "DocumentLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDocuments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectLocations",
                columns: table => new
                {
                    LocationsId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLocations", x => new { x.LocationsId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_ProjectLocations_Locations_LocationsId",
                        column: x => x.LocationsId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectLocations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    ComputerId = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_ComputerSystems_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "ComputerSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComputerSystemProject",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "integer", nullable: false),
                    SystemsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerSystemProject", x => new { x.ProjectsId, x.SystemsId });
                    table.ForeignKey(
                        name: "FK_ComputerSystemProject_ComputerSystems_SystemsId",
                        column: x => x.SystemsId,
                        principalTable: "ComputerSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputerSystemProject_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComputerSystemId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidationHistories_ComputerSystems_ComputerSystemId",
                        column: x => x.ComputerSystemId,
                        principalTable: "ComputerSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ValidationHistories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectDocumentDependencies",
                columns: table => new
                {
                    DependsOnId = table.Column<int>(type: "integer", nullable: false),
                    ProjectDocumentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocumentDependencies", x => new { x.DependsOnId, x.ProjectDocumentId });
                    table.ForeignKey(
                        name: "FK_ProjectDocumentDependencies_ProjectDocuments_DependsOnId",
                        column: x => x.DependsOnId,
                        principalTable: "ProjectDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectDocumentDependencies_ProjectDocuments_ProjectDocumen~",
                        column: x => x.ProjectDocumentId,
                        principalTable: "ProjectDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectDocumentId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTasks_ProjectDocuments_ProjectDocumentId",
                        column: x => x.ProjectDocumentId,
                        principalTable: "ProjectDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contributors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ProjectTaskId = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contributors_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contributors_ProjectTasks_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ComputerId",
                table: "Attachments",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputerSystemProject_SystemsId",
                table: "ComputerSystemProject",
                column: "SystemsId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputerSystems_Code",
                table: "ComputerSystems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComputerSystems_DepartmentId",
                table: "ComputerSystems",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputerSystems_LocationId",
                table: "ComputerSystems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputerSystems_Name",
                table: "ComputerSystems",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComputerSystems_OwnerId",
                table: "ComputerSystems",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_EmployeeId",
                table: "Contributors",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_ProjectTaskId_EmployeeId",
                table: "Contributors",
                columns: new[] { "ProjectTaskId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentListDependencies_DocumentListId",
                table: "DocumentListDependencies",
                column: "DocumentListId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocumentDependencies_ProjectDocumentId",
                table: "ProjectDocumentDependencies",
                column: "ProjectDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_DocumentListId",
                table: "ProjectDocuments",
                column: "DocumentListId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_ProjectId_DocumentListId",
                table: "ProjectDocuments",
                columns: new[] { "ProjectId", "DocumentListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLocations_ProjectId",
                table: "ProjectLocations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ManagerId",
                table: "Projects",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectDocumentId_Type_Number",
                table: "ProjectTasks",
                columns: new[] { "ProjectDocumentId", "Type", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValidationHistories_ComputerSystemId",
                table: "ValidationHistories",
                column: "ComputerSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationHistories_ProjectId",
                table: "ValidationHistories",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ComputerSystemProject");

            migrationBuilder.DropTable(
                name: "Contributors");

            migrationBuilder.DropTable(
                name: "DocumentListDependencies");

            migrationBuilder.DropTable(
                name: "ProjectDocumentDependencies");

            migrationBuilder.DropTable(
                name: "ProjectLocations");

            migrationBuilder.DropTable(
                name: "ValidationHistories");

            migrationBuilder.DropTable(
                name: "ProjectTasks");

            migrationBuilder.DropTable(
                name: "ComputerSystems");

            migrationBuilder.DropTable(
                name: "ProjectDocuments");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "DocumentLists");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
