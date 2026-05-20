using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO customers ("Id", "Name")
                VALUES
                    (1, 'André'),
                    (2, 'José'),
                    (3, 'Maria')
                ON CONFLICT ("Id") DO NOTHING;

                SELECT setval(
                    pg_get_serial_sequence('customers', 'Id'),
                    GREATEST((SELECT MAX("Id") FROM customers), 1),
                    true);
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM customers
                WHERE "Id" IN (1, 2, 3)
                  AND "Name" IN ('André', 'José', 'Maria');
                """);

        }
    }
}
