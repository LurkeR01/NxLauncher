using System.Collections.Generic;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public interface IGenreRepository
{
    IEnumerable<Genre> GetAll();
    Genre GetById(int id);
    IEnumerable<Genre> GetAllGameGenres(int gameId);
    void Add(Genre genre);
}