using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using quan_ly_tai_nguyen_rung.DATA;
using quan_ly_tai_nguyen_rung.Interfaces;
using quan_ly_tai_nguyen_rung.Models.section1;
using quan_ly_tai_nguyen_rung.Models.section4;
using quan_ly_tai_nguyen_rung.Repository;
using quan_ly_tai_nguyen_rung.ViewModels;

namespace quan_ly_tai_nguyen_rung.Controllers
{
    public class AnimalController : Controller
    {
        private readonly IAnimalRepository _animalRepository;
        private readonly ApplicationDbContext _context;

        public AnimalController(IAnimalRepository animalRepository, ApplicationDbContext context)
        {
            _animalRepository = animalRepository;
            _context = context;
        }

        // GET: Animal
        public async Task<IActionResult> Index(int facilityId)
        {
            var animals = await _animalRepository.GetAllOfFacility(facilityId);
            return View(animals);
        }

        // GET: Animal/Details/5
        public async Task<IActionResult> Details(int id,int facilityId)
        {
            var animal = await _animalRepository.GetIdByAsyncNoTrackingOfFacility(id, facilityId);
            if (animal == null)
            {
                TempData["ErrorMessage"] = "Động vật không tồn tại.";
                return RedirectToAction(nameof(Index), new { facilityId = facilityId });
            }
            return View(animal);
        }

        private void PopulateIsActiveOptions()
        {
            var isActiveOptions = Enum.GetValues(typeof(DATA.@enum.Is_active))
                .Cast<DATA.@enum.Is_active>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString() // Bạn có thể chuyển đổi để có định dạng dễ đọc hơn nếu cần
                }).ToList();

            ViewBag.IsActiveOptions = isActiveOptions;
        }

        private void PopulateStatusOptions()
        {
            var statusOptions = Enum.GetValues(typeof(DATA.@enum.status))
                .Cast<DATA.@enum.status>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString() // Bạn có thể chuyển đổi để có định dạng dễ đọc hơn nếu cần
                }).ToList();

            ViewBag.StatusOptions = statusOptions;
        }

        private void PopulateGenericOptions()
        {
            var genericOptions = Enum.GetValues(typeof(DATA.@enum.generic))
                .Cast<DATA.@enum.generic>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString() // Bạn có thể chuyển đổi để có định dạng dễ đọc hơn nếu cần
                }).ToList();

            ViewBag.GenericOptions = genericOptions;
        }

        // GET: Animal/Create
        public IActionResult Create()
        {
            PopulateGenericOptions();
            PopulateIsActiveOptions();
            PopulateStatusOptions();
            return View();
        }

        // POST: Animal/Create
        [HttpPost]
        public async Task<IActionResult> Create(AnimalViewModel animalViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(animalViewModel);
            }

            var newAnimal = new Animal
            {
                Name = animalViewModel.Name,
                Generic = animalViewModel.Generic,
                DateFound = animalViewModel.DateFound,
                PreviousQuantity = animalViewModel.PreviousQuantity,
                Status = animalViewModel.Status,
                Fluctuation = animalViewModel.Fluctuation,
                Date = animalViewModel.Date,
                Reason = animalViewModel.Reason,
                Location = animalViewModel.Location,
                is_Active = animalViewModel.IsActive,
                CurrentQuantity = animalViewModel.CurrentQuantity,
            };

            _animalRepository.Add(newAnimal);
            await _context.SaveChangesAsync(); // Lưu động vật mới vào cơ sở dữ liệu

            return RedirectToAction(nameof(Index));
        }

        // GET: Animal/Edit/5
        public async Task<IActionResult> Edit(int id,int facilityId)
        {
            var animal = await _animalRepository.GetIdByAsyncOfFacility(id, facilityId);
            if (animal == null)
            {
                return NotFound();
            }
            var animalVM = new AnimalViewModel
            {
                Name = animal.Name,
                Generic = animal.Generic,
                DateFound = animal.DateFound,
                PreviousQuantity = animal.PreviousQuantity,
                Status = animal.Status,
                Fluctuation = animal.Fluctuation,
                Date = animal.Date,
                Reason = animal.Reason,
                Location = animal.Location,
                IsActive = animal.is_Active,
                CurrentQuantity = animal.CurrentQuantity,
            };
            PopulateGenericOptions();
            PopulateIsActiveOptions();
            PopulateStatusOptions(); 
            return View(animalVM);
        }

        // POST: Animal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnimalViewModel animalVM, int facilityId)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit!");
                return View(animalVM);
            }

            var existingAnimal = await _animalRepository.GetIdByAsyncOfFacility(id, facilityId);
            if (existingAnimal == null)
            {
                return View("Error"); // Nếu không tìm thấy động vật, trả về trang lỗi
            }

            // Cập nhật thông tin động vật từ ViewModel
            existingAnimal.Name = animalVM.Name;
            existingAnimal.Generic = animalVM.Generic;
            existingAnimal.DateFound = animalVM.DateFound;
            existingAnimal.PreviousQuantity = animalVM.PreviousQuantity;
            existingAnimal.Status = animalVM.Status;
            existingAnimal.Fluctuation = animalVM.Fluctuation;
            existingAnimal.Date = animalVM.Date;
            existingAnimal.Reason = animalVM.Reason;
            existingAnimal.Location = animalVM.Location;
            existingAnimal.is_Active = animalVM.IsActive;
            existingAnimal.CurrentQuantity = animalVM.CurrentQuantity;

            // Cập nhật động vật trong repository
            _animalRepository.Update(existingAnimal);

            // Chuyển hướng về danh sách động vật
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchByName(string name, int facilityId)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                // Nếu không có tên được nhập, trả về danh sách đầy đủ
                IEnumerable<Animal> animals = await _animalRepository.GetAllOfFacility(facilityId);
                return View("Index", animals);
            }
            IEnumerable<Animal> searchResults = await _animalRepository.GetAnimalByNameOfFacility(name, facilityId);
            return View("Index", searchResults);
        }

        // GET: Animal/Delete/5
        public async Task<IActionResult> Delete(int id, int facilityId)
        {
            var animal = await _animalRepository.GetIdByAsyncOfFacility(id,facilityId);
            if (animal == null)
            {
                return View("Error");
            }
            return View(animal);
        }

        // POST: Animal/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteAnimal(int id, int facilityId)
        {
            var animal = await _animalRepository.GetIdByAsyncOfFacility(id, facilityId);
            if (animal != null)
            {
                _animalRepository.Delete(animal);
                var aaf = await _context.animalAnimalFacilities
                    .FirstOrDefaultAsync(aaf => aaf.AnimalId == id && aaf.AnimalFacilityId == facilityId); 
                if (aaf != null)
                {
                    _context.animalAnimalFacilities.Remove(aaf);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index),new { facilityId = facilityId }); 
        }
    }
}
