using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IGenericRepository<T, TId> where T : class, IModel
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(TId id);
    Task Insert(T model);
    Task Update(T model);
    Task Delete(TId id);
}