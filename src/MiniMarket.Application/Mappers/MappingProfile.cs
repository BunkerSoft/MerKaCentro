using AutoMapper;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateCategoryMaps();
        CreateProductMaps();
        CreateCustomerMaps();
        CreateSupplierMaps();
        CreateSaleMaps();
        CreateCashRegisterMaps();
        CreateUserMaps();
        CreatePurchaseOrderMaps();
        CreateExpenseMaps();
    }

    private void CreateCategoryMaps()
    {
        CreateMap<Category, CategoryDto>()
            .ForCtorParam("ProductCount", opt => opt.MapFrom(src => src.Products.Count));
    }

    private void CreateProductMaps()
    {
        CreateMap<Product, ProductDto>()
            .ForCtorParam("Barcode", opt => opt.MapFrom(src => src.Barcode != null ? src.Barcode.Value : null))
            .ForCtorParam("CategoryName", opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForCtorParam("PurchasePrice", opt => opt.MapFrom(src => src.PurchasePrice.Amount))
            .ForCtorParam("SalePrice", opt => opt.MapFrom(src => src.SalePrice.Amount))
            .ForCtorParam("Currency", opt => opt.MapFrom(src => src.SalePrice.Currency))
            .ForCtorParam("MinStock", opt => opt.MapFrom(src => src.MinStock.Value))
            .ForCtorParam("CurrentStock", opt => opt.MapFrom(src => src.CurrentStock.Value))
            .ForCtorParam("ProfitMargin", opt => opt.MapFrom(src => src.GetProfitMargin().Value))
            .ForCtorParam("IsLowStock", opt => opt.MapFrom(src => src.IsLowStock()));

        CreateMap<Product, ProductStockDto>()
            .ForCtorParam("CurrentStock", opt => opt.MapFrom(src => src.CurrentStock.Value))
            .ForCtorParam("MinStock", opt => opt.MapFrom(src => src.MinStock.Value))
            .ForCtorParam("IsLowStock", opt => opt.MapFrom(src => src.IsLowStock()));
    }

    private void CreateCustomerMaps()
    {
        CreateMap<Customer, CustomerDto>()
            .ForCtorParam("DocumentNumber", opt => opt.MapFrom(src => src.DocumentNumber != null ? src.DocumentNumber.Value : null))
            .ForCtorParam("DocumentType", opt => opt.MapFrom(src => src.DocumentNumber != null ? src.DocumentNumber.Type : (Domain.Enums.DocumentType?)null))
            .ForCtorParam("Phone", opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null))
            .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email != null ? src.Email.Value : null))
            .ForCtorParam("Address", opt => opt.MapFrom(src => src.Address.ToString()))
            .ForCtorParam("CreditLimit", opt => opt.MapFrom(src => src.CreditLimit.Amount))
            .ForCtorParam("CurrentDebt", opt => opt.MapFrom(src => src.CurrentDebt.Amount))
            .ForCtorParam("AvailableCredit", opt => opt.MapFrom(src => src.GetAvailableCredit().Amount));

        CreateMap<Customer, CustomerDebtDto>()
            .ForCtorParam("DocumentNumber", opt => opt.MapFrom(src => src.DocumentNumber != null ? src.DocumentNumber.Value : null))
            .ForCtorParam("CurrentDebt", opt => opt.MapFrom(src => src.CurrentDebt.Amount))
            .ForCtorParam("CreditLimit", opt => opt.MapFrom(src => src.CreditLimit.Amount))
            .ForCtorParam("LastPurchaseDate", opt => opt.MapFrom(src =>
                src.Sales.OrderByDescending(s => s.CreatedAt).FirstOrDefault() != null
                    ? src.Sales.OrderByDescending(s => s.CreatedAt).First().CreatedAt
                    : (DateTime?)null));
    }

    private void CreateSupplierMaps()
    {
        CreateMap<Supplier, SupplierDto>()
            .ForCtorParam("Ruc", opt => opt.MapFrom(src => src.Ruc != null ? src.Ruc.Value : null))
            .ForCtorParam("Phone", opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null))
            .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email != null ? src.Email.Value : null))
            .ForCtorParam("Address", opt => opt.MapFrom(src => src.Address.ToString()));
    }

    private void CreateSaleMaps()
    {
        CreateMap<Sale, SaleDto>()
            .ForCtorParam("CustomerName", opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForCtorParam("Subtotal", opt => opt.MapFrom(src => src.Subtotal.Amount))
            .ForCtorParam("Discount", opt => opt.MapFrom(src => src.Discount.Amount))
            .ForCtorParam("Tax", opt => opt.MapFrom(src => src.Tax.Amount))
            .ForCtorParam("Total", opt => opt.MapFrom(src => src.Total.Amount))
            .ForCtorParam("AmountPaid", opt => opt.MapFrom(src => src.AmountPaid.Amount))
            .ForCtorParam("Change", opt => opt.MapFrom(src => src.Change.Amount))
            .ForCtorParam("Items", opt => opt.MapFrom(src => src.Items))
            .ForCtorParam("Payments", opt => opt.MapFrom(src => src.Payments));

        CreateMap<SaleItem, SaleItemDto>()
            .ForCtorParam("Quantity", opt => opt.MapFrom(src => src.Quantity.Value))
            .ForCtorParam("UnitPrice", opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForCtorParam("Discount", opt => opt.MapFrom(src => src.Discount.Value))
            .ForCtorParam("Total", opt => opt.MapFrom(src => src.Total.Amount));

        CreateMap<SalePayment, SalePaymentDto>()
            .ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount.Amount));
    }

    private void CreateCashRegisterMaps()
    {
        CreateMap<CashRegister, CashRegisterDto>()
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForCtorParam("InitialCash", opt => opt.MapFrom(src => src.InitialCash.Amount))
            .ForCtorParam("CurrentCash", opt => opt.MapFrom(src => src.CurrentCash.Amount))
            .ForCtorParam("ExpectedCash", opt => opt.MapFrom(src => src.ExpectedCash.Amount))
            .ForCtorParam("FinalCash", opt => opt.MapFrom(src => src.FinalCash != null ? src.FinalCash.Amount : (decimal?)null))
            .ForCtorParam("Difference", opt => opt.MapFrom(src => src.Difference != null ? src.Difference.Amount : (decimal?)null))
            .ForCtorParam("Summary", opt => opt.MapFrom(src => new CashRegisterSummaryDto(
                src.GetTotalSales().Amount,
                src.GetTotalExpenses().Amount,
                src.GetTotalWithdrawals().Amount,
                src.GetTotalDeposits().Amount,
                src.Movements.Where(m => m.Type == Domain.Enums.CashMovementType.CreditPayment).Sum(m => m.Amount.Amount),
                src.Sales.Count)));

        CreateMap<CashMovement, CashMovementDto>()
            .ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount.Amount))
            .ForCtorParam("BalanceAfter", opt => opt.MapFrom(src => src.BalanceAfter.Amount));
    }

    private void CreateUserMaps()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, LoginResultDto>()
            .ForCtorParam("UserId", opt => opt.MapFrom(src => src.Id));
    }

    private void CreatePurchaseOrderMaps()
    {
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForCtorParam("SupplierName", opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForCtorParam("Total", opt => opt.MapFrom(src => src.Total.Amount))
            .ForCtorParam("Items", opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForCtorParam("Quantity", opt => opt.MapFrom(src => src.Quantity.Value))
            .ForCtorParam("ReceivedQuantity", opt => opt.MapFrom(src => src.ReceivedQuantity.Value))
            .ForCtorParam("PendingQuantity", opt => opt.MapFrom(src => src.GetPendingQuantity().Value))
            .ForCtorParam("UnitCost", opt => opt.MapFrom(src => src.UnitCost.Amount))
            .ForCtorParam("Total", opt => opt.MapFrom(src => src.Total.Amount))
            .ForCtorParam("IsFullyReceived", opt => opt.MapFrom(src => src.IsFullyReceived()));
    }

    private void CreateExpenseMaps()
    {
        CreateMap<Expense, ExpenseDto>()
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForCtorParam("CategoryName", opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount.Amount));

        CreateMap<ExpenseCategory, ExpenseCategoryDto>()
            .ForCtorParam("ExpenseCount", opt => opt.MapFrom(src => src.Expenses.Count));
    }
}
