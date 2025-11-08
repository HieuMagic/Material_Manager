#!/usr/bin/env fish

# Building Material Manager - Setup Script
# This script helps set up the project on Linux with Mono

echo "=== Building Material Manager - Project Setup ==="
echo ""

# Check if Mono is installed
if not command -v mono &>/dev/null
    echo "❌ Mono is not installed!"
    echo "Please install Mono: sudo apt install mono-complete"
    exit 1
end

echo "✓ Mono is installed"

# Check if NuGet is installed
if not command -v nuget &>/dev/null
    echo "❌ NuGet is not installed!"
    echo "Please install NuGet: sudo apt install nuget"
    exit 1
end

echo "✓ NuGet is installed"

# Check if MySQL is installed
if not command -v mysql &>/dev/null
    echo "⚠️  MySQL client is not installed!"
    echo "Please install MySQL: sudo apt install mysql-client"
    echo ""
else
    echo "✓ MySQL client is installed"
end

echo ""
echo "=== Step 1: Restore NuGet Packages ==="
cd (dirname (status -f))
cd BuildingMaterialManager
nuget restore packages.config -PackagesDirectory ../packages

echo ""
echo "=== Step 2: Database Setup ==="
echo "To set up the database, run the following MySQL command:"
echo ""
echo "  mysql -u root -p < ../database_script.sql"
echo ""
echo "Or manually:"
echo "  1. Login to MySQL: mysql -u root -p"
echo "  2. Execute the script: source /path/to/database_script.sql"
echo ""

echo "=== Step 3: Update Connection String ==="
echo "Edit App.config and update the MySQL connection string if needed:"
echo "  - Server: localhost (default)"
echo "  - Database: BuildingMaterialDB"
echo "  - User: root"
echo "  - Password: your_password"
echo ""

echo "=== Step 4: Build Project ==="
echo "To build the project, run:"
echo "  msbuild BuildingMaterialManager.csproj"
echo ""
echo "Or use xbuild (older Mono versions):"
echo "  xbuild BuildingMaterialManager.csproj"
echo ""

echo "=== Step 5: Run Application ==="
echo "After successful build, run:"
echo "  mono bin/Debug/BuildingMaterialManager.exe"
echo ""

echo "=== Default Login Credentials ==="
echo "  Admin Account:"
echo "    Username: admin"
echo "    Password: admin123"
echo ""
echo "  Staff Account:"
echo "    Username: staff1"
echo "    Password: staff123"
echo ""

echo "✓ Setup instructions complete!"
echo "Follow the steps above to complete the project setup."
