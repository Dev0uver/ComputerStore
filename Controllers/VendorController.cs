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
using Humanizer.Localisation.TimeToClockNotation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection;

namespace ComputerStore.Controllers
{
    public class VendorController : Controller
    {
        private readonly ComputerStoreContext _context;

        public VendorController(ComputerStoreContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var computerStoreContext = _context.Products
                .Include(v => v.Category)
                .Include(v => v.Subcategory)
                .OrderBy(m => m.Id);
            return View(await computerStoreContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> MakeDeliveryFromVendor(int id, int amount)
        {
            if (amount < 1)
            {
                TempData["ErrorMessage"] = "Product amount can't be 0 or less!";
                return RedirectToAction(nameof(Index));
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!product.Availability)
                {
                    product.Availability = true;
                    product.Amount += amount;
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    product.Amount += amount;
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
