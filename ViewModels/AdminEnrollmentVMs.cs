using System.ComponentModel.DataAnnotations;

namespace RSWEB.ViewModels
{
    public class StudentPickVM
    {
        public long StudentId { get; set; }
        public string Display { get; set; } = "";
        public bool Selected { get; set; }
    }

    public class ManageEnrollmentsVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        [Required]
        public int Year { get; set; } = DateTime.Now.Year;

        [Required]
        public string Semester { get; set; } = "Winter"; // или "Zimski"

        public List<StudentPickVM> Students { get; set; } = new();
    }

    public class DeactivateEnrollmentsVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        [Required]
        public int Year { get; set; }

        [Required]
        public string Semester { get; set; } = "Winter";

        [Required]
        [DataType(DataType.Date)]
        public DateTime FinishDate { get; set; } = DateTime.Today;

        public List<StudentPickVM> ActiveEnrollments { get; set; } = new();
    }
}
