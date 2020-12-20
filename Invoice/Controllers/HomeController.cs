using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Invoice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Invoice.Helpers;
using Rotativa;

namespace Invoice.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InvoiceCoreContext _DbContext;
        public HomeController(ILogger<HomeController> logger, InvoiceCoreContext DbContext)
        {
            _logger = logger;
            _DbContext = DbContext;
        }

        public IActionResult Index(DateTime? orderDate)
        {
            ViewBag.cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");
            List<Store> stores = new List<Store>();
            stores = (from c in _DbContext.Stores select c).ToList();
            stores.Insert(0, new Store { Id = 0, Name = "--Select store--" });
            ViewBag.storeList = stores;
            //ViewBag.orderDate = orderDate;
            // ViewBag.cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");


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


        [HttpPost]
        public ActionResult<List<Item>> AddToCart(int id, int qty, decimal discount,DateTime orderDate)
        {
            if (qty > 0)
            {
                var items = _DbContext.Items.FindAsync(id);

                if (items == null)
                {
                    return NotFound();
                }

                decimal price = _DbContext.Items.Where(x => x.Id == id).Select(x => x.Price).FirstOrDefault();
                int uomID = _DbContext.Items.Where(x => x.Id == id).Select(x => x.UomId).FirstOrDefault();
                Uom uom = _DbContext.Items.Where(x => x.Id == id).Select(x => x.Uom).FirstOrDefault();

                if (SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart") == null)
                {

                    List<OrderDetail> cart = new List<OrderDetail>();
                    cart.Add(new OrderDetail
                    {
                        ItemId = id,
                        Item = _DbContext.Items.Find(id),
                        ItemPrice = price,
                        TotalPrice = (price - discount) * qty,
                        UomId = uomID,
                        Uom = uom,
                        Qty = qty,
                        Discount = discount


                    });

                    SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);

                    //CalculateCartPrice(cart);
                    //return Ok(cart);
                    //????return Index(cart)||return json(cart)
                    ViewBag.cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");
                    ViewBag.orderDate = orderDate;
                    //return View();
                    return Redirect("Index");
                }
                else
                {


                    List<OrderDetail> cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");

                    int index = isExist(id);

                    if (index == -1)
                    {

                        cart.Add(new OrderDetail
                        {
                            ItemId = id,
                            Item = _DbContext.Items.Find(id),
                            ItemPrice = price,
                            TotalPrice = (price - discount) * qty,
                            UomId = uomID,
                            Uom = uom,
                            Qty = qty,
                            Discount = discount
                        });

                        SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);

                        //CalculateCartPrice(cart);
                        // return Ok(cart);
                        //return PartialView("getItem", cart);
                        ViewBag.cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");
                        ViewBag.orderDate = orderDate;
                        //return View();
                        return Redirect("Index");

                    }
                    else
                    {//remoooove +qty++
                     //return Ok(cart);
                     // return PartialView("getItem", cart);
                        ViewBag.cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");
                        ViewBag.orderDate = orderDate;
                        //return View();
                        return RedirectToAction("Index", orderDate);

                    }


                }

            }
            else
            {
                //return View();
                return RedirectToAction("Index");

            }
        }

        [HttpPost, ActionName("CheckOut")]
        public async Task<ActionResult> CheckOut(DateTime requestdate)
        {

            if (SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart") != null)
            {
                List<OrderDetail> cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");
                var tax = Convert.ToDecimal(0.14);
                var orderinfo = new OrderHeader
                {

                    DiscountValue = 0,
                    OrderDate = requestdate,
                    TaxValue = tax,
                    Status = "open",
                    TotalPrice = CalculateOrderPrice(tax)

                };
                _DbContext.OrderHeaders.Add(orderinfo);
                await _DbContext.SaveChangesAsync();
                //var tax = _DbContext.OrderHeaders.Where(x => x.Id == orderinfo.Id).Select(x => x.TaxValue).FirstOrDefault();


                foreach (var item in cart)
                {
                    var discount = item.Discount;
                    var uomID = item.UomId;

                    var orderDetail = new OrderDetail
                    {
                        OrderHeaderId = orderinfo.Id,
                        ItemId = item.ItemId,
                        ItemPrice = item.ItemPrice,
                        Qty = item.Qty,

                        Discount = discount,
                        UomId = uomID,
                        TotalPrice = (item.TotalPrice)
                    };

                    _DbContext.OrderDetails.Add(orderDetail);
                    await _DbContext.SaveChangesAsync();
                    //update item qty in store
                    var entity = _DbContext.Items.Find(item.ItemId);
                    entity.Qty = (entity.Qty) - (item.Qty);
                    _DbContext.Entry(entity).Property(x => x.Qty).IsModified = true;
                    _DbContext.SaveChanges();


                }

                //return new PartialViewAsPdf("PrintCart", orderinfo)
                //{
                //    FileName = $"Invoice{ DateTime.Now.ToString("yyyyMMdd") }.pdf"
                //};


            }

            return Redirect("Index");
        }
        private decimal CalculateOrderPrice(decimal tax)
        {
            decimal orderPrice = 0;
            if (SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart") != null)
            {
               
                List<OrderDetail> cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");
                foreach (var item in cart)
                {
                    orderPrice = (decimal)((item.TotalPrice) + (item.TotalPrice * tax)) + orderPrice;
                }
            }
            return orderPrice;
        }
        private int isExist(int id)
        {
            List<OrderDetail> cart = SessionHelper.GetObjectFromJson<List<OrderDetail>>(HttpContext.Session, "cart");

            int index = -1;
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].ItemId == id)
                {
                    index = i;
                }

            }

            return index;
        }
        public IActionResult getItem()
        {
            var itemsCart = _DbContext.Items.Find();

            return PartialView("getItem", itemsCart);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
