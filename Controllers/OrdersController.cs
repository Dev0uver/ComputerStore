using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComputerStore.Enums;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace ComputerStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ComputerStoreContext _context;

        public OrdersController(ComputerStoreContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var computerStoreContext = _context.Orders
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .OrderBy(o => o.Id);
            return View(await computerStoreContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize(Roles = "Seller, Manager")]
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckPayment([Bind("Id,DeliveryDate,OrderStatus,PaymentStatus,OnlinePayment,Total,UserId, CreationDate")] Order order)
        {
            var cartItems = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.Product)
                .Where(c => c.UserId == order.UserId)
                .OrderBy(c => c.Id)
                .ToListAsync();
            foreach (var item in cartItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return NotFound();
                }
                else if (cartItems.Count() < 1)
                {
                    return RedirectToAction(nameof(Index), "Carts");
                }
                else if (!product.Availability)
                {
                    TempData["ErrorMessage"] = String
                        .Format("Product {0} isn't available", product.Name);
                    return RedirectToAction(nameof(Create), "Orders", new { userId = order.UserId });
                }
                else if (item.Amount > product.Amount)
                {

                    TempData["ErrorMessage"] = String
                        .Format("Amount of product {0} in cart is too much ({1}), available amount is {2}", product.Name, item.Amount, product.Amount);
                    return RedirectToAction(nameof(Create), "Orders", new { userId = order.UserId });
                }
            }
                if (!order.OnlinePayment)
            {
                order.PaymentStatus = OrderPaymentStatus.Awaiting.ToString();
                await Create(order);
                return RedirectToAction("Index", "Orders");
            }
            else if (order.OnlinePayment && order.PaymentStatus != OrderPaymentStatus.Paid.ToString())
            {
                return RedirectToAction("CardInput", "Card", order);
            }
            else
            {
                await Create(order);
                return RedirectToAction("Index", "Orders");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create1(Order order)
        {
            await CheckPayment(order);
            return RedirectToAction("Index", "Orders");
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create(string userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Id)
                .ToListAsync();
            ViewBag.cartItems = cartItems;
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DeliveryDate,OrderStatus,PaymentStatus,OnlinePayment,Total,UserId, CreationDate")] Order order)
        {
            long total = 0;
            order.OrderStatus = OrderStatus.New.ToString();
            var cartItems = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.Product)
                .Where(c => c.UserId == order.UserId)
                .OrderBy(c => c.Id)
                .ToListAsync();
            foreach(var item in cartItems)
            {
                total += item.Product.Price * item.Amount;
            }
            order.OrderStatus = OrderStatus.Confirmed.ToString();
            order.CreationDate = DateOnly.FromDateTime(DateTime.Now);
            order.DeliveryDate = order.CreationDate.AddDays(2);
            order.Total = total;

            _context.Add(order);
            _context.SaveChanges();
                
            var orderModel = await _context.Orders
                .OrderBy(o => o.Id)
                .LastOrDefaultAsync(o => o.UserId == order.UserId);
            OrderItem orderItem;
            foreach(var item in cartItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                orderItem = new OrderItem();
                orderItem.ProductId = item.ProductId;
                orderItem.OrderNumber = orderModel.Id;
                orderItem.Amount = item.Amount;

                product.Amount -= item.Amount;
                if (product.Amount == 0)
                {
                    product.Availability = false;
                }

                _context.Add(orderItem);
                _context.Carts.Remove(item);
                _context.Update(product);
                _context.SaveChanges();
            }

            orderModel.OrderStatus = OrderStatus.OnDelivery.ToString();
            _context.Update(orderModel);
            _context.SaveChanges();
           
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Seller, Manager")]
        public async Task<IActionResult> OrdersPanel(string userName)
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Seller, Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order.PaymentStatus == OrderPaymentStatus.Awaiting.ToString())
            {
                order.PaymentStatus = OrderPaymentStatus.Paid.ToString();
            }
            order.OrderStatus = OrderStatus.Completed.ToString();

            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrdersPanel));
        }

        [HttpPost]
        [Authorize(Roles = "Seller, Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            order.OrderStatus = OrderStatus.Delivered.ToString();
            order.DeliveryDate = DateOnly.FromDateTime(DateTime.Now);

            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrdersPanel));
        }

        [HttpPost]
        [Authorize(Roles = "Seller, Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            var orderItems = _context.OrderItems
                .Where(o => o.OrderNumber == id)
                .ToList();

            foreach(var item in orderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product.Availability == false)
                {
                    product.Availability = true;
                }
                product.Amount += item.Amount;

                _context.Update(product);
                _context.SaveChanges();
            }

            order.OrderStatus = OrderStatus.Canceled.ToString();

            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrdersPanel));
        }

        [Authorize(Roles = "Seller, Manager")]
        public async Task<IActionResult> Search(int orderId, string userName)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .AsQueryable();
            if (orderId > 0)
            {
                orders = orders
                    .Where(o => o.Id == orderId);
            }
            else if (orderId < 0)
            {
                TempData["WarningMessage"] = "Nothing found, try to change search attributes!";
                return PartialView("_SearchResults");
            }
            if (!string.IsNullOrEmpty(userName))
            {
                orders = orders
                    .Where(o => o.User.UserName.ToLower().Contains(userName.ToLower().ToLower()));
            }


            var filteredOrders = orders.ToList();
            if (filteredOrders.Count() == 0)
            {
                TempData["WarningMessage"] = "Nothing found, try to change search attributes!";
                return PartialView("_SearchResults");
            }

            return PartialView("_SearchResults", filteredOrders.OrderBy(o => o.Id));
        }
    }
}
