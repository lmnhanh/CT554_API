using CT554_API.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CT554_API.Data;

public class CT554DbContext : IdentityDbContext<User>
{
	public DbSet<Category> Categories { get; set; } = null!;
	public DbSet<Product> Products { get; set; } = null!;
	public DbSet<ProductDetail> ProductDetails { get; set; } = null!;
	public DbSet<Price> Prices { get; set; } = null!;
	public DbSet<Stock> Stocks { get; set; } = null!;
	public DbSet<Cart> Carts { get; set; } = null!;
	public DbSet<Order> Orders { get; set; } = null!;
	public DbSet<Image> Images { get; set; } = null!;
	public DbSet<Invoice> Invoices { get; set; } = null!;
	public DbSet<InvoiceDetail> InvoiceDetails { get; set; } = null!;
	public CT554DbContext(DbContextOptions<CT554DbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
		builder.HasDefaultSchema("dbo");

		builder.Entity<User>(user =>
		{
			user.ToTable("Users");
			user.Property(u => u.FullName).HasMaxLength(50);
			user.Property(u => u.DayOfBirth).IsRequired(false);
		});

		builder.Entity<Category>(category =>
		{
			category.HasKey(c => c.Id);
			category.Property(c => c.Id).ValueGeneratedOnAdd();
			category.Property(c => c.Name).HasMaxLength(50);
			category.Property(c => c.DateUpdate).HasConversion(typeof(UtcValueConverter));
			category.HasMany<Product>(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId);
		});

		builder.Entity<Image>(image => {
			image.HasKey(i => i.URL);
			image.Property(i => i.URL).HasMaxLength(100);
		});

		builder.Entity<Product>(product =>
		{
			product.HasKey(p => p.Id);
			product.Property(p => p.Id).ValueGeneratedOnAdd();
			product.Property(p => p.WellKnownId).HasMaxLength(20);
			product.Property(p => p.Name).HasMaxLength(50);
			product.Property(p => p.Description).HasMaxLength(500);
			product.HasMany<ProductDetail>(p => p.Details).WithOne(pd => pd.Product).HasForeignKey(pd => pd.ProductId);
			product.HasMany<Image>(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId);
		});

		builder.Entity<ProductDetail>(detail =>
		{
			detail.HasKey(pd => pd.Id);
			detail.Property(pd => pd.Id).ValueGeneratedOnAdd();
			detail.Property(pd => pd.Unit).HasMaxLength(30);
			detail.Property(pd => pd.Description).HasMaxLength(500);
			detail.HasMany<Price>(pr => pr.Prices).WithOne(p => p.ProductDetail).HasForeignKey(p => p.ProductDetailId);
			detail.HasMany<Stock>(pd => pd.Stocks).WithOne(s => s.ProductDetail).HasForeignKey(p => p.ProductDetailId);
			detail.HasMany<Cart>(pd => pd.Carts).WithOne(c => c.ProductDetail).HasForeignKey(c => c.ProductDetailId);
			detail.HasMany<InvoiceDetail>(pd => pd.InvoiceDetails).WithOne(i => i.ProductDetail).HasForeignKey(i => i.ProductDetailId);
		});

		builder.Entity<Stock>(stock =>
		{
			stock.HasKey(s => new { s.DateUpdate, s.ProductDetailId, s.IsManualUpdate });
		});

		builder.Entity<Cart>(cart =>
		{
			cart.HasKey(c => c.Id);
			cart.Property(c => c.Id).ValueGeneratedOnAdd();
		});

		builder.Entity<Order>(order =>
		{
			order.HasKey(c => c.Id);
			order.Property(c => c.Id).ValueGeneratedOnAdd();
			order.Property(c => c.Description).HasMaxLength(500);
			order.HasMany<Cart>(o => o.Carts).WithOne(c => c.Order).HasForeignKey(c => c.OrderId);
		});

		builder.Entity<Price>(price =>
		{
			price.HasKey(p => p.Id);
			price.Property(p => p.Id).ValueGeneratedOnAdd();
		});

		builder.Entity<Invoice>(invoice =>
		{
			invoice.HasKey(p => p.Id);
			invoice.Property(p => p.Id).ValueGeneratedOnAdd();
			invoice.HasMany<InvoiceDetail>(i => i.Details).WithOne(detail => detail.Invoice).HasForeignKey(detail => detail.InvoiceId);
		});

		builder.Entity<InvoiceDetail>(invoiceDetail =>
		{
			invoiceDetail.HasKey(detail => new { detail.InvoiceId, detail.ProductDetailId });
		});


		builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
		builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
		builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));
		builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));
		builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));
		builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));
	}
	class UtcValueConverter : ValueConverter<DateTime, DateTime>
	{
		public UtcValueConverter(): base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
		{
		}
	}
}
