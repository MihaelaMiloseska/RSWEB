using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSWEB.Models
{
    public class Student
    {
        [Key]
        public long Id { get; set; }   // bigint

        [Required, StringLength(10)]
        public string StudentId { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? EnrollmentDate { get; set; }

        public int? AcquiredCredits { get; set; }

        public int? CurrentSemester { get; set; }

        [StringLength(25)]
        public string? EducationLevel { get; set; }

        public string? PhotoPath { get; set; }

        public string? Email { get; set; }


        // Navigation
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}

