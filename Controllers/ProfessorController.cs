using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSWEB.Data;
using RSWEB.Models;

namespace RSWEB.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfessorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Professor/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null) return Forbid();

            var email = user.Email.Trim();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email != null && t.Email.Trim() == email);

            if (teacher == null) return Forbid();

            var courses = await _context.Courses
                .Where(c => c.FirstTeacherId == teacher.Id || c.SecondTeacherId == teacher.Id)
                .OrderBy(c => c.Title)
                .ToListAsync();

            return View(courses);
        }



        // GET: /Professor/CourseStudents/5?year=2025
        public async Task<IActionResult> CourseStudents(int id, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null) return Forbid();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == user.Email);
            if (teacher == null) return Forbid();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            if (course.FirstTeacherId != teacher.Id && course.SecondTeacherId != teacher.Id)
                return Forbid();

            var lastYear = await _context.Enrollments
                .Where(e => e.CourseId == id)
                .Select(e => e.Year)
                .MaxAsync();

            int selectedYear = year ?? lastYear ?? DateTime.Now.Year;

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == id && e.Year == selectedYear)
                .OrderBy(e => e.Student.StudentId)
                .ToListAsync();

            var availableYears = await _context.Enrollments
                .Where(e => e.CourseId == id && e.Year != null)
                .Select(e => e.Year!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Any())
                availableYears.Add(selectedYear);

            var vm = new ProfessorCourseStudentsVM
            {
                Course = course,
                Year = selectedYear,
                AvailableYears = availableYears,
                Enrollments = enrollments
            };

            return View(vm);
        }

        // POST: /Professor/UpdateEnrollment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrollment(ProfessorEnrollmentEditVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null) return Forbid();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == user.Email);

            if (teacher == null) return Forbid();

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == vm.EnrollmentId);

            if (enrollment == null) return NotFound();

            // Security: professor must teach this course
            if (enrollment.Course.FirstTeacherId != teacher.Id &&
                enrollment.Course.SecondTeacherId != teacher.Id)
                return Forbid();

            // Only active students can be edited
            if (enrollment.FinishDate != null)
            {
                return RedirectToAction(nameof(CourseStudents),
                    new { id = enrollment.CourseId, year = enrollment.Year });
            }

            // Update allowed fields
            enrollment.ExamPoints = vm.ExamPoints;
            enrollment.SeminarPoints = vm.SeminarPoints;
            enrollment.ProjectPoints = vm.ProjectPoints;
            enrollment.AdditionalPoints = vm.AdditionalPoints;

            enrollment.Grade = vm.Grade;
            enrollment.FinishDate = vm.FinishDate;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CourseStudents),
                new { id = enrollment.CourseId, year = enrollment.Year });
        }
    }

    // ===== ViewModels =====

    public class ProfessorCourseStudentsVM
    {
        public Course Course { get; set; } = null!;
        public int Year { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public List<Enrollment> Enrollments { get; set; } = new();
    }

    public class ProfessorEnrollmentEditVM
    {
        public long EnrollmentId { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        public int? Grade { get; set; }
        public DateTime? FinishDate { get; set; }
    }
}
