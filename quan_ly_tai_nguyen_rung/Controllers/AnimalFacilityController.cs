using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using quan_ly_tai_nguyen_rung.DATA;
using quan_ly_tai_nguyen_rung.Interfaces;
using quan_ly_tai_nguyen_rung.Models.section2;
using quan_ly_tai_nguyen_rung.Models.section4;
using quan_ly_tai_nguyen_rung.Repository;
using quan_ly_tai_nguyen_rung.ViewModels;

namespace quan_ly_tai_nguyen_rung.Controllers
{
    public class AnimalFacilityController : Controller
    {
        private readonly IAnimalFacilityRepository _animalFacilityRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly ApplicationDbContext _context;

        public AnimalFacilityController(IAnimalFacilityRepository animalFacilityRepository, IAnimalRepository animalRepository, ApplicationDbContext context)
        {
            _animalFacilityRepository = animalFacilityRepository;
            _animalRepository = animalRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var facilities = await _animalFacilityRepository.GetAll();
            return View(facilities);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var facility = await _animalFacilityRepository.GetIdByAsync(id);
            if (facility == null)
            {
                ViewData["ErrorMessage"] = "ID không hợp lệ. Vui lòng nhập lại.";
                return View("Detail");
            }
            return View(facility);
        }
        private async Task PopulateCommunes()
        {
            var communes = await _context.Communes.ToListAsync();
            ViewBag.Communes = new SelectList(communes, "Id", "Name");
        }
        public async Task<IActionResult> Create()
        {
            await PopulateCommunes();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AnimalFacilityViewModel facilityVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCommunes();
                return View(facilityVM);
            }

            var facility = new AnimalFacility
            {
                Name = facilityVM.Name,
                Address = facilityVM.Address,
                ContactFace = facilityVM.ContactFace,
                ContactMail = facilityVM.ContactMail,
                ContactPhone = facilityVM.ContactPhone,
                Labor = facilityVM.Labor,
                Acreage = facilityVM.Acreage,
                ImageAnimalStorage = facilityVM.ImageAnimalStorage,
                CommuneId = facilityVM.CommuneId
            };

            _animalFacilityRepository.Add(facility);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var facility = await _animalFacilityRepository.GetIdByAsync(id);
            if (facility == null) return View("Error");
            await PopulateCommunes();

            var facilityVM = new AnimalFacilityViewModel
            {
                Name = facility.Name,
                Address = facility.Address,
                ContactFace = facility.ContactFace,
                ContactMail = facility.ContactMail,
                ContactPhone = facility.ContactPhone,
                Labor = facility.Labor,
                Acreage = facility.Acreage,
                ImageAnimalStorage = facility.ImageAnimalStorage,
                CommuneId = facility.CommuneId
            };

            return View(facilityVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, AnimalFacilityViewModel facilityVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCommunes();
                return View(facilityVM);
            }

            var facility = await _animalFacilityRepository.GetByIdAsyncNoTracking(id);
            if (facility == null) return View("Error");

            facility.Name = facilityVM.Name;
            facility.Address = facilityVM.Address;
            facility.ContactFace = facilityVM.ContactFace;
            facility.ContactMail = facilityVM.ContactMail;
            facility.ContactPhone = facilityVM.ContactPhone;
            facility.Labor = facilityVM.Labor;
            facility.Acreage = facilityVM.Acreage;
            facility.ImageAnimalStorage = facilityVM.ImageAnimalStorage;
            facility.CommuneId = facilityVM.CommuneId;

            _animalFacilityRepository.Update(facility);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SearchByName(string name)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ.");
                IEnumerable<AnimalFacility> facilities = await _animalFacilityRepository.GetAll();
                return View("Index", facilities); // Trả về danh sách tất cả nếu dữ liệu không hợp lệ
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                var facilities = await _animalFacilityRepository.GetAll();
                return View("Index", facilities);
            }

            var searchResults = await _animalFacilityRepository.GetStorageByName(name);
            return View("Index", searchResults);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var facility = await _animalFacilityRepository.GetIdByAsync(id);
            if (facility == null) return View("Error");
            return View(facility);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _animalFacilityRepository.GetIdByAsync(id);
            if (facility == null) return View("Error");

            var relatedRecords = _context.animalAnimalFacilities
                              .Where(aaf => aaf.AnimalFacilityId == id)
                              .ToList();
            if (relatedRecords.Any())
            {
                _context.animalAnimalFacilities.RemoveRange(relatedRecords);
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }
            _animalFacilityRepository.Delete(facility);
            return RedirectToAction("Index");
        }
    }
}
