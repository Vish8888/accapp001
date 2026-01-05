# Employee Management System - Blazor Web App

A modern, enterprise-grade employee management system built with **Blazor Server** and **Entity Framework Core**, featuring a clean Azure portal-inspired UI and full CRUD functionality.

## 🚀 Features

### 📊 **Employee Management**
- ✅ Add new employees with form validation
- ✅ View employees in professional data table
- ✅ Edit employee status (Active/Inactive)
- ✅ Delete employees with confirmation
- ✅ Real-time search and filtering
- ✅ Email domain extraction and validation

### 💼 **Professional UI**
- ✅ Azure portal-inspired design
- ✅ Vertical sidebar navigation
- ✅ Responsive Bootstrap 5.3 layout
- ✅ Microsoft Fluent Design System colors
- ✅ Professional typography and spacing
- ✅ Loading states and error handling

### 🗄️ **Database Integration**
- ✅ Entity Framework Core with SQL Server
- ✅ Code-first migrations
- ✅ Azure SQL Database support
- ✅ Connection string configuration
- ✅ Seed data for development
- ✅ Automatic timestamp tracking

### ☁️ **Azure Ready**
- ✅ Azure App Service deployment configuration
- ✅ Azure SQL Database integration
- ✅ Health checks for monitoring
- ✅ Environment-specific configurations
- ✅ Retry policies for resilience

## 🛠️ Technology Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| **ASP.NET Core** | 8.0 | Web framework |
| **Blazor Server** | 8.0 | Interactive UI framework |
| **Entity Framework Core** | 8.0 | Data access layer |
| **SQL Server** | 2019+ | Database |
| **Bootstrap** | 5.3.0 | CSS framework |
| **Bootstrap Icons** | 1.11.0 | Icon library |

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (for local development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for Azure deployment)

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/accapp001.git
cd accapp001
```

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Configure Database
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "EmployeeDatabase": "Server=(localdb)\\mssqllocaldb;Database=EmployeeDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. Run the Application
```bash
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

## 🗃️ Database Setup

### Local Development
The application uses Entity Framework Code-First approach with automatic database creation:

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration (optional - already included)
dotnet ef migrations add InitialCreate

# Update database (automatic on first run)
dotnet ef database update
```

### Sample Data
The application automatically seeds the database with sample employees:
- John Doe (Information Technology)
- Jane Smith (Human Resources) 
- Mike Johnson (Finance)

## 🎨 UI Features

### Navigation
- **Vertical Sidebar**: Azure portal-inspired navigation
- **Responsive Design**: Collapses on mobile devices
- **Active State Highlighting**: Visual feedback for current page

### Employee Management Page
- **Add Employee Form**: Collapsible form with validation
- **Data Table**: Sortable, searchable employee grid
- **Action Buttons**: Edit status and delete functionality
- **Status Badges**: Visual indicators for active/inactive employees
- **Email Links**: Clickable mailto links with domain display

### Styling
- **Microsoft Fluent Design**: Professional color palette
- **Bootstrap Components**: Cards, tables, forms, buttons
- **Custom CSS**: Enterprise-grade styling and animations
- **Loading States**: Spinners and skeleton screens

## 🌐 Azure Deployment

### Prerequisites
- Azure subscription
- Azure CLI or Azure portal access

### 1. Create Azure Resources
```bash
# Create resource group
az group create --name rg-accapp001 --location "East US"

# Create SQL Server and Database
az sql server create --name your-sql-server --resource-group rg-accapp001 --location "East US" --admin-user youradmin --admin-password YourPassword123!

az sql db create --resource-group rg-accapp001 --server your-sql-server --name employeedb001 --service-objective Basic

# Create App Service
az appservice plan create --name plan-accapp001 --resource-group rg-accapp001 --sku B1

az webapp create --resource-group rg-accapp001 --plan plan-accapp001 --name your-app-name --runtime "DOTNET|8.0"
```

### 2. Configure Connection String
```bash
az webapp config connection-string set --resource-group rg-accapp001 --name your-app-name --connection-string-type SQLAzure --settings EmployeeDatabase="Server=tcp:your-sql-server.database.windows.net,1433;Initial Catalog=employeedb001;User ID=youradmin;Password=YourPassword123!;Encrypt=True"
```

### 3. Deploy Application
```bash
# Build and publish
dotnet publish -c Release

# Deploy to Azure (using VS or Azure CLI)
az webapp up --resource-group rg-accapp001 --name your-app-name --plan plan-accapp001
```

## 🏗️ Project Structure

```
accapp001/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor          # Main application layout
│   │   ├── NavMenu.razor             # Sidebar navigation
│   │   └── *.css                     # Component-specific styles
│   └── Pages/
│       ├── Home.razor                # Landing page
│       ├── Counter.razor             # Counter demo page
│       ├── Weather.razor             # Weather demo page
│       ├── Employees.razor           # Employee management page
│       └── Employees.razor.cs        # Employee page code-behind
├── Data/
│   └── ApplicationDbContext.cs       # EF Core database context
├── Properties/
│   └── Models/
│       └── Employee.cs               # Employee entity model
├── wwwroot/
│   ├── app.css                       # Global application styles
│   └── bootstrap/                    # Bootstrap assets
├── appsettings.json                  # Application configuration
├── appsettings.Production.json       # Production configuration
├── Program.cs                        # Application startup
└── README.md                         # This file
```

## 📖 API Documentation

### Employee Entity
```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }                    // Required, 2-100 chars
    public string Email { get; set; }                   // Required, valid email, unique
    public string Department { get; set; }              // Required, max 50 chars
    public DateTime CreatedDate { get; set; }           // Auto-set on creation
    public DateTime? LastModified { get; set; }         // Auto-updated on changes
    public bool IsActive { get; set; } = true;          // Default true
    
    // Computed Properties
    public string EmailDomain { get; }                  // Extracts domain from email
    public int DaysSinceCreated { get; }                // Calculates days since creation
    public string Status { get; }                       // "Active" or "Inactive"
}
```

### Available Departments
- Information Technology
- Human Resources
- Finance
- Marketing
- Operations
- Sales

## 🔧 Configuration

### Environment Variables
| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | Development |
| `AZURE_SQL_CONNECTIONSTRING` | Azure SQL connection string | (from appsettings.json) |

### Health Checks
- **Endpoint**: `/health/ready`
- **Purpose**: Database connectivity check
- **Response**: `Healthy` or `Unhealthy`

## 🧪 Development

### Adding New Employees
1. Navigate to `/employees`
2. Click "Add Employee" button
3. Fill out the form with required fields
4. Click "Add Employee" to save

### Modifying Employee Status
1. Find employee in the table
2. Click the toggle button (play/pause icon)
3. Confirm the status change

### Deleting Employees
1. Find employee in the table
2. Click the delete button (trash icon)
3. Confirm deletion in the prompt

### Custom Styling
Modify `wwwroot/app.css` for global styles or create component-specific CSS files using the naming convention `ComponentName.razor.css`.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**MD Azhar Uddin**
- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your LinkedIn](https://linkedin.com/in/yourprofile)

## 🙏 Acknowledgments

- Microsoft for the excellent Blazor framework
- Bootstrap team for the UI components
- Entity Framework Core team for the data access layer
- Azure team for the cloud platform

## 📊 Screenshots

### Home Page
![Home Page](screenshots/home-page.png)

### Employee Management
![Employee Management](screenshots/employees-page.png)

### Add Employee Form
![Add Employee](screenshots/add-employee-form.png)

---

⭐ If you found this project helpful, please give it a star!