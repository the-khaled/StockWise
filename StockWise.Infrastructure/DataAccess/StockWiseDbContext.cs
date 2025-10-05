using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.DataAccess
{
    public class StockWiseDbContext : DbContext
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


            //Warehouse
            modelBuilder.Entity<Warehouse>().Property(w => w.WarehouseType)
                .HasConversion<string>(); // store Enum as string (not int )

            modelBuilder.Entity<Warehouse>().Property(w => w.Address)
                .IsRequired(false);

            //Product
            modelBuilder.Entity<Product>()
                .OwnsOne(p => p.Price, price =>
                {
                    price.Property(m => m.Amount).HasColumnName("PriceAmount");
                    price.Property(m => m.Currency).HasColumnName("PriceCurrency");
                });

            modelBuilder.Entity<Product>().Property(p=>p.Condition)
                .HasConversion<string>();

            //Stock

            modelBuilder.Entity<Stock>()//عملت ال ID بناء علي الجدولين عشان نفس المنتج مينفعش يبقي في مخذنين
                .HasIndex(p => new{p.WarehouseId,p.ProductId }).IsUnique();

            modelBuilder.Entity<Stock>()
                .HasOne(h => h.Warehouse)
                .WithMany(h => h.Stocks).HasForeignKey(h => h.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                    .HasOne(s => s.Product)
                    .WithMany(p => p.stocks)
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

            //Representative
            modelBuilder.Entity<Representative>()
                .HasOne(h => h.Warehouse).WithMany(h => h.Representatives)
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
                .OwnsOne(p => p.TotalAmount, p => {
                    p.Property(p => p.Amount).HasColumnName("TotalAmount");
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
                    price.Property(m => m.Amount).HasColumnName("PriceAmount");
                    price.Property(m => m.Currency).HasColumnName("PriceCurrency");
                });

            //Payment
            modelBuilder.Entity<Payment>()
                .OwnsOne(p => p.Amount, amount =>
                {
                    amount.Property(m => m.Amount).HasJsonPropertyName("Amount");
                    amount.Property(m => m.Currency).HasJsonPropertyName("Currency");

                });

            modelBuilder.Entity<Payment>()
                .Property(p => p.Method)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            //Return
            modelBuilder.Entity<Return>()
                .Property(r => r.ReturnType)
                .HasConversion<string>();

            //Expense
            modelBuilder.Entity<Expense>()
                .OwnsOne(p => p.Amount, amount =>
                {
                    amount.Property(p => p.Currency).HasColumnName("Currency");
                    amount.Property(p => p.Amount).HasColumnName("Amount");

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

            //Location
            modelBuilder.Entity<Location>()
                .HasOne(p => p.Representative)
                .WithMany(p => p.Locations)
                .HasForeignKey(p=>p.RepresentativeId)
                .OnDelete(DeleteBehavior.Restrict);




        }





    }
}
