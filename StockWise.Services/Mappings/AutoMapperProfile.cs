using AutoMapper;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS;
using StockWise.Domain.Enums;

namespace StockWise.Services.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Custom converters
            CreateMap<DateTime?, DateTime>().ConvertUsing(src => src ?? DateTime.UtcNow);

            // MoneyDto <-> Money
            CreateMap<MoneyDto, Money>()
                .ConstructUsing(src => new Money(src.Amount,src.Currency) { Currency = src.Currency ?? "EGP" });

            CreateMap<Money, MoneyDto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

            // Product Mappings
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()) // تجاهل Price لأننا بننشئه يدويًا
                .ForMember(dest => dest.InitialQuantity, opt => opt.MapFrom(src => src.InitialQuantity ?? 0))
                .ForMember(dest => dest.ProductionDate, opt => opt.MapFrom(src => src.ProductionDate))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition));

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.Price.Amount,
                    Currency = src.Price.Currency
                }));

            // Invoice Mappings
            CreateMap<InvoiceDto, Invoice>()
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // تجاهل لأننا بنحسبه يدويًا
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Items, opt => opt.Ignore()); // تجاهل لأننا بننشئها يدويًا

            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.TotalAmount.Amount,
                    Currency = src.TotalAmount.Currency
                }))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            // InvoiceItem Mappings
            CreateMap<InvoiceItemDto, InvoiceItem>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()) // تجاهل لأننا بننشئه يدويًا
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore()); // تجاهل لأننا بنعينه يدويًا

            CreateMap<InvoiceItem, InvoiceItemDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.Price.Amount,
                    Currency = src.Price.Currency
                }));

            // Warehouse Mappings
            CreateMap<WarehouseDto, Warehouse>();
            CreateMap<Warehouse, WarehouseDto>();

            // Customer Mappings
            CreateMap<CustomerDto, Customer>()
                .ForMember(dest => dest.CreditBalance, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumbers, opt => opt.MapFrom(src => src.PhoneNumbers ?? new List<string>()));

            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CreditBalance, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.CreditBalance.Amount,
                    Currency = src.CreditBalance.Currency
                }));

            // Representative Mappings
            CreateMap<RepresentativeDto, Representative>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? new List<string>()));

            CreateMap<Representative, RepresentativeDto>();

            // Payment Mappings
            CreateMap<PaymentDto, Payment>()
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.Amount.Amount,
                    Currency = src.Amount.Currency
                }));

            // Return Mappings
            CreateMap<ReturnDto, Return>()
                .ForMember(dest => dest.ReturnType, opt => opt.MapFrom(src => src.ReturnType));

            CreateMap<Return, ReturnDto>();

            // Expense Mappings
            CreateMap<ExpenseDto, Expense>()
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.ExpenseType, opt => opt.MapFrom(src => src.ExpenseType));

            CreateMap<Expense, ExpenseDto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.Amount.Amount,
                    Currency = src.Amount.Currency
                }));

            // Stock Mappings
            CreateMap<StockDto, Stock>();
            CreateMap<Stock, StockDto>();

            // Transfer Mappings
            CreateMap<TransferDto, Transfer>();
            CreateMap<Transfer, TransferDto>();

            // Location Mappings
            CreateMap<LocationDto, Location>();
            CreateMap<Location, LocationDto>();
        }
    }
}