using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.DAL.IEntities
{
    public interface IEntityUnique
    {
        int Id { get; set; }

        bool IsActive { get; set; }
    }
}
