using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.DataAccess
{
    public class StockWiseDbContext : IdentityDbContext<ApplicationUser>
    {
        public StockWiseDbContext(DbContextOptions<StockWiseDbContext> options):base(options) { }

        public DbSet<Customer> customers { get; set; }
        public DbSet<Expense> expenses { get; set; }
        public DbSet<Invoice> invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Location> locations { get; set; }
        public DbSet<Payment> payments { get; set; }
        public DbSet<Product> Products { get; set;}
        public DbSet<Representative> representatives { get; set; }
        public DbSet<Return> returns { get; set; }
        public DbSet<Stock> stocks { get; set; }
        public DbSet<Transfer> transfers { get; set; }
        public DbSet<Warehouse> warehouses { get; set; }
        public DbSet<DamagedProduct> damagedProducts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Stock>().ToTable("Stocks");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Warehouse>().ToTable("Warehouses");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Representative>().ToTable("Representatives");
            modelBuilder.Entity<Invoice>().ToTable("Invoices");
            modelBuilder.Entity<InvoiceItem>().ToTable("InvoiceItems");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<Return>().ToTable("Returns");
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<Transfer>().ToTable("Transfers");
            modelBuilder.Entity<Location>().ToTable("Locations");
            modelBuilder.Entity<DamagedProduct>().ToTable("DamagedProduct");


           // modelBuilder.Entity<BaseEntity>().HasKey(e => e.Id);  عمل Error لانو اعتبرو جدول منفصل 
            //Warehouse
            modelBuilder.Entity<Warehouse>()
                .Property(w => w.WarehouseType)
                .HasConversion<string>(); // store Enum as string (not int )

            modelBuilder.Entity<Warehouse>()
                .Property(w => w.Address)
                .IsRequired(false);

            //Product
            modelBuilder.Entity<Product>()
                .OwnsOne(p => p.Price, price =>
                {
                    price.WithOwner();
                    price.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("PriceAmount");
                    price.Property(m => m.Currency).HasColumnName("PriceCurrency");
                });

            modelBuilder.Entity<Product>().Property(p=>p.Condition)
                .HasConversion<string>();

            modelBuilder.Entity<Product>()
                .Property(p => p.InitialQuantity) //////////////////////
                .HasDefaultValue(0);
            //Stock

            modelBuilder.Entity<Stock>()//عملت ال ID بناء علي الجدولين عشان نفس المنتج مينفعش يبقي في مخذنين
                .HasIndex(p => new{p.WarehouseId,p.ProductId }).IsUnique();

            modelBuilder.Entity<Stock>()
                .HasOne(h => h.Warehouse)
                .WithMany(h => h.Stocks)
                .HasForeignKey(h => h.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                    .HasOne(s => s.Product)
                    .WithMany(p => p.stocks)
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

            //Representative
            modelBuilder.Entity<Representative>()
                .HasOne(h => h.Warehouse)
                .WithMany(h => h.Representatives)
                .HasForeignKey(h=>h.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Representative>()
                .Property(h => h.NationalId)
                .IsRequired(false).HasMaxLength(14);

            modelBuilder.Entity<Representative>()
                .HasIndex(h => h.NationalId).IsUnique()
                .HasFilter("[NationalId] IS NOT NULL ");

            modelBuilder.Entity<Representative>()
                .Property(h => h.PhoneNumber)
                .HasConversion(
                    v => string.Join(";", v),
                    v=>v.Split(';',StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<Representative>()
                .Property(h => h.Address)
                .IsRequired(false);

            modelBuilder.Entity<Representative>()
                .Property(h => h.Notes)
                .IsRequired(false);

            //customer
            modelBuilder.Entity<Customer>()
                .OwnsOne(c => c.CreditBalance, cb => {
                    cb.WithOwner();
                    cb.Property(m => m.Amount).HasColumnName("CreditBalanceAmount");
                    cb.Property(m => m.Currency).HasColumnName("CreditBalanceCurrency");
                });

            modelBuilder.Entity<Customer>()
                .Property(c => c.PhoneNumbers)
                .HasConversion(
                    v=> string.Join(";", v),
                    v=>v.Split(";",StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<Customer>()
                .Property(c => c.Address)
                .IsRequired(false);

            //Invoice

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasConversion<string>(); // Enum as string
            

            modelBuilder.Entity<Invoice>()
                .OwnsOne(p => p.TotalAmount, p => {
                    p.WithOwner();//  هنا  بيقول للـ EF إن ده مملوك بالكامل بدون مفتاح منفصل
                    p.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("TotalAmount");
                    p.Property(p => p.Currency).HasColumnName("TotalAmountCurrency");
                });

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(i => i.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Representative)
                .WithMany(i => i.Invoices)
                .HasForeignKey(i => i.RepresentativeId)
                .OnDelete(DeleteBehavior.Restrict);

            //InvoiceItem
            modelBuilder.Entity<InvoiceItem>()
                .OwnsOne(i => i.Price, price =>
                {

                    price.WithOwner();
                    price.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("PriceAmount");
                    price.Property(m => m.Currency).HasColumnName("PriceCurrency");
                });
            modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Product)
            .WithMany(p => p.invoiceItems)
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

            //Payment
            modelBuilder.Entity<Payment>()
                .OwnsOne(p => p.Amount, amount =>
                {
                    amount.WithOwner();
                    amount.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Amount");
                    amount.Property(m => m.Currency).HasColumnName("Currency");

                });

            modelBuilder.Entity<Payment>()
                .Property(p => p.Method)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CustomerId) ///////////////
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments) /////////////////////
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            //Return
            modelBuilder.Entity<Return>()
                .Property(r => r.ReturnType)
                .HasConversion<string>();

            modelBuilder.Entity<Return>()
                .HasOne(r => r.Product)
                .WithMany(p => p.returns)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            //Expense
            modelBuilder.Entity<Expense>()
                .OwnsOne(p => p.Amount, amount =>
                {
                    amount.WithOwner();
                    amount.Property(p => p.Currency).HasColumnName("Currency");
                    amount.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("Amount");

                });

            modelBuilder.Entity<Expense>()
                .Property(e => e.ExpenseType)
                .HasConversion<string>();
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Representative)
                .WithMany(r => r.Expenses)
                .HasForeignKey(e => e.RepresentativeId)
                .IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            //Transfer
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.FromWarehouse)
                .WithMany(w => w.TransfersFrom)
                .HasForeignKey(t => t.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                    .HasOne(t => t.ToWarehouse)
                    .WithMany(w => w.TransfersTo)
                    .HasForeignKey(t => t.ToWarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
            .HasOne(t => t.Product)
            .WithMany(p => p.transfers)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);



            //Location
            modelBuilder.Entity<Location>()
                .HasOne(p => p.Representative)
                .WithMany(p => p.Locations)
                .HasForeignKey(p=>p.RepresentativeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            // للاستعلامات السريعة 
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.CustomerId);
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.RepresentativeId);
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.Status);


            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<string>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
        }
    }





    }

