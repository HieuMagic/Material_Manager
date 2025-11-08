using System;
using System.Windows.Forms;

namespace BuildingMaterialManager.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Display user information
            lblWelcome.Text = $"Xin chào, {GlobalVariables.UserFullName}!";
            lblUserInfo.Text = $"Người dùng: {GlobalVariables.UserFullName} ({GlobalVariables.UserRole})";
            
            // Update status
            statusLabel.Text = $"Đã đăng nhập: {GlobalVariables.UserFullName} - {DateTime.Now:dd/MM/yyyy HH:mm}";
        }

        private void btnQuanLyDanhMuc_Click(object sender, EventArgs e)
        {
            CategoryManagementForm form = new CategoryManagementForm();
            form.ShowDialog();
        }

        private void btnQuanLySanPham_Click(object sender, EventArgs e)
        {
            ProductManagementForm form = new ProductManagementForm();
            form.ShowDialog();
        }

        private void btnQuanLyKhachHang_Click(object sender, EventArgs e)
        {
            CustomerManagementForm form = new CustomerManagementForm();
            form.ShowDialog();
        }

        private void btnQuanLyDonHang_Click(object sender, EventArgs e)
        {
            OrderManagementForm form = new OrderManagementForm();
            form.ShowDialog();
        }

        private void menuQuanLyDanhMuc_Click(object sender, EventArgs e)
        {
            btnQuanLyDanhMuc_Click(sender, e);
        }

        private void menuQuanLySanPham_Click(object sender, EventArgs e)
        {
            btnQuanLySanPham_Click(sender, e);
        }

        private void menuQuanLyKhachHang_Click(object sender, EventArgs e)
        {
            btnQuanLyKhachHang_Click(sender, e);
        }

        private void menuQuanLyDonHang_Click(object sender, EventArgs e)
        {
            btnQuanLyDonHang_Click(sender, e);
        }

        private void menuDangXuat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void menuThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn thoát chương trình?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
