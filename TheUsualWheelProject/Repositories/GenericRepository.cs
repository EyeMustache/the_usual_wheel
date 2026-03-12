using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Repositories;

public class GenericRepository<T, TId> : IGenericRepository<T, TId> where T : class, IModel
{
    protected readonly string TableName;
    protected readonly string _conString;

    public GenericRepository(string tableName, string conString)
    {
        TableName = tableName;
        _conString = conString;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        using var connection = new SqliteConnection(_conString);
        return await connection.GetAllAsync<T>();
    }

    public async Task<T?> GetById(TId id)
    {
        using var connection = new SqliteConnection(_conString);
        return await connection.GetAsync<T>(id);
    }

    public async Task Insert(T model)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.InsertAsync(model);
    }

    public async Task Update(T model)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.UpdateAsync(model);
    }

    // Dapper.Contrib does not have a DeleteAsync method that takes an id, so the old way here.
    public async Task Delete(TId id)
    {
        using var connection = new SqliteConnection(_conString);
        var query = $"DELETE FROM {TableName} WHERE Id = @Id";
        await connection.ExecuteAsync(query, new { Id = id });
    }
}