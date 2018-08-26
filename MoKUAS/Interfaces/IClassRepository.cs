using MoKUAS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoKUAS.Interfaces
{
    public interface IClassRepository
    {
        Task<int> Insert(Class @class);
        Task<IEnumerable<Class>> Select(Class @class, bool sqlLike = false, bool sqlAnd = true);
    }
}