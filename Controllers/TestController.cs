using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComputerStore.Controllers
{
    public class TestController : Controller
    {

        public readonly ComputerStoreContext _context;

        public TestController(ComputerStoreContext context)
        {
            _context = context;
        }

        // GET: HomeController1
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.Include(b => b.Category).Include(b => b.Subcategory).ToListAsync());
        }

        // GET: HomeController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeController1/Create
        public IActionResult Create()
        {
            var categories =  _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, nameof(Category.Id), nameof(Category.Name));
            var subcategories = _context.Subcategories.ToList();
            ViewBag.Subcategories = new SelectList(subcategories, nameof(Subcategory.Id), nameof(Subcategory.Name));
            return View();
        }

        // POST: HomeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(nameof(Product.Name), nameof(Product.Description), nameof(Product.Price),
            nameof(Product.CategoryId), nameof(Product.SubcategoryId), nameof(Product.Amount))] Product product)
        {
            product.Availability = true;
            product.Category = _context.Categories.Where(c => c.Id == (product.CategoryId)).First();
            product.Subcategory = _context.Subcategories.Where(c => c.Id == (product.SubcategoryId)).First();
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            return View(product);
        }

        // GET: HomeController1/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
