using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    [PrimaryKey(nameof(OrderID))]
    public class Order
    {
        public int OrderID { get; set; }

        public string CustomerID { get; set; } = default!;

        public int EmployeeID { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public int ShipVia { get; set; }

        public double Freight { get; set; }

        public string ShipName { get; set; } = default!;

        public string ShipAddress { get; set; } = default!;

        public string ShipCity { get; set; } = default!;

        public string? ShipRegion { get; set; } = default!;

        public string ShipPostalCode { get; set; } = default!;

        public string ShipCountry { get; set; } = default!;

        public ICollection<OrderDetail> OrderDetails { get; } = new List<OrderDetail>();
    }
}
