using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.ViewModels
{
    public class PostsViewModel
    {
        public List<Post> Posts { get; set; }
        public PageViewModel Paginator { get; set; }
    }
}
