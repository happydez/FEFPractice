using System;
using System.Collections.Generic;

namespace AutoLot.Models.Entities
{
    public partial class Inventory
    {
        public Inventory()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public int MakeId { get; set; }
        public string Color { get; set; } = null!;
        public string PetName { get; set; } = null!;
        public byte[]? TimeStamp { get; set; }

        public virtual Make Make { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; }
    }
}
