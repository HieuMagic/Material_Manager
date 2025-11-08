using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using BuildingMaterialManager.Data;

namespace BuildingMaterialManager.Forms
{
    public partial class ProductManagementForm : Form
    {
        private DataTable dtProducts;
        private int? selectedProductID = null;

        public ProductManagementForm()
        {
            InitializeComponent();
            this.Load += ProductManagementForm_Load;
        }

        private void ProductManagementForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            string query = "SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            
            cboCategory.DataSource = dt;
            cboCategory.DisplayMember = "CategoryName";
            cboCategory.ValueMember = "CategoryID";
            cboCategory.SelectedIndex = -1;
        }

        private void LoadProducts()
        {
            string query = @"SELECT p.ProductID, p.ProductCode, p.ProductName, 
                             c.CategoryName, c.CategoryID, p.Unit, p.UnitPrice, 
                             p.QuantityInStock, p.Supplier
                             FROM Products p
                             INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                             ORDER BY p.ProductID";
            
            dtProducts = DatabaseHelper.ExecuteQuery(query);
            dgvProducts.DataSource = dtProducts;
            
            if (dgvProducts.Columns.Count > 0)
            {
                dgvProducts.Columns["ProductID"].HeaderText = "Mã SP";
                dgvProducts.Columns["ProductID"].Width = 80;
                dgvProducts.Columns["ProductCode"].HeaderText = "Mã vật liệu";
                dgvProducts.Columns["ProductName"].HeaderText = "Tên vật liệu";
                dgvProducts.Columns["CategoryName"].HeaderText = "Danh mục";
                dgvProducts.Columns["CategoryID"].Visible = false;
                dgvProducts.Columns["Unit"].HeaderText = "Đơn vị";
                dgvProducts.Columns["Unit"].Width = 80;
                dgvProducts.Columns["UnitPrice"].HeaderText = "Đơn giá";
                dgvProducts.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
                dgvProducts.Columns["QuantityInStock"].HeaderText = "Tồn kho";
                dgvProducts.Columns["QuantityInStock"].Width = 100;
                dgvProducts.Columns["Supplier"].HeaderText = "Nhà cung cấp";
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
                selectedProductID = Convert.ToInt32(row.Cells["ProductID"].Value);
                txtProductCode.Text = row.Cells["ProductCode"].Value?.ToString() ?? "";
                txtProductName.Text = row.Cells["ProductName"].Value?.ToString() ?? "";
                txtUnit.Text = row.Cells["Unit"].Value?.ToString() ?? "";
                txtUnitPrice.Text = row.Cells["UnitPrice"].Value?.ToString() ?? "";
                txtQuantityInStock.Text = row.Cells["QuantityInStock"].Value?.ToString() ?? "";
                txtSupplier.Text = row.Cells["Supplier"].Value?.ToString() ?? "";
                
                cboCategory.SelectedValue = row.Cells["CategoryID"].Value;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                LoadProducts();
                return;
            }
            
            string query = $@"SELECT p.ProductID, p.ProductCode, p.ProductName, 
                              c.CategoryName, c.CategoryID, p.Unit, p.UnitPrice, 
                              p.QuantityInStock, p.Supplier
                              FROM Products p
                              INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                              WHERE LOWER(p.ProductCode) LIKE LOWER('%{searchText.Replace("'", "''")}%') 
                              OR LOWER(p.ProductName) LIKE LOWER('%{searchText.Replace("'", "''")}%')
                              ORDER BY p.ProductID";
            
            dtProducts = DatabaseHelper.ExecuteQuery(query);
            dgvProducts.DataSource = dtProducts;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateProduct())
                return;
            
            string checkQuery = $"SELECT COUNT(*) FROM Products WHERE ProductCode = '{txtProductCode.Text.Trim().Replace("'", "''")}'";
            DataTable dtCheck = DatabaseHelper.ExecuteQuery(checkQuery);
            if (Convert.ToInt32(dtCheck.Rows[0][0]) > 0)
            {
                MessageBox.Show("Mã vật liệu đã tồn tại!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductCode.Focus();
                return;
            }
            
            try
            {
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@ProductCode", txtProductCode.Text.Trim()),
                    new MySqlParameter("@ProductName", txtProductName.Text.Trim()),
                    new MySqlParameter("@CategoryID", cboCategory.SelectedValue),
                    new MySqlParameter("@Unit", txtUnit.Text.Trim()),
                    new MySqlParameter("@UnitPrice", decimal.Parse(txtUnitPrice.Text.Trim())),
                    new MySqlParameter("@QuantityInStock", int.Parse(txtQuantityInStock.Text.Trim())),
                    new MySqlParameter("@Supplier", txtSupplier.Text.Trim())
                };
                
                string query = @"INSERT INTO Products (ProductCode, ProductName, CategoryID, Unit, UnitPrice, QuantityInStock, Supplier)
                                 VALUES (@ProductCode, @ProductName, @CategoryID, @Unit, @UnitPrice, @QuantityInStock, @Supplier)";
                
                if (DatabaseHelper.ExecuteNonQueryWithParams(query, parameters) > 0)
                {
                    MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedProductID == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần sửa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateProduct())
                return;
            
            try
            {
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@ProductCode", txtProductCode.Text.Trim()),
                    new MySqlParameter("@ProductName", txtProductName.Text.Trim()),
                    new MySqlParameter("@CategoryID", cboCategory.SelectedValue),
                    new MySqlParameter("@Unit", txtUnit.Text.Trim()),
                    new MySqlParameter("@UnitPrice", decimal.Parse(txtUnitPrice.Text.Trim())),
                    new MySqlParameter("@QuantityInStock", int.Parse(txtQuantityInStock.Text.Trim())),
                    new MySqlParameter("@Supplier", txtSupplier.Text.Trim()),
                    new MySqlParameter("@ProductID", selectedProductID)
                };
                
                string query = @"UPDATE Products SET 
                                 ProductCode = @ProductCode,
                                 ProductName = @ProductName,
                                 CategoryID = @CategoryID,
                                 Unit = @Unit,
                                 UnitPrice = @UnitPrice,
                                 QuantityInStock = @QuantityInStock,
                                 Supplier = @Supplier
                                 WHERE ProductID = @ProductID";
                
                if (DatabaseHelper.ExecuteNonQueryWithParams(query, parameters) > 0)
                {
                    MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa sản phẩm này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                string checkQuery = $"SELECT COUNT(*) FROM OrderDetails WHERE ProductID = {selectedProductID}";
                DataTable dtCheck = DatabaseHelper.ExecuteQuery(checkQuery);
                
                if (Convert.ToInt32(dtCheck.Rows[0][0]) > 0)
                {
                    MessageBox.Show(
                        "Không thể xóa sản phẩm này vì đã có trong đơn hàng!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                string query = $"DELETE FROM Products WHERE ProductID = {selectedProductID}";
                if (DatabaseHelper.ExecuteNonQuery(query) > 0)
                {
                    MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                    ClearInputFields();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Dữ liệu đã được lưu tự động sau mỗi thao tác Thêm/Sửa/Xóa.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn làm mới dữ liệu?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                LoadProducts();
                ClearInputFields();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ValidateProduct()
        {
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                MessageBox.Show("Mã vật liệu không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductCode.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Tên vật liệu không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return false;
            }
            
            if (cboCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn danh mục!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategory.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Đơn vị không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnit.Focus();
                return false;
            }
            
            decimal unitPrice;
            if (!decimal.TryParse(txtUnitPrice.Text, out unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Đơn giá phải là số dương!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnitPrice.Focus();
                return false;
            }
            
            int quantity;
            if (!int.TryParse(txtQuantityInStock.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Tồn kho phải là số không âm!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantityInStock.Focus();
                return false;
            }
            
            return true;
        }

        private void ClearInputFields()
        {
            selectedProductID = null;
            txtProductCode.Text = "";
            txtProductName.Text = "";
            txtUnit.Text = "";
            txtUnitPrice.Text = "";
            txtQuantityInStock.Text = "0";
            txtSupplier.Text = "";
            cboCategory.SelectedIndex = -1;
            txtProductCode.Focus();
        }
    }
}
