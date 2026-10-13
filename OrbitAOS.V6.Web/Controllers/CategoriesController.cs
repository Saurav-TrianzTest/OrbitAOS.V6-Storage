using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;

namespace OrbitAOS.V6.Web.Controllers
{
    /// <summary>
    /// Controller for Category management CRUD operations.
    /// </summary>
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Categories - Lists all categories.
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        /// <summary>
        /// GET: /Categories/Details/5
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        /// <summary>
        /// GET: /Categories/Create
        /// </summary>
        public IActionResult Create()
        {
            return View(new CategoryDto { IsActive = true });
        }

        /// <summary>
        /// POST: /Categories/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
                return View(categoryDto);

            try
            {
                var created = await _categoryService.CreateCategoryAsync(categoryDto);
                TempData["SuccessMessage"] = $"Category '{created.Name}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category '{Name}'", categoryDto.Name);
                ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
                return View(categoryDto);
            }
        }

        /// <summary>
        /// GET: /Categories/Edit/5
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        /// <summary>
        /// POST: /Categories/Edit/5
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(categoryDto);

            try
            {
                var updated = await _categoryService.UpdateCategoryAsync(categoryDto);
                TempData["SuccessMessage"] = $"Category '{updated.Name}' was updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID {Id}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
                return View(categoryDto);
            }
        }

        /// <summary>
        /// GET: /Categories/Delete/5
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        /// <summary>
        /// POST: /Categories/Delete/5
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["SuccessMessage"] = "Category was deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
