using Microsoft.AspNetCore.Mvc;
using ControllerWebApplication.Model;

namespace ControllerWebApplication.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        [Route("/")]
        [Route("/Home")]
        public ContentResult Index()
        {
            return Content("This is content result form controller class", "text/html");
            //return new ContentResult()
            //{
            //    Content = "This is Home Page",
            //    ContentType = "text/html"

            //};
        }

        [Route("/About")]
        public string About()
        {
            return "This is About Page";
        }

        [Route("/Products")]
        public IActionResult Products()
        {
            //return "This is Products Page";

            return RedirectToAction("CatProducts", "Home", new { });
        }

        [Route("/Services")]
        public string Services()
        {
            return "This is Services Page";
        }

        [Route("/User")]
        public IActionResult GetUser()
        {
            User u = new User() { Name = "Darshan", Id = 101 };
            //return new JsonResult(u);
            return Json(u);
        }

        [Route("/VirtualFile")]
        public IActionResult GetVirtualTextFile()
        {
            //return new VirtualFileResult("/TextFile.txt", "application/txt");
            return File("/TextFile.txt", "application/txt");
        }

        [Route("/PhysicalFile")]
        public IActionResult GetPhysicalTextFile()
        {
            //return new PhysicalFileResult(@"C:\Users\dakhairn\OneDrive - Capgemini\Desktop\Resources\7. Practice Coding\6. ASP.Net Core & MVC\3. ControllerWebApplication\PhysicalFile\PhysicalText.txt", "application/txt");
            return PhysicalFile(@"C:\Users\dakhairn\OneDrive - Capgemini\Desktop\Resources\7. Practice Coding\6. ASP.Net Core & MVC\3. ControllerWebApplication\PhysicalFile\PhysicalText.txt", "application/txt");
        }

        [Route("/FileContentResult")]
        public IActionResult GetFileContentResult()
        {
            byte[] bytes = System.IO.File.ReadAllBytes((@"C:\Users\dakhairn\OneDrive - Capgemini\Desktop\Resources\7. Practice Coding\6. ASP.Net Core & MVC\3. ControllerWebApplication\PhysicalFile\PhysicalText.txt"));
            //return new FileContentResult(bytes, "application/txt");
            return File(bytes, "application/txt");
        }

        [Route("/File")]
        public IActionResult GetFile()
        {
            if (!Request.Query.ContainsKey("id"))
            {
                //return new BadRequestObjectResult("Id is not mentioned");
                return BadRequest("Id is not mentioned");
            }
            else
            {
                var id = int.Parse(Request.Query["id"]);

                if(id < 1 || id > 1000)
                {
                    //return new NotFoundObjectResult("File with id not found");
                    return NotFound("File is not found");
                }
                else if (!bool.Parse(Request.Query["IsLoggedIn"]))
                {
                    //return new UnauthorizedObjectResult("You are not authorized");
                    return Unauthorized("You are not authorized");
                }
            }
            return File("/TextFile.txt", "application/txt");
        }

        [Route("/Category/Products")]
        public IActionResult CatProducts()
        {
            return Content("This is new category products page");
        }

        [Route("/Person/{personId}/{personName}")]
        public IActionResult GetPerson(int? personId,string personName)
        {
            return Content($"Person Id : {personId}\nPerson Name : {personName}");
        }


    }
}
