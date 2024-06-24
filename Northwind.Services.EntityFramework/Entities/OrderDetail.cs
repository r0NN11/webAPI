using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    [PrimaryKey(nameof(OrderID), nameof(ProductID))]
    public class OrderDetail
    {
        public int OrderID { get; set; }

        public long ProductID { get; set; }

        public double UnitPrice { get; set; }

        public long Quantity { get; set; }

        public double Discount { get; set; }
    }
}
