using System;
using System.Collections.Generic;

#nullable disable

namespace Invoice.Models
{
    public partial class OrderDetail
    {
        public int Id { get; set; }
        public int OrderHeaderId { get; set; }
        public int ItemId { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal Discount { get; set; }
        public int Qty { get; set; }
        public decimal TotalPrice { get; set; }
        public int UomId { get; set; }
        //discount

        public virtual Item Item { get; set; }
        public virtual OrderHeader OrderHeader { get; set; }
        public virtual Uom Uom { get; set; }
    }
}
