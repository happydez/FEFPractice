using AutoLot.Dal.EfStructures;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FEFPractice.AutoLot.Dal.EfStructures.Migrations
{
    public partial class SQL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationHelpers.CreateSproc(migrationBuilder);
            MigrationHelpers.CreateCustomerOrderView(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            MigrationHelpers.DropSproc(migrationBuilder);
            MigrationHelpers.DropCustomerOrderView(migrationBuilder);
        }
    }
}