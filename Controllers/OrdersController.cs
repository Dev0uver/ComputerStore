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

namespace ComputerStore.Controllers
{
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
            int total = 0;
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

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DeliveryDate,OrderStatus,PaymentStatus,OnlinePayment,Total,UserId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ComputerStoreContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> OrdersPanel()
        {
            var computerStoreContext = _context.Orders
                .Include(o => o.User)
                .OrderBy(o => o.Id);
            return View(await computerStoreContext.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order.PaymentStatus == OrderPaymentStatus.Awaiting.ToString())
            {
                order.PaymentStatus = OrderPaymentStatus.Paid.ToString();
            }
            order.OrderStatus = OrderStatus.Confirmed.ToString();

            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrdersPanel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            order.OrderStatus = OrderStatus.Delivered.ToString();

            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrdersPanel));
        }

        [HttpPost]
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

                product.Amount += item.Amount;

                _context.Update(product);
                _context.SaveChanges();
            }

            order.OrderStatus = OrderStatus.Canceled.ToString();

            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(OrdersPanel));
        }

        public async Task<IActionResult> Search(int orderId, string userName)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .AsQueryable();
            if (orderId <= 0 && userName == null)
            {
                TempData["ErrorMessage"] = "Error, both field are empty! Enter at least one!";
                return RedirectToAction(nameof(OrdersPanel));
            }
            if (orderId > 0)
            {
                // Фильтрация по ID заказа
                orders = orders
                    .Where(o => o.Id == orderId);
            }
            if (!string.IsNullOrEmpty(userName))
            {
                // Фильтрация по имени пользователя
                orders = orders
                    .Where(o => o.User.UserName.Contains(userName));
            }
            if (orders == null)
            {
                TempData["ErrorMessage"] = "Order not found, check the search arguments and try again!";
                return View();
            }

            var filteredOrders = orders.ToList();
            return View("OrdersPanel", filteredOrders);
        }

    }
}
