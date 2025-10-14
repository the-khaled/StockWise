using AutoMapper;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS;
using StockWise.Domain.Enums;
using StockWise.Services.DTOS.InvoiceDto;
using StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto;
using StockWise.Services.DTOS.InvoiceItemDto;
using StockWise.Services.DTOS.ProductDto;
using StockWise.Services.DTOS.WarehouseDto;
using StockWise.Services.DTOS.StockDto;

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
            CreateMap<ProductForCreationDto, Product>()
              .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new Money(src.Price.Amount, src.Price.Currency ?? "EGP")))
              .ForMember(dest => dest.stocks, opt => opt.Ignore())
              .ForMember(dest => dest.invoiceItems, opt => opt.Ignore())
              .ForMember(dest => dest.returns, opt => opt.Ignore())
              .ForMember(dest => dest.transfers, opt => opt.Ignore());

            CreateMap<StandaloneProductCreateDto, Product>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new Money(src.Price.Amount, src.Price.Currency ?? "EGP")))
                .ForMember(dest => dest.stocks, opt => opt.Ignore())
                .ForMember(dest => dest.invoiceItems, opt => opt.Ignore())
                .ForMember(dest => dest.returns, opt => opt.Ignore())
                .ForMember(dest => dest.transfers, opt => opt.Ignore());

            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new MoneyDto
                {
                    Amount = src.Price.Amount,
                    Currency = src.Price.Currency
                }))
                .ForMember(dest => dest.WarehouseIds, opt => opt.MapFrom(src => src.stocks.Select(s => s.WarehouseId).ToList()));
            // Invoice Mappings
            CreateMap<InvoiceCreateDto, Invoice>()
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // هيتحسب تلقائيًا
            .ForMember(dest => dest.Items, opt => opt.Ignore());

         

            CreateMap<Invoice, InvoiceResponseDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.RepresentativeName, opt => opt.MapFrom(src => src.Representative.Name))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        
            // InvoiceItem Mappings
            CreateMap<InvoiceItemCreateDto, InvoiceItem>()
                 .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price != null
                    ? new Money(src.Price.Amount, src.Price.Currency ?? "EGP")
                    : null));// سيتم التعامل مع null في الـ Service

            CreateMap<InvoiceItem, InvoiceItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Quantity * src.Price.Amount));
            // Warehouse Mappings
            CreateMap<WarehouseCreateDto, Warehouse>();

            CreateMap<Warehouse, WarehouseResponseDto>()
                .ForMember(dest => dest.RepresentativeId, opt => opt.MapFrom(src => src.Representatives.Select(r => r.Id).ToList()))
                .ForMember(dest => dest.StockId , opt => opt.MapFrom(src => src.Stocks.Select(s => s.Id).ToList()));


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
            CreateMap<StockCreateDto, Stock>()
                 .ForMember(dest => dest.MinQuantity, opt => opt.MapFrom(src => src.MinQuantity ?? 10));
            CreateMap<Stock, StockResponseDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            // Transfer Mappings
            CreateMap<TransferDto, Transfer>();
            CreateMap<Transfer, TransferDto>();

            // Location Mappings
            CreateMap<LocationDto, Location>();
            CreateMap<Location, LocationDto>();
        }
    }
}