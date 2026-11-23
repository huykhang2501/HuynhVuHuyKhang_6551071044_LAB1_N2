using HotelManagementLab1.Data;
using HotelManagementLab1.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementLab1;

internal abstract class Lab2CrudControl : UserControl
{
    protected readonly DataGridView Grid = new();
    protected readonly TextBox Search;
    private readonly string _exportName;

    protected Lab2CrudControl(string placeholder, string addText, string exportName, bool canDelete)
    {
        _exportName = exportName;
        BackColor = Color.Transparent;
        Ui.ConfigureGrid(Grid);
        Search = new TextBox { PlaceholderText = placeholder, Width = 250, Location = new Point(0, 12) };
        Search.TextChanged += (_, _) => Reload();
        var bar = new Panel { Dock = DockStyle.Top, Height = 56 };
        var add = MakeButton(addText, 262, 112, true, (_, _) => AddItem());
        var edit = MakeButton("Sửa", 382, 76, false, (_, _) => EditItem());
        var remove = MakeButton("Xóa", 466, 76, false, (_, _) => DeleteItem());
        remove.Enabled = canDelete;
        var export = MakeButton("Xuất Excel", 550, 112, false, (_, _) => ExcelExport.Save(Grid, _exportName));
        bar.Controls.AddRange([Search, add, edit, remove, export]);
        Controls.Add(Grid); Controls.Add(bar);
        Grid.CellDoubleClick += (_, _) => EditItem();
        Load += (_, _) => Reload();
    }

    private static Button MakeButton(string text, int x, int width, bool primary, EventHandler action)
    {
        var button = new Button { Text = text, Width = width, Height = 34, Location = new Point(x, 11) };
        if (primary) Ui.StylePrimaryButton(button); else Ui.StyleSecondaryButton(button);
        button.Click += action; return button;
    }

    protected string? CurrentKey() => Grid.CurrentRow?.Cells[0].Value?.ToString();
    protected abstract void Reload();
    protected abstract void AddItem();
    protected abstract void EditItem();
    protected abstract void DeleteItem();
}

