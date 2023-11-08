using System;
using System.ComponentModel.DataAnnotations;
using ComputerStore.Models;

namespace ComputerStore.ViewModels
{
    public class CardInputViewModel
    {
        [Required]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Please enter a valid 16-digit card number.")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Please enter a valid expiration date (MM/YY).")]
        public string ExpirationDate { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "Please enter a valid 3-digit CVV.")]
        public string CVV { get; set; }
    }
}