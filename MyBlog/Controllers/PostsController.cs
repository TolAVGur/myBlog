using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Models;
using MyBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace MyBlog.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _env;

        private readonly string[] permittedExtensions = new string[]
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif"
        };

        public PostsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Posts
        public async Task<IActionResult> Index(int? categoryId ,int pageNumber = 1)
        {
            // 1 - Формирование коллекции выводимых постов:
            var posts = _context.Posts.ToList();
            if (categoryId != null && categoryId != 0)
                posts = posts.Where(p => p.CategoryId == categoryId).ToList();

            // 2 - Разбивка коллекции постов на страницы пагинации:
            int pageSize = 3; 
            int count = posts.Count();
            var items = posts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToList();

            // 3 - Формирование коллекции категорий для создания фильтра:
            List<Category> categories = _context.Categories.ToList();
            categories.Insert(0, new Category() { Id = 0, Name = "Все категории" });

            // 4 - Создание менеджера пагинации:
            PageViewModel paginator = new PageViewModel(count, pageNumber, pageSize);

            // 5 - Создание модели представления постов:
            PostsViewModel viewModel = new PostsViewModel()
            {
                Posts = items,
                Paginator = paginator,
                Categories = new SelectList(categories, "Id", "Name")
            };
            return View(viewModel);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
                return NotFound();

            return View(post);
        }

        // GET: Posts/Create
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            var categories = _context.Categories.ToList();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Content,PublishDate,PublishTime,ImagePath,CategoryId")] Post post,
            IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string name = uploadFile.FileName;
                    var ext = Path.GetExtension(name);
                    if (permittedExtensions.Contains(ext))
                    {
                        string path = $"/files/{name}";
                        string serverPath = _env.WebRootPath + path;

                        using (FileStream fs = new FileStream(serverPath, FileMode.Create, FileAccess.Write))
                        {
                            await uploadFile.CopyToAsync(fs);
                        }
                        post.ImagePath = path;
                        _context.Posts.Add(post);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction("ExtansionError", "Errors");
                    }
                }
                else
                {
                    return RedirectToAction("UploadError", "Errors");
                }
            }
            var categories = _context.Categories.ToList();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", post.CategoryId);
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var categories = _context.Categories.ToList();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Content,PublishDate,PublishTime,ImagePath,CategoryId")] Post post,
            IFormFile uploadFile)
        {
            if (id != post.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string name = uploadFile.FileName;
                    var ext = Path.GetExtension(name);
                    if (permittedExtensions.Contains(ext)) 
                    {
                        try  
                        {
                            //*
                            string path = $"/files/{name}";
                            string oldImagePath = _context.Posts.Where(p => p.Id == id)
                                .AsNoTracking() // блокировка кеш памяти чтоб не конфликтовали 2 сущьности id 
                                .FirstOrDefault().ImagePath;
                            string oldServerPath = _env.WebRootPath + oldImagePath;
                            if (path != oldImagePath)
                            {
                                string serverPath = _env.WebRootPath + path;
                                using (FileStream fs = new FileStream(serverPath, FileMode.Create, FileAccess.Write))
                                {
                                    await uploadFile.CopyToAsync(fs);
                                }
                                //*
                                //System.IO.File.Delete(oldServerPath);
                            }
                            
                            post.ImagePath = path;
                            _context.Update(post);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!PostExists(post.Id))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        return RedirectToAction(nameof(Index), new { categoryId = post.CategoryId });
                    }
                    else
                        return RedirectToAction("ExtansionError", "Errors");
                }
                else
                    return RedirectToAction("UploadError", "Errors");
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
