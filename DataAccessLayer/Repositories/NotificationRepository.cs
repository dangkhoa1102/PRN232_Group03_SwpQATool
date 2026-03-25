using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly swp_qa_toolsContext _context;
        public NotificationRepository(swp_qa_toolsContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId, bool? isRead = null)
       {
            var query = _context.Notifications.Where(x => x.UserId == userId);

            if (isRead.HasValue)
                query = query.Where(x => x.IsRead == isRead.Value);

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
       }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
       {
            var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return true;
       }
    }
}