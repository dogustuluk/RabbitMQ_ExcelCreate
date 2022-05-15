using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class CreateExcelMessage
    {
        public string UserId { get; set; }
        public int FileId { get; set; }

        //public List<Product> Products { get; set; } //çok fazla data olduğu için data'ları mesajın içerisine gömmemeliyiz. Dataları worker service'in veritabanı ile iletişim kurmasıyla dosyanın oluşturulması lazım
    }
}
