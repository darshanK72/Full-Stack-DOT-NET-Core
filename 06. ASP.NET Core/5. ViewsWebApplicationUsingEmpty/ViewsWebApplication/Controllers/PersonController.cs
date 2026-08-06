using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ViewsWebApplication.Models;

namespace ViewsWebApplication.Controllers
{
    [Controller]
    public class PersonController : Controller
    {
        [Route("/Persons")]
        public IActionResult ShowAllPersons()
        {
            List<Person> persons = new List<Person>()
            {
                new Person() {Name = "Darshan", RollNumber = 1, Marks = 89.60},
                new Person() {Name = "Aakash", RollNumber = 2, Marks = 87.84},
                new Person() {Name = "Prasad", RollNumber = 3, Marks = 72.64},
                new Person() {Name = "Abhishek", RollNumber = 4, Marks = 84.32},
                new Person() {Name = "Krushna", RollNumber = 5, Marks = 84.44},
                new Person() {Name = "Vaishnavi", RollNumber = 6, Marks = 82.87},
                new Person() {Name = "Khushi", RollNumber = 7, Marks = 81.32}
            };

            ViewData["persons"] = persons;

            ViewBag.Persons = persons;

            return View("persons");
        }
    }
}
