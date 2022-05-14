using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Models
{
    public enum FileStatus
    {
        Created,
        Completed
    }
    public class UserFile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedDate { get; set; } //direkt olarak almıyoruz, null durumunda geliyor başlangıçta ta ki worker service gerekli işlemi bitirdiğinde
                                                   //oluşacak olan "createdDate" yazılmış olmalıdır.
        public FileStatus FileStatus { get; set; }

        [NotMapped] //veri tabanına map'lenmesini istemiyoruz.
        //eğer entity içerisindeki ilgili property'nin veri tabanında ilgili tabloda tanımlanmasını istemiyorsak "NotMapped" kullanırız. DTO'da yapmak daha iyidir, burada yapmaktan

        //satece "get" bloğunu inşa ettiğimiz için direkt olarak lambda ile girebiliriz.
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() : "-";
    }
}
