using HotelManagementLab1.Data;
using HotelManagementLab1.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementLab1;

public sealed class LoginForm : Form
{
    private readonly TextBox _user = new() { PlaceholderText = "Tên đăng nhập" };
    private readonly TextBox _password = new() { PlaceholderText = "Mật khẩu", UseSystemPasswordChar = true };
    private readonly Button _login = new() { Text = "ĐĂNG NHẬP" };

    public LoginForm()
    {
        Text = "Hotel Management - Đăng nhập";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(940, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(242, 247, 246);

        var left = new Panel { Dock = DockStyle.Left, Width = 430, BackColor = Color.FromArgb(21, 99, 94) };
        var brand = new Label { Text = "HOTEL\nMANAGEMENT", ForeColor = Color.White, Font = new Font("Segoe UI", 28, FontStyle.Bold), AutoSize = true, Location = new Point(55, 105) };
        var intro = new Label { Text = "Hệ thống quản lý khách sạn\nLab 1 - C# WinForms & EF Core", ForeColor = Color.FromArgb(211, 236, 232), Font = new Font("Segoe UI", 12), AutoSize = true, Location = new Point(58, 215) };
        left.Controls.AddRange([brand, intro]);

        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(72, 52, 72, 42) };
        var title = new Label { Text = "Chào mừng trở lại", Font = new Font("Segoe UI", 23, FontStyle.Bold), ForeColor = Color.FromArgb(27, 54, 61), AutoSize = true };
        var note = new Label { Text = "Đăng nhập để quản lý phòng và loại phòng.", Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray, AutoSize = true };
        Ui.StyleInput(_user); Ui.StyleInput(_password); Ui.StylePrimaryButton(_login);
        _login.Height = 48; _login.Dock = DockStyle.Top;
        _login.Click += (_, _) => Login();
        _password.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Login(); };

        var stack = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 0, 0, 0) };
        stack.Controls.Add(title); stack.SetFlowBreak(title, true);
        stack.Controls.Add(note); stack.SetFlowBreak(note, true);
        stack.Controls.Add(Ui.Spacer(36));
        stack.Controls.Add(Ui.Field("Tên đăng nhập", _user));
        stack.Controls.Add(Ui.Field("Mật khẩu", _password));
        stack.Controls.Add(Ui.Spacer(10));
        stack.Controls.Add(_login);
        stack.Controls.Add(Ui.Spacer(18));
        stack.Controls.Add(new Label { Text = "Tài khoản mẫu: admin / 1234", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true });
        right.Controls.Add(stack);
        Controls.Add(right); Controls.Add(left);
        AcceptButton = _login;
    }

    private void Login()
    {
        var username = _user.Text.Trim();
        var password = _password.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Ui.Warn("Hãy nhập tên đăng nhập và mật khẩu."); return;
        }
        try
        {
            using var db = new HotelDbContext();
            var account = db.TaiKhoans.Include(x => x.NhanVien).AsNoTracking()
                .FirstOrDefault(x => x.TenTK == username && x.DaXoa != true);
            if (account is null || account.NhanVien is null || account.NhanVien.DaXoa == true || account.Password != password)
            {
                Ui.Error("Tên đăng nhập hoặc mật khẩu không đúng.");
                _password.Clear(); _password.Focus(); return;
            }
            var session = new UserSession(account.TenTK, account.NhanVien.TenNV, account.MaNV, account.CapDoQuyen);
            Hide();
            using var main = new MainForm(session);
            main.ShowDialog(this);
            Show(); _password.Clear(); _password.Focus();
        }
        catch (Exception ex)
        {
            Ui.Error("Không thể kết nối SQL Server. Kiểm tra service SQLEXPRESS và database HotelManagement.\n\n" + ex.Message);
        }
    }
}
