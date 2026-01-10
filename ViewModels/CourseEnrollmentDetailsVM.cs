namespace RSWEB.ViewModels
{
    public class CourseEnrollmentDetailsVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        public List<CourseEnrollmentRowVM> Enrollments { get; set; } = new();
    }

    public class CourseEnrollmentRowVM
    {
        public long EnrollmentId { get; set; }
        public long StudentId { get; set; }

        public string StudentIndex { get; set; } = "";
        public string FullName { get; set; } = "";

        public string? Semester { get; set; }
        public int? Year { get; set; }
        public int? Grade { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        public string? SeminarUrl { get; set; }
        public string? ProjectUrl { get; set; }

        public DateTime? FinishDate { get; set; }
    }
}
