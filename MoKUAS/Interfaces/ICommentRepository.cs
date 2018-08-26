using MoKUAS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoKUAS.Interfaces
{
    public interface ICommentRepository
    {
        Task<int> Delete(Comment comment);
        Task<int> Insert(Comment comment);
        Task<IEnumerable<Comment>> Select(Comment comment);
        Task<int> Update(Comment comment);
    }
}