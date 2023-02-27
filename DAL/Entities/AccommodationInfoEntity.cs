using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.DAL.Entities;

namespace DAL.Entities
{
    public class AccommodationInfoEntity: BaseEntity
    {
        /// <summary>
        /// Id of 
        /// </summary>
        public uint TenantId { get; set; }

        /// <summary>
        /// Id of apartments block
        /// </summary>
        public uint ABID { get; set; }


    }
}
