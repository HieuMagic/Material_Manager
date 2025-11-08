using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using BuildingMaterialManager.Data;

namespace BuildingMaterialManager.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            
            // Test database connection on form load
            this.Load += LoginForm_Load;
            
            // Allow Enter key to submit
            txtPassword.KeyPress += txtPassword_KeyPress;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Test database connection
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show(
                    "Không thể kết nối đến cơ sở dữ liệu!\n" +
                    "Vui lòng kiểm tra:\n" +
                    "1. MySQL Server đã được khởi động\n" +
                    "2. Database 'BuildingMaterialDB' đã được tạo\n" +
                    "3. Connection string trong App.config đúng",
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Press Enter to login
            if (e.KeyChar == (char)Keys.Return)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Check credentials using parameterized query (prevent SQL injection)
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT UserID, FullName, Role FROM Users " +
                                   "WHERE Username=@username AND Password=@password AND IsActive=1";
                    
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    MySqlDataReader reader = cmd.ExecuteReader();
                    
                    if (reader.Read())
                    {
                        // Store user info in global variables
                        GlobalVariables.UserID = reader.GetInt32("UserID");
                        GlobalVariables.UserFullName = reader.GetString("FullName");
                        GlobalVariables.UserRole = reader.GetString("Role");
                        GlobalVariables.IsLoggedIn = true;

                        reader.Close();

                        MessageBox.Show(
                            $"Đăng nhập thành công!\n\nXin chào {GlobalVariables.UserFullName}",
                            "Thông báo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        // Open MainForm
                        MainForm mainForm = new MainForm();
                        this.Hide();
                        mainForm.ShowDialog();
                        
                        // Clear session after MainForm closes
                        GlobalVariables.ClearSession();
                        
                        // Clear password field
                        txtPassword.Text = "";
                        txtUsername.Text = "";
                        
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Tên đăng nhập hoặc mật khẩu không đúng!",
                            "Lỗi đăng nhập",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        txtPassword.Text = "";
                        txtPassword.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn thoát?",
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
