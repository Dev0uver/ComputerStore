using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using ComputerStore.Models;
using System.Security.Claims;
using ComputerStore.Enums;

namespace ComputerStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ComputerStoreContext _context;

        public ProductsController(ComputerStoreContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var computerStoreContext = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .Where(p => p.Availability == true)
                .OrderBy(p => p.Id);
            return View(await computerStoreContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["SubcategoryId"] = new SelectList(_context.Subcategories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,CategoryId,SubcategoryId,Availability,Amount")] Product product)
        {
            if (product.Amount < 1)
            {
                TempData["ErrorMessage"] = "Product amount can't be 0 or less!";
                return RedirectToAction(nameof(Create));
            }

            if (ModelState.IsValid)
            {
                product.Availability = true;
                product.IsOnSale = true;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubcategoryId"] = new SelectList(_context.Subcategories, "Id", "Name", product.SubcategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubcategoryId"] = new SelectList(_context.Subcategories, "Id", "Name", product.SubcategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId,SubcategoryId,Availability,Amount")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (product.Amount < 1)
            {
                TempData["ErrorMessage"] = "Product amount can't be 0 or less!";
                return RedirectToAction(nameof(Edit));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (product.Amount == 0)
                    {
                        product.Availability = false;
                    }
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubcategoryId"] = new SelectList(_context.Subcategories, "Id", "Name", product.SubcategoryId);
            return View(product);
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]

        public async Task<IActionResult> ProductsPanel()
        {

            var computerStoreContext = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .OrderBy(p => p.Id);
            return View(await computerStoreContext.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> SalesTurnover(string firstDate, string secondDate)
        {
            if (firstDate == null || secondDate == null)
            {
                TempData["ErrorMessage"] = "Enter proper date!";
                return RedirectToAction(nameof(ProductsPanel));
            }

            var fDate = DateOnly.Parse(firstDate);
            var sDate = DateOnly.Parse(secondDate);

            if (fDate > sDate)
            {
                TempData["ErrorMessage"] = "Start date can't be less than the end date!";
                return RedirectToAction(nameof(ProductsPanel));
            }

            var products = await _context.Products
                .ToListAsync();
            Dictionary<int, Product> productsDict = new Dictionary<int, Product>();

            foreach (var item in products)
            {
                item.Amount = 0;
                productsDict[item.Id] = item;
            }

            var orders = await _context.Orders
                .Where(o => o.DeliveryDate >= fDate && o.DeliveryDate <= sDate && o.OrderStatus == OrderStatus.Completed.ToString())
                .ToListAsync();

            if (orders.Count == 0)
            {
                TempData["WarningMessage"] = "Not a single product was sold during this period!";
                return View("SalesTurnover");
            }

            foreach (var order in orders)
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderNumber == order.Id)
                    .ToListAsync();
                foreach (var item in orderItems)
                {
                    var product = _context.Products
                        .Where(p => p.Id == item.ProductId)
                        .FirstOrDefault();
                    if (productsDict.ContainsKey(product.Id))
                    {
                        productsDict[product.Id].Amount += item.Amount;
                    }
                }
            }
            var filteredList = productsDict.Values.ToList();
            return View("SalesTurnover", filteredList);
        }


        public async Task<IActionResult> StopSales(int id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("StopSales")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopSalesConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ComputerStoreContext.Products'  is null.");
            }

            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
            }

            product.IsOnSale = false;
            product.Availability = false;
            product.Amount = 0;

            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProductsPanel));
        }
    }
}
