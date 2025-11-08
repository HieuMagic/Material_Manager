using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BuildingMaterialManager.Data;

namespace BuildingMaterialManager.Forms
{
    /// <summary>
    /// Category Management Form - Following the example pattern from requirements
    /// Implements simple CRUD operations on DataGridView
    /// </summary>
    public partial class CategoryManagementForm : Form
    {
        private DataTable dtCategories;

        public CategoryManagementForm()
        {
            InitializeComponent();
            this.Load += CategoryManagementForm_Load;
        }

        /// <summary>
        /// Form Load: Load categories into DataGridView
        /// </summary>
        private void CategoryManagementForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
        }

        /// <summary>
        /// Load all categories from database into DataGridView
        /// </summary>
        private void LoadCategories()
        {
            string query = "SELECT CategoryID, CategoryName, Description FROM Categories ORDER BY CategoryID";
            dtCategories = DatabaseHelper.ExecuteQuery(query);
            dgvCategories.DataSource = dtCategories;

            // Customize DataGridView columns
            if (dgvCategories.Columns.Count > 0)
            {
                dgvCategories.Columns["CategoryID"].HeaderText = "Mã danh mục";
                dgvCategories.Columns["CategoryID"].Width = 100;
                dgvCategories.Columns["CategoryName"].HeaderText = "Tên danh mục";
                dgvCategories.Columns["CategoryName"].Width = 200;
                dgvCategories.Columns["Description"].HeaderText = "Mô tả";
                dgvCategories.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        /// <summary>
        /// When clicking on DataGridView row: Display selected data in text fields
        /// </summary>
        private void dgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCategories.Rows[e.RowIndex];
                txtCategoryName.Text = row.Cells["CategoryName"].Value?.ToString() ?? "";
                txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? "";
            }
        }

