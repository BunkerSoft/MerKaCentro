using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerkaCentro.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCurrencyPenToCop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all existing currency data from PEN to COP
            migrationBuilder.Sql("UPDATE Products SET PurchaseCurrency = 'COP' WHERE PurchaseCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Products SET SaleCurrency = 'COP' WHERE SaleCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Sales SET SubtotalCurrency = 'COP' WHERE SubtotalCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Sales SET DiscountCurrency = 'COP' WHERE DiscountCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Sales SET TaxCurrency = 'COP' WHERE TaxCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Sales SET TotalCurrency = 'COP' WHERE TotalCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Sales SET AmountPaidCurrency = 'COP' WHERE AmountPaidCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Sales SET ChangeCurrency = 'COP' WHERE ChangeCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE SaleItems SET UnitPriceCurrency = 'COP' WHERE UnitPriceCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE SaleItems SET TotalCurrency = 'COP' WHERE TotalCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE SalePayments SET Currency = 'COP' WHERE Currency = 'PEN'");
            migrationBuilder.Sql("UPDATE Customers SET CreditCurrency = 'COP' WHERE CreditCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Customers SET DebtCurrency = 'COP' WHERE DebtCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE CreditPayments SET Currency = 'COP' WHERE Currency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashRegisters SET InitialCashCurrency = 'COP' WHERE InitialCashCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashRegisters SET CurrentCashCurrency = 'COP' WHERE CurrentCashCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashRegisters SET ExpectedCashCurrency = 'COP' WHERE ExpectedCashCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashRegisters SET FinalCashCurrency = 'COP' WHERE FinalCashCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashRegisters SET DifferenceCurrency = 'COP' WHERE DifferenceCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashMovements SET Currency = 'COP' WHERE Currency = 'PEN'");
            migrationBuilder.Sql("UPDATE CashMovements SET BalanceAfterCurrency = 'COP' WHERE BalanceAfterCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE Expenses SET Currency = 'COP' WHERE Currency = 'PEN'");
            migrationBuilder.Sql("UPDATE PurchaseOrders SET TotalCurrency = 'COP' WHERE TotalCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE PurchaseOrderItems SET UnitCostCurrency = 'COP' WHERE UnitCostCurrency = 'PEN'");
            migrationBuilder.Sql("UPDATE PurchaseOrderItems SET TotalCurrency = 'COP' WHERE TotalCurrency = 'PEN'");

            migrationBuilder.DropTable(
                name: "ProductPriceHistories");

            migrationBuilder.AlterColumn<string>(
                name: "TotalCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "TaxCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "SubtotalCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "DiscountCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "ChangeCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "AmountPaidCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "TotalCurrency",
                table: "PurchaseOrders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "SaleCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "PurchaseCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "DebtCurrency",
                table: "Customers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "CreditCurrency",
                table: "Customers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "InitialCashCurrency",
                table: "CashRegisters",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "ExpectedCashCurrency",
                table: "CashRegisters",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCashCurrency",
                table: "CashRegisters",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "COP",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TotalCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "TaxCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "SubtotalCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "DiscountCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "ChangeCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "AmountPaidCurrency",
                table: "Sales",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "TotalCurrency",
                table: "PurchaseOrders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "SaleCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "PurchaseCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "DebtCurrency",
                table: "Customers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "CreditCurrency",
                table: "Customers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "InitialCashCurrency",
                table: "CashRegisters",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "ExpectedCashCurrency",
                table: "CashRegisters",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCashCurrency",
                table: "CashRegisters",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "COP");

            migrationBuilder.CreateTable(
                name: "ProductPriceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchaseCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaleCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPriceHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductId_EffectiveDate",
                table: "ProductPriceHistories",
                columns: new[] { "ProductId", "EffectiveDate" });
        }
    }
}
