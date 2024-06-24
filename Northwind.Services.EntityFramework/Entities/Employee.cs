using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    [PrimaryKey(nameof(EmployeeID))]
    public class Employee
    {
        public int EmployeeID { get; set; }

        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string Country { get; set; } = default!;
    }
}
