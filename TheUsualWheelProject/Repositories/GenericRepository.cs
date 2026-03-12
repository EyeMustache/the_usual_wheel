using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories;

public class GenericRepository<T, TId> : IGenericRepository<T, TId>
{   
    private readonly string TableName;
    private readonly TId Id;
    private readonly string _conString;

    public GenericRepository(string tableName, TId id, string conString)
    {
        TableName = tableName;
        Id = id;
        _conString = conString;
    }

    public IEnumerable<T> GetAll()
    {
        throw new NotImplementedException();
    }

    public T GetById(TId id)
    {
        throw new NotImplementedException();
    }

    public void Insert(T entity)
    {
        throw new NotImplementedException();
    }

    public void Update(T entity)
    {
        throw new NotImplementedException();
    }
    
    public void Delete(TId id)
    {
        throw new NotImplementedException();
    }
}