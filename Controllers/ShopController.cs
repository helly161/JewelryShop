using JewelryShop.Models.Data;
using JewelryShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JewelryShop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            // Declare list of CategoryVM
            List<CategoryVM> catrgoryVMList;

            // Init the list
            using (Db db = new Db())
            {
                catrgoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            return PartialView(catrgoryVMList);
        }

        public ActionResult Category(string name)
        {
            // Declare a list of prodoctVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                // Get category id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // Init the list
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                // Get category name
                var productCat = db.Categories.Where(x => x.Id == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.Name;
                    
            }

            // Return view with list
            return PartialView(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Declare the VM and the DTO
            ProductVM model;
            ProductDTO dto;

            // Init product id
            int id = 0;

            using (Db db = new Db())
            {
                // Check if product exists
                if (! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Get inserted id
                id = dto.Id;

                // Init model
                model = new ProductVM(dto);
            }

            // Return view with model
            return View("ProductDetails", model);
        }
    }
}