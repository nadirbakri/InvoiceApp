using System;
using System.Linq;
using System.Web.Mvc;
using InvoiceWebApp.DTO;
using InvoiceWebApp.Models;
using InvoiceWebApp.Services;

namespace InvoiceWebApp.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly InvoiceService _svc;
        public InvoicesController() { _svc = new InvoiceService(_db); }

        public ActionResult Index()
        {
            ViewBag.Sales = _db.Sales.Select(s => new IdNameDto { id = s.SalesID, name = s.SalesName }).ToList();
            ViewBag.Couriers = _db.Couriers.Select(c => new IdNameDto { id = c.CourierID, name = c.CourierName }).ToList();
            ViewBag.Payments = _db.Payments.Select(p => new IdNameDto { id = p.PaymentID, name = p.PaymentName }).ToList();
            ViewBag.Products = _db.Products.Select(p => new ProductDto { id = p.ProductID, name = p.ProductName, weight = p.Weight, price = p.Price }).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult List(string q)
        {
            var rows = _svc.GetInvoices(q).Select(i => new InvoiceListDto
            {
                InvoiceNo = i.InvoiceNo,
                Date = i.InvoiceDate.ToString("yyyy-MM-dd"),
                Sales = i.Sales.SalesName,
                Courier = i.Courier.CourierName,
                Payment = i.Payment.PaymentName,
                CourierFee = i.CourierFee
            }).ToList();
            return Json(new { rows }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Get(string id)
        {
            var i = _svc.GetInvoice(id, includeDetails: true);
            if (i == null) return Json(new InvoiceGetDto { ok = false, msg = "Not found" }, JsonRequestBehavior.AllowGet);
            return Json(new InvoiceGetDto
            {
                ok = true,
                header = new InvoiceHeaderDto
                {
                    InvoiceNo = i.InvoiceNo,
                    Date = i.InvoiceDate.ToString("yyyy-MM-dd"),
                    InvoiceTo = i.InvoiceTo,
                    ShipTo = i.ShipTo,
                    SalesID = i.SalesID,
                    CourierID = i.CourierID,
                    PaymentType = i.PaymentType,
                    CourierFee = i.CourierFee
                },
                details = i.Details.Select(d => new InvoiceDetailDto
                {
                    ProductID = d.ProductID,
                    Weight = d.Weight,
                    Qty = d.Qty,
                    Price = d.Price
                }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(InvoiceSaveDto dto)
        {
            if (dto == null) return Json(new OkDto { ok = false, msg = "Invalid payload" });
            DateTime date; if (!DateTime.TryParse(dto.Date, out date)) date = DateTime.Today;
            var header = new Invoice
            {
                InvoiceNo = dto.InvoiceNo,
                InvoiceDate = date,
                InvoiceTo = dto.InvoiceTo ?? "",
                ShipTo = dto.ShipTo ?? "",
                SalesID = dto.SalesID,
                CourierID = dto.CourierID,
                PaymentType = dto.PaymentType
            };
            try
            {
                var exists = _svc.GetInvoice(dto.InvoiceNo) != null;
                if (exists) _svc.UpdateInvoice(header, dto.ProductID, dto.Qty, dto.Price);
                else _svc.CreateInvoice(header, dto.ProductID, dto.Qty, dto.Price);
                return Json(new OkDto { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new OkDto { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Remove(string id)
        {
            _svc.DeleteInvoice(id);
            return Json(new OkDto { ok = true });
        }

        [HttpPost]
        public JsonResult PreviewCourierFee(CourierFeePreviewRequest req)
        {
            if (req == null) return Json(new { fee = 0M });
            var fee = _svc.PreviewCourierFee(req.courierId, req.productIds, req.qtys);
            return Json(new { fee });
        }
    }
}
