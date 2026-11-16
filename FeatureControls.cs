using ClosedXML.Excel;
using HotelManagementLab1.Data;
using HotelManagementLab1.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementLab1;

public sealed class HomeControl : UserControl
{
    private readonly Label _roomCount = Card("...");
    private readonly Label _typeCount = Card("...");
    private readonly Label _userLabel;

    public HomeControl(UserSession session, Action roomsBoard, Action rooms, Action types)
    {
        BackColor = Color.FromArgb(247, 250, 249);
        _userLabel = new Label { Text = $"Xin chào, {session.StaffName}!", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Ui.Ink, AutoSize = true, Location = new Point(5, 8) };
        var summary = new FlowLayoutPanel { Location = new Point(2, 60), Width = 900, Height = 128, FlowDirection = FlowDirection.LeftToRight };
        summary.Controls.Add(MakeCard("PHÒNG ĐANG QUẢN LÝ", _roomCount, Color.FromArgb(225, 243, 238)));
        summary.Controls.Add(MakeCard("LOẠI PHÒNG", _typeCount, Color.FromArgb(228, 238, 250)));
        var quick = new Label { Text = "Truy cập nhanh", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Ui.Ink, AutoSize = true, Location = new Point(5, 220) };
        var buttons = new FlowLayoutPanel { Location = new Point(4, 260), Width = 900, Height = 70 };
        buttons.Controls.Add(Quick("Sơ đồ phòng", roomsBoard)); buttons.Controls.Add(Quick("Quản lý phòng", rooms)); buttons.Controls.Add(Quick("Loại phòng", types));
        Controls.AddRange([_userLabel, summary, quick, buttons]);
        Load += (_, _) => LoadSummary();
    }
    private static Label Card(string text) => new() { Text = text, Font = new Font("Segoe UI", 25, FontStyle.Bold), ForeColor = Ui.Ink, AutoSize = true, Location = new Point(20, 43) };
    private static Panel MakeCard(string caption, Label value, Color color)
    {
        var panel = new Panel { BackColor = color, Width = 255, Height = 115, Margin = new Padding(0, 0, 18, 0) };
        panel.Controls.Add(new Label { Text = caption, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DimGray, AutoSize = true, Location = new Point(20, 17) });
        panel.Controls.Add(value); return panel;
    }
    private static Button Quick(string text, Action action)
    {
        var button = new Button { Text = text, Width = 185, Height = 44, Margin = new Padding(0, 0, 13, 0) };
        Ui.StylePrimaryButton(button); button.Click += (_, _) => action(); return button;
    }
    private void LoadSummary()
    {
        try { using var db = new HotelDbContext(); _roomCount.Text = db.Phongs.Count(x => x.DaXoa != true).ToString(); _typeCount.Text = db.LoaiPhongs.Count().ToString(); }
        catch { _roomCount.Text = _typeCount.Text = "-"; }
    }
}

public sealed class RoomBoardControl : UserControl
{
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
    private readonly ComboBox _clean = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly TextBox _query = new() { PlaceholderText = "Tìm mã phòng...", Width = 170 };
    private readonly DataGridView _grid = new();

