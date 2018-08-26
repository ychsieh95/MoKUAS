using MoKUAS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoKUAS.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<Feedback> SelectByCreatorAsync(Feedback feedback);
        Task<int> Insert(Feedback feedback);
    }
}