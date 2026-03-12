using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces; 

namespace TheUsualWheelProject.Repositories;

public class WheelRepository : GenericRepository<Wheel, int>, IWheelRepository
{
    public WheelRepository(string conString) : base("Wheel", conString) { }
}