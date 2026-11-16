namespace HotelManagementLab1;

internal static class Ui
{
    public static readonly Color Teal = Color.FromArgb(21, 99, 94);
    public static readonly Color Ink = Color.FromArgb(27, 54, 61);

    public static void StyleInput(TextBox input)
    {
        input.Width = 360; input.Height = 39; input.Font = new Font("Segoe UI", 11);
        input.BorderStyle = BorderStyle.FixedSingle; input.Margin = new Padding(0, 5, 0, 16);
    }
    public static void StylePrimaryButton(Button button)
    {
        button.BackColor = Teal; button.ForeColor = Color.White; button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0; button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }
    public static void StyleSecondaryButton(Button button)
    {
        button.BackColor = Color.White; button.ForeColor = Teal; button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Teal; button.Font = new Font("Segoe UI", 9, FontStyle.Bold); button.Cursor = Cursors.Hand;
    }
    public static Control Spacer(int height) => new Panel { Height = height, Width = 2, Margin = Padding.Empty };
    public static Control Field(string label, Control input)
    {
        var panel = new Panel { Width = 365, Height = 83, Margin = Padding.Empty };
        var caption = new Label { Text = label, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Ink, Location = new Point(0, 0) };
        input.Location = new Point(0, 24); input.Width = 360;
        panel.Controls.AddRange([caption, input]); return panel;
    }
    public static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill; grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None;
        grid.AutoGenerateColumns = true; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false;
        grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.ReadOnly = true;
        grid.RowHeadersVisible = false; grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Teal; grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9); grid.RowTemplate.Height = 30;
    }
    public static void Error(string message) => MessageBox.Show(message, "Hotel Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
    public static void Warn(string message) => MessageBox.Show(message, "Hotel Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Info(string message) => MessageBox.Show(message, "Hotel Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
