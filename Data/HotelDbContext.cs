using HotelManagementLab1.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementLab1.Data;

public sealed class HotelDbContext : DbContext
{
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
    public DbSet<Phong> Phongs => Set<Phong>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<DichVu> DichVus => Set<DichVu>();
    public DbSet<TienNghi> TienNghis => Set<TienNghi>();
    public DbSet<ChiTietTienNghi> ChiTietTienNghis => Set<ChiTietTienNghi>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connection = Environment.GetEnvironmentVariable("HOTEL_DB_CONNECTION")
            ?? "Server=.\\SQLEXPRESS;Database=HotelManagement;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";
        options.UseSqlServer(connection);
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<NhanVien>().ToTable("NhanVien").HasKey(x => x.MaNV);
        model.Entity<TaiKhoan>().ToTable("TaiKhoan").HasKey(x => x.TenTK);
        model.Entity<LoaiPhong>().ToTable("LoaiPhong").HasKey(x => x.MaLPH);
        model.Entity<Phong>().ToTable("Phong").HasKey(x => x.MaPH);
        model.Entity<KhachHang>().ToTable("KhachHang").HasKey(x => x.MaKH);
        model.Entity<KhachHang>().Property(x => x.CccdPassport).HasColumnName("CCCD/Passport");
        model.Entity<DichVu>().ToTable("DichVu").HasKey(x => x.MaDV);
        model.Entity<TienNghi>().ToTable("TienNghi").HasKey(x => x.MaTN);
        model.Entity<ChiTietTienNghi>().ToTable("CTTN").HasKey(x => new { x.MaLPH, x.MaTN });

        model.Entity<TaiKhoan>().HasOne(x => x.NhanVien).WithMany(x => x.TaiKhoans)
            .HasForeignKey(x => x.MaNV).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Phong>().HasOne(x => x.LoaiPhong).WithMany(x => x.Phongs)
            .HasForeignKey(x => x.MaLPH).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ChiTietTienNghi>().HasOne(x => x.LoaiPhong).WithMany(x => x.ChiTietTienNghis)
            .HasForeignKey(x => x.MaLPH).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ChiTietTienNghi>().HasOne(x => x.TienNghi).WithMany(x => x.ChiTietTienNghis)
            .HasForeignKey(x => x.MaTN).OnDelete(DeleteBehavior.Restrict);
    }
}
