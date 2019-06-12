using JewelryShop.Models.Data;
using JewelryShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JewelryShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            // Declare list of PageVM
            List<PageVM> pagesList;

            using (Db db = new Db())
            {
                // Init the list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            // Return view with list
            return View(pagesList);
        }

        // Get Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // Post Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Declare slug
                string slug;

                // Init pageDTO 
                PageDTO dto = new PageDTO();

                // DTO title
                dto.Title = model.Title;

                // Check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists. ");
                    return View(model);
                }

                // DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 100;

                // Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have added a new page!";

            // Redirect
            return RedirectToAction("AddPage");
        }

        // Get Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // Declare pageVM
            PageVM model;

            using (Db db = new Db())
            {
                // Get the page
                PageDTO dto = db.Pages.Find(id);

                // Confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist. ");
                }

                //Init pageVM
                model = new PageVM(dto);
            }

            //Return view with model
            return View(model);
        }

        // Post Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Get page id
                int id = model.Id;

                // Declare slug
                string slug = "home";

                // Get the page
                PageDTO dto = db.Pages.Find(id);

                // DTO title
                dto.Title = model.Title;

                if (model.Slug != "home")
                {
                    // Check for and set slug if need be
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //Make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists. ");
                    return View(model);
                }

                // DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;

                // Save DTO
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited page!";

            // Redirect
            return RedirectToAction("EditPage");
        }

        // Get Admin/Pages/PageDetails
        public ActionResult PageDetails(int id)
        {
            // Declare PageVM
            PageVM model;

            using (Db db = new Db())
            {
                // Get the page
                PageDTO dto = db.Pages.Find(id);

                // Confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist. ");
                }

                //Init pageVM
                model = new PageVM(dto);
            }

            //Return view with model
            return View(model);
        }

        // Get Admin/Pages/DeletePage
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                // Get the page
                PageDTO dto = db.Pages.Find(id);

                // Remove the page
                db.Pages.Remove(dto);

                // Save
                db.SaveChanges();
            }

            // Redirect
            return RedirectToAction("Index");
        }

        // Get Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            // Declare model
            SidebarVM model;

            using (Db db = new Db())
            {
                // Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // Init model
                model = new SidebarVM(dto);
            }


            // Return view with model
            return View(model);

        }

        // Post Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                // Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // DTO the body
                dto.Body = model.Body;
                // Save
                db.SaveChanges();

            }

            // Set TempData message
            TempData["SM"] = "You have edited the sidebar!";

            // Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}