using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Services
{
    public class ProduceKnowledgeService : IProduceKnowledgeService
    {
        private readonly FieldDbTrackContext _dbContext;

        public ProduceKnowledgeService(FieldDbTrackContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Produce>> GetAllProduces(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Produces
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Produce>> GetRecommendedCrops(double currentTemp, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Produces
                .AsNoTracking()
                .Where(p => p.MinTempC <= currentTemp && p.MaxTempC >= currentTemp)
                .OrderBy(p => Math.Abs(p.IdealTempC - currentTemp))
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
