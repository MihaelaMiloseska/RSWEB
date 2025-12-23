using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSWEB.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int Credits { get; set; }

        [Required]
        public int Semester { get; set; }

        [StringLength(50)]
        public string? Programme { get; set; }

        [StringLength(25)]
        public string? EducationLevel { get; set; }

        // Two teachers (FK)
        public int? FirstTeacherId { get; set; }

        [ForeignKey(nameof(FirstTeacherId))]
        public Teacher? FirstTeacher { get; set; }

        public int? SecondTeacherId { get; set; }

        [ForeignKey(nameof(SecondTeacherId))]
        public Teacher? SecondTeacher { get; set; }

        // Many-to-many via Enrollment
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
