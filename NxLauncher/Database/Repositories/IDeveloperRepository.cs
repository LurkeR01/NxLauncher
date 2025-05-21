using System.Collections;
using System.Collections.Generic;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public interface IDeveloperRepository
{
    IEnumerable<Developer> GetAll();
    Developer GetById(int id);
    int GetIdByName(string name);
    void Add(Developer developer);
}