using Models;

namespace Services;

public interface IProducerService
{
    Task<bool> Create(Producer producer);
    Task<Producer?> ReadOne(int id);
    Task<List<Producer>> ReadAll();
    Task<bool> Delete(int id);
    Task<bool> Update(int id, Producer updatedProducer);
}