using Microsoft.AspNetCore.Mvc;
using Northwind.Orders.WebApi.Models;
using Northwind.Services.Repositories;

namespace Northwind.Orders.WebApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly ILogger<OrdersController> logger;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<FullOrder>> GetOrderAsync(long orderId)
        {
            try
            {
                var repositoryOrder = await this.orderRepository.GetOrderAsync(orderId);

                if (repositoryOrder == null)
                {
                    return this.NotFound();
                }

                var modelsOrder = MapRepositoryOrderToModelsOrder(repositoryOrder);

                return this.Ok(modelsOrder);
            }
            catch (OrderNotFoundException ex)
            {
                this.logger.LogError(ex, "Order with id {OrderId} not found.", orderId);
                return this.NotFound();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to retrieve order with id {OrderId}.", orderId);
                return this.StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BriefOrder>>> GetOrdersAsync(int? skip = null, int? count = null)
        {
            try
            {
                var repositoryOrders = await this.orderRepository.GetOrdersAsync(skip ?? 0, count ?? 10);

                var modelsOrders = repositoryOrders.Select(this.MapRepositoryOrderToModelsBriefOrder).ToList();

                return this.Ok(modelsOrders);
            }
            catch (ArgumentException ex)
            {
                this.logger.LogError(ex, "Invalid arguments provided for retrieving orders.");
                return this.BadRequest();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to retrieve orders.");
                return this.StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddOrder>> AddOrderAsync(BriefOrder order)
        {
            if (order == null)
            {
                this.logger.LogError("Order object is null.");
                return this.BadRequest("Order object is null.");
            }

            try
            {
                var repositoryOrder = MapModelsOrderToRepositoryOrder(order, order.Id);
                var orderId = await this.orderRepository.AddOrderAsync(repositoryOrder);

                var modelsAddOrder = new AddOrder { OrderId = orderId };

                return this.Ok(modelsAddOrder);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to add order.");
                return this.StatusCode(500);
            }
        }

        [HttpPut("{orderId}")]
        public async Task<ActionResult> UpdateOrderAsync(long orderId, BriefOrder order)
        {
            if (order == null)
            {
                this.logger.LogError("Order object is null for orderId {OrderId}.", orderId);
                return this.BadRequest("Order object is null.");
            }

            try
            {
                var repositoryOrder = MapModelsOrderToRepositoryOrder(order, orderId);
                await this.orderRepository.UpdateOrderAsync(repositoryOrder);

                return this.NoContent();
            }
            catch (OrderNotFoundException ex)
            {
                this.logger.LogError(ex, "Order with id {OrderId} not found.", orderId);
                return this.NotFound();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update order with id {OrderId}.", orderId);
                return this.StatusCode(500);
            }
        }

        [HttpDelete("{orderId}")]
        public async Task<ActionResult> RemoveOrderAsync(long orderId)
        {
            try
            {
                await this.orderRepository.RemoveOrderAsync(orderId);

                return this.NoContent();
            }
            catch (OrderNotFoundException ex)
            {
                this.logger.LogError(ex, "Order with id {OrderId} not found.", orderId);
                return this.NotFound();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to remove order with id {OrderId}.", orderId);
                return this.StatusCode(500);
            }
        }

        private static Order MapModelsOrderToRepositoryOrder(BriefOrder modelsOrder, long orderId)
        {
            var repositoryOrder = new Order(orderId)
            {
                Customer = new Northwind.Services.Repositories.Customer(new CustomerCode(modelsOrder.CustomerId))
                {
                    CompanyName = modelsOrder.ShipName,
                },
                Employee = new Northwind.Services.Repositories.Employee(modelsOrder.EmployeeId),
                Shipper = new Northwind.Services.Repositories.Shipper(modelsOrder.ShipperId),
                ShippingAddress = new Northwind.Services.Repositories.ShippingAddress(
                    modelsOrder.ShipAddress,
                    modelsOrder.ShipCity,
                    modelsOrder.ShipRegion,
                    modelsOrder.ShipPostalCode,
                    modelsOrder.ShipCountry),
                OrderDate = modelsOrder.OrderDate,
                RequiredDate = modelsOrder.RequiredDate,
                ShippedDate = modelsOrder.ShippedDate,
                Freight = modelsOrder.Freight,
            };

            return repositoryOrder;
        }

        private static FullOrder MapRepositoryOrderToModelsOrder(Order repositoryOrder)
        {
            return new FullOrder
            {
                Id = repositoryOrder.Id,
                Customer = new Models.Customer
                {
                    Code = repositoryOrder.Customer.Code.Code,
                    CompanyName = repositoryOrder.Customer.CompanyName,
                },
                Employee = new Models.Employee
                {
                    Id = repositoryOrder.Employee.Id,
                    FirstName = repositoryOrder.Employee.FirstName ?? string.Empty,
                    LastName = repositoryOrder.Employee.LastName ?? string.Empty,
                    Country = repositoryOrder.Employee.Country ?? string.Empty,
                },
                OrderDate = repositoryOrder.OrderDate,
                RequiredDate = repositoryOrder.RequiredDate,
                ShippedDate = repositoryOrder.ShippedDate,
                Freight = repositoryOrder.Freight,
                ShipName = repositoryOrder.ShipName,
                Shipper = new Models.Shipper
                {
                    Id = repositoryOrder.Shipper.Id,
                    CompanyName = repositoryOrder.Shipper.CompanyName ?? string.Empty,
                },
                ShippingAddress = new Models.ShippingAddress
                {
                    Address = repositoryOrder.ShippingAddress.Address,
                    City = repositoryOrder.ShippingAddress.City,
                    Region = repositoryOrder.ShippingAddress.Region,
                    PostalCode = repositoryOrder.ShippingAddress.PostalCode,
                    Country = repositoryOrder.ShippingAddress.Country,
                },
            };
        }

        private BriefOrder MapRepositoryOrderToModelsBriefOrder(Order repositoryOrder)
        {
            return new BriefOrder
            {
                Id = repositoryOrder.Id,
                CustomerId = repositoryOrder.Customer.Code.Code,
                EmployeeId = repositoryOrder.Employee.Id,
                OrderDate = repositoryOrder.OrderDate,
                RequiredDate = repositoryOrder.RequiredDate,
                ShippedDate = repositoryOrder.ShippedDate,
                Freight = repositoryOrder.Freight,
                ShipName = repositoryOrder.ShipName,
                ShipperId = repositoryOrder.Shipper.Id,
                ShipAddress = repositoryOrder.ShippingAddress.Address,
                ShipCity = repositoryOrder.ShippingAddress.City,
                ShipRegion = repositoryOrder.ShippingAddress.Region,
                ShipPostalCode = repositoryOrder.ShippingAddress.PostalCode,
                ShipCountry = repositoryOrder.ShippingAddress.Country,
            };
        }
    }
}