        /// <summary>
        /// Add button: Add new row to DataGridView
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            // Check for duplicate category name in current DataTable
            foreach (DataRow row in dtCategories.Rows)
            {
                if (row["CategoryName"].ToString().Trim().ToLower() == txtCategoryName.Text.Trim().ToLower())
                {
                    MessageBox.Show("Tên danh mục đã tồn tại!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCategoryName.Focus();
                    return;
                }
            }

            // Add new row to DataTable
            DataRow newRow = dtCategories.NewRow();
            newRow["CategoryName"] = txtCategoryName.Text.Trim();
            newRow["Description"] = txtDescription.Text.Trim();
            dtCategories.Rows.Add(newRow);

            MessageBox.Show("Đã thêm danh mục mới! Nhấn 'Lưu' để lưu vào cơ sở dữ liệu.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClearInputFields();
        }

        /// <summary>
        /// Edit button: Update selected row in DataGridView
        /// </summary>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục cần sửa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput())
            {
                return;
            }

            // Check for duplicate category name (excluding current row)
            int currentRowIndex = dgvCategories.CurrentRow.Index;
            for (int i = 0; i < dtCategories.Rows.Count; i++)
            {
                if (i != currentRowIndex &&
                    dtCategories.Rows[i]["CategoryName"].ToString().Trim().ToLower() == txtCategoryName.Text.Trim().ToLower())
                {
                    MessageBox.Show("Tên danh mục đã tồn tại!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCategoryName.Focus();
                    return;
                }
            }

            // Update selected row
            int rowIndex = dgvCategories.CurrentRow.Index;
            dtCategories.Rows[rowIndex]["CategoryName"] = txtCategoryName.Text.Trim();
            dtCategories.Rows[rowIndex]["Description"] = txtDescription.Text.Trim();

            MessageBox.Show("Đã cập nhật danh mục! Nhấn 'Lưu' để lưu vào cơ sở dữ liệu.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Delete button: Remove selected row from DataGridView
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa danh mục này?\n\nLưu ý: Không thể xóa nếu danh mục đã có sản phẩm!",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Check if category has products
                int categoryID = Convert.ToInt32(dgvCategories.CurrentRow.Cells["CategoryID"].Value);
                string checkQuery = $"SELECT COUNT(*) FROM Products WHERE CategoryID = {categoryID}";
                DataTable dtCheck = DatabaseHelper.ExecuteQuery(checkQuery);
                
                if (dtCheck.Rows.Count > 0 && Convert.ToInt32(dtCheck.Rows[0][0]) > 0)
                {
                    MessageBox.Show(
                        "Không thể xóa danh mục này vì đã có sản phẩm!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                dgvCategories.Rows.RemoveAt(dgvCategories.CurrentRow.Index);
                ClearInputFields();

                MessageBox.Show("Đã xóa danh mục! Nhấn 'Lưu' để lưu vào cơ sở dữ liệu.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Save button: Save all changes from DataGridView to database
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc muốn lưu tất cả thay đổi?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // Strategy: Update existing and insert new categories
                    // Get all current CategoryIDs from database
                    DataTable dtDbCategories = DatabaseHelper.ExecuteQuery("SELECT CategoryID FROM Categories");
                    List<int> dbCategoryIDs = new List<int>();
                    foreach (DataRow row in dtDbCategories.Rows)
                    {
                        dbCategoryIDs.Add(Convert.ToInt32(row["CategoryID"]));
                    }

                    // Get CategoryIDs from DataGridView
                    List<int> gridCategoryIDs = new List<int>();
                    foreach (DataRow row in dtCategories.Rows)
                    {
                        if (row["CategoryID"] != DBNull.Value && !string.IsNullOrEmpty(row["CategoryID"].ToString()))
                        {
                            gridCategoryIDs.Add(Convert.ToInt32(row["CategoryID"]));
                        }
                    }

                    // Find categories to delete (in DB but not in grid)
                    List<int> toDelete = dbCategoryIDs.Except(gridCategoryIDs).ToList();
                    foreach (int categoryID in toDelete)
                    {
                        // Check if category has products
                        string checkQuery = $"SELECT COUNT(*) FROM Products WHERE CategoryID = {categoryID}";
                        DataTable dtCheck = DatabaseHelper.ExecuteQuery(checkQuery);
                        
                        if (dtCheck.Rows.Count > 0 && Convert.ToInt32(dtCheck.Rows[0][0]) > 0)
                        {
                            MessageBox.Show(
                                $"Không thể xóa danh mục ID {categoryID} vì đã có sản phẩm!",
                                "Lỗi",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            LoadCategories(); // Reload to restore deleted category
                            return;
                        }
                        
                        // Safe to delete
                        DatabaseHelper.ExecuteNonQuery($"DELETE FROM Categories WHERE CategoryID = {categoryID}");
                    }

                    // Update existing and insert new
                    foreach (DataRow row in dtCategories.Rows)
                    {
                        string categoryName = row["CategoryName"].ToString().Replace("'", "''");
                        string description = row["Description"].ToString().Replace("'", "''");
                        
                        if (row["CategoryID"] == DBNull.Value || string.IsNullOrEmpty(row["CategoryID"].ToString()))
                        {
                            // New category - INSERT
                            string query = $"INSERT INTO Categories (CategoryName, Description) " +
                                         $"VALUES ('{categoryName}', '{description}')";
                            DatabaseHelper.ExecuteNonQuery(query);
                        }
                        else
                        {
                            // Existing category - UPDATE
                            int categoryID = Convert.ToInt32(row["CategoryID"]);
                            string query = $"UPDATE Categories SET CategoryName = '{categoryName}', " +
                                         $"Description = '{description}' WHERE CategoryID = {categoryID}";
                            DatabaseHelper.ExecuteNonQuery(query);
                        }
                    }

                    MessageBox.Show("Đã lưu thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload data to get new IDs
                    LoadCategories();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cancel button: Reload data from database (discard changes)
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn hủy tất cả thay đổi?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                LoadCategories();
                ClearInputFields();
                MessageBox.Show("Đã hủy thay đổi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Exit button: Close form
        /// </summary>
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Validate input fields
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Tên danh mục không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
                return false;
            }

            if (txtCategoryName.Text.Trim().Length > 100)
            {
                MessageBox.Show("Tên danh mục không được vượt quá 100 ký tự!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoryName.Focus();
                return false;
            }

            if (txtDescription.Text.Trim().Length > 255)
            {
                MessageBox.Show("Mô tả không được vượt quá 255 ký tự!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescription.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clear all input fields
        /// </summary>
        private void ClearInputFields()
        {
            txtCategoryName.Text = "";
            txtDescription.Text = "";
            txtCategoryName.Focus();
        }
    }
}