    public RoomBoardControl()
    {
        BackColor = Color.Transparent; Ui.ConfigureGrid(_grid);
        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 82, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 10, 0, 10) };
        _status.Items.AddRange(["Tất cả trạng thái", "Bình thường", "Đã đặt", "Đang thuê", "Đang sửa chữa"]);
        _clean.Items.AddRange(["Tất cả dọn dẹp", "Đã dọn dẹp", "Chưa dọn dẹp"]);
        _status.SelectedIndex = _clean.SelectedIndex = 0;
        _status.SelectedIndexChanged += (_, _) => LoadRows(); _type.SelectedIndexChanged += (_, _) => LoadRows(); _clean.SelectedIndexChanged += (_, _) => LoadRows(); _query.TextChanged += (_, _) => LoadRows();
        filter.Controls.AddRange([Caption("Trạng thái", _status), Caption("Loại phòng", _type), Caption("Dọn dẹp", _clean), Caption("Tìm kiếm", _query)]);
        var book = new Button { Text = "ĐẶT PHÒNG", Width = 125, Height = 34, Margin = new Padding(15, 22, 0, 0) }; Ui.StylePrimaryButton(book); book.Click += (_, _) => Reserve(); filter.Controls.Add(book);
        Controls.Add(_grid); Controls.Add(filter); Load += (_, _) => LoadTypes();
    }
    private static Control Caption(string text, Control control)
    {
        var panel = new Panel { Width = control.Width + 8, Height = 56, Margin = new Padding(0, 0, 12, 0) };
        panel.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DimGray, AutoSize = true, Location = new Point(0, 0) });
        control.Location = new Point(0, 18); panel.Controls.Add(control); return panel;
    }
    private void LoadTypes()
    {
        try
        {
            using var db = new HotelDbContext();
            _type.Items.Clear(); _type.Items.Add("Tất cả loại phòng");
            foreach (var t in db.LoaiPhongs.AsNoTracking().OrderBy(x => x.MaLPH)) _type.Items.Add(new TypeOption(t.MaLPH, t.TenLPH));
            _type.SelectedIndex = 0; LoadRows();
        }
        catch (Exception ex) { Ui.Error("Không tải được dữ liệu phòng.\n" + ex.Message); }
    }
    private void LoadRows()
    {
        if (_type.Items.Count == 0) return;
        try
        {
            using var db = new HotelDbContext();
            var rows = db.Phongs.AsNoTracking().Include(x => x.LoaiPhong).Where(x => x.DaXoa != true).AsEnumerable();
            if (_status.SelectedIndex > 0) rows = rows.Where(x => x.TTPH == _status.SelectedItem!.ToString());
            if (_clean.SelectedIndex > 0) rows = rows.Where(x => x.TTDD == _clean.SelectedItem!.ToString());
            if (_type.SelectedItem is TypeOption option) rows = rows.Where(x => x.MaLPH == option.Id);
            var q = _query.Text.Trim(); if (q.Length > 0) rows = rows.Where(x => x.MaPH.Contains(q, StringComparison.OrdinalIgnoreCase));
            _grid.DataSource = rows.OrderBy(x => x.MaPH).Select(x => new { Mã_phòng = x.MaPH, Trạng_thái = x.TTPH, Dọn_dẹp = x.TTDD, Loại_phòng = x.LoaiPhong?.TenLPH ?? x.MaLPH, Ghi_chú = x.GhiChu ?? "" }).ToList();
        }
        catch { }
    }
    private void Reserve()
    {
        var code = _grid.CurrentRow?.Cells[0].Value?.ToString();
        Ui.Info(string.IsNullOrWhiteSpace(code) ? "Hãy chọn một phòng trước." : $"Đã chọn phòng {code}. Chức năng tạo phiếu thuê sẽ làm ở lab tiếp theo.");
    }
    private sealed record TypeOption(string Id, string Name) { public override string ToString() => $"{Id} - {Name}"; }
}

