using Microsoft.AspNetCore.Mvc;
using UserController;
namespace UserController.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet]
    public ActionResult<HardwareReport> GetReport()
    {
        // Apelează clasa care citește toate componentele
        var report = HardWareDiagnostics.GetDiagnostics();

        // Returnează un răspuns HTTP 200 OK cu datele JSON
        return Ok(report);
    }
}