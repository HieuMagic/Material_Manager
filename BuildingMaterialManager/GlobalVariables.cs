namespace BuildingMaterialManager
{
    /// <summary>
    /// Global variables to store logged-in user information
    /// </summary>
    public static class GlobalVariables
    {
        public static int UserID { get; set; }
        public static string UserFullName { get; set; }
        public static string UserRole { get; set; }
        public static bool IsLoggedIn { get; set; }

        /// <summary>
        /// Clear user session
        /// </summary>
        public static void ClearSession()
        {
            UserID = 0;
            UserFullName = string.Empty;
            UserRole = string.Empty;
            IsLoggedIn = false;
        }

        /// <summary>
        /// Check if user is Admin
        /// </summary>
        public static bool IsAdmin()
        {
            return UserRole == "Admin";
        }
    }
}
