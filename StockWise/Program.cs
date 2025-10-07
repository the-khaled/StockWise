
using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Errors;
using StockWise.Infrastructure.DataAccess;
using StockWise.Infrastructure.Repositories;
using StockWise.Services.IServices;
using StockWise.Services.Services;

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
