using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JewelryShop.Models.ViewModels.Cart
{
    public class CartVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Image { get; set; }

    }
}