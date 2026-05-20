using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO products ("Id", "Name", "UnitPrice", "AvailableQuantity")
                VALUES
                    (1, 'Notebook', 3500.00, 10),
                    (2, 'Keyboard', 150.00, 50),
                    (3, 'Mouse', 80.00, 100)
                ON CONFLICT ("Id") DO NOTHING;

                SELECT setval(
                    pg_get_serial_sequence('products', 'Id'),
                    GREATEST((SELECT MAX("Id") FROM products), 1),
                    true);
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM products
                WHERE "Id" IN (1, 2, 3)
                  AND "Name" IN ('Notebook', 'Keyboard', 'Mouse');
                """);

        }
    }
}
