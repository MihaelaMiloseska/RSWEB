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
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,CourseId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudentId,CourseId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(long id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }

        // GET: /Enrollments/Manage?courseId=1
        public async Task<IActionResult> Manage(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var students = await _context.Students
                .OrderBy(s => s.StudentId)
                .ToListAsync();

            var vm = new ManageEnrollmentsVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Year = DateTime.Now.Year,
                Semester = "Winter",
                Students = students.Select(s => new StudentPickVM
                {
                    StudentId = s.Id,
                    Display = $"{s.StudentId} - {s.FirstName} {s.LastName}",
                    Selected = false
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageEnrollmentsVM vm)
        {
            var course = await _context.Courses.FindAsync(vm.CourseId);
            if (course == null) return NotFound();

            var selectedStudentIds = vm.Students
                .Where(x => x.Selected)
                .Select(x => x.StudentId)
                .ToList();

            foreach (var sid in selectedStudentIds)
            {
                // IMPORTANT: кај тебе UNIQUE е StudentId + CourseId
                var existing = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseId == vm.CourseId && e.StudentId == sid);

                if (existing != null)
                {
                    // ако е веќе активен -> не прави дупликат, само прескокни (или можеш да update-ираш Year/Semester)
                    if (existing.FinishDate == null)
                    {
                        // ако сакаш сепак да го "префрлиш" во нова година/семестар, откоментирај:
                        // existing.Year = vm.Year;
                        // existing.Semester = vm.Semester;
                        continue;
                    }

                    // бил деактивиран -> реактивирај за нова година/семестар
                    existing.Year = vm.Year;
                    existing.Semester = vm.Semester;
                    existing.FinishDate = null;

                    // според спецификација: останато NULL кога се запишува
                    existing.Grade = null;
                    existing.ExamPoints = null;
                    existing.SeminarPoints = null;
                    existing.ProjectPoints = null;
                    existing.AdditionalPoints = null;
                    existing.SeminarUrl = null;
                    existing.ProjectUrl = null;

                    continue;
                }

                // не постои -> креирај нов
                _context.Enrollments.Add(new Enrollment
                {
                    CourseId = vm.CourseId,
                    StudentId = sid,
                    Year = vm.Year,
                    Semester = vm.Semester,

                    Grade = null,
                    ExamPoints = null,
                    SeminarPoints = null,
                    ProjectPoints = null,
                    AdditionalPoints = null,
                    SeminarUrl = null,
                    ProjectUrl = null,
                    FinishDate = null
                });
            }

            await _context.SaveChangesAsync();

            // подобро UX: врати на Courses Index (таму пак ќе кликнеш Manage ако треба)
            return RedirectToAction("Index", "Courses");
        }


        // GET: /Enrollments/Deactivate?courseId=1&year=2025&semester=Winter
        public async Task<IActionResult> Deactivate(int courseId, int year, string semester)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var active = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.Year == year && e.Semester == semester && e.FinishDate == null)
                .OrderBy(e => e.Student.StudentId)
                .ToListAsync();

            var vm = new DeactivateEnrollmentsVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Year = year,
                Semester = semester,
                FinishDate = DateTime.Today,
                ActiveEnrollments = active.Select(e => new StudentPickVM
                {
                    StudentId = e.Id, // ⚠️ тука чуваме EnrollmentId во StudentId поле (за да не правиме нов VM)
                    Display = $"{e.Student.StudentId} - {e.Student.FirstName} {e.Student.LastName}",
                    Selected = false
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(DeactivateEnrollmentsVM vm)
        {
            var selectedEnrollmentIds = vm.ActiveEnrollments
                .Where(x => x.Selected)
                .Select(x => x.StudentId) // овде е EnrollmentId
                .ToList();

            var enrollments = await _context.Enrollments
                .Where(e => selectedEnrollmentIds.Contains(e.Id))
                .ToListAsync();

            foreach (var e in enrollments)
            {
                e.FinishDate = vm.FinishDate;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
