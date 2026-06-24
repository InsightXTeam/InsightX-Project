using InsightX.Application.Interfaces;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Repositories
{
    public class KPIRepository : IKPIRepository
    {
        private readonly AppDbContext _context;

        public KPIRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetKpiNamesByCompanyIdAsync(int companyId)
        {
            return await _context.KPIs
                .Where(k => k.CompanyId == companyId)
                .Select(k => k.Name)
                .ToListAsync();
        }
    }
}
