using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.PaymentTerms).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.HasIndex(x => x.Name);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SupplierId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Draft");
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedAt);
        builder.HasIndex(x => new { x.SupplierId, x.Status });
    }
}

public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("purchase_order_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PurchaseOrderId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.QuantityOrdered).HasPrecision(18, 3);
        builder.Property(x => x.QuantityReceived).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.HasIndex(x => x.PurchaseOrderId);
    }
}

public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("goods_receipts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PurchaseOrderId).IsRequired();
        builder.Property(x => x.ReceivedBy).HasMaxLength(100);
        builder.Property(x => x.ReceivedAt).HasDefaultValueSql("now()");
        builder.HasIndex(x => x.PurchaseOrderId);
    }
}

public class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("goods_receipt_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GoodsReceiptId).IsRequired();
        builder.Property(x => x.PurchaseOrderLineId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.QuantityReceived).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.HasIndex(x => x.GoodsReceiptId);
    }
}

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("purchase_returns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SupplierId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Submitted");
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.SupplierId, x.Status });
    }
}

public class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("purchase_return_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PurchaseReturnId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.HasIndex(x => x.PurchaseReturnId);
    }
}

public class SupplierProductPriceConfiguration : IEntityTypeConfiguration<SupplierProductPrice>
{
    public void Configure(EntityTypeBuilder<SupplierProductPrice> builder)
    {
        builder.ToTable("supplier_product_prices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SupplierId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.LastPrice).HasPrecision(18, 4);
        builder.Property(x => x.LastUpdated).HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.SupplierId, x.ProductId }).IsUnique();
    }
}

