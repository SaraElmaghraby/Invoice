using System;
using System.Collections.Generic;

#nullable disable

namespace Invoice.Models
{
    public partial class Item
    {
        public Item()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int UomId { get; set; }
        public int? Qty { get; set; }
        public decimal? Discount { get; set; }
        public int StoreId { get; set; }

        public virtual Store Store { get; set; }
        public virtual Uom Uom { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
