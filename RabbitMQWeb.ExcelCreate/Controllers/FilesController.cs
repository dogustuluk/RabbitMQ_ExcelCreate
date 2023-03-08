using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FilesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest();
            
            //dosyayı bul
            var userFile = await _context.UserFiles.FirstAsync(x => x.Id== fileId);

            //path'i ayarlar -> dosyaadı.uzantısı
            var filePath = userFile.FileName + Path.GetExtension(file.FileName);

            //wwwroot'taki klasöre kaydedilecek path'i al
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            using FileStream stream = new(path, FileMode.Create);

            //dosyanın içeriğini ver. file'ın içeriğini stream'e kopyala.
            await file.CopyToAsync(stream);

            //dosya kaydoldu şuan yani userFile'ın oluşturulma tarihini verebiliriz şuan.
            userFile.CreatedDate = DateTime.Now;
            //path'i ver
            userFile.FilePath = filePath;
            //status'ü güncelle
            userFile.FileStatus = FileStatus.Completed;
            //db'ye yansıt
            await _context.SaveChangesAsync();

            //signalR ile gerçek zamanlı notification oluştur.

            return Ok();

        }
    }
}