public sealed class RoomsControl : UserControl
{
    private readonly DataGridView _grid = new();
    private readonly TextBox _query = new() { PlaceholderText = "Tìm mã phòng...", Width = 220 };
    public RoomsControl()
    {
        BackColor = Color.Transparent; Ui.ConfigureGrid(_grid);
        var bar = Toolbar(_query, "Thêm phòng", Add, "Xuất Excel", Export);
        bar.Controls.OfType<Button>().Single(x => x.Text == "Sửa").Click += (_, _) => Edit();
        bar.Controls.OfType<Button>().Single(x => x.Text == "Xóa").Click += (_, _) => Delete();
        _query.TextChanged += (_, _) => LoadRows();
        _grid.CellDoubleClick += (_, _) => Edit();
        Controls.Add(_grid); Controls.Add(bar); Load += (_, _) => LoadRows();
    }
    private void LoadRows()
    {
        try
        {
            using var db = new HotelDbContext(); var q = _query.Text.Trim();
            _grid.DataSource = db.Phongs.AsNoTracking().Include(x => x.LoaiPhong).Where(x => x.DaXoa != true && (q == "" || x.MaPH.Contains(q)))
                .OrderBy(x => x.MaPH).Select(x => new RoomRow(x.MaPH, x.TTPH, x.TTDD, x.LoaiPhong!.TenLPH, x.GhiChu ?? "")).ToList();
        }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
    private void Add()
    {
        using var form = new RoomDialog(); if (form.ShowDialog(this) == DialogResult.OK) LoadRows();
    }
    private void Edit()
    {
        if (CurrentCode() is not { } code) { Ui.Warn("Hãy chọn phòng cần sửa."); return; }
        using var form = new RoomDialog(code); if (form.ShowDialog(this) == DialogResult.OK) LoadRows();
    }
    private void Delete()
    {
        if (CurrentCode() is not { } code || MessageBox.Show($"Xóa mềm phòng {code}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        using var db = new HotelDbContext(); var room = db.Phongs.Find(code); if (room is null) return; room.DaXoa = true; db.SaveChanges(); LoadRows();
    }
    private string? CurrentCode() => _grid.CurrentRow?.Cells[0].Value?.ToString();
    private void Export() => ExcelExport.Save(_grid, "DanhSachPhong");
    private static Panel Toolbar(TextBox search, string addText, Action add, string exportText, Action export)
    {
        var bar = new Panel { Dock = DockStyle.Top, Height = 56 }; search.Location = new Point(0, 12); bar.Controls.Add(search);
        var addBtn = new Button { Text = addText, Width = 120, Height = 34, Location = new Point(238, 11) }; Ui.StylePrimaryButton(addBtn); addBtn.Click += (_, _) => add();
        var editBtn = new Button { Text = "Sửa", Width = 78, Height = 34, Location = new Point(368, 11) }; Ui.StyleSecondaryButton(editBtn);
        var deleteBtn = new Button { Text = "Xóa", Width = 78, Height = 34, Location = new Point(454, 11) }; Ui.StyleSecondaryButton(deleteBtn);
        // Events are attached by the parent after it locates buttons by text.
        bar.Controls.AddRange([addBtn, editBtn, deleteBtn]);
        var exportBtn = new Button { Text = exportText, Width = 112, Height = 34, Location = new Point(544, 11) }; Ui.StyleSecondaryButton(exportBtn); exportBtn.Click += (_, _) => export(); bar.Controls.Add(exportBtn);
        return bar;
    }
    private sealed record RoomRow(string Mã_phòng, string Trạng_thái, string Dọn_dẹp, string Loại_phòng, string Ghi_chú);
}

public sealed class RoomTypesControl : UserControl
{
    private readonly DataGridView _grid = new();
    private readonly TextBox _query = new() { PlaceholderText = "Tìm mã hoặc tên loại phòng...", Width = 240 };
    public RoomTypesControl()
    {
        BackColor = Color.Transparent; Ui.ConfigureGrid(_grid);
        var bar = new Panel { Dock = DockStyle.Top, Height = 56 }; _query.Location = new Point(0, 12); bar.Controls.Add(_query);
        var add = ActionButton("Thêm loại", 250, true, Add); var edit = ActionButton("Sửa", 372, false, Edit); var delete = ActionButton("Xóa", 458, false, Delete); var export = ActionButton("Xuất Excel", 544, false, Export);
        bar.Controls.AddRange([add, edit, delete, export]);
        _query.TextChanged += (_, _) => LoadRows(); _grid.CellDoubleClick += (_, _) => Edit();
        Controls.Add(_grid); Controls.Add(bar); Load += (_, _) => LoadRows();
    }
    private static Button ActionButton(string text, int x, bool primary, Action action)
    {
        var b = new Button { Text = text, Width = text == "Xuất Excel" ? 112 : 78, Height = 34, Location = new Point(x, 11) };
        if (primary) Ui.StylePrimaryButton(b); else Ui.StyleSecondaryButton(b); b.Click += (_, _) => action(); return b;
    }
    private void LoadRows()
    {
        try
        {
            using var db = new HotelDbContext(); var q = _query.Text.Trim();
            _grid.DataSource = db.LoaiPhongs.AsNoTracking().Where(x => q == "" || x.MaLPH.Contains(q) || x.TenLPH.Contains(q)).OrderBy(x => x.MaLPH)
                .Select(x => new TypeRow(x.MaLPH, x.TenLPH, x.SoGiuong, x.SoNguoiToiDa, x.GiaNgay, x.GiaGio)).ToList();
        }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
    private void Add() { using var f = new RoomTypeDialog(); if (f.ShowDialog(this) == DialogResult.OK) LoadRows(); }
    private void Edit()
    {
        if (CurrentCode() is not { } code) { Ui.Warn("Hãy chọn loại phòng cần sửa."); return; }
        using var f = new RoomTypeDialog(code); if (f.ShowDialog(this) == DialogResult.OK) LoadRows();
    }
    private void Delete()
    {
        if (CurrentCode() is not { } code) { Ui.Warn("Hãy chọn loại phòng cần xóa."); return; }
        try
        {
            using var db = new HotelDbContext();
            if (db.Phongs.Any(x => x.MaLPH == code && x.DaXoa != true)) { Ui.Warn("Không thể xóa vì loại phòng đang được dùng bởi phòng còn hoạt động."); return; }
            var type = db.LoaiPhongs.Find(code); if (type is not null) { db.Remove(type); db.SaveChanges(); LoadRows(); }
        }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
    private string? CurrentCode() => _grid.CurrentRow?.Cells[0].Value?.ToString();
    private void Export() => ExcelExport.Save(_grid, "DanhSachLoaiPhong");
    private sealed record TypeRow(string Mã_loại, string Tên_loại, int Số_giường, int Số_người_tối_đa, decimal Giá_ngày, decimal Giá_giờ);
}

public sealed class RoomDialog : Form
{
    private readonly string? _code;
    private readonly TextBox _id = new();
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _clean = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _note = new() { Multiline = true, Height = 70 };
    public RoomDialog(string? code = null)
    {
        _code = code; Text = code is null ? "Thêm phòng" : "Cập nhật phòng"; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(440, 470); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        _status.Items.AddRange(["Bình thường", "Đã đặt", "Đang thuê", "Đang sửa chữa"]); _clean.Items.AddRange(["Đã dọn dẹp", "Chưa dọn dẹp"]); _status.SelectedIndex = _clean.SelectedIndex = 0;
        LoadTypes(); if (code is not null) LoadRoom(code);
        var save = new Button { Text = "LƯU", Width = 125, Height = 38, Location = new Point(160, 405) }; Ui.StylePrimaryButton(save); save.Click += (_, _) => SaveRoom();
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 385, Padding = new Padding(35, 24, 35, 0), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        panel.Controls.Add(Ui.Field("Mã phòng", _id)); panel.Controls.Add(Ui.Field("Trạng thái phòng", _status)); panel.Controls.Add(Ui.Field("Tình trạng dọn dẹp", _clean)); panel.Controls.Add(Ui.Field("Loại phòng", _type)); panel.Controls.Add(Ui.Field("Ghi chú", _note)); Controls.Add(panel); Controls.Add(save);
    }
    private void LoadTypes()
    {
        using var db = new HotelDbContext(); var values = db.LoaiPhongs.AsNoTracking().OrderBy(x => x.MaLPH).ToList(); _type.DataSource = values; _type.DisplayMember = nameof(LoaiPhong.TenLPH); _type.ValueMember = nameof(LoaiPhong.MaLPH);
    }
    private void LoadRoom(string code)
    {
        using var db = new HotelDbContext(); var r = db.Phongs.AsNoTracking().First(x => x.MaPH == code); _id.Text = r.MaPH; _id.ReadOnly = true; _status.SelectedItem = r.TTPH; _clean.SelectedItem = r.TTDD; _type.SelectedValue = r.MaLPH; _note.Text = r.GhiChu ?? "";
    }
    private void SaveRoom()
    {
        var id = _id.Text.Trim().ToUpperInvariant(); if (id.Length == 0 || _type.SelectedValue is null) { Ui.Warn("Nhập mã phòng và chọn loại phòng."); return; }
        try
        {
            using var db = new HotelDbContext(); var room = _code is null ? null : db.Phongs.Find(_code);
            if (room is null)
            {
                if (db.Phongs.Any(x => x.MaPH == id)) { Ui.Warn("Mã phòng đã tồn tại."); return; }
                room = new Phong { MaPH = id }; db.Phongs.Add(room);
            }
            room.TTPH = _status.Text; room.TTDD = _clean.Text; room.MaLPH = _type.SelectedValue.ToString()!; room.GhiChu = _note.Text.Trim(); room.DaXoa = false; db.SaveChanges(); DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
}

public sealed class RoomTypeDialog : Form
{
    private readonly string? _code;
    private readonly TextBox _id = new(); private readonly TextBox _name = new(); private readonly NumericUpDown _beds = new() { Minimum = 1, Maximum = 10, Value = 1 }; private readonly NumericUpDown _people = new() { Minimum = 1, Maximum = 20, Value = 2 }; private readonly NumericUpDown _day = new() { Minimum = 1, Maximum = 100000000, Increment = 50000, ThousandsSeparator = true }; private readonly NumericUpDown _hour = new() { Minimum = 1, Maximum = 100000000, Increment = 10000, ThousandsSeparator = true };
    public RoomTypeDialog(string? code = null)
    {
        _code = code; Text = code is null ? "Thêm loại phòng" : "Cập nhật loại phòng"; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(440, 555); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        if (code is not null) LoadType(code);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 465, Padding = new Padding(35, 24, 35, 0), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        panel.Controls.Add(Ui.Field("Mã loại phòng", _id)); panel.Controls.Add(Ui.Field("Tên loại phòng", _name)); panel.Controls.Add(Ui.Field("Số giường", _beds)); panel.Controls.Add(Ui.Field("Số người tối đa", _people)); panel.Controls.Add(Ui.Field("Giá ngày", _day)); panel.Controls.Add(Ui.Field("Giá giờ", _hour));
        var save = new Button { Text = "LƯU", Width = 125, Height = 38, Location = new Point(160, 490) }; Ui.StylePrimaryButton(save); save.Click += (_, _) => SaveType(); Controls.Add(panel); Controls.Add(save);
    }
    private void LoadType(string code)
    {
        using var db = new HotelDbContext(); var item = db.LoaiPhongs.AsNoTracking().First(x => x.MaLPH == code); _id.Text = item.MaLPH; _id.ReadOnly = true; _name.Text = item.TenLPH; _beds.Value = item.SoGiuong; _people.Value = item.SoNguoiToiDa; _day.Value = item.GiaNgay; _hour.Value = item.GiaGio;
    }
    private void SaveType()
    {
        var id = _id.Text.Trim().ToUpperInvariant(); if (id.Length == 0 || _name.Text.Trim().Length == 0) { Ui.Warn("Nhập mã và tên loại phòng."); return; }
        try
        {
            using var db = new HotelDbContext(); var item = _code is null ? null : db.LoaiPhongs.Find(_code);
            if (item is null) { if (db.LoaiPhongs.Any(x => x.MaLPH == id)) { Ui.Warn("Mã loại phòng đã tồn tại."); return; } item = new LoaiPhong { MaLPH = id }; db.LoaiPhongs.Add(item); }
            item.TenLPH = _name.Text.Trim(); item.SoGiuong = Decimal.ToInt32(_beds.Value); item.SoNguoiToiDa = Decimal.ToInt32(_people.Value); item.GiaNgay = _day.Value; item.GiaGio = _hour.Value; db.SaveChanges(); DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { Ui.Error(ex.Message); }
    }
}

internal static class ExcelExport
{
    public static void Save(DataGridView grid, string prefix)
    {
        if (grid.Rows.Count == 0) { Ui.Warn("Không có dữ liệu để xuất."); return; }
        using var dialog = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            using var book = new XLWorkbook(); var sheet = book.Worksheets.Add("Dữ liệu");
            for (var c = 0; c < grid.Columns.Count; c++) sheet.Cell(1, c + 1).Value = grid.Columns[c].HeaderText;
            for (var r = 0; r < grid.Rows.Count; r++) for (var c = 0; c < grid.Columns.Count; c++) sheet.Cell(r + 2, c + 1).Value = grid.Rows[r].Cells[c].Value?.ToString() ?? "";
            sheet.Row(1).Style.Font.Bold = true; sheet.Columns().AdjustToContents(); book.SaveAs(dialog.FileName); Ui.Info("Đã xuất file Excel thành công.");
        }
        catch (Exception ex) { Ui.Error("Không thể xuất Excel.\n" + ex.Message); }
    }
}
