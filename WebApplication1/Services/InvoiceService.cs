using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using InvoiceWebApp.Models;

namespace InvoiceWebApp.Services
{
    public class InvoiceService
    {
        private readonly AppDbContext _db;
        public InvoiceService(AppDbContext db) { _db = db; }

        public IEnumerable<Invoice> GetInvoices(string q)
        {
            var query = _db.Invoices.Include("Sales").Include("Courier").Include("Payment").AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(i => i.InvoiceNo.Contains(q));
            return query.OrderByDescending(i => i.InvoiceDate).ToList();
        }

        public Invoice GetInvoice(string invoiceNo, bool includeDetails = false)
        {
            var qry = _db.Invoices.Include("Sales").Include("Courier").Include("Payment").AsQueryable();
            if (includeDetails) qry = qry.Include("Details.Product");
            return qry.FirstOrDefault(i => i.InvoiceNo == invoiceNo);
        }

        public void CreateInvoice(Invoice header, int[] productIds, short[] qtys, decimal[] prices)
        {
            if (_db.Invoices.Any(x => x.InvoiceNo == header.InvoiceNo)) throw new InvalidOperationException("Invoice number already exists.");
            header.CourierFee = 0;
            _db.Invoices.Add(header);
            AddDetails(header.InvoiceNo, productIds, qtys, prices);
            _db.SaveChanges();
            RecalcAndSaveCourierFee(header.InvoiceNo);
        }

        public void UpdateInvoice(Invoice header, int[] productIds, short[] qtys, decimal[] prices)
        {
            var inv = _db.Invoices.Include("Details").FirstOrDefault(i => i.InvoiceNo == header.InvoiceNo);
            if (inv == null) throw new InvalidOperationException("Invoice not found.");
            inv.InvoiceDate = header.InvoiceDate;
            inv.InvoiceTo = header.InvoiceTo;
            inv.ShipTo = header.ShipTo;
            inv.SalesID = header.SalesID;
            inv.CourierID = header.CourierID;
            inv.PaymentType = header.PaymentType;
            var old = _db.InvoiceDetails.Where(d => d.InvoiceNo == inv.InvoiceNo).ToList();
            _db.InvoiceDetails.RemoveRange(old);
            AddDetails(inv.InvoiceNo, productIds, qtys, prices);
            _db.SaveChanges();
            RecalcAndSaveCourierFee(inv.InvoiceNo);
        }

        public void DeleteInvoice(string invoiceNo)
        {
            var inv = _db.Invoices.FirstOrDefault(i => i.InvoiceNo == invoiceNo);
            if (inv == null) return;
            _db.Invoices.Remove(inv);
            _db.SaveChanges();
        }

        public decimal PreviewCourierFee(int courierId, int[] productIds, short[] qtys)
        {
            if (productIds == null || qtys == null || productIds.Length == 0)
                return CalcCourierFeeFromDb(courierId, 0);

            var n = Math.Min(productIds.Length, qtys.Length);
            var ids = productIds.Take(n).Distinct().ToList();

            var weightMap = _db.Products
                .Where(p => ids.Contains(p.ProductID))
                .Select(p => new { p.ProductID, p.Weight })
                .ToList()
                .ToDictionary(x => x.ProductID, x => x.Weight);

            decimal totalWeight = 0;
            for (int i = 0; i < n; i++)
            {
                decimal w = 0;
                if (weightMap.TryGetValue(productIds[i], out var found))
                    w = found;
                totalWeight += w * qtys[i];
            }

            return CalcCourierFeeFromDb(courierId, totalWeight);
        }

        private void AddDetails(string invoiceNo, int[] productIds, short[] qtys, decimal[] prices)
        {
            var n = productIds?.Length ?? 0;
            for (int i = 0; i < n; i++)
            {
                if (productIds[i] <= 0 || qtys[i] <= 0) continue;
                var prod = _db.Products.Find(productIds[i]);
                _db.InvoiceDetails.Add(new InvoiceDetail
                {
                    InvoiceNo = invoiceNo,
                    ProductID = productIds[i],
                    Weight = prod?.Weight ?? 0,
                    Qty = qtys[i],
                    Price = (prices != null && i < prices.Length && prices[i] > 0) ? prices[i] : (prod?.Price ?? 0)
                });
            }
        }

        private void RecalcAndSaveCourierFee(string invoiceNo)
        {
            var totalWeight = _db.InvoiceDetails.Where(d => d.InvoiceNo == invoiceNo).Select(d => (decimal?)d.Weight * d.Qty).DefaultIfEmpty(0).Sum() ?? 0;
            var inv = _db.Invoices.FirstOrDefault(i => i.InvoiceNo == invoiceNo);
            if (inv == null) return;
            inv.CourierFee = CalcCourierFeeFromDb(inv.CourierID, totalWeight);
            _db.SaveChanges();
        }

        private decimal CalcCourierFeeFromDb(int courierId, decimal totalWeight)
        {
            var kg = (int)Math.Ceiling((double)totalWeight);
            var rate = _db.CourierFees.Where(f => f.CourierID == courierId && kg >= f.StartKg && (f.EndKg == null || kg <= f.EndKg)).OrderBy(f => f.StartKg).Select(f => (decimal?)f.Price).FirstOrDefault();
            if (rate == null)
            {
                rate = _db.CourierFees.Where(f => f.CourierID == courierId).OrderByDescending(f => (int?)f.EndKg ?? int.MaxValue).Select(f => (decimal?)f.Price).FirstOrDefault();
            }
            return (rate ?? 0) * kg;
        }
    }
}
