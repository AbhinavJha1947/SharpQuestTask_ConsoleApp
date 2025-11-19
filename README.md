# ManageEmployee Console Application

The application manages employee information stored in SQL Server and supports:
- Listing all employees with their current salary
- Searching employees by name or title
- Viewing all titles with minimum and maximum salaries
- Adding new employee records interactively
- 
## 📂 Project Structure

ManageEmployee/
│
├── ManageEmployee/
│   ├── Program.cs
│   ├── ManageEmployee.csproj
│   │
│   ├── Models/
│   │   ├── Employee.cs
│   │   └── EmployeeSalary.cs
│   │
│   ├── Data/
│   │   └── Database.cs
│   │
│   └── Services/
│       └── EmployeeService.cs
│
├── Database/
│   └── script.sql
│
├── README.md
└── .gitignore

---

## 🛠 Technologies Used
- **C# .NET 8 Console Application**
- **SQL Server** (T-SQL)
- **Microsoft.Data.SqlClient**

---

## 🚀 How to Run

### 1. Create Database & Insert Data
Run the SQL script:
/Database/script.sql

This will:
- Create the database  
- Create tables  
- Insert **100 realistic employees**  
- Insert salary records  

### 2. Update Connection String
Set the connection string inside **Program.cs** or via environment variable:

```csharp
static string ConnectionString = "Server=YOUR_SERVER;Database=EmployeesDB;Trusted_Connection=True;";
Build the Project
dotnet build
. Run Commands
List all employees with current salary
dotnet run -- -list

List titles with min & max salary
dotnet run -- -titles

Search employees by name
dotnet run -- -list "john"

Search employees by title
dotnet run -- -list "developer"

Add a new employee
dotnet run -- -add
