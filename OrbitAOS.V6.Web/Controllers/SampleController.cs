using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;

namespace OrbitAOS.V6.Web.Controllers;

/// <summary>
/// Sample controller demonstrating Clean Architecture pattern
/// </summary>
public class SampleController : Controller
{
    private readonly ISampleService _sampleService;
    private readonly ILogger<SampleController> _logger;

    public SampleController(ISampleService sampleService, ILogger<SampleController> logger)
    {
        _sampleService = sampleService;
        _logger = logger;
    }

    // GET: Sample
    public async Task<IActionResult> Index()
    {
        try
        {
            var samples = await _sampleService.GetAllSamplesAsync();
            return View(samples);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving samples");
            return View("Error");
        }
    }

    // GET: Sample/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var sample = await _sampleService.GetSampleByIdAsync(id);
            if (sample == null)
            {
                return NotFound();
            }
            return View(sample);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sample {Id}", id);
            return View("Error");
        }
    }

    // GET: Sample/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Sample/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SampleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _sampleService.CreateSampleAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sample");
            ModelState.AddModelError("", "An error occurred while creating the sample");
            return View(dto);
        }
    }

    // GET: Sample/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var sample = await _sampleService.GetSampleByIdAsync(id);
            if (sample == null)
            {
                return NotFound();
            }
            return View(sample);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sample {Id} for edit", id);
            return View("Error");
        }
    }

    // POST: Sample/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SampleDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _sampleService.UpdateSampleAsync(id, dto);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sample {Id}", id);
            ModelState.AddModelError("", "An error occurred while updating the sample");
            return View(dto);
        }
    }

    // GET: Sample/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var sample = await _sampleService.GetSampleByIdAsync(id);
            if (sample == null)
            {
                return NotFound();
            }
            return View(sample);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sample {Id} for delete", id);
            return View("Error");
        }
    }

    // POST: Sample/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _sampleService.DeleteSampleAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sample {Id}", id);
            ModelState.AddModelError("", "An error occurred while deleting the sample");
            return View();
        }
    }
}
