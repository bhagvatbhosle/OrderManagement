using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the Order aggregate (and everything it owns: OrderLines, ShippingAddress) to
/// tables using pure Fluent API -- the Domain classes stay free of EF Core attributes.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.CustomerId).IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>() // store the enum as text -- much easier to read/debug in the DB
            .IsRequired();

        builder.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        builder.Property(o => o.CreatedOnUtc).IsRequired();
        builder.Property(o => o.PlacedOnUtc);
        builder.Property(o => o.ShippedOnUtc);

        // Order.Total is a computed property (sum of line totals), not a column.
        builder.Ignore(o => o.Total);

        // Order.DomainEvents is in-memory only; never persisted.
        builder.Ignore(o => o.DomainEvents);

        // ShippingAddress is a Value Object owned by Order (a "dependent" with no
        // identity of its own -- it lives and dies with the Order row). Address has
        // get-only properties (no setters), so EF must be told to populate them via
        // their backing fields rather than via property setters that don't exist.
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.UsePropertyAccessMode(PropertyAccessMode.Field);
            address.Property(a => a.Line1).HasColumnName("ShippingLine1").HasMaxLength(200);
            address.Property(a => a.Line2).HasColumnName("ShippingLine2").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
        });

        // OrderLine is also a Value Object, owned in a one-to-many collection. We map
        // it to the private "_lines" backing field, since the public Lines property is
        // deliberately read-only (IReadOnlyCollection) -- external code should only be
        // able to change lines through Order.AddLine()/RemoveLine(), never by handing
        // EF Core a mutable list to poke directly.
        builder.OwnsMany(o => o.Lines, line =>
        {
            line.UsePropertyAccessMode(PropertyAccessMode.Field);
            line.ToTable("OrderLines");
            line.WithOwner().HasForeignKey("OrderId");
            line.Property<int>("Id"); // EF Core still needs a shadow key for the owned collection table
            line.HasKey("Id");

            line.Property(l => l.ProductId).IsRequired();
            line.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
            line.Property(l => l.Quantity).IsRequired();

            line.OwnsOne(l => l.UnitPrice, price =>
            {
                price.UsePropertyAccessMode(PropertyAccessMode.Field);
                price.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasColumnType("decimal(18,2)");
                price.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });

            line.Ignore(l => l.LineTotal); // computed, not stored
        });

        builder.Metadata.FindNavigation(nameof(Order.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
