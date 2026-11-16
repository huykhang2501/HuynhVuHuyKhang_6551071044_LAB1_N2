using System.ComponentModel.DataAnnotations;

namespace HotelManagementLab1.Models;

public sealed class NhanVien
{
    [Key] public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public bool? DaXoa { get; set; }
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = [];
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
