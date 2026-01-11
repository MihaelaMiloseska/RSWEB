using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSWEB.Data;
using RSWEB.Models;
using RSWEB.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace RSWEB.Controllers
{
    [Authorize(Roles = "Admin")]

    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string? title, int? semester, string? programme)
        {
            var query = _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(c => c.Title.Contains(title));

            if (semester.HasValue)
                query = query.Where(c => c.Semester == semester.Value);

            if (!string.IsNullOrWhiteSpace(programme))
                query = query.Where(c => c.Programme != null && c.Programme.Contains(programme));

            // за да останат внесените вредности во form-ата
            ViewData["TitleFilter"] = title;
            ViewData["SemesterFilter"] = semester;
            ViewData["ProgrammeFilter"] = programme;

            return View(await query.ToListAsync());
        }
        //Courses by Teachers
        public async Task<IActionResult> ByTeacher(int teacherId)
        {
            var courses = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .Where(c => c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId)
                .ToListAsync();

            var teacher = await _context.Teachers.FindAsync(teacherId);
            ViewData["TeacherName"] = teacher != null ? teacher.FirstName + " " + teacher.LastName : "Teacher";

            return View(courses);
        }



        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        public async Task<IActionResult> ManageStudents(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var enrolledStudentIds = course.Enrollments
                .Select(e => e.StudentId)
                .ToHashSet();

            var vm = new CourseManageStudentsVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Students = await _context.Students
                    .OrderBy(s => s.StudentId)
                    .Select(s => new StudentEnrollRowVM
                    {
                        StudentDbId = s.Id,
                        Index = s.StudentId,
                        FullName = s.FirstName + " " + s.LastName,
                        Selected = enrolledStudentIds.Contains(s.Id)
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageStudents(CourseManageStudentsVM vm)
        {
            var existing = await _context.Enrollments
                .Where(e => e.CourseId == vm.CourseId)
                .ToListAsync();

            var selectedIds = vm.Students
                .Where(s => s.Selected)
                .Select(s => s.StudentDbId)
                .ToHashSet();

            // Remove enrollments (отпиши)
            var toRemove = existing.Where(e => !selectedIds.Contains(e.StudentId)).ToList();
            _context.Enrollments.RemoveRange(toRemove);

            // Add enrollments (запиши)
            var existingIds = existing.Select(e => e.StudentId).ToHashSet();
            var toAdd = selectedIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new Enrollment
                {
                    CourseId = vm.CourseId,
                    StudentId = id
                });

            await _context.Enrollments.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = vm.CourseId });
        }

        public async Task<IActionResult> EditEnrollmentDetails(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.Student)
                .ToListAsync();

            var vm = new CourseEnrollmentDetailsVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Enrollments = enrollments.Select(e => new CourseEnrollmentRowVM
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentIndex = e.Student.StudentId,
                    FullName = e.Student.FirstName + " " + e.Student.LastName,

                    Semester = e.Semester,
                    Year = e.Year,
                    Grade = e.Grade,
                    ExamPoints = e.ExamPoints,
                    SeminarPoints = e.SeminarPoints,
                    ProjectPoints = e.ProjectPoints,
                    AdditionalPoints = e.AdditionalPoints,
                    SeminarUrl = e.SeminarUrl,
                    ProjectUrl = e.ProjectUrl,
                    FinishDate = e.FinishDate
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollmentDetails(
        int id,
        CourseEnrollmentDetailsVM vm)
        {
            if (id != vm.CourseId) return NotFound();

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == id)
                .ToDictionaryAsync(e => e.Id);

            foreach (var row in vm.Enrollments)
            {
                if (!enrollments.TryGetValue(row.EnrollmentId, out var e))
                    continue;

                e.Semester = row.Semester;
                e.Year = row.Year;
                e.Grade = row.Grade;
                e.ExamPoints = row.ExamPoints;
                e.SeminarPoints = row.SeminarPoints;
                e.ProjectPoints = row.ProjectPoints;
                e.AdditionalPoints = row.AdditionalPoints;
                e.SeminarUrl = row.SeminarUrl;
                e.ProjectUrl = row.ProjectUrl;
                e.FinishDate = row.FinishDate;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = vm.CourseId });
        }










        // GET: Courses/Create
        public IActionResult Create()
        {
            var teachers = _context.Teachers
            .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
            .ToList();
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName");
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var teachers = _context.Teachers
            .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
            .ToList();
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.SecondTeacherId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            var teachers = _context.Teachers
            .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
            .ToList();
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.SecondTeacherId);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var teachers = _context.Teachers
            .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
            .ToList();
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.SecondTeacherId);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }

}
