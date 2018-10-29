using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SampleProject.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string MobileNo { get; set; }
    }
}