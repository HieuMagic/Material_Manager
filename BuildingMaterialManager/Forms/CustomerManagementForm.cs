using System;
using System.Data;
using System.Windows.Forms;
using BuildingMaterialManager.Data;
using MySql.Data.MySqlClient;

namespace BuildingMaterialManager.Forms
{
    public partial class CustomerManagementForm : Form
    {
        private DataTable dtCustomers;
        private bool isAdding = false;
        private bool isEditing = false;

        public CustomerManagementForm()
        {
            InitializeComponent();
            LoadCustomers();
            SetButtonStates(false);
        }

        private void LoadCustomers()
        {
            try
            {
                string query = "SELECT CustomerID, CustomerCode, CustomerName, Phone, Address, City FROM Customers ORDER BY CustomerCode";
                dtCustomers = DatabaseHelper.ExecuteQuery(query);
                dgvCustomers.DataSource = dtCustomers;

                dgvCustomers.Columns["CustomerID"].Visible = false;
                dgvCustomers.Columns["CustomerCode"].HeaderText = "Mã khách hàng";
                dgvCustomers.Columns["CustomerName"].HeaderText = "Tên khách hàng";
                dgvCustomers.Columns["Phone"].HeaderText = "Số điện thoại";
                dgvCustomers.Columns["Address"].HeaderText = "Địa chỉ";
                dgvCustomers.Columns["City"].HeaderText = "Thành phố";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];
                txtCustomerCode.Text = row.Cells["CustomerCode"].Value?.ToString() ?? "";
                txtCustomerName.Text = row.Cells["CustomerName"].Value?.ToString() ?? "";
                txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                txtCity.Text = row.Cells["City"].Value?.ToString() ?? "";
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            isAdding = true;
            ClearInputFields();
            SetButtonStates(true);
            txtCustomerCode.Focus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isEditing = true;
            SetButtonStates(true);
            txtCustomerName.Focus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            try
            {
                int customerID = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value);

                string checkQuery = "SELECT COUNT(*) FROM Orders WHERE CustomerID = @CustomerID";
                MySqlParameter[] checkParams = new MySqlParameter[]
                {
                    new MySqlParameter("@CustomerID", customerID)
                };
                DataTable checkResult = DatabaseHelper.ExecuteQueryWithParams(checkQuery, checkParams);
                int orderCount = Convert.ToInt32(checkResult.Rows[0][0]);

                if (orderCount > 0)
                {
                    MessageBox.Show("Không thể xóa khách hàng này vì đã có đơn hàng liên quan!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string deleteQuery = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
                MySqlParameter[] deleteParams = new MySqlParameter[]
                {
                    new MySqlParameter("@CustomerID", customerID)
                };

                int rowsAffected = DatabaseHelper.ExecuteNonQueryWithParams(deleteQuery, deleteParams);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Xóa khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCustomers();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa khách hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateCustomer()) return;

            try
            {
                if (isAdding)
                {
                    string checkQuery = "SELECT COUNT(*) FROM Customers WHERE CustomerCode = @CustomerCode";
                    MySqlParameter[] checkParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@CustomerCode", txtCustomerCode.Text.Trim())
                    };
                    DataTable checkResult = DatabaseHelper.ExecuteQueryWithParams(checkQuery, checkParams);
                    int count = Convert.ToInt32(checkResult.Rows[0][0]);

                    if (count > 0)
                    {
                        MessageBox.Show("Mã khách hàng đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCustomerCode.Focus();
                        return;
                    }

                    string insertQuery = @"INSERT INTO Customers (CustomerCode, CustomerName, Phone, Address, City) 
                                          VALUES (@CustomerCode, @CustomerName, @Phone, @Address, @City)";
                    MySqlParameter[] insertParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@CustomerCode", txtCustomerCode.Text.Trim()),
                        new MySqlParameter("@CustomerName", txtCustomerName.Text.Trim()),
                        new MySqlParameter("@Phone", string.IsNullOrEmpty(txtPhone.Text.Trim()) ? (object)DBNull.Value : txtPhone.Text.Trim()),
                        new MySqlParameter("@Address", string.IsNullOrEmpty(txtAddress.Text.Trim()) ? (object)DBNull.Value : txtAddress.Text.Trim()),
                        new MySqlParameter("@City", string.IsNullOrEmpty(txtCity.Text.Trim()) ? (object)DBNull.Value : txtCity.Text.Trim())
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQueryWithParams(insertQuery, insertParams);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        isAdding = false;
                        SetButtonStates(false);
                        LoadCustomers();
                        ClearInputFields();
                    }
                }
                else if (isEditing)
                {
                    int customerID = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value);

                    string updateQuery = @"UPDATE Customers 
                                          SET CustomerName = @CustomerName, 
                                              Phone = @Phone, 
                                              Address = @Address, 
                                              City = @City 
                                          WHERE CustomerID = @CustomerID";
                    MySqlParameter[] updateParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@CustomerName", txtCustomerName.Text.Trim()),
                        new MySqlParameter("@Phone", string.IsNullOrEmpty(txtPhone.Text.Trim()) ? (object)DBNull.Value : txtPhone.Text.Trim()),
                        new MySqlParameter("@Address", string.IsNullOrEmpty(txtAddress.Text.Trim()) ? (object)DBNull.Value : txtAddress.Text.Trim()),
                        new MySqlParameter("@City", string.IsNullOrEmpty(txtCity.Text.Trim()) ? (object)DBNull.Value : txtCity.Text.Trim()),
                        new MySqlParameter("@CustomerID", customerID)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQueryWithParams(updateQuery, updateParams);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Cập nhật khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        isEditing = false;
                        SetButtonStates(false);
                        LoadCustomers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isAdding = false;
            isEditing = false;
            SetButtonStates(false);
            ClearInputFields();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ValidateCustomer()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerCode.Text))
            {
                MessageBox.Show("Vui lòng nhập mã khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                string phone = txtPhone.Text.Trim();
                if (phone.Length < 10 || phone.Length > 11 || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[0-9]+$"))
                {
                    MessageBox.Show("Số điện thoại không hợp lệ! (10-11 chữ số)", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPhone.Focus();
                    return false;
                }
            }

            return true;
        }

        private void ClearInputFields()
        {
            txtCustomerCode.Clear();
            txtCustomerName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtCity.Clear();
        }

        private void SetButtonStates(bool isSaveMode)
        {
            if (isSaveMode)
            {
                btnAdd.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnSave.Enabled = true;
                btnCancel.Enabled = true;
                txtCustomerCode.Enabled = isAdding;
                dgvCustomers.Enabled = false;
            }
            else
            {
                btnAdd.Enabled = true;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
                btnSave.Enabled = false;
                btnCancel.Enabled = false;
                txtCustomerCode.Enabled = true;
                dgvCustomers.Enabled = true;
            }
        }
    }
}
