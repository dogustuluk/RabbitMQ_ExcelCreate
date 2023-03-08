using FileCreateWorkerService.Services;
using FileCreateWorkerService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Shared;
using System.Text;
using System.IO;
using ClosedXML.Excel;
using System.Net.Http;
using RabbitMQ.Client;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        /*AdventureWorks2019Context hakkýnda
         * program.cs tarafýnda context'imiz scope olarak eklenmiþ.
         * BackgroundService'i miras alan Worker sýnýfýnda scope olarak eklenmiþ nesneleri DI Container'a alamayýz.
         * Eðer almak istersek service provider üzerinden bu iþlemi yaparýz.
         */
        private readonly IServiceProvider _serviceProvider;

        private IModel _channel;
        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }
       
        //rabbitmq'ya baðlan
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);


            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer((RabbitMQ.Client.IModel)_channel);
            
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            //gecikme yap, test için
            await Task.Delay(5000);

            //kuyruktan mesajý al
            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            //excell dosyasýný oluþturmadan önce bunu bir memory stream'e at
            using var ms = new MemoryStream();

            //önce workbook oluþtur
            var wb = new XLWorkbook();
            //dataSet oluþtur
            var ds = new DataSet();
            //GetTable metodundan gelen verileri DataSet'e ekle
            ds.Tables.Add(GetTable("products"));
            //worksheet oluþtur
            wb.Worksheets.Add(ds);
            //memory stream'e kaydetme iþlemini yap
            wb.SaveAs(ms);//excell dosyasý þuanda bellekte

            //þuan FilesController'daki Upload endpointini çaðýrabiliriz.
            //ilk olarak metottaki ilk parametre olan file nesnesini oluþtur
            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()), "file", Guid.NewGuid().ToString()+".xlsx");

            //istek yapabiliriz.
            var baseUrl = "https://localhost:44346/api/files";
            //istek gerçekleþtirmek için kod yazabiliriz.
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", multipartFormDataContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File (Id:{createExcelMessage.FileId}) was created by successfull");
                    //response'dan 200'le  baþlayan bir kod geliyorsa kuyruktan sil
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }


        }

        //tablo oluþturma iþlemi için
        private DataTable GetTable(string tableName)
        {
            List<Product> products;
            //db'ye baðlan
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();

                products = context.Products.ToList();
            }//products nesnemiz dolu bir þekilde var.

            //geriye datatable dön
            DataTable table = new DataTable { TableName= tableName };
            //tabloya sütun isimlerini ekle
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId, x.Name, x.ProductNumber, x.Color);
            });//þuanda memory'de dataTable var.

            return table;
        }
    }
}
