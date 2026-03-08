using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHM.Application.DTOs.Patients;
using RHM.Infrastructure.Persistence;

namespace RHM.API.Controllers;

/// <summary>
/// Endpoint público para autocomplete de municipios DIVIPOLA.
/// No requiere autenticación (usado en el formulario público).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DivipolaController : ControllerBase
{
    private readonly AppDbContext _db;

    public DivipolaController(AppDbContext db) => _db = db;

    /// <summary>
    /// Busca municipios por nombre (mínimo 2 caracteres).
    /// Retorna hasta 10 resultados ordenados por nombre.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(Array.Empty<DivipolaDto>());

        var normalized = q.Trim().ToUpperInvariant()
            .Replace("Á", "A").Replace("É", "E").Replace("Í", "I")
            .Replace("Ó", "O").Replace("Ú", "U").Replace("Ñ", "N");

        var results = await _db.DivipolaCodes
            .Where(d => d.MunicipioNormalized.Contains(normalized) ||
                        d.Departamento.ToUpper().Contains(normalized))
            .OrderBy(d => d.Municipio)
            .Take(10)
            .Select(d => new DivipolaDto
            {
                MunCode     = d.MunCode,
                DeptCode    = d.DeptCode,
                Municipio   = d.Municipio,
                Departamento = d.Departamento
            })
            .ToListAsync();

        return Ok(results);
    }
}