internal sealed class ServicesControl : Lab2CrudControl
{
    public ServicesControl(bool canDelete) : base("Tìm mã hoặc tên dịch vụ...", "Thêm dịch vụ", "DanhSachDichVu", canDelete) { }
    protected override void Reload()
    {
        try
        {
            using var db = new HotelDbContext(); var q = Search.Text.Trim();
            Grid.DataSource = db.DichVus.AsNoTracking().Where(x => x.DaXoa != true && (q == "" || x.MaDV.Contains(q) || x.TenDV.Contains(q) || x.LoaiDV.Contains(q)))
                .OrderBy(x => x.MaDV).Select(x => new { Mã_dịch_vụ = x.MaDV, Tên_dịch_vụ = x.TenDV, Loại = x.LoaiDV, Đơn_giá = x.DonGia, Số_lượng_còn_lại = x.SLConLai < 0 ? "Không giới hạn" : x.SLConLai.ToString() }).ToList();
        }
        catch (Exception ex) { Ui.Error("Không tải được dịch vụ.\n" + ex.Message); }
    }
    protected override void AddItem() { using var f = new ServiceDialog(); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void EditItem() { if (CurrentKey() is not { } key) { Ui.Warn("Hãy chọn dịch vụ cần sửa."); return; } using var f = new ServiceDialog(key); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void DeleteItem()
    {
        if (CurrentKey() is not { } key || MessageBox.Show($"Xóa dịch vụ {key}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { using var db = new HotelDbContext(); var item = db.DichVus.Find(key); if (item is not null) { item.DaXoa = true; db.SaveChanges(); Reload(); } } catch (Exception ex) { Ui.Error(ex.Message); }
    }
}

internal sealed class AmenitiesControl : Lab2CrudControl
{
    public AmenitiesControl(bool canDelete) : base("Tìm mã hoặc tên tiện nghi...", "Thêm tiện nghi", "DanhSachTienNghi", canDelete) { }
    protected override void Reload()
    {
        try { using var db = new HotelDbContext(); var q = Search.Text.Trim(); Grid.DataSource = db.TienNghis.AsNoTracking().Where(x => x.DaXoa != true && (q == "" || x.MaTN.Contains(q) || x.TenTN.Contains(q))).OrderBy(x => x.MaTN).Select(x => new { Mã_tiện_nghi = x.MaTN, Tên_tiện_nghi = x.TenTN }).ToList(); }
        catch (Exception ex) { Ui.Error("Không tải được tiện nghi.\n" + ex.Message); }
    }
    protected override void AddItem() { using var f = new AmenityDialog(); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void EditItem() { if (CurrentKey() is not { } key) { Ui.Warn("Hãy chọn tiện nghi cần sửa."); return; } using var f = new AmenityDialog(key); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void DeleteItem()
    {
        if (CurrentKey() is not { } key || MessageBox.Show($"Xóa tiện nghi {key}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { using var db = new HotelDbContext(); if (db.ChiTietTienNghis.Any(x => x.MaTN == key && x.DaXoa != true)) { Ui.Warn("Không thể xóa tiện nghi đang được gán cho loại phòng."); return; } var item = db.TienNghis.Find(key); if (item is not null) { item.DaXoa = true; db.SaveChanges(); Reload(); } } catch (Exception ex) { Ui.Error(ex.Message); }
    }
}

internal sealed class CustomersControl : Lab2CrudControl
{
    public CustomersControl(bool canDelete) : base("Tìm mã, tên, CCCD hoặc điện thoại...", "Thêm khách", "DanhSachKhachHang", canDelete) { }
    protected override void Reload()
    {
        try
        {
            using var db = new HotelDbContext(); var q = Search.Text.Trim();
            Grid.DataSource = db.KhachHangs.AsNoTracking().Where(x => x.DaXoa != true && (q == "" || x.MaKH.Contains(q) || x.TenKH.Contains(q) || x.CccdPassport.Contains(q) || (x.Sdt ?? "").Contains(q)))
                .OrderBy(x => x.MaKH).Select(x => new { Mã_khách = x.MaKH, Tên_khách_hàng = x.TenKH, Số_điện_thoại = x.Sdt ?? "", CCCD_Passport = x.CccdPassport, Quốc_tịch = x.QuocTich, Giới_tính = x.GioiTinh }).ToList();
        }
        catch (Exception ex) { Ui.Error("Không tải được khách hàng.\n" + ex.Message); }
    }
    protected override void AddItem() { using var f = new CustomerDialog(); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void EditItem() { if (CurrentKey() is not { } key) { Ui.Warn("Hãy chọn khách hàng cần sửa."); return; } using var f = new CustomerDialog(key); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void DeleteItem()
    {
        if (CurrentKey() is not { } key || MessageBox.Show($"Xóa khách hàng {key}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { using var db = new HotelDbContext(); var item = db.KhachHangs.Find(key); if (item is not null) { item.DaXoa = true; db.SaveChanges(); Reload(); } } catch (Exception ex) { Ui.Error(ex.Message); }
    }
}

internal sealed class EmployeesControl : Lab2CrudControl
{
    public EmployeesControl(bool canDelete) : base("Tìm mã, tên, chức vụ hoặc điện thoại...", "Thêm nhân viên", "DanhSachNhanVien", canDelete) { }
    protected override void Reload()
    {
        try
        {
            using var db = new HotelDbContext(); var q = Search.Text.Trim();
            Grid.DataSource = db.NhanViens.AsNoTracking().Where(x => x.DaXoa != true && (q == "" || x.MaNV.Contains(q) || x.TenNV.Contains(q) || x.ChucVu.Contains(q) || x.Sdt.Contains(q)))
                .OrderBy(x => x.MaNV).Select(x => new { Mã_nhân_viên = x.MaNV, Họ_tên = x.TenNV, Chức_vụ = x.ChucVu, Lương = x.Luong, Số_điện_thoại = x.Sdt, CCCD = x.Cccd, Ngày_sinh = x.NgaySinh, Giới_tính = x.GioiTinh, Email = x.Email ?? "" }).ToList();
        }
        catch (Exception ex) { Ui.Error("Không tải được nhân viên.\n" + ex.Message); }
    }
    protected override void AddItem() { using var f = new EmployeeDialog(); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void EditItem() { if (CurrentKey() is not { } key) { Ui.Warn("Hãy chọn nhân viên cần sửa."); return; } using var f = new EmployeeDialog(key); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    protected override void DeleteItem()
    {
        if (CurrentKey() is not { } key || MessageBox.Show($"Xóa nhân viên {key}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { using var db = new HotelDbContext(); if (db.TaiKhoans.Any(x => x.MaNV == key && x.DaXoa != true)) { Ui.Warn("Không thể xóa nhân viên đang có tài khoản hoạt động."); return; } var item = db.NhanViens.Find(key); if (item is not null) { item.DaXoa = true; db.SaveChanges(); Reload(); } } catch (Exception ex) { Ui.Error(ex.Message); }
    }
}

internal sealed class RoomAmenitiesControl : UserControl
{
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly DataGridView _grid = new();
    private readonly bool _canDelete;
    public RoomAmenitiesControl(bool canDelete)
    {
        _canDelete = canDelete; BackColor = Color.Transparent; Ui.ConfigureGrid(_grid);
        var bar = new Panel { Dock = DockStyle.Top, Height = 64 };
        bar.Controls.Add(new Label { Text = "Loại phòng", AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(0, 2) });
        _type.Location = new Point(0, 22); _type.SelectedIndexChanged += (_, _) => Reload(); bar.Controls.Add(_type);
        var add = Button("Thêm tiện nghi", 295, 120, true, (_, _) => AddItem()); var edit = Button("Sửa", 423, 76, false, (_, _) => EditItem()); var remove = Button("Xóa", 507, 76, false, (_, _) => DeleteItem()); remove.Enabled = canDelete;
        bar.Controls.AddRange([add, edit, remove]); Controls.Add(_grid); Controls.Add(bar); _grid.CellDoubleClick += (_, _) => EditItem(); Load += (_, _) => LoadTypes();
    }
    private static Button Button(string text, int x, int width, bool primary, EventHandler action) { var b = new Button { Text = text, Width = width, Height = 34, Location = new Point(x, 21) }; if (primary) Ui.StylePrimaryButton(b); else Ui.StyleSecondaryButton(b); b.Click += action; return b; }
    private void LoadTypes()
    {
        try { using var db = new HotelDbContext(); _type.DataSource = db.LoaiPhongs.AsNoTracking().OrderBy(x => x.MaLPH).Select(x => new Option(x.MaLPH, x.TenLPH)).ToList(); _type.DisplayMember = nameof(Option.Name); _type.ValueMember = nameof(Option.Id); }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
    private void Reload()
    {
        if (_type.SelectedValue is not string typeId) return;
        try { using var db = new HotelDbContext(); _grid.DataSource = db.ChiTietTienNghis.AsNoTracking().Include(x => x.TienNghi).Where(x => x.MaLPH == typeId && x.DaXoa != true).OrderBy(x => x.MaTN).Select(x => new { Mã_tiện_nghi = x.MaTN, Tên_tiện_nghi = x.TienNghi!.TenTN, Số_lượng = x.SL < 0 ? "Không giới hạn" : x.SL.ToString() }).ToList(); }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
    private string? TypeId => _type.SelectedValue?.ToString();
    private void AddItem() { if (TypeId is not { } type) return; using var f = new RoomAmenityDialog(type); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    private void EditItem() { if (TypeId is not { } type || _grid.CurrentRow?.Cells[0].Value?.ToString() is not { } amenity) { Ui.Warn("Hãy chọn tiện nghi cần sửa."); return; } using var f = new RoomAmenityDialog(type, amenity); if (f.ShowDialog(this) == DialogResult.OK) Reload(); }
    private void DeleteItem()
    {
        if (TypeId is not { } type || _grid.CurrentRow?.Cells[0].Value?.ToString() is not { } amenity || MessageBox.Show("Xóa tiện nghi khỏi loại phòng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { using var db = new HotelDbContext(); var item = db.ChiTietTienNghis.Find(type, amenity); if (item is not null) { item.DaXoa = true; db.SaveChanges(); Reload(); } } catch (Exception ex) { Ui.Error(ex.Message); }
    }
    private sealed record Option(string Id, string Name) { public override string ToString() => $"{Id} - {Name}"; }
}

internal abstract class Lab2Dialog : Form
{
    protected readonly FlowLayoutPanel Fields = new() { Dock = DockStyle.Top, Height = 460, Padding = new Padding(34, 22, 34, 0), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
    protected Lab2Dialog(string title, int height = 540)
    {
        Text = title; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(440, height); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        Controls.Add(Fields);
    }
    protected Button SaveButton(int y, EventHandler handler) { var button = new Button { Text = "LƯU", Width = 125, Height = 38, Location = new Point(158, y) }; Ui.StylePrimaryButton(button); button.Click += handler; Controls.Add(button); return button; }
}

internal sealed class ServiceDialog : Lab2Dialog
{
    private readonly string? _key; private readonly TextBox _id = new(); private readonly TextBox _name = new(); private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList }; private readonly NumericUpDown _price = Money(); private readonly NumericUpDown _stock = new() { Minimum = -1, Maximum = 999999, Value = -1, ThousandsSeparator = true };
    public ServiceDialog(string? key = null) : base(key is null ? "Thêm dịch vụ" : "Cập nhật dịch vụ") { _key = key; _kind.Items.AddRange(["Thức uống", "Đồ ăn", "Dịch vụ"]); _kind.SelectedIndex = 0; Fields.Controls.AddRange([Ui.Field("Mã dịch vụ", _id), Ui.Field("Tên dịch vụ", _name), Ui.Field("Loại dịch vụ", _kind), Ui.Field("Đơn giá", _price), Ui.Field("Số lượng còn lại (-1 = không giới hạn)", _stock)]); if (key is not null) LoadItem(); SaveButton(480, (_, _) => Save()); }
    private void LoadItem() { using var db = new HotelDbContext(); var x = db.DichVus.AsNoTracking().First(x => x.MaDV == _key); _id.Text = x.MaDV; _id.ReadOnly = true; _name.Text = x.TenDV; _kind.Text = x.LoaiDV; _price.Value = x.DonGia; _stock.Value = x.SLConLai; }
    private void Save() { var id = _id.Text.Trim().ToUpperInvariant(); if (id == "" || _name.Text.Trim() == "") { Ui.Warn("Nhập mã và tên dịch vụ."); return; } try { using var db = new HotelDbContext(); var x = _key is null ? null : db.DichVus.Find(_key); if (x is null) { if (db.DichVus.Any(y => y.MaDV == id)) { Ui.Warn("Mã dịch vụ đã tồn tại."); return; } x = new DichVu { MaDV = id }; db.Add(x); } x.TenDV = _name.Text.Trim(); x.LoaiDV = _kind.Text; x.DonGia = _price.Value; x.SLConLai = Decimal.ToInt32(_stock.Value); x.DaXoa = false; db.SaveChanges(); DialogResult = DialogResult.OK; } catch (Exception ex) { Ui.Error(ex.Message); } }
    private static NumericUpDown Money() => new() { Minimum = 0, Maximum = 100000000, Increment = 5000, ThousandsSeparator = true };
}

internal sealed class AmenityDialog : Lab2Dialog
{
    private readonly string? _key; private readonly TextBox _id = new(); private readonly TextBox _name = new();
    public AmenityDialog(string? key = null) : base(key is null ? "Thêm tiện nghi" : "Cập nhật tiện nghi", 290) { _key = key; Fields.Height = 190; Fields.Controls.AddRange([Ui.Field("Mã tiện nghi", _id), Ui.Field("Tên tiện nghi", _name)]); if (key is not null) LoadItem(); SaveButton(225, (_, _) => Save()); }
    private void LoadItem() { using var db = new HotelDbContext(); var x = db.TienNghis.AsNoTracking().First(x => x.MaTN == _key); _id.Text = x.MaTN; _id.ReadOnly = true; _name.Text = x.TenTN; }
    private void Save() { var id = _id.Text.Trim().ToUpperInvariant(); if (id == "" || _name.Text.Trim() == "") { Ui.Warn("Nhập mã và tên tiện nghi."); return; } try { using var db = new HotelDbContext(); var x = _key is null ? null : db.TienNghis.Find(_key); if (x is null) { if (db.TienNghis.Any(y => y.MaTN == id)) { Ui.Warn("Mã tiện nghi đã tồn tại."); return; } x = new TienNghi { MaTN = id }; db.Add(x); } x.TenTN = _name.Text.Trim(); x.DaXoa = false; db.SaveChanges(); DialogResult = DialogResult.OK; } catch (Exception ex) { Ui.Error(ex.Message); } }
}

internal sealed class CustomerDialog : Lab2Dialog
{
    private readonly string? _key; private readonly TextBox _id = new(); private readonly TextBox _name = new(); private readonly TextBox _phone = new(); private readonly TextBox _identity = new(); private readonly TextBox _nationality = new() { Text = "Việt Nam" }; private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public CustomerDialog(string? key = null) : base(key is null ? "Thêm khách hàng" : "Cập nhật khách hàng", 610) { _key = key; _gender.Items.AddRange(["Nam", "Nữ"]); _gender.SelectedIndex = 0; Fields.Height = 520; Fields.Controls.AddRange([Ui.Field("Mã khách hàng", _id), Ui.Field("Tên khách hàng", _name), Ui.Field("Số điện thoại", _phone), Ui.Field("CCCD/Passport", _identity), Ui.Field("Quốc tịch", _nationality), Ui.Field("Giới tính", _gender)]); if (key is not null) LoadItem(); SaveButton(545, (_, _) => Save()); }
    private void LoadItem() { using var db = new HotelDbContext(); var x = db.KhachHangs.AsNoTracking().First(x => x.MaKH == _key); _id.Text = x.MaKH; _id.ReadOnly = true; _name.Text = x.TenKH; _phone.Text = x.Sdt; _identity.Text = x.CccdPassport; _nationality.Text = x.QuocTich; _gender.Text = x.GioiTinh; }
    private void Save() { var id = _id.Text.Trim().ToUpperInvariant(); if (id == "" || _name.Text.Trim() == "" || _identity.Text.Trim() == "" || _nationality.Text.Trim() == "") { Ui.Warn("Nhập đủ mã, tên, CCCD/Passport và quốc tịch."); return; } try { using var db = new HotelDbContext(); var x = _key is null ? null : db.KhachHangs.Find(_key); if (x is null) { if (db.KhachHangs.Any(y => y.MaKH == id)) { Ui.Warn("Mã khách hàng đã tồn tại."); return; } x = new KhachHang { MaKH = id }; db.Add(x); } x.TenKH = _name.Text.Trim(); x.Sdt = _phone.Text.Trim(); x.CccdPassport = _identity.Text.Trim(); x.QuocTich = _nationality.Text.Trim(); x.GioiTinh = _gender.Text; x.DaXoa = false; db.SaveChanges(); DialogResult = DialogResult.OK; } catch (Exception ex) { Ui.Error("Không thể lưu khách hàng.\n" + ex.Message); } }
}

internal sealed class EmployeeDialog : Lab2Dialog
{
    private readonly string? _key; private readonly TextBox _id = new(); private readonly TextBox _name = new(); private readonly TextBox _position = new(); private readonly NumericUpDown _salary = new() { Minimum = 0, Maximum = 100000000, Increment = 500000, ThousandsSeparator = true }; private readonly TextBox _phone = new(); private readonly TextBox _identity = new(); private readonly DateTimePicker _birth = new() { Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today }; private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList }; private readonly TextBox _address = new(); private readonly TextBox _email = new();
    public EmployeeDialog(string? key = null) : base(key is null ? "Thêm nhân viên" : "Cập nhật nhân viên", 690) { _key = key; _gender.Items.AddRange(["Nam", "Nữ"]); _gender.SelectedIndex = 0; Fields.Height = 600; Fields.Controls.AddRange([Ui.Field("Mã nhân viên", _id), Ui.Field("Họ tên", _name), Ui.Field("Chức vụ", _position), Ui.Field("Lương", _salary), Ui.Field("Số điện thoại", _phone), Ui.Field("CCCD", _identity), Ui.Field("Ngày sinh", _birth), Ui.Field("Giới tính", _gender), Ui.Field("Địa chỉ", _address), Ui.Field("Email", _email)]); if (key is not null) LoadItem(); SaveButton(625, (_, _) => Save()); }
    private void LoadItem() { using var db = new HotelDbContext(); var x = db.NhanViens.AsNoTracking().First(x => x.MaNV == _key); _id.Text = x.MaNV; _id.ReadOnly = true; _name.Text = x.TenNV; _position.Text = x.ChucVu; _salary.Value = x.Luong; _phone.Text = x.Sdt; _identity.Text = x.Cccd; _birth.Value = x.NgaySinh; _gender.Text = x.GioiTinh; _address.Text = x.DiaChi; _email.Text = x.Email; }
    private void Save() { var id = _id.Text.Trim().ToUpperInvariant(); if (id == "" || _name.Text.Trim() == "" || _position.Text.Trim() == "" || _phone.Text.Trim() == "" || _identity.Text.Trim() == "" || _address.Text.Trim() == "") { Ui.Warn("Hãy nhập đủ các trường bắt buộc."); return; } try { using var db = new HotelDbContext(); var x = _key is null ? null : db.NhanViens.Find(_key); if (x is null) { if (db.NhanViens.Any(y => y.MaNV == id)) { Ui.Warn("Mã nhân viên đã tồn tại."); return; } x = new NhanVien { MaNV = id }; db.Add(x); } x.TenNV = _name.Text.Trim(); x.ChucVu = _position.Text.Trim(); x.Luong = _salary.Value; x.Sdt = _phone.Text.Trim(); x.Cccd = _identity.Text.Trim(); x.NgaySinh = _birth.Value.Date; x.GioiTinh = _gender.Text; x.DiaChi = _address.Text.Trim(); x.Email = string.IsNullOrWhiteSpace(_email.Text) ? null : _email.Text.Trim(); x.DaXoa = false; db.SaveChanges(); DialogResult = DialogResult.OK; } catch (Exception ex) { Ui.Error("Không thể lưu nhân viên.\n" + ex.Message); } }
}

internal sealed class RoomAmenityDialog : Lab2Dialog
{
    private readonly string _type; private readonly string? _amenity; private readonly ComboBox _item = new() { DropDownStyle = ComboBoxStyle.DropDownList }; private readonly NumericUpDown _quantity = new() { Minimum = -1, Maximum = 9999, Value = 1, ThousandsSeparator = true };
    public RoomAmenityDialog(string type, string? amenity = null) : base(amenity is null ? "Thêm tiện nghi cho loại phòng" : "Cập nhật tiện nghi", 310) { _type = type; _amenity = amenity; Fields.Height = 200; LoadAmenities(); Fields.Controls.AddRange([Ui.Field("Tiện nghi", _item), Ui.Field("Số lượng (-1 = không giới hạn)", _quantity)]); if (amenity is not null) { _item.SelectedValue = amenity; _item.Enabled = false; using var db = new HotelDbContext(); _quantity.Value = db.ChiTietTienNghis.AsNoTracking().First(x => x.MaLPH == type && x.MaTN == amenity).SL; } SaveButton(240, (_, _) => Save()); }
    private void LoadAmenities() { using var db = new HotelDbContext(); _item.DataSource = db.TienNghis.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.TenTN).Select(x => new AmenityOption(x.MaTN, x.TenTN)).ToList(); _item.DisplayMember = nameof(AmenityOption.Name); _item.ValueMember = nameof(AmenityOption.Id); }
    private void Save() { if (_item.SelectedValue is not string amenity) return; try { using var db = new HotelDbContext(); var x = db.ChiTietTienNghis.Find(_type, amenity); if (x is null) { x = new ChiTietTienNghi { MaLPH = _type, MaTN = amenity }; db.Add(x); } x.SL = Decimal.ToInt32(_quantity.Value); x.DaXoa = false; db.SaveChanges(); DialogResult = DialogResult.OK; } catch (Exception ex) { Ui.Error("Không thể lưu tiện nghi cho loại phòng.\n" + ex.Message); } }
    private sealed record AmenityOption(string Id, string Name);
}
