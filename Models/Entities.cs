using System.ComponentModel.DataAnnotations;

namespace HotelManagementLab1.Models;

public sealed class NhanVien
{
    [Key] public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public decimal Luong { get; set; }
    public string Sdt { get; set; } = string.Empty;
    public string Cccd { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; }
    public string GioiTinh { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool? DaXoa { get; set; }
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = [];
}

public sealed class KhachHang
{
    [Key] public string MaKH { get; set; } = string.Empty;
    public string TenKH { get; set; } = string.Empty;
    public string? Sdt { get; set; }
    public string CccdPassport { get; set; } = string.Empty;
    public string QuocTich { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    public bool? DaXoa { get; set; }
}

public sealed class DichVu
{
    [Key] public string MaDV { get; set; } = string.Empty;
    public string TenDV { get; set; } = string.Empty;
    public decimal DonGia { get; set; }
    public int SLConLai { get; set; } = -1;
    public string LoaiDV { get; set; } = string.Empty;
    public bool? DaXoa { get; set; }
}

public sealed class TienNghi
{
    [Key] public string MaTN { get; set; } = string.Empty;
    public string TenTN { get; set; } = string.Empty;
    public bool? DaXoa { get; set; }
    public ICollection<ChiTietTienNghi> ChiTietTienNghis { get; set; } = [];
}

public sealed class ChiTietTienNghi
{
    public string MaLPH { get; set; } = string.Empty;
    public string MaTN { get; set; } = string.Empty;
    public int SL { get; set; }
    public bool? DaXoa { get; set; }
    public LoaiPhong? LoaiPhong { get; set; }
    public TienNghi? TienNghi { get; set; }
}

public sealed class TaiKhoan
{
    [Key] public string TenTK { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int CapDoQuyen { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public bool? DaXoa { get; set; }
    public NhanVien? NhanVien { get; set; }
}

public sealed class LoaiPhong
{
    [Key] public string MaLPH { get; set; } = string.Empty;
    public string TenLPH { get; set; } = string.Empty;
    public int SoGiuong { get; set; }
    public int SoNguoiToiDa { get; set; }
    public decimal GiaNgay { get; set; }
    public decimal GiaGio { get; set; }
    public ICollection<Phong> Phongs { get; set; } = [];
    public ICollection<ChiTietTienNghi> ChiTietTienNghis { get; set; } = [];
}

public sealed class Phong
{
    [Key] public string MaPH { get; set; } = string.Empty;
    public string TTPH { get; set; } = string.Empty;
    public string TTDD { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public string MaLPH { get; set; } = string.Empty;
    public bool? DaXoa { get; set; }
    public LoaiPhong? LoaiPhong { get; set; }
}

public sealed record UserSession(string UserName, string StaffName, string StaffId, int Role);
