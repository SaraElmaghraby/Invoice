using System;
using System.Collections.Generic;

#nullable disable

namespace Invoice.Models
{
    public partial class Uom
    {
        public Uom()
        {
            Items = new HashSet<Item>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Uom1 { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
