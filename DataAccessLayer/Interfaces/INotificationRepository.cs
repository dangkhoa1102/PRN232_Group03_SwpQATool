using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId, bool? isRead = null);

        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    }
}