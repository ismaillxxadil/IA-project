using Microsoft.EntityFrameworkCore;
using SmartRentApi.Models; // لاستدعاء الجداول التي أنشأناها

namespace SmartRentApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor لاستقبال إعدادات الاتصال بقاعدة البيانات
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // تمثيل الجداول في قاعدة البيانات (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<VisitRequest> VisitRequests { get; set; }
        public DbSet<RentalApplication> RentalApplications { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // إعدادات إضافية لمنع خطأ الحذف المتسلسل (Cascade Delete) في SQL Server
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // منع الحذف المتسلسل لطلبات الزيارة لتجنب تعارض العلاقات
            modelBuilder.Entity<VisitRequest>()
                .HasOne(v => v.Tenant)
                .WithMany(u => u.VisitRequests)
                .HasForeignKey(v => v.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // منع الحذف المتسلسل لطلبات الإيجار
            modelBuilder.Entity<RentalApplication>()
                .HasOne(r => r.Tenant)
                .WithMany(u => u.RentalApplications)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // منع الحذف المتسلسل للتقييمات
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tenant)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}