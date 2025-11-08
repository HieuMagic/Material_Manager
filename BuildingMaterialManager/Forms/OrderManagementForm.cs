using System;
using System.Data;
using System.Windows.Forms;
using BuildingMaterialManager.Data;
using MySql.Data.MySqlClient;

namespace BuildingMaterialManager.Forms
{
    public partial class OrderManagementForm : Form
    {
        private DataTable dtOrders;
        private DataTable dtOrderDetails;
        private bool isAdding = false;
        private bool isEditing = false;
        private int currentOrderID = -1;

        public OrderManagementForm()
        {
            InitializeComponent();
            InitializeStatusComboBox();
            LoadCustomers();
            LoadProducts();
            LoadOrders();
            SetButtonStates(false);
            SetDetailControlsEnabled(false);
        }

        private void InitializeStatusComboBox()
        {
            cboStatus.Items.AddRange(new string[] { "Pending", "Processing", "Completed", "Cancelled" });
            cboStatus.SelectedIndex = 0;
        }

        private void LoadCustomers()
        {
            try
            {
                string query = "SELECT CustomerID, CONCAT(CustomerCode, ' - ', CustomerName) AS DisplayName FROM Customers ORDER BY CustomerCode";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                cboCustomer.DataSource = dt;
                cboCustomer.DisplayMember = "DisplayName";
                cboCustomer.ValueMember = "CustomerID";
                cboCustomer.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khách hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                string query = "SELECT ProductID, CONCAT(ProductCode, ' - ', ProductName, ' (', Unit, ')') AS DisplayName, UnitPrice FROM Products ORDER BY ProductCode";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                cboProduct.DataSource = dt;
                cboProduct.DisplayMember = "DisplayName";
                cboProduct.ValueMember = "ProductID";
                cboProduct.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"SELECT o.OrderID, o.OrderCode, c.CustomerName, o.OrderDate, o.Status, o.TotalAmount 
                                FROM Orders o 
                                INNER JOIN Customers c ON o.CustomerID = c.CustomerID 
                                ORDER BY o.OrderDate DESC";
                dtOrders = DatabaseHelper.ExecuteQuery(query);
                dgvOrders.DataSource = dtOrders;

                dgvOrders.Columns["OrderID"].Visible = false;
                dgvOrders.Columns["OrderCode"].HeaderText = "Mã đơn hàng";
                dgvOrders.Columns["CustomerName"].HeaderText = "Khách hàng";
                dgvOrders.Columns["OrderDate"].HeaderText = "Ngày đặt";
                dgvOrders.Columns["Status"].HeaderText = "Trạng thái";
                dgvOrders.Columns["TotalAmount"].HeaderText = "Tổng tiền";
                dgvOrders.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
                dgvOrders.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách đơn hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderDetails(int orderID)
        {
            try
            {
                string query = @"SELECT od.OrderDetailID, p.ProductCode, p.ProductName, od.Quantity, od.UnitPrice, od.TotalPrice 
                                FROM OrderDetails od 
                                INNER JOIN Products p ON od.ProductID = p.ProductID 
                                WHERE od.OrderID = @OrderID";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@OrderID", orderID)
                };
                dtOrderDetails = DatabaseHelper.ExecuteQueryWithParams(query, parameters);
                dgvOrderDetails.DataSource = dtOrderDetails;

                dgvOrderDetails.Columns["OrderDetailID"].Visible = false;
                dgvOrderDetails.Columns["ProductCode"].HeaderText = "Mã SP";
                dgvOrderDetails.Columns["ProductName"].HeaderText = "Tên sản phẩm";
                dgvOrderDetails.Columns["Quantity"].HeaderText = "Số lượng";
                dgvOrderDetails.Columns["UnitPrice"].HeaderText = "Đơn giá";
                dgvOrderDetails.Columns["TotalPrice"].HeaderText = "Thành tiền";
                dgvOrderDetails.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
                dgvOrderDetails.Columns["TotalPrice"].DefaultCellStyle.Format = "N0";
                dgvOrderDetails.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvOrderDetails.Columns["TotalPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                CalculateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết đơn hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvOrders.Rows[e.RowIndex];
                currentOrderID = Convert.ToInt32(row.Cells["OrderID"].Value);
                txtOrderCode.Text = row.Cells["OrderCode"].Value?.ToString() ?? "";
                
                string customerName = row.Cells["CustomerName"].Value?.ToString() ?? "";
                for (int i = 0; i < cboCustomer.Items.Count; i++)
                {
                    if (((DataRowView)cboCustomer.Items[i])["DisplayName"].ToString().Contains(customerName))
                    {
                        cboCustomer.SelectedIndex = i;
                        break;
                    }
                }

                dtpOrderDate.Value = Convert.ToDateTime(row.Cells["OrderDate"].Value);
                cboStatus.Text = row.Cells["Status"].Value?.ToString() ?? "Pending";
                txtTotalAmount.Text = Convert.ToDecimal(row.Cells["TotalAmount"].Value).ToString("N0");

                LoadOrderDetails(currentOrderID);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            isAdding = true;
            ClearInputFields();
            SetButtonStates(true);
            SetDetailControlsEnabled(true);
            txtOrderCode.Focus();

            dtOrderDetails = new DataTable();
            dtOrderDetails.Columns.Add("ProductCode", typeof(string));
            dtOrderDetails.Columns.Add("ProductName", typeof(string));
            dtOrderDetails.Columns.Add("Quantity", typeof(int));
            dtOrderDetails.Columns.Add("UnitPrice", typeof(decimal));
            dtOrderDetails.Columns.Add("TotalPrice", typeof(decimal));
            dgvOrderDetails.DataSource = dtOrderDetails;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboStatus.Text == "Completed" || cboStatus.Text == "Cancelled")
            {
                MessageBox.Show("Không thể sửa đơn hàng đã hoàn thành hoặc đã hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isEditing = true;
            SetButtonStates(true);
            SetDetailControlsEnabled(true);
            cboCustomer.Focus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa đơn hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            try
            {
                string deleteDetailsQuery = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";
                MySqlParameter[] deleteDetailsParams = new MySqlParameter[]
                {
                    new MySqlParameter("@OrderID", currentOrderID)
                };
                DatabaseHelper.ExecuteNonQueryWithParams(deleteDetailsQuery, deleteDetailsParams);

                string deleteOrderQuery = "DELETE FROM Orders WHERE OrderID = @OrderID";
                MySqlParameter[] deleteOrderParams = new MySqlParameter[]
                {
                    new MySqlParameter("@OrderID", currentOrderID)
                };
                int rowsAffected = DatabaseHelper.ExecuteNonQueryWithParams(deleteOrderQuery, deleteOrderParams);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Xóa đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadOrders();
                    ClearInputFields();
                    dtOrderDetails = new DataTable();
                    dgvOrderDetails.DataSource = dtOrderDetails;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa đơn hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateOrder()) return;

            try
            {
                if (isAdding)
                {
                    string checkQuery = "SELECT COUNT(*) FROM Orders WHERE OrderCode = @OrderCode";
                    MySqlParameter[] checkParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@OrderCode", txtOrderCode.Text.Trim())
                    };
                    DataTable checkResult = DatabaseHelper.ExecuteQueryWithParams(checkQuery, checkParams);
                    int count = Convert.ToInt32(checkResult.Rows[0][0]);

                    if (count > 0)
                    {
                        MessageBox.Show("Mã đơn hàng đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtOrderCode.Focus();
                        return;
                    }

                    string insertOrderQuery = @"INSERT INTO Orders (OrderCode, CustomerID, OrderDate, Status, TotalAmount) 
                                               VALUES (@OrderCode, @CustomerID, @OrderDate, @Status, @TotalAmount);
                                               SELECT LAST_INSERT_ID();";
                    MySqlParameter[] insertOrderParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@OrderCode", txtOrderCode.Text.Trim()),
                        new MySqlParameter("@CustomerID", cboCustomer.SelectedValue),
                        new MySqlParameter("@OrderDate", dtpOrderDate.Value.Date),
                        new MySqlParameter("@Status", cboStatus.Text),
                        new MySqlParameter("@TotalAmount", decimal.Parse(txtTotalAmount.Text.Replace(",", "")))
                    };
                    DataTable result = DatabaseHelper.ExecuteQueryWithParams(insertOrderQuery, insertOrderParams);
                    int newOrderID = Convert.ToInt32(result.Rows[0][0]);

                    foreach (DataRow row in dtOrderDetails.Rows)
                    {
                        string productCode = row["ProductCode"].ToString();
                        string getProductIDQuery = "SELECT ProductID FROM Products WHERE ProductCode = @ProductCode";
                        MySqlParameter[] productParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@ProductCode", productCode)
                        };
                        DataTable productResult = DatabaseHelper.ExecuteQueryWithParams(getProductIDQuery, productParams);
                        int productID = Convert.ToInt32(productResult.Rows[0]["ProductID"]);

                        string insertDetailQuery = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, TotalPrice) 
                                                    VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";
                        MySqlParameter[] insertDetailParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@OrderID", newOrderID),
                            new MySqlParameter("@ProductID", productID),
                            new MySqlParameter("@Quantity", row["Quantity"]),
                            new MySqlParameter("@UnitPrice", row["UnitPrice"]),
                            new MySqlParameter("@TotalPrice", row["TotalPrice"])
                        };
                        DatabaseHelper.ExecuteNonQueryWithParams(insertDetailQuery, insertDetailParams);
                    }

                    if (cboStatus.Text == "Completed")
                    {
                        UpdateProductStock(newOrderID, false);
                    }

                    MessageBox.Show("Thêm đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    isAdding = false;
                    SetButtonStates(false);
                    SetDetailControlsEnabled(false);
                    LoadOrders();
                    ClearInputFields();
                }
                else if (isEditing)
                {
                    string oldStatus = dgvOrders.SelectedRows[0].Cells["Status"].Value.ToString();

                    string updateOrderQuery = @"UPDATE Orders 
                                               SET CustomerID = @CustomerID, 
                                                   OrderDate = @OrderDate, 
                                                   Status = @Status, 
                                                   TotalAmount = @TotalAmount 
                                               WHERE OrderID = @OrderID";
                    MySqlParameter[] updateOrderParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@CustomerID", cboCustomer.SelectedValue),
                        new MySqlParameter("@OrderDate", dtpOrderDate.Value.Date),
                        new MySqlParameter("@Status", cboStatus.Text),
                        new MySqlParameter("@TotalAmount", decimal.Parse(txtTotalAmount.Text.Replace(",", ""))),
                        new MySqlParameter("@OrderID", currentOrderID)
                    };
                    DatabaseHelper.ExecuteNonQueryWithParams(updateOrderQuery, updateOrderParams);

                    string deleteDetailsQuery = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";
                    MySqlParameter[] deleteDetailsParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@OrderID", currentOrderID)
                    };
                    DatabaseHelper.ExecuteNonQueryWithParams(deleteDetailsQuery, deleteDetailsParams);

                    foreach (DataRow row in dtOrderDetails.Rows)
                    {
                        string productCode = row["ProductCode"].ToString();
                        string getProductIDQuery = "SELECT ProductID FROM Products WHERE ProductCode = @ProductCode";
                        MySqlParameter[] productParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@ProductCode", productCode)
                        };
                        DataTable productResult = DatabaseHelper.ExecuteQueryWithParams(getProductIDQuery, productParams);
                        int productID = Convert.ToInt32(productResult.Rows[0]["ProductID"]);

                        string insertDetailQuery = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, TotalPrice) 
                                                    VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";
                        MySqlParameter[] insertDetailParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@OrderID", currentOrderID),
                            new MySqlParameter("@ProductID", productID),
                            new MySqlParameter("@Quantity", row["Quantity"]),
                            new MySqlParameter("@UnitPrice", row["UnitPrice"]),
                            new MySqlParameter("@TotalPrice", row["TotalPrice"])
                        };
                        DatabaseHelper.ExecuteNonQueryWithParams(insertDetailQuery, insertDetailParams);
                    }

                    if (oldStatus != "Completed" && cboStatus.Text == "Completed")
                    {
                        UpdateProductStock(currentOrderID, false);
                    }
                    else if (oldStatus == "Completed" && cboStatus.Text != "Completed")
                    {
                        UpdateProductStock(currentOrderID, true);
                    }

                    MessageBox.Show("Cập nhật đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    isEditing = false;
                    SetButtonStates(false);
                    SetDetailControlsEnabled(false);
                    LoadOrders();
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
            SetDetailControlsEnabled(false);
            ClearInputFields();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProduct.SelectedIndex >= 0)
            {
                DataRowView row = (DataRowView)cboProduct.SelectedItem;
                decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);
                txtUnitPrice.Text = unitPrice.ToString("N0");
                CalculateDetailTotal();
            }
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            CalculateDetailTotal();
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            if (cboProduct.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return;
            }

            DataRowView selectedProduct = (DataRowView)cboProduct.SelectedItem;
            string displayName = selectedProduct["DisplayName"].ToString();
            string productCode = displayName.Split('-')[0].Trim();
            string productName = displayName.Split('-')[1].Split('(')[0].Trim();

            int productID = Convert.ToInt32(selectedProduct["ProductID"]);
            string checkStockQuery = "SELECT QuantityInStock FROM Products WHERE ProductID = @ProductID";
            MySqlParameter[] checkStockParams = new MySqlParameter[]
            {
                new MySqlParameter("@ProductID", productID)
            };
            DataTable stockResult = DatabaseHelper.ExecuteQueryWithParams(checkStockQuery, checkStockParams);
            int stockQuantity = Convert.ToInt32(stockResult.Rows[0][0]);

            if (quantity > stockQuantity)
            {
                MessageBox.Show($"Số lượng trong kho không đủ! (Còn {stockQuantity})", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (DataRow row in dtOrderDetails.Rows)
            {
                if (row["ProductCode"].ToString() == productCode)
                {
                    MessageBox.Show("Sản phẩm đã có trong đơn hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            decimal unitPrice = decimal.Parse(txtUnitPrice.Text.Replace(",", ""));
            decimal totalPrice = quantity * unitPrice;

            dtOrderDetails.Rows.Add(productCode, productName, quantity, unitPrice, totalPrice);
            CalculateTotalAmount();

            cboProduct.SelectedIndex = -1;
            txtQuantity.Text = "1";
            txtUnitPrice.Text = "0";
            txtDetailTotalPrice.Text = "0";
        }

        private void btnRemoveProduct_Click(object sender, EventArgs e)
        {
            if (dgvOrderDetails.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dgvOrderDetails.SelectedRows[0].Index;
            dtOrderDetails.Rows[rowIndex].Delete();
            CalculateTotalAmount();
        }

        private void CalculateDetailTotal()
        {
            if (int.TryParse(txtQuantity.Text, out int quantity) && decimal.TryParse(txtUnitPrice.Text.Replace(",", ""), out decimal unitPrice))
            {
                decimal totalPrice = quantity * unitPrice;
                txtDetailTotalPrice.Text = totalPrice.ToString("N0");
            }
        }

        private void CalculateTotalAmount()
        {
            decimal total = 0;
            foreach (DataRow row in dtOrderDetails.Rows)
            {
                if (row.RowState != DataRowState.Deleted)
                {
                    total += Convert.ToDecimal(row["TotalPrice"]);
                }
            }
            txtTotalAmount.Text = total.ToString("N0");
        }

        private void UpdateProductStock(int orderID, bool isRestore)
        {
            string query = @"SELECT ProductID, Quantity FROM OrderDetails WHERE OrderID = @OrderID";
            MySqlParameter[] parameters = new MySqlParameter[]
            {
                new MySqlParameter("@OrderID", orderID)
            };
            DataTable details = DatabaseHelper.ExecuteQueryWithParams(query, parameters);

            foreach (DataRow row in details.Rows)
            {
                int productID = Convert.ToInt32(row["ProductID"]);
                int quantity = Convert.ToInt32(row["Quantity"]);

                string updateQuery;
                if (isRestore)
                {
                    updateQuery = "UPDATE Products SET QuantityInStock = QuantityInStock + @Quantity WHERE ProductID = @ProductID";
                }
                else
                {
                    updateQuery = "UPDATE Products SET QuantityInStock = QuantityInStock - @Quantity WHERE ProductID = @ProductID";
                }

                MySqlParameter[] updateParams = new MySqlParameter[]
                {
                    new MySqlParameter("@Quantity", quantity),
                    new MySqlParameter("@ProductID", productID)
                };
                DatabaseHelper.ExecuteNonQueryWithParams(updateQuery, updateParams);
            }
        }

        private bool ValidateOrder()
        {
            if (string.IsNullOrWhiteSpace(txtOrderCode.Text))
            {
                MessageBox.Show("Vui lòng nhập mã đơn hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrderCode.Focus();
                return false;
            }

            if (cboCustomer.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCustomer.Focus();
                return false;
            }

            if (dtOrderDetails.Rows.Count == 0)
            {
                MessageBox.Show("Đơn hàng phải có ít nhất một sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearInputFields()
        {
            txtOrderCode.Clear();
            cboCustomer.SelectedIndex = -1;
            dtpOrderDate.Value = DateTime.Now;
            cboStatus.SelectedIndex = 0;
            txtTotalAmount.Text = "0";
            cboProduct.SelectedIndex = -1;
            txtQuantity.Text = "1";
            txtUnitPrice.Text = "0";
            txtDetailTotalPrice.Text = "0";
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
                txtOrderCode.Enabled = isAdding;
                dgvOrders.Enabled = false;
                groupBoxOrderInfo.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = true;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
                btnSave.Enabled = false;
                btnCancel.Enabled = false;
                txtOrderCode.Enabled = true;
                dgvOrders.Enabled = true;
                groupBoxOrderInfo.Enabled = false;
            }
        }

        private void SetDetailControlsEnabled(bool enabled)
        {
            cboProduct.Enabled = enabled;
            txtQuantity.Enabled = enabled;
            btnAddProduct.Enabled = enabled;
            btnRemoveProduct.Enabled = enabled;
        }
    }
}
