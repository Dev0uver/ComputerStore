using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ComputerStore.Controllers
{
    public class CartsController : Controller
    {
        private readonly ComputerStoreContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CartsController(ComputerStoreContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var computerStoreContext = _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .Include(c => c.Product.Category)
                .Include(c => c.Product.Subcategory)
                .Where(c => c.UserId == userId)
                .OrderBy(m => m.Id);
            return View(await computerStoreContext.ToListAsync());
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Carts == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .Include(c => c.Product.Category)
                .Include(c => c.Product.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Carts == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .Include(c => c.Product.Category)
                .Include(c => c.Product.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Amount,UserId")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            if (cart.Amount < 1)
            {
                TempData["ErrorMessage"] = "Product amount can't be 0 or less!";
                return RedirectToAction(nameof(Edit));
            }
            else if (cart.Amount > 1000) {
                TempData["ErrorMessage"] = "Amount of one product can't be more than 1000!";
                return RedirectToAction(nameof(Edit));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Id))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            return View(cart);
        }


        // POST: Carts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Carts == null)
            {
                return Problem("Entity set 'ComputerStoreContext.Carts'  is null.");
            }
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
          return (_context.Carts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.ProductId == id && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Amount++;
                _context.Carts.Update(cartItem);
                _context.SaveChanges();
                return RedirectToAction("Index", "Carts");
            }
            else
            {
                cartItem = new Cart
                {
                    ProductId = id,
                    UserId = userId,
                    Amount = 1
                };

                if (ModelState.IsValid)
                {
                    _context.Carts.Add(cartItem);
                    _context.SaveChanges();
                }
                return RedirectToAction("Index", "Carts");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll(string userId)
        {
            if (_context.Carts == null)
            {
                return Problem("Entity set 'ComputerStoreContext.Carts'  is null.");
            }
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var cartItems = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
                foreach (var cartItem in cartItems)
                {
                    _context.Carts.Remove(cartItem);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
