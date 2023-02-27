using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.DAL.Entities;

namespace DAL.Entities
{
    public class ApartmentBlocksEntity : BaseEntity
    {
        /// <summary>
        /// Apartment blocks location city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Apartment blocks location street
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Number of the apartment blocks
        /// </summary>
        public uint Number { get; set; }
    }
}
