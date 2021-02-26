namespace SoftUni
{
    using SoftUni.Data;
    using SoftUni.Models;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.EntityFrameworkCore;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new SoftUniContext();

            using (context)
            {
                Console.WriteLine(RemoveTown(context));
            }
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => new { e.FirstName, e.LastName, e.MiddleName, e.JobTitle, e.Salary });

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:F2}");
            }

            return sb.ToString();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new { e.FirstName, e.Salary })
                .Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName);

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:F2}");
            }

            return sb.ToString();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    DepartmentName = e.Department.Name,
                    e.Salary
                })
                .Where(e => e.DepartmentName == "Research and Development")
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName);

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.DepartmentName} - ${employee.Salary:F2}");
            }

            return sb.ToString();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAddress = new Address();
            newAddress.AddressText = "Vitoshka 15";
            newAddress.TownId = 4;
            var nakovEmployee = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");
            nakovEmployee.Address = newAddress;

            context.SaveChanges();

            var employees = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Select(e => new { e.AddressId, e.Address.AddressText })
                .Take(10);

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.AddressText}");
            }

            return sb.ToString();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects.Select(ep => ep.Project)
                })
                .Where(e => e.Projects.Any(p => p.StartDate.Year >= 2001 && p.StartDate.Year <= 2003))
                .Take(10)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - Manager: {emp.ManagerFirstName} {emp.ManagerLastName}");

                foreach (var pr in emp.Projects)
                {
                    sb.Append(
                            $"--{pr.Name} - {pr.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} - ");

                    if (pr.EndDate == null)
                    {
                        sb.AppendLine("not finished");
                    }
                    else
                    {
                        sb.AppendLine($"{pr.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
                    }
                }
            }

            return sb.ToString();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Select(a => new { a.AddressText, TownName = a.Town.Name, EmployeesCount = a.Employees.Count, })
                .OrderByDescending(a => a.EmployeesCount)
                .ThenBy(a => a.TownName)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var addr in addresses)
            {
                sb.AppendLine($"{addr.AddressText}, {addr.TownName} - {addr.EmployeesCount} employees");
            }

            return sb.ToString();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context
                .Employees
                .Select(e => new
                {
                   e.EmployeeId, e.FirstName, e.LastName, e.JobTitle, Projects = e.EmployeesProjects.Select(ep => ep.Project.Name).ToList()
                })
                .FirstOrDefault(e => e.EmployeeId == 147);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            foreach (var pr in employee.Projects.OrderBy(p => p))
            {
                sb.AppendLine($"{pr}");
            }

            return sb.ToString();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context
                .Departments
                .Include(d => d.Employees)
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    DepartmentName = d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
                    Employees = d.Employees.Select(e => new
                    {
                        EmployeeFirstName = e.FirstName, EmployeeLastName = e.LastName, e.JobTitle
                    }).ToList(),
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var dep in departments)
            {
                sb.AppendLine($"{dep.DepartmentName} – {dep.ManagerFirstName} {dep.ManagerLastName}");

                foreach (var emp in dep.Employees.OrderBy(e => e.EmployeeFirstName).ThenBy(e => e.EmployeeLastName))
                {
                    sb.AppendLine($"{emp.EmployeeFirstName} {emp.EmployeeLastName} - {emp.JobTitle}");
                }
            }

            return sb.ToString();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context
                .Projects
                .Select(p => new { p.Name, p.Description, p.StartDate })
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var pr in projects)
            {
                sb.AppendLine(pr.Name);
                sb.AppendLine(pr.Description);
                sb.AppendLine(pr.StartDate.ToString("M/d/yyyy h:mm:ss tt"));
            }

            return sb.ToString();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            string[] tokenDepartments = new[] { "Engineering", "Tool Design", "Marketing", "Information Services" };

            var employees = context
                .Employees
                .Include(e => e.Department)
                .Where(e => tokenDepartments.Contains(e.Department.Name))
                .ToList();

            employees.ForEach(e => e.Salary *= 1.12M);

            context.SaveChanges();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName))
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} (${emp.Salary:F2})");
            }

            return sb.ToString();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Select(e => new { e.FirstName, e.LastName, e.JobTitle, e.Salary })
                .Where(e => e.FirstName.ToUpper().StartsWith("SA"))
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName))
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle} - (${emp.Salary:F2})");
            }

            return sb.ToString();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectToDelete = context.Projects.Include(p => p.EmployeesProjects).FirstOrDefault(p => p.ProjectId == 2);
            var employeeProjects = projectToDelete.EmployeesProjects.Where(ep => ep.ProjectId == 2).ToList();

            context.EmployeesProjects.RemoveRange(employeeProjects);
            context.Projects.Remove(projectToDelete);

            context.SaveChanges();

            StringBuilder sb = new StringBuilder();

            var projectsOutput = context.Projects.Take(10).ToList();

            foreach (var pr in projectsOutput)
            {
                sb.AppendLine(pr.Name);
            }

            return sb.ToString();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var town = context
                .Towns
                .Include(t => t.Addresses)
                .FirstOrDefault(t => t.Name == "Seattle");
            var addresses = town.Addresses.ToList();
            int countOfAddresses = addresses.Count;
            var employees = context
                .Employees
                .Where(e => e.AddressId.HasValue && addresses.Select(a => a.AddressId).Contains(e.AddressId.Value))
                .ToList();

            employees.ForEach(e => e.AddressId = null);

            context.RemoveRange(addresses);
            context.Remove(town);

            context.SaveChanges();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{countOfAddresses} addresses in Seattle were deleted");

            return sb.ToString();
        }
    }
}
