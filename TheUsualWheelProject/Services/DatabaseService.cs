using Dapper;
using Microsoft.Data.Sqlite; 
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Models;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public class DatabaseService : LoggingBase<DatabaseService>
{
    private readonly string _conString;
    private readonly IWheelRepository _wheelRepository;
    private readonly IMovieRepository _movieRepository;

    public DatabaseService(string conString, IWheelRepository wheelRepository, IMovieRepository movieRepository, ILogger<DatabaseService> logger) : base(logger)
    {
         _conString = conString;
         _wheelRepository = wheelRepository;
         _movieRepository = movieRepository;
         Logger.LogInformation("Initialized.");
    }
    public async Task InitAsync()
    {
        Logger.LogInformation("Initializing database.");
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Wheel (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT
            );
            CREATE TABLE IF NOT EXISTS Movie (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Year INTEGER,
                Genre TEXT,
                Director TEXT,
                Cast TEXT,
                DurationInMinutes INTEGER,
                Synopsis TEXT,
                PosterUrl TEXT,
                TmdbRating REAL,
                LetterboxdRating REAL,
                TmdbId INTEGER
            );
            CREATE TABLE IF NOT EXISTS WheelMovie (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                WheelId INTEGER NOT NULL,
                MovieId INTEGER NOT NULL,
                IsEliminated INTEGER NOT NULL DEFAULT 0,
                WatchedDate TEXT,
                FOREIGN KEY (WheelId) REFERENCES Wheel(Id),
                FOREIGN KEY (MovieId) REFERENCES Movie(Id)
            );
            CREATE TABLE IF NOT EXISTS WatchProvider (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProviderId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                LogoUrl TEXT NOT NULL,
                DisplayPriority INTEGER NOT NULL DEFAULT 0
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_WatchProvider_ProviderId
            ON WatchProvider(ProviderId);
            CREATE TABLE IF NOT EXISTS MovieProvider (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MovieId INTEGER NOT NULL,
                ProviderId INTEGER NOT NULL,
                Type TEXT NOT NULL,
                FOREIGN KEY (MovieId) REFERENCES Movie(Id),
                FOREIGN KEY (ProviderId) REFERENCES WatchProvider(ProviderId)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_MovieProvider_UniqueLink
            ON MovieProvider(MovieId, ProviderId, Type);
            CREATE INDEX IF NOT EXISTS IX_MovieProvider_MovieId
            ON MovieProvider(MovieId);
            CREATE INDEX IF NOT EXISTS IX_MovieProvider_ProviderId
            ON MovieProvider(ProviderId);
        ");
        // SeedTestWheelsAsync()
    }

    // For testing purposes, to clear all data from the database
    public async Task ClearAllDataAsync()
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(@"
            DELETE FROM MovieProvider;
            DELETE FROM WatchProvider;
            DELETE FROM WheelMovie;
            DELETE FROM Movie;
            DELETE FROM Wheel;
        ");
    }

    private async Task SeedTestWheelsAsync()
    {
        var wheels = await _wheelRepository.GetAll();
        if (!wheels.Any())
        {
            await _wheelRepository.Insert(new Wheel { Name = "Denzel Washington", Description = "Sample description" });
            await _wheelRepository.Insert(new Wheel { Name = "Willem Dafoe", Description = "Sample description" });
        }
    }

    public async Task ResetAndSeedTestDataAsync()
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        using var tx = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(@"
            PRAGMA foreign_keys = OFF;
            DROP TABLE IF EXISTS MovieProvider;
            DROP TABLE IF EXISTS WatchProvider;
            DROP TABLE IF EXISTS WheelMovie;
            DROP TABLE IF EXISTS Movie;
            DROP TABLE IF EXISTS Wheel;
            PRAGMA foreign_keys = ON;
        ", transaction: tx);

        await tx.CommitAsync();

        await InitAsync();

        using var seedConn = new SqliteConnection(_conString);
        await seedConn.OpenAsync();
        using var seedTx = await seedConn.BeginTransactionAsync();

        await seedConn.ExecuteAsync(@"
            INSERT INTO Wheel (Id, Name, Description) VALUES
            (17, 'Russell Crowe', 'The wheel of Russell Crowe');

            INSERT INTO Movie (Id, Title, Year, Genre, Director, Cast, DurationInMinutes, Synopsis, PosterUrl, TmdbRating, LetterboxdRating, TmdbId) VALUES
            (33, 'Gladiator', 2000, 'Unknown Genre', 'Unknown Director', 'Unknown Cast', 0, 'Unknown Synopsis', NULL, 0, NULL, 98),
            (34, 'Master and Commander', 2003, 'Unknown Genre', 'Unknown Director', 'Unknown Cast', 0, 'Unknown Synopsis', NULL, 0, NULL, 1778),
            (36, 'A Beautiful Mind', 2001, 'Unknown Genre', 'Unknown Director', 'Unknown Cast', 0, 'Unknown Synopsis', NULL, 0, NULL, 453);

            INSERT INTO WheelMovie (WheelId, MovieId, IsEliminated, WatchedDate) VALUES
            (17, 33, 0, NULL),
            (17, 34, 0, NULL),
            (17, 36, 0, NULL);

            INSERT INTO WatchProvider (ProviderId, Name, LogoUrl, DisplayPriority) VALUES
            (337, 'Disney Plus', '/7rwgEs15tFwyR9NPQ5vpzxTj19Q.jpg', 1),
            (1001, 'KPN', '/kpn-placeholder.png', 2),
            (444, 'Pathé Thuis', '/pathé-placeholder.png', 3),
            (2, 'Apple TV', '/peURlLlr8jggOwK53fJ5wdQl05y.jpg', 4);

            INSERT INTO MovieProvider (MovieId, ProviderId, Type) VALUES
            (34, 337, 'flatrate'),
            (34, 1001, 'flatrate'),
            (34, 444, 'rent'),
            (34, 2, 'rent');
        ", transaction: seedTx);

        await seedTx.CommitAsync();
    }
}