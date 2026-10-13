using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;

namespace OrbitAOS.V6.Web.Controllers
{
    /// <summary>
    /// Controller for Book management CRUD operations.
    /// Migrated from legacy ASP.NET MVC to ASP.NET Core MVC on .NET 8.
    /// Key changes:
    /// - Async/await throughout (replaces synchronous actions)
    /// - Constructor DI for services (replaces legacy DependencyResolver)
    /// - IActionResult return types
    /// - No System.Web.Mvc references
    /// </summary>
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(
            IBookService bookService,
            ICategoryService categoryService,
            ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Books - Lists all books with optional search and category filter.
        /// </summary>
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId)
        {
            IEnumerable<BookDto> books;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = await _bookService.SearchBooksAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else if (categoryId.HasValue)
            {
                books = await _bookService.GetBooksByCategoryAsync(categoryId.Value);
                ViewBag.CategoryId = categoryId;
            }
            else
            {
                books = await _bookService.GetAllBooksAsync();
            }

            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);

            return View(books);
        }

        /// <summary>
        /// GET: /Books/Details/5 - Shows book details.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound();
            }
            return View(book);
        }

        /// <summary>
        /// GET: /Books/Create - Shows create book form.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdown();
            return View(new CreateBookDto());
        }

        /// <summary>
        /// POST: /Books/Create - Creates a new book.
        /// Replaces legacy synchronous POST action with async pattern.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateBookDto createBookDto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdown(createBookDto.CategoryId);
                return View(createBookDto);
            }

            try
            {
                var book = await _bookService.CreateBookAsync(createBookDto);
                _logger.LogInformation("Book '{Title}' created with ID {Id}", book.Title, book.Id);
                TempData["SuccessMessage"] = $"Book '{book.Title}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book '{Title}'", createBookDto.Title);
                ModelState.AddModelError(string.Empty, "An error occurred while creating the book. Please try again.");
                await PopulateCategoriesDropdown(createBookDto.CategoryId);
                return View(createBookDto);
            }
        }

        /// <summary>
        /// GET: /Books/Edit/5 - Shows edit book form.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            var updateDto = new UpdateBookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Description = book.Description,
                Price = book.Price,
                Quantity = book.Quantity,
                CategoryId = book.CategoryId,
                IsAvailable = book.IsAvailable,
                PublishedDate = book.PublishedDate
            };

            await PopulateCategoriesDropdown(book.CategoryId);
            return View(updateDto);
        }

        /// <summary>
        /// POST: /Books/Edit/5 - Updates an existing book.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdateBookDto updateBookDto)
        {
            if (id != updateBookDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdown(updateBookDto.CategoryId);
                return View(updateBookDto);
            }

            try
            {
                var book = await _bookService.UpdateBookAsync(updateBookDto);
                _logger.LogInformation("Book '{Title}' (ID: {Id}) updated", book.Title, book.Id);
                TempData["SuccessMessage"] = $"Book '{book.Title}' was updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book ID {Id}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the book. Please try again.");
                await PopulateCategoriesDropdown(updateBookDto.CategoryId);
                return View(updateBookDto);
            }
        }

        /// <summary>
        /// GET: /Books/Delete/5 - Shows delete confirmation.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        /// <summary>
        /// POST: /Books/Delete/5 - Deletes a book.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                    return NotFound();

                await _bookService.DeleteBookAsync(id);
                _logger.LogInformation("Book ID {Id} deleted", id);
                TempData["SuccessMessage"] = $"Book '{book.Title}' was deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the book.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task PopulateCategoriesDropdown(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }
    }
}
