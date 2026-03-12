using Dapper;
using Microsoft.Data.Sqlite; 

namespace TheUsualWheelProject.Services;

public class DatabaseService
{
    private readonly string _conString;

    public DatabaseService(string conString)
    {
        _conString = conString;
    }

    public async Task InitAsync()
    {
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
        ");
    }

    // For testing purposes, to clear all data from the database
    public async Task ClearAllDataAsync()
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(@"
            DELETE FROM WheelMovie;
            DELETE FROM Movie;
            DELETE FROM Wheel;
        ");
    }
}