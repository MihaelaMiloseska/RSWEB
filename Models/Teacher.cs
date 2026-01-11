using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSWEB.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Degree { get; set; }

        [StringLength(25)]
        public string? AcademicRank { get; set; }

        [StringLength(10)]
        public string? OfficeNumber { get; set; }

        public DateTime? HireDate { get; set; }

        public string? PhotoPath { get; set; }//photo path 

        public string? Email { get; set; }


        // Navigation
        public ICollection<Course> FirstCourses { get; set; } = new List<Course>();
        public ICollection<Course> SecondCourses { get; set; } = new List<Course>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}


