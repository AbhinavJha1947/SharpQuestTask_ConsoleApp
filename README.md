# ManageEmployee Console Application

The application connects to SQL Server and supports:

- Listing all employees with their current salary  
- Searching employees by name or title  
- Viewing all distinct titles with minimum and maximum salaries  
- Adding new employee records interactively  

---

## 🛠 Technologies Used
- **C# .NET 8 Console Application**
- **SQL Server (T-SQL)**
- **Microsoft.Data.SqlClient**
---

## 🚀 How to Run

### 1. Create Database & Insert Sample Data
Run the SQL script:
/Database/script.sql

This will:
- Create the database  
- Create required tables  
- Insert **100 realistic employees**  
- Insert salary records  

---

### 2. Update Connection String
Modify the connection string in **Program.cs**:

```csharp
    static string ConnectionString = "Server=ABHINAV\\SQLEXPRESS01;Database=EmployeesDB;Trusted_Connection=True;TrustServerCertificate=True;";
### 3. Build the Project
dotnet build
### 4. Run Commands
List all employees with current salary
dotnet run -- -list

List titles with minimum & maximum salary
dotnet run -- -titles

Search employees by name
dotnet run -- -list "john"

Search employees by title
dotnet run -- -list "developer"

Add a new employee
dotnet run -- -add

