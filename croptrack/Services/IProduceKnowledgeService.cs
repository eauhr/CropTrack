using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IProduceKnowledgeService
    {
        Task<List<Produce>> GetRecommendedCrops(double currentTemp, CancellationToken cancellationToken = default);
        Task<List<Produce>> GetAllProduces(CancellationToken cancellationToken = default);
    }
}
