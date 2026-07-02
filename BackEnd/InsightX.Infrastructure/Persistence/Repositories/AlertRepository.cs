using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;

        public AlertRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Alert>> GetByCompanyAsync(int companyId, bool? seenFilter)
        {
            var alerts = _context.Alerts.Where(a => a.CompanyId == companyId);
            if (seenFilter.HasValue) alerts = alerts.Where(a => a.SeenByOwner == seenFilter.Value);


            return await alerts
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert?> GetByIdAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            return alert;
        }

        public async Task MarkAsSeenAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert != null)
            {
                alert.SeenByOwner = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
