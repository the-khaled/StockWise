using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Domain.Interfaces;
using StockWise.Errors;
using StockWise.Infrastructure.DataAccess;
using StockWise.Infrastructure.Repositories;
using StockWise.Services.IServices;
using StockWise.Services.Services;
using StockWise.Services.Mappings;

namespace StockWise
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;//to avoid infinty loop

                options.SerializerSettings.Converters
                .Add(new Newtonsoft.Json.Converters.StringEnumConverter());// Enum => string
            });

            // Add services to the container.
            builder.Services.AddDbContext<StockWiseDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c=>c.SwaggerDoc("v1",
                new() { Title = "StockWise API", Version = "v1" }
                ));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ITransferService, TransferService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IExpenseService, ExpenseService>();
            builder.Services.AddScoped<IInvoiceItemService, InvoiceItemService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<ILocationService, LocationService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IRepresentativeService, RepresentativeService>();
            builder.Services.AddScoped<IReturnService, ReturnService>();
            builder.Services.AddScoped<IStockService, StockService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();


            builder.Services.AddAutoMapper(typeof(StockWise.Services.Mappings.AutoMapperProfile));

            /*            builder.Services.AddAutoMapper(typeof(StockWise.Services.Mappings.AutoMapperProfile));
            */            /*   builder.Services.AddAutoMapper(cfg =>
                           {
                               cfg.AddProfile<AutoMapperProfile>();
                           });*/
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c=>c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockWise API v1"));
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseMiddleware<ErrorHandlerMiddleware>();



            app.MapControllers();

            app.Run();
        }
    }
}
