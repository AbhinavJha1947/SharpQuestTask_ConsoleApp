using ManageEmployee.Data;
using ManageEmployee.Models;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Services
{
    public class EmployeeService
    {
        private readonly Database _db;
        public EmployeeService(Database db) => _db = db;

        public void ListAllWithCurrentSalary()
        {
            var sql = @"
SELECT e.EmployeeId, e.FullName, e.SSN, e.City, e.State, e.JoinDate,
       s.Title, s.Salary
FROM dbo.Employee e
LEFT JOIN dbo.EmployeeSalary s ON s.EmployeeId = e.EmployeeId
    AND (s.ToDate IS NULL OR s.ToDate >= CAST(GETDATE() AS DATE))
    AND s.FromDate = (
        SELECT MAX(FromDate) FROM dbo.EmployeeSalary ss WHERE ss.EmployeeId = e.EmployeeId AND (ss.ToDate IS NULL OR ss.ToDate >= CAST(GETDATE() AS DATE))
    )
ORDER BY e.FullName;
";
            var dt = _db.QueryTable(sql);
            Console.WriteLine("{0,-5} {1,-25} {2,-20} {3,-15} {4,-12}", "Id", "Name", "Title", "Salary", "JoinDate");
            foreach (DataRow r in dt.Rows)
            {
                Console.WriteLine("{0,-5} {1,-25} {2,-20} {3,-12} {4,-12}",
                    r["EmployeeId"],
                    Trunc(r["FullName"]?.ToString(), 25),
                    Trunc(r["Title"]?.ToString() ?? "(none)", 20),
                    r["Salary"] == DBNull.Value ? "(n/a)" : ((decimal)r["Salary"]).ToString("C0", CultureInfo.InvariantCulture),
                    ((DateTime)r["JoinDate"]).ToString("yyyy-MM-dd")
                );
            }
        }

        public void ListByNameOrTitle(string filter)
        {
            // search name or current title
            var sql = @"
SELECT e.EmployeeId, e.FullName, e.SSN, e.City, e.State, e.JoinDate,
       s.Title, s.Salary
FROM dbo.Employee e
LEFT JOIN dbo.EmployeeSalary s ON s.EmployeeId = e.EmployeeId
    AND (s.ToDate IS NULL OR s.ToDate >= CAST(GETDATE() AS DATE))
    AND s.FromDate = (
        SELECT MAX(FromDate) FROM dbo.EmployeeSalary ss WHERE ss.EmployeeId = e.EmployeeId AND (ss.ToDate IS NULL OR ss.ToDate >= CAST(GETDATE() AS DATE))
    )
WHERE e.FullName LIKE @filter OR (s.Title IS NOT NULL AND s.Title LIKE @filter)
ORDER BY e.FullName;
";
            var dt = _db.QueryTable(sql, new SqlParameter("@filter", "%" + filter + "%"));
            if (dt.Rows.Count == 0)
            {
                Console.WriteLine("No results for filter: " + filter);
                return;
            }

            Console.WriteLine("{0,-5} {1,-25} {2,-20} {3,-12} {4,-12}", "Id", "Name", "Title", "Salary", "JoinDate");
            foreach (DataRow r in dt.Rows)
            {
                Console.WriteLine("{0,-5} {1,-25} {2,-20} {3,-12} {4,-12}",
                    r["EmployeeId"],
                    Trunc(r["FullName"]?.ToString(), 25),
                    Trunc(r["Title"]?.ToString() ?? "(none)", 20),
                    r["Salary"] == DBNull.Value ? "(n/a)" : ((decimal)r["Salary"]).ToString("C0", CultureInfo.InvariantCulture),
                    ((DateTime)r["JoinDate"]).ToString("yyyy-MM-dd")
                );
            }
        }

        public void ListTitlesMinMax()
        {
            var sql = @"
SELECT Title, MIN(Salary) AS MinSalary, MAX(Salary) AS MaxSalary
FROM dbo.EmployeeSalary
GROUP BY Title
ORDER BY Title;
";
            var dt = _db.QueryTable(sql);
            Console.WriteLine("{0,-35} {1,15} {2,15}", "Title", "Min Salary", "Max Salary");
            foreach (DataRow r in dt.Rows)
            {
                Console.WriteLine("{0,-35} {1,15} {2,15}",
                    Trunc(r["Title"]?.ToString(), 35),
                    ((decimal)r["MinSalary"]).ToString("C0", CultureInfo.InvariantCulture),
                    ((decimal)r["MaxSalary"]).ToString("C0", CultureInfo.InvariantCulture)
                );
            }
        }

        public void AddInteractive()
        {
            Console.WriteLine("Add new employee:");
            var emp = new Employee();
            emp.FullName = Prompt("Full name:");
            emp.SSN = Prompt("SSN:");
            emp.DOB = PromptDate("DOB (yyyy-MM-dd):");
            emp.Address = Prompt("Address:");
            emp.City = Prompt("City:");
            emp.State = Prompt("State:");
            emp.Zip = Prompt("Zip:");
            emp.Phone = Prompt("Phone:");
            emp.JoinDate = PromptDate("Join Date (yyyy-MM-dd):");

            var title = Prompt("Title:");
            var salary = PromptDecimal("Salary (e.g. 75000):");

            var insertEmpSql = @"
INSERT INTO dbo.Employee (FullName, SSN, DOB, Address, City, State, Zip, Phone, JoinDate, ExitDate)
VALUES (@FullName, @SSN, @DOB, @Address, @City, @State, @Zip, @Phone, @JoinDate, NULL);
SELECT SCOPE_IDENTITY();";

            var empIdObj = _db.ExecuteScalar(insertEmpSql,
                new SqlParameter("@FullName", emp.FullName),
                new SqlParameter("@SSN", emp.SSN),
                new SqlParameter("@DOB", emp.DOB),
                new SqlParameter("@Address", emp.Address ?? (object)DBNull.Value),
                new SqlParameter("@City", emp.City ?? (object)DBNull.Value),
                new SqlParameter("@State", emp.State ?? (object)DBNull.Value),
                new SqlParameter("@Zip", emp.Zip ?? (object)DBNull.Value),
                new SqlParameter("@Phone", emp.Phone ?? (object)DBNull.Value),
                new SqlParameter("@JoinDate", emp.JoinDate)
            );

            int empId = Convert.ToInt32(empIdObj);

            var insertSalSql = @"
INSERT INTO dbo.EmployeeSalary (EmployeeId, FromDate, ToDate, Title, Salary)
VALUES (@EmployeeId, @FromDate, NULL, @Title, @Salary);";

            _db.Execute(insertSalSql,
                new SqlParameter("@EmployeeId", empId),
                new SqlParameter("@FromDate", emp.JoinDate),
                new SqlParameter("@Title", title),
                new SqlParameter("@Salary", salary)
            );

            Console.WriteLine($"Employee {emp.FullName} (Id={empId}) added with title {title} salary {salary}.");
        }

        private static string Prompt(string label)
        {
            Console.Write(label + " ");
            return Console.ReadLine() ?? "";
        }

        private static DateTime PromptDate(string label)
        {
            while (true)
            {
                var s = Prompt(label);
                if (DateTime.TryParseExact(s, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var dt))
                    return dt;
                Console.WriteLine("Invalid date. Use yyyy-MM-dd.");
            }
        }

        private static decimal PromptDecimal(string label)
        {
            while (true)
            {
                var s = Prompt(label);
                if (decimal.TryParse(s, out var d)) return d;
                Console.WriteLine("Invalid number.");
            }
        }

        private static string Trunc(string? s, int len)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= len ? s : s.Substring(0, len - 3) + "...";
        }
    }
}
