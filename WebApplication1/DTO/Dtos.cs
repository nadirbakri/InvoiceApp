using System.Collections.Generic;

namespace InvoiceWebApp.DTO
{
    public class IdNameDto
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class ProductDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal weight { get; set; }
        public decimal price { get; set; }
    }

    public class InvoiceListDto
    {
        public string InvoiceNo { get; set; }
        public string Date { get; set; }
        public string Sales { get; set; }
        public string Courier { get; set; }
        public string Payment { get; set; }
        public decimal CourierFee { get; set; }
    }

    public class InvoiceHeaderDto
    {
        public string InvoiceNo { get; set; }
        public string Date { get; set; }
        public string InvoiceTo { get; set; }
        public string ShipTo { get; set; }
        public int SalesID { get; set; }
        public int CourierID { get; set; }
        public int PaymentType { get; set; }
        public decimal CourierFee { get; set; }
    }

    public class InvoiceDetailDto
    {
        public int ProductID { get; set; }
        public decimal Weight { get; set; }
        public short Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class InvoiceGetDto
    {
        public bool ok { get; set; }
        public InvoiceHeaderDto header { get; set; }
        public IEnumerable<InvoiceDetailDto> details { get; set; }
        public string msg { get; set; }
    }

    public class InvoiceSaveDto
    {
        public string InvoiceNo { get; set; }
        public string Date { get; set; }
        public string InvoiceTo { get; set; }
        public string ShipTo { get; set; }
        public int SalesID { get; set; }
        public int CourierID { get; set; }
        public int PaymentType { get; set; }
        public int[] ProductID { get; set; }
        public short[] Qty { get; set; }
        public decimal[] Price { get; set; }
    }

    public class CourierFeePreviewRequest
    {
        public int courierId { get; set; }
        public int[] productIds { get; set; }
        public short[] qtys { get; set; }
    }

    public class OkDto
    {
        public bool ok { get; set; }
        public string msg { get; set; }
    }
}
