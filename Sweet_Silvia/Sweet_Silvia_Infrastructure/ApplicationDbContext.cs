using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Infrastructure;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Sweet_Silvia_Domain.Address.Address> Addresses => Set<Sweet_Silvia_Domain.Address.Address>();
    public DbSet<Sweet_Silvia_Domain.Cart.Cart> Carts => Set<Sweet_Silvia_Domain.Cart.Cart>();
    public DbSet<Sweet_Silvia_Domain.CartAppliedCoupon.CartAppliedCoupon> CartAppliedCoupons => Set<Sweet_Silvia_Domain.CartAppliedCoupon.CartAppliedCoupon>();
    public DbSet<Sweet_Silvia_Domain.CartItem.CartItem> CartItems => Set<Sweet_Silvia_Domain.CartItem.CartItem>();
    public DbSet<Sweet_Silvia_Domain.Category.Category> Categories => Set<Sweet_Silvia_Domain.Category.Category>();
    public DbSet<Sweet_Silvia_Domain.Collection.Collection> Collections => Set<Sweet_Silvia_Domain.Collection.Collection>();
    public DbSet<Sweet_Silvia_Domain.Coupon.Coupon> Coupons => Set<Sweet_Silvia_Domain.Coupon.Coupon>();
    public DbSet<Sweet_Silvia_Domain.CouponRestriction.CouponRestriction> CouponRestrictions => Set<Sweet_Silvia_Domain.CouponRestriction.CouponRestriction>();
    public DbSet<Sweet_Silvia_Domain.CouponUsage.CouponUsage> CouponUsages => Set<Sweet_Silvia_Domain.CouponUsage.CouponUsage>();
    public DbSet<Sweet_Silvia_Domain.Courier.Courier> Couriers => Set<Sweet_Silvia_Domain.Courier.Courier>();
    public DbSet<Sweet_Silvia_Domain.Department.Department> Departments => Set<Sweet_Silvia_Domain.Department.Department>();
    public DbSet<Sweet_Silvia_Domain.District.District> Districts => Set<Sweet_Silvia_Domain.District.District>();
    public DbSet<Sweet_Silvia_Domain.Order.Order> Orders => Set<Sweet_Silvia_Domain.Order.Order>();
    public DbSet<Sweet_Silvia_Domain.OrderItem.OrderItem> OrderItems => Set<Sweet_Silvia_Domain.OrderItem.OrderItem>();
    public DbSet<Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory> OrderStatusHistories => Set<Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory>();
    public DbSet<Sweet_Silvia_Domain.Payment.Payment> Payments => Set<Sweet_Silvia_Domain.Payment.Payment>();
    public DbSet<Sweet_Silvia_Domain.PaymentMethod.PaymentMethod> PaymentMethods => Set<Sweet_Silvia_Domain.PaymentMethod.PaymentMethod>();
    public DbSet<Sweet_Silvia_Domain.PaymentReceipt.PaymentReceipt> PaymentReceipts => Set<Sweet_Silvia_Domain.PaymentReceipt.PaymentReceipt>();
    public DbSet<Sweet_Silvia_Domain.PaymentReview.PaymentReview> PaymentReviews => Set<Sweet_Silvia_Domain.PaymentReview.PaymentReview>();
    public DbSet<Sweet_Silvia_Domain.Product.Product> Products => Set<Sweet_Silvia_Domain.Product.Product>();
    public DbSet<Sweet_Silvia_Domain.ProductImage.ProductImage> ProductImages => Set<Sweet_Silvia_Domain.ProductImage.ProductImage>();
    public DbSet<Sweet_Silvia_Domain.ProductVariant.ProductVariant> ProductVariants => Set<Sweet_Silvia_Domain.ProductVariant.ProductVariant>();
    public DbSet<Sweet_Silvia_Domain.Province.Province> Provinces => Set<Sweet_Silvia_Domain.Province.Province>();
    public DbSet<Sweet_Silvia_Domain.RefreshToken.RefreshToken> RefreshTokens => Set<Sweet_Silvia_Domain.RefreshToken.RefreshToken>();
    public DbSet<Sweet_Silvia_Domain.Role.Role> Roles => Set<Sweet_Silvia_Domain.Role.Role>();
    public DbSet<Sweet_Silvia_Domain.Shipment.Shipment> Shipments => Set<Sweet_Silvia_Domain.Shipment.Shipment>();
    public DbSet<Sweet_Silvia_Domain.ShipmentEvent.ShipmentEvent> ShipmentEvents => Set<Sweet_Silvia_Domain.ShipmentEvent.ShipmentEvent>();
    public DbSet<Sweet_Silvia_Domain.ShippingRate.ShippingRate> ShippingRates => Set<Sweet_Silvia_Domain.ShippingRate.ShippingRate>();
    public DbSet<Sweet_Silvia_Domain.StockMovement.StockMovement> StockMovements => Set<Sweet_Silvia_Domain.StockMovement.StockMovement>();
    public DbSet<Sweet_Silvia_Domain.User.User> Users => Set<Sweet_Silvia_Domain.User.User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
