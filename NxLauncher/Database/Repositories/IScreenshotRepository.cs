using System.Collections.Generic;
using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public interface IScreenshotRepository
{
    int AddScreenshot(Screenshot screenshot, Game game);
    IEnumerable<Screenshot> GetAllGameScreenshots(int gameId);
    void DeleteScreenshot(int? screenshotId);
}