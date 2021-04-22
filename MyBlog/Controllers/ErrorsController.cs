using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult UploadError()
        {
            return View();
        }

        public IActionResult ExtansionError()
        {
            return View();
        }
    }
}
