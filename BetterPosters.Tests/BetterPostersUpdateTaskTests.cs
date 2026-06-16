using System.Linq;
using BetterPosters.ScheduledTasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BetterPosters.Tests;

public class BetterPostersUpdateTaskTests
{
    [Fact]
    public void IsEnabled_ByDefault_ReturnsTrue()
    {
        var task = CreateTask();

        var isEnabled = task.IsEnabled;

        Assert.True(isEnabled);
    }

    [Fact]
    public void GetDefaultTriggers_ByDefault_ReturnsNoTriggers()
    {
        var task = CreateTask();

        var triggers = task.GetDefaultTriggers().ToArray();

        Assert.Empty(triggers);
    }

    private static BetterPostersUpdateTask CreateTask()
    {
        return new BetterPostersUpdateTask(
            null!,
            null!,
            null!,
            NullLogger<BetterPostersUpdateTask>.Instance);
    }
}
