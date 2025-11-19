using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using Services;
using ManageEmployee.Data;

class Program
{
    static string ConnectionString = "Server=ABHINAV\\SQLEXPRESS01;Database=EmployeesDB;Trusted_Connection=True;TrustServerCertificate=True;";

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var db = new Database(ConnectionString);
        var svc = new EmployeeService(db);

        if (args.Length == 0)
        {
            PrintHelp();
            return;
        }

        var cmd = args[0].ToLowerInvariant();

        try
        {
            if (cmd == "-list")
            {
                if (args.Length == 1)
                {
                    svc.ListAllWithCurrentSalary();
                    return;
                }

                var filter = string.Join(' ', args, 1, args.Length - 1).Trim('"');
                svc.ListByNameOrTitle(filter);
                return;
            }

            if (cmd == "-titles")
            {
                svc.ListTitlesMinMax();
                return;
            }

            if (cmd == "-add")
            {
                svc.AddInteractive();
                return;
            }

            PrintHelp();
        }
        catch (SqlException ex)
        {
            Console.WriteLine("SQL error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("ManageEmployee Console App");
        Console.WriteLine("Usage:");
        Console.WriteLine("  -seed                  Seed DB with 100 realistic employees (run once)");
        Console.WriteLine("  -list                  List all employees with current salary");
        Console.WriteLine("  -list \"john\"           List employees where name contains 'john' or title contains 'john'");
        Console.WriteLine("  -list \"developer\"      List employees whose current title contains 'developer'");
        Console.WriteLine("  -titles                List distinct titles with min and max salary (across all records)");
        Console.WriteLine("  -add                   Add a new employee (interactive prompts)");
        Console.WriteLine();
        Console.WriteLine("Make sure the database exists and connection string is correct in Program.cs or in env var IRMEPL_DB.");
    }
}
