using ComputerStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ComputerStore.ViewModels;
using ComputerStore.Models;
using ComputerStore.Data;
using ComputerStore.Enums;
using System.Drawing.Text;

public class CardController : Controller
{

    public IActionResult CardInput(Order order)
    {
        Temp.Order = order;
        return View();
    }

    [HttpPost]
    public IActionResult ProcessCard(CardInputViewModel model)
    {
        if (ModelState.IsValid)
        {
            var rand = new Random();
            var order = Temp.Order;
            Temp.Order = null;
            if (rand.Next(3) == 0) {
                TempData["ErrorMessage"] = "Payment error, insufficient funds! Try again!";
                return RedirectToAction("CardInput", Temp.Order);
            }
            else
            {
                order.PaymentStatus = OrderPaymentStatus.Paid.ToString();
                return RedirectToAction("Create1", "Orders", order);
            }
        }
        else
        { 
            TempData["ErrorMessage"] = "Invalid card data!";
            return RedirectToAction("CardInput", Temp.Order);
        }

        return View("CardInput", model);
    }
}

public class Temp
{
    public static Order Order { get; set; }
}