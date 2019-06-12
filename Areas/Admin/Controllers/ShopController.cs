using JewelryShop.Areas.Admin.Models.ViewModels.Shop;
using JewelryShop.Models.Data;
using JewelryShop.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace JewelryShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // Declare a list of models
            List<CategoryVM> categoriesList;

            using (Db db = new Db())
            {
                // Init the list
                categoriesList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            // Return view with list
            return View(categoriesList);
        }

        [HttpGet]
        public ActionResult AddNewCategory()
        {
            return View();
        }

        // Post Admin/shop/AddNewCategory
        [HttpPost]
        public ActionResult AddNewCategory(CategoryVM model)
        {
            // Declare id
            string catName = model.Name;

            using (Db db = new Db())
            {
                // Check that the cat name is unique
                if (db.Categories.Any(x => x.Name == catName))
                {
                    ModelState.AddModelError("", "That title or slug already exists. ");
                    return RedirectToAction("addNewCategory");
                }
                else
                {


                    // Init the DTO
                    CategoryDTO dto = new CategoryDTO();

                    // Add to DTO
                    dto.Name = catName;
                    dto.Slug = catName.Replace(" ", "-").ToLower();
                    dto.Sorting = 100; // when the category is added is going to be the last one

                    // Save the DTO
                    db.Categories.Add(dto);
                    db.SaveChanges();
                }
            }

            // Return the view
            return RedirectToAction("Categories");
        }

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // Get the page
                CategoryDTO dto = db.Categories.Find(id);

                // Remove the page
                db.Categories.Remove(dto);

                // Save
                db.SaveChanges();
            }

            // Redirect
            return RedirectToAction("Categories");
        }

        // Post Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Check that the cat name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                // Get the DTO
                CategoryDTO dto = db.Categories.Find(id);

                // Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Save
                db.SaveChanges();

            }

            // Redirect
            return "ok";
        }

        // Get Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Init model
            ProductVM model = new ProductVM();

            // Add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // Return view with model
            return View(model);
        }

        // Post Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }

            }

            // Declare product id
            int id;

            // Init and save productDTO
            using (Db db = new Db())
            {
                ProductDTO dto = new ProductDTO();

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ","-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.Products.Add(dto);
                db.SaveChanges();

                // Get the id
                id = dto.Id;
            }

            // Set TempData message
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            // Create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Check if a file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                // Get file extension
                string ext = file.ContentType.ToLower();

                // Verify extension
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" && ext != "image/x-png" && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }

                //Init image name
                string imageName = file.FileName;

                // Save image name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();

                }

                // Set orginal and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // Save original
                file.SaveAs(path);

                // Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            // Redirect
            return RedirectToAction("AddProduct");
        }

        // Get Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            // Declare a list of ProductVM
            List<ProductVM> listOfProductVM;

            // Set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // Init the list
                listOfProductVM = db.Products.ToArray().Where(x => catId == null || catId == 0 || x.CategoryId == catId).
                    Select(x => new ProductVM(x)).ToList();

                // populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            // Set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            // Return view with list
            return View(listOfProductVM);
        }

        // Get Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Declare productVM
            ProductVM model;

            using (Db db = new Db())
            {
                // Get the product
                ProductDTO dto = db.Products.Find(id);

                // Make sure product exist
                if (dto == null)
                {
                    return Content("The product does not exist. ");
                }

                // Init model
                model = new ProductVM(dto);

                // Make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery//Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }

            return View(model);
        }

        // Post Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Get product id
            int id = model.Id;

            // Populate categories select list and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery//Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();


            }

            // Set TempData message
            TempData["SM"] = "You have edited the product";

            #region Image Upload

            // Check for file upload
            if (file != null && file.ContentLength > 0)
            {
                // Get file extension
                string ext = file.ContentType.ToLower();

                // Verify extension
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" && ext != "image/x-png" && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }

                // Set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // Delete files from directories

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (FileInfo file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                // Save image name
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Save original and thumbs images
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            // Redirect
            return RedirectToAction("EditProduct");
        }

        // Get Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            //Delete product from db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            // Delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads\\Products\\" + id.ToString(), Server.MapPath(@"\")));
            var originalDirectory2 = new DirectoryInfo(string.Format("{0}Images\\Uploads\\", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory2.ToString(), "Products\\" + id.ToString());

            foreach (var file in originalDirectory.GetFiles("*", SearchOption.AllDirectories))
                file.Attributes &= ~FileAttributes.ReadOnly;

            Directory.Delete(pathString, true);

            // Redirect
            return RedirectToAction("Products");
        }

        public ActionResult Orders(string name)
        {
            // Init list of OrdersForAdminVM
            List<OrdersForAdminVM> orderssForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Init list of orderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop Throught list of orderVM
                foreach (var order in orders)
                {
                    // Init product dict
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of orderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;

                    // Loop throught list of orderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to orderForAdminVM list
                    orderssForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            // Return view with orderForAdminVM list
            return View(orderssForAdmin);

        }
    }
}