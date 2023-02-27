using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class TenToABView
    {
        public int Id { get; set; }

        public int ABID { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public uint Number { get; set; }

        public uint TenantId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public uint Age { get; set; }
    }
}
