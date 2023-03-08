using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name); //kullanıcıyı bulmamızı sağlayan kod.

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}"; //dosya ismini oluşturmamızı sağlayan kod.

            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };
            //userFile'ı veri tabanında oluşturalım
            await _context.UserFiles.AddAsync(userFile);

            await _context.SaveChangesAsync();

            //rabbitMQ mesaj gönderme başlangıcı>>>>
            _rabbitMQPublisher.Publish(new Shared.CreateExcelMessage() { FileId = userFile.Id});
                        //userFile Id'sini userFile'da tanımlamadık ama EF Core veritabanına kaydettiği için memory'deki ilgili alanın Id propety'sini kendisi otomatik olarak dolduruyor.
            //<<<<rabbitMQ mesaj gönderme sonu
            TempData["StartCreatingExcel"] = true; //bir request'ten diğer bir request'e data taşımak için ViewBag kullanılmaz, TempData kullanılır.
                //ViewBag aynı request'e, model tarafına datayı taşımamıza izin verir.
                //TempData >>> requestler arasında data paylaşımını ilgili datayı cookie'de tutarak yapmaktadır.
                //Diğer request'te data okununca cookie'yi siliyor.

            return RedirectToAction(nameof(Files));

        }

        public async Task<IActionResult> Files() //kullanıcıya ait dosyaları gösterelim
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var file = await _context.UserFiles.Where(x => x.UserId == user.Id).ToListAsync();

            return View(file);
        }
    }
}
