using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IGenericRepository<T, TId>
{
    IEnumerable<T> GetAll();
    T GetById(TId id);
    void Insert(T entity); 
    void Update(T entity);
    void Delete(TId id);
}