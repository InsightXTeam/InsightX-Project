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

        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .Include(x => x.ExtractedMetrics)
                .ToListAsync();
        }

        public async Task<List<Report>> GetByCompanyAndUserAsync(int companyId, string role, string userId)
        {
            var query = _context.Reports.Where(r => r.CompanyId == companyId);

            if (role != "Owner")
            {
                query = query.Where(r => r.UploadedById == userId);
            }

            return await query.Include(x => x.ExtractedMetrics).ToListAsync();
        }

        public async Task<Report?> GetByIdAsync(int id)
        {
            return await _context.Reports
                .Include(x => x.ExtractedMetrics)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Report report)
        {
            _context.Reports.Update(report);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Report report)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }

        public async Task<Report?> GetByIdForCompanyAsync(int id, int companyId)
        {
            return await _context.Reports
                .Include(x => x.ExtractedMetrics)
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
        }
    }
}