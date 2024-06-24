using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.Repositories;
using Employee = Northwind.Services.Repositories.Employee;
using Order = Northwind.Services.Repositories.Order;
using OrderDetail = Northwind.Services.Repositories.OrderDetail;
using Product = Northwind.Services.Repositories.Product;
using Shipper = Northwind.Services.Repositories.Shipper;

namespace Northwind.Services.EntityFramework.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly NorthwindContext context;

        public OrderRepository(NorthwindContext context)
        {
            this.context = context;
        }

        public async Task<IList<Order>> GetOrdersAsync(int skip, int count)
        {
            CheckParameters(skip, count);
            try
            {
                var entities = await this.context.Orders
                    .OrderBy(o => o.OrderID)
                    .Skip(skip)
                    .Take(count).ToListAsync();
                var orders = new List<Order>();
                foreach (var entity in entities)
                {
                    orders.Add(await this.GetOrderAsync(entity.OrderID));
                }

                return orders;
            }
            catch (OrderNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("An error occurred while retrieving the order.", ex);
            }
        }

        public async Task<Order> GetOrderAsync(long orderId)
        {
            var entity = await this.context.Orders
                .Include(o => o.OrderDetails)
                .SingleOrDefaultAsync(o => o.OrderID == orderId);

            if (entity == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }

            var employee = await this.GetEmployeeEntity(entity.EmployeeID);
            var shipper = await this.GetShipperEntity(entity.ShipVia);
            return this.MapEntityToOrder(entity, employee, shipper);
        }

        public async Task<long> AddOrderAsync(Order order)
        {
            CheckOrder(order);
            try
            {
                var entity = new Entities.Order();
                MapOrderToEntity(order, entity);
                _ = this.context.Orders.Add(entity);
                _ = await this.context.SaveChangesAsync();

                return entity.OrderID;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("An error occurred while adding the order.", ex);
            }
        }

        public async Task RemoveOrderAsync(long orderId)
        {
            var entity = await this.context.Orders
                .Include(o => o.OrderDetails)
                .SingleOrDefaultAsync(o => o.OrderID == orderId);

            if (entity == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }

            _ = this.context.Orders.Remove(entity);
            _ = await this.context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            CheckOrder(order);
            var entity = await this.context.Orders
                .Include(o => o.OrderDetails)
                .SingleOrDefaultAsync(o => o.OrderID == order.Id);

            if (entity == null)
            {
                throw new OrderNotFoundException($"Order with ID {order.Id} not found.");
            }

            MapOrderToEntity(order, entity);

            _ = await this.context.SaveChangesAsync();
        }

        private static void CheckParameters(int skip, int count)
        {
            if (skip < 0 || count <= 0)
            {
                throw new ArgumentOutOfRangeException(string.Empty);
            }
        }

        private static void CheckOrder(Order order)
        {
            if (order == null)
            {
                throw new OrderNotFoundException($"Order not found.");
            }
        }

        private static void MapOrderToEntity(Order order, Entities.Order entity)
        {
            entity.CustomerID = order.Customer.Code.Code;
            entity.EmployeeID = (int)order.Employee.Id;
            entity.OrderDate = order.OrderDate;
            entity.RequiredDate = order.RequiredDate;
            entity.ShippedDate = order.ShippedDate;
            entity.ShipVia = (int)order.Shipper.Id;
            entity.Freight = order.Freight;
            entity.ShipName = order.ShipName;
            entity.ShipAddress = order.ShippingAddress.Address;
            entity.ShipCity = order.ShippingAddress.City;
            entity.ShipRegion = order.ShippingAddress.Region ?? string.Empty;
            entity.ShipPostalCode = order.ShippingAddress.PostalCode;
            entity.ShipCountry = order.ShippingAddress.Country;

            entity.OrderDetails.Clear();
            foreach (var od in order.OrderDetails)
            {
                entity.OrderDetails.Add(MapOrderDetailToEntity(od));
            }
        }

        private static Entities.OrderDetail MapOrderDetailToEntity(OrderDetail orderDetail)
        {
            return new Entities.OrderDetail
            {
                ProductID = orderDetail.Product.Id,
                UnitPrice = orderDetail.UnitPrice,
                Quantity = orderDetail.Quantity,
                Discount = orderDetail.Discount,
            };
        }

        private Order MapEntityToOrder(Entities.Order entity, Entities.Employee? employee, Entities.Shipper? shipper)
        {
            var customerId = entity.CustomerID;
            var returnOrder = new Order(entity.OrderID)
            {
                Customer = new Customer(new CustomerCode(customerId))
                {
                    CompanyName = entity.ShipName,
                },
                Employee = new Employee(entity.EmployeeID)
                {
                    FirstName = employee?.FirstName ?? string.Empty,
                    LastName = employee?.LastName ?? string.Empty,
                    Country = employee?.Country ?? string.Empty,
                },
                OrderDate = entity.OrderDate,
                RequiredDate = entity.RequiredDate,
                ShippedDate = entity.ShippedDate,
                Shipper = new Shipper(entity.ShipVia)
                {
                    CompanyName = shipper?.CompanyName ?? string.Empty,
                },
                Freight = entity.Freight,
                ShipName = entity.ShipName,
                ShippingAddress = new ShippingAddress(
                    entity.ShipAddress,
                    entity.ShipCity,
                    entity.ShipRegion,
                    entity.ShipPostalCode,
                    entity.ShipCountry),
            };

            foreach (var od in entity.OrderDetails)
            {
                var orderDetail = this.MapEntityDetailToOrder(returnOrder, od).Result;
                returnOrder.OrderDetails.Add(orderDetail);
            }

            return returnOrder;
        }

        private async Task<OrderDetail> MapEntityDetailToOrder(Order returnOrder, Entities.OrderDetail od)
        {
            var product = await this.GetProductEntity(od.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Product is not valid");
            }

            var supplier = await this.GetSupplierEntity(product.SupplierID);
            var category = await this.GetCategoryEntity(product.CategoryID);

            return new OrderDetail(returnOrder)
            {
                Product = new Product(od.ProductID)
                {
                    ProductName = product.ProductName,
                    SupplierId = product.SupplierID,
                    Supplier = supplier?.CompanyName ?? string.Empty,
                    CategoryId = product.CategoryID,
                    Category = category?.CategoryName ?? string.Empty,
                },
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount,
            };
        }

        private async Task<Entities.Employee?> GetEmployeeEntity(int employeeID)
        {
            return await this.context.Employees.SingleOrDefaultAsync(o => o.EmployeeID == employeeID);
        }

        private async Task<Entities.Shipper?> GetShipperEntity(int shipperID)
        {
            return await this.context.Shippers.SingleOrDefaultAsync(o => o.ShipperID == shipperID);
        }

        private async Task<Entities.Product?> GetProductEntity(long productID)
        {
            return await this.context.Products.SingleOrDefaultAsync(o => o.ProductID == productID);
        }

        private async Task<Supplier?> GetSupplierEntity(int supplierID)
        {
            return await this.context.Suppliers.SingleOrDefaultAsync(o => o.SupplierID == supplierID);
        }

        private async Task<Category?> GetCategoryEntity(int categoryID)
        {
            return await this.context.Categories.SingleOrDefaultAsync(o => o.CategoryID == categoryID);
        }
    }
}
