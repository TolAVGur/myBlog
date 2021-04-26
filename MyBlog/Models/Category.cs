using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name ="Category")]
        public string Name { get; set; }

        public virtual List<Post> Posts { get; set; }
    }
}
