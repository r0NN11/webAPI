using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    [PrimaryKey(nameof(CategoryID))]
    public class Category
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = default!;

        public string Description { get; set; } = default!;
    }
}
