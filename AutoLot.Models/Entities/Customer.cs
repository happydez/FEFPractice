using System;
using System.Collections.Generic;

namespace AutoLot.Models.Entities
{
    public partial class Customer
    {
        public Customer()
        {
            CreditRisks = new HashSet<CreditRisk>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public byte[]? TimeStamp { get; set; }

        public virtual ICollection<CreditRisk> CreditRisks { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
