using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Invoice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Invoice.Controllers
{
    public class MasterInvoiceController : Controller
    {
        private readonly InvoiceCoreContext _DbContext;
        public MasterInvoiceController( InvoiceCoreContext DbContext)
        {
            
            _DbContext = DbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult LoadItems(int storeId)
        {
            var itemList = _DbContext.Items.Where(x => x.StoreId == storeId).ToList();

            return Json(new SelectList(itemList, "Id", "ItemName"));
        }
        public JsonResult LoadUoms(int itemId)
        {
            var uomList = _DbContext.Uoms.Where(x => x.Id == _DbContext.Items.Find(itemId).UomId).ToList();
            ViewBag.itemInfo = _DbContext.Items.Find(itemId);
            return Json(new SelectList(uomList, "Id", "Uom1"));
        }
    }
}