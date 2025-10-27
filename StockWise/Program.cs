using AutoMapper;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Errors;
using StockWise.Infrastructure.DataAccess;
using StockWise.Infrastructure.Repositories;
using StockWise.Services.IServices;
using StockWise.Services.Mappings;
using StockWise.Services.Services;
using System;
using System.Text;

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

            //_____________________________________________________________________
            // ≈÷«›… Œœ„«  «·‹ JWT

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<StockWiseDbContext>();
            

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = builder.Configuration["Jwt:Issuer"],
                         ValidAudience = builder.Configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey(
                             Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                     };
                 });
                 

            builder.Services.AddAuthorization();
            //___________________________________________________________________
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
          /*  builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "StockWise API",
                    Version = "v1"
                });
            });*/
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StockWise API", Version = "v1" });
                // ≈÷«›…  ⁄—Ì› «·√„«‰ ·‹ JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                //  ÿ»Ìﬁ «·√„«‰ ⁄·Ï ﬂ· «·‹ endpoints
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
            });


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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockWise API v1"));
            }
            /*   app.UseSwagger();
               app.UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockWise API v1");
                   c.RoutePrefix = string.Empty; // ÌŒ·Ì Swagger ÂÊ «·’›Õ… «·—∆Ì”Ì…
               });*/

            app.UseHttpsRedirection();

            
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}
