namespace RSWEB.ViewModels
{
    public class StudentEnrollRowVM
    {
        public long StudentDbId { get; set; }         // Student.Id
        public string Index { get; set; } = "";      // Student.StudentId (index)
        public string FullName { get; set; } = "";   // First + Last
        public bool Selected { get; set; }           // enrolled or not
    }

    public class CourseManageStudentsVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public List<StudentEnrollRowVM> Students { get; set; } = new();
    }
}
