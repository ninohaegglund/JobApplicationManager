using JobApplicationManager.API.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationManager.API.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20260608090000_AddNotificationDeletedAt")]
    public partial class AddNotificationDeletedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Notifications]', N'U') IS NOT NULL
                    AND COL_LENGTH(N'dbo.Notifications', N'DeletedAt') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Notifications]
                    ADD [DeletedAt] datetime2 NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Notifications]', N'U') IS NOT NULL
                    AND COL_LENGTH(N'dbo.Notifications', N'DeletedAt') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Notifications]
                    DROP COLUMN [DeletedAt];
                END
                """);
        }
    }
}
