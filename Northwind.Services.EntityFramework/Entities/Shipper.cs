using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    [PrimaryKey(nameof(ShipperID))]
    public class Shipper
    {
        public int ShipperID { get; set; }

        public string CompanyName { get; set; } = default!;

        public string Phone { get; set; } = default!;
    }
}
