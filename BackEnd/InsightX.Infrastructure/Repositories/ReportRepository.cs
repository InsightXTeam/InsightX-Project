using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Report>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Reports
                .Include(x => x.ExtractedMetrics)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Report>> GetByCompanyAndUserAsync(int companyId, string role, string userId, CancellationToken cancellationToken = default)
        {
            var query = _context.Reports.IgnoreQueryFilters().Where(r => r.CompanyId == companyId);

            if (role != "Owner")
            {
                query = query.Where(r => r.UploadedById == userId);
            }

            return await query.Include(x => x.ExtractedMetrics)
                              .Include(x => x.UploadedBy)
                              .ToListAsync(cancellationToken);
        }

        public async Task<Report?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Reports
                .Include(x => x.ExtractedMetrics)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
        {
            await _context.Reports.AddAsync(report, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
        {
            // _context.Reports.Update(report); // Removed: Let change tracker handle it
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Report report, CancellationToken cancellationToken = default)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Report?> GetByIdForCompanyAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.Reports
                .Include(x => x.ExtractedMetrics)
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, cancellationToken);
        }
    }
}