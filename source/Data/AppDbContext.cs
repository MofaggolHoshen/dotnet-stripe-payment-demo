using Microsoft.EntityFrameworkCore;
using DotnetStripePaymentDemo.Models;

namespace DotnetStripePaymentDemo.Data
{
    /// <summary>
    /// Entity Framework Core database context for Stripe Payment Demo
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<WebhookEvent> WebhookEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StripeCustomerId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.DefaultPaymentMethodId).HasMaxLength(100);
                entity.Property(e => e.BillingAddress).HasMaxLength(500);
                
                entity.HasIndex(e => e.StripeCustomerId).IsUnique();
                entity.HasIndex(e => e.Email);
                
                entity.HasMany(e => e.Subscriptions)
                    .WithOne(s => s.Customer)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(e => e.Invoices)
                    .WithOne(i => i.Customer)
                    .HasForeignKey(i => i.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Subscription entity
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StripeSubscriptionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StripePriceId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StripeProductId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BillingInterval).HasMaxLength(20);
                
                entity.HasIndex(e => e.StripeSubscriptionId).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => new { e.CustomerId, e.Status });
            });

            // Configure Invoice entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StripeInvoiceId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Currency).HasMaxLength(3);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.PdfUrl).HasMaxLength(1000);
                
                entity.HasIndex(e => e.StripeInvoiceId).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.SubscriptionId);
                entity.HasIndex(e => new { e.CustomerId, e.Status });
            });

            // Configure WebhookEvent entity
            modelBuilder.Entity<WebhookEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StripeEventId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApiVersion).HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(e => e.StripeEventId).IsUnique();
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.CustomerId, e.EventType });
            });
        }
    }
}
