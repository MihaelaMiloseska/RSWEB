using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSWEB.Models
{
    public class Enrollment
    {
        [Key]
        public long Id { get; set; }

        // FK → Student
        [Required]
        public long StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; } = null!;

        // FK → Course
        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        [StringLength(10)]
        public string? Semester { get; set; }

        public int? Year { get; set; }
        public int? Grade { get; set; }

        [StringLength(255)]
        public string? SeminarUrl { get; set; }

        [StringLength(255)]
        public string? ProjectUrl { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        public DateTime? FinishDate { get; set; }
    }
}
