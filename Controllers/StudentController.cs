using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RSWEB.Data;
using Microsoft.EntityFrameworkCore;

//gleda lista predmeti na koi e zapisan
[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public StudentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 1) Листа на мои предмети
    public async Task<IActionResult> MyCourses()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Email == null) return Forbid();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Email == user.Email);

        if (student == null) return Forbid();

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == student.Id)
            .OrderByDescending(e => e.Year)
            .ToListAsync();




        return View(enrollments);
    }

    // GET: Student/EditEnrollment/5
    public async Task<IActionResult> EditEnrollment(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Email == null) return Forbid();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Email == user.Email);
        if (student == null) return Forbid();

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == student.Id);

        if (enrollment == null) return NotFound();


        return View(enrollment);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEnrollment(
    long id,
    IFormFile? seminarFile,
    string? ProjectUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Email == null) return Forbid();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Email == user.Email);
        if (student == null) return Forbid();

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == student.Id);

        if (enrollment == null) return NotFound();

        // студент НЕ смее да менува ако предметот е завршен
        if (enrollment.FinishDate != null)
        {
            TempData["Error"] = "Cannot edit URLs because the course is finished.";
            return RedirectToAction(nameof(MyCourses));
        }

        // UPLOAD за Seminar
        if (seminarFile != null && seminarFile.Length > 0)
        {
            var ext = Path.GetExtension(seminarFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".doc", ".docx", ".pdf" };

            if (allowed.Contains(ext))
            {
                var uploadsDir = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads"
                );

                Directory.CreateDirectory(uploadsDir); // ако не постои

                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await seminarFile.CopyToAsync(stream);

                enrollment.SeminarUrl = "/uploads/" + fileName;
            }
        }

        // ✅ Project URL (GitHub)
        enrollment.ProjectUrl = ProjectUrl;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MyCourses));
    }
}
