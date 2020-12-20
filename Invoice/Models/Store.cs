using System;
using System.Collections.Generic;

#nullable disable

namespace Invoice.Models
{
    public partial class Store
    {
        public Store()
        {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}
