using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.DAL.Entities;

namespace DAL.Entities
{
    public class TenantsEntity: BaseEntity
    {
        /// <summary>
        /// First name of the Tenant
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Second name of the Tenant
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Second name of the Tenant
        /// </summary>
        public uint Age { get; set; }
    }
}
