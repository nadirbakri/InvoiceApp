using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace InvoiceWebApp.Models
{
    public class Courier
    {
        public int CourierID { get; set; }
        public string CourierName { get; set; }
    }

    public class Payment
    {
        public int PaymentID { get; set; }
        public string PaymentName { get; set; }
    }

    public class Sales
    {
        public int SalesID { get; set; }
        public string SalesName { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Weight { get; set; }
        public decimal Price { get; set; }
    }

    public class CourierFee
    {
        public int WeightID { get; set; }
        public int CourierID { get; set; }
        public int StartKg { get; set; }
        public int? EndKg { get; set; }
        public decimal Price { get; set; }
        public virtual Courier Courier { get; set; }
    }

    public class Invoice
    {
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceTo { get; set; }
        public string ShipTo { get; set; }
        public int SalesID { get; set; }
        public int CourierID { get; set; }
        public int PaymentType { get; set; }
        public decimal CourierFee { get; set; }
        public virtual Sales Sales { get; set; }
        public virtual Courier Courier { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual ICollection<InvoiceDetail> Details { get; set; }
    }

    public class InvoiceDetail
    {
        public string InvoiceNo { get; set; }
        public int ProductID { get; set; }
        public decimal Weight { get; set; }
        public short Qty { get; set; }
        public decimal Price { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Product Product { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("AppDb") { }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CourierFee> CourierFees { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Conventions.Remove<PluralizingTableNameConvention>();

            mb.Entity<Courier>().ToTable("mscourier").HasKey(x => x.CourierID);
            mb.Entity<Payment>().ToTable("mspayment").HasKey(x => x.PaymentID);
            mb.Entity<Sales>().ToTable("mssales").HasKey(x => x.SalesID);
            mb.Entity<Product>().ToTable("msproduct").HasKey(x => x.ProductID);

            mb.Entity<CourierFee>().ToTable("ltcourierfee").HasKey(x => x.WeightID);
            mb.Entity<CourierFee>().HasRequired(cf => cf.Courier).WithMany().HasForeignKey(cf => cf.CourierID).WillCascadeOnDelete(false);

            mb.Entity<Invoice>().ToTable("trinvoice").HasKey(x => x.InvoiceNo);
            mb.Entity<Invoice>().HasRequired(i => i.Sales).WithMany().HasForeignKey(i => i.SalesID).WillCascadeOnDelete(false);
            mb.Entity<Invoice>().HasRequired(i => i.Courier).WithMany().HasForeignKey(i => i.CourierID).WillCascadeOnDelete(false);
            mb.Entity<Invoice>().HasRequired(i => i.Payment).WithMany().HasForeignKey(i => i.PaymentType).WillCascadeOnDelete(false);

            mb.Entity<InvoiceDetail>().ToTable("trinvoicedetail").HasKey(d => new { d.InvoiceNo, d.ProductID });
            mb.Entity<InvoiceDetail>().HasRequired(d => d.Invoice).WithMany(i => i.Details).HasForeignKey(d => d.InvoiceNo).WillCascadeOnDelete(true);
            mb.Entity<InvoiceDetail>().HasRequired(d => d.Product).WithMany().HasForeignKey(d => d.ProductID).WillCascadeOnDelete(false);

            mb.Entity<Product>().Property(p => p.Weight).HasPrecision(10, 3);
            mb.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            mb.Entity<CourierFee>().Property(p => p.Price).HasPrecision(18, 2);
            mb.Entity<Invoice>().Property(p => p.CourierFee).HasPrecision(18, 2);
            mb.Entity<InvoiceDetail>().Property(p => p.Weight).HasPrecision(10, 3);
            mb.Entity<InvoiceDetail>().Property(p => p.Price).HasPrecision(18, 2);
        }
    }
}
