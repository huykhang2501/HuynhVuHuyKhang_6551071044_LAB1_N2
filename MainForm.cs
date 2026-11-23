using HotelManagementLab1.Models;

namespace HotelManagementLab1;

public sealed class MainForm : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(247, 250, 249), Padding = new Padding(24) };
    private readonly Label _title = new() { Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Ui.Ink, AutoSize = true };
    private readonly UserSession _session;

    public MainForm(UserSession session)
    {
        _session = session;
        Text = "Hotel Management - Lab 2";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 680);
        BackColor = Color.White;

        var sidebar = BuildSidebar();
        var top = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Color.White, Padding = new Padding(28, 18, 28, 0) };
        var user = new Label { Text = $"{session.StaffName}\n{RoleName(session.Role)}", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Right, Width = 260, Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray };
        top.Controls.Add(_title); top.Controls.Add(user);
        var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(247, 250, 249) };
        host.Controls.Add(_content); host.Controls.Add(top);
        Controls.Add(host); Controls.Add(sidebar);
        ShowHome();
    }

    private Panel BuildSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 245, BackColor = Ui.Teal, Padding = new Padding(14, 24, 14, 14) };
        var brand = new Label { Text = "HOTEL\nMANAGEMENT", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 17, FontStyle.Bold), Location = new Point(22, 20) };
        var line = new Label { Text = "LAB 2 • WINFORMS", AutoSize = true, ForeColor = Color.FromArgb(195, 227, 222), Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(24, 78) };
        var menu = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 620, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 10, 0, 0), AutoScroll = true };
        menu.Controls.Add(MenuButton("Trang chủ", (_, _) => ShowHome()));
        menu.Controls.Add(MenuButton("Sơ đồ phòng", (_, _) => ShowRoomBoard()));
        menu.Controls.Add(MenuButton("Quản lý phòng", (_, _) => ShowRooms()));
        menu.Controls.Add(MenuButton("Loại phòng", (_, _) => ShowRoomTypes()));
        menu.Controls.Add(MenuButton("Dịch vụ", (_, _) => ShowServices()));
        menu.Controls.Add(MenuButton("Tiện nghi", (_, _) => ShowAmenities()));
        menu.Controls.Add(MenuButton("Tiện nghi loại phòng", (_, _) => ShowRoomAmenities()));
        menu.Controls.Add(MenuButton("Khách hàng", (_, _) => ShowCustomers()));
        menu.Controls.Add(MenuButton("Nhân viên", (_, _) => ShowEmployees()));
        menu.Controls.Add(MenuButton("Hóa đơn", (_, _) => ShowComingSoon("Hóa đơn")));
        menu.Controls.Add(MenuButton("Thống kê", (_, _) => ShowComingSoon("Thống kê")));
        menu.Controls.Add(Ui.Spacer(10));
        menu.Controls.Add(MenuButton("Đăng xuất", (_, _) => { if (MessageBox.Show("Bạn muốn đăng xuất?", "Hotel Management", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) Close(); }));
        sidebar.Controls.AddRange([brand, line, menu]);
        return sidebar;
    }

    private static Button MenuButton(string text, EventHandler action)
    {
        var button = new Button { Text = text, Width = 215, Height = 43, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Margin = new Padding(0, 2, 0, 2), FlatStyle = FlatStyle.Flat, BackColor = Ui.Teal, ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
        button.FlatAppearance.BorderSize = 0; button.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 125, 117); button.Click += action;
        return button;
    }

    private void SetContent(string title, Control control)
    {
        _title.Text = title; _content.Controls.Clear();
        control.Dock = DockStyle.Fill; _content.Controls.Add(control);
    }

    private void ShowHome()
    {
        var home = new HomeControl(_session, ShowRoomBoard, ShowRooms, ShowRoomTypes);
        SetContent("Trang chủ", home);
    }
    private void ShowRoomBoard() => SetContent("Sơ đồ phòng", new RoomBoardControl());
    private void ShowRooms() => SetContent("Quản lý phòng", new RoomsControl());
    private void ShowRoomTypes() => SetContent("Quản lý loại phòng", new RoomTypesControl());
    private bool CanDelete => _session.Role >= 3;
    private void ShowServices() => SetContent("Quản lý dịch vụ", new ServicesControl(CanDelete));
    private void ShowAmenities() => SetContent("Quản lý tiện nghi", new AmenitiesControl(CanDelete));
    private void ShowRoomAmenities() => SetContent("Tiện nghi theo loại phòng", new RoomAmenitiesControl(CanDelete));
    private void ShowCustomers() => SetContent("Quản lý khách hàng", new CustomersControl(CanDelete));
    private void ShowEmployees() => SetContent("Quản lý nhân viên", new EmployeesControl(CanDelete));
    private void ShowComingSoon(string name) => SetContent(name, new Label { Text = $"{name}\nChức năng sẽ được phát triển ở các bài lab tiếp theo.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15), ForeColor = Color.Gray });
    private static string RoleName(int role) => role switch { 3 => "Quản trị viên", 2 => "Quản lý", _ => "Nhân viên" };
}
