using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterPosters.ScheduledTasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BetterPosters.Tests;

public class BetterPostersUpdateTaskExecutionTests
{
    private const string ImdbId = "tt0111161";
    private const string DefaultPosterUrl = "https://btttr.cc/poster/imdb/poster-default/tt0111161.jpg";

    [Fact]
    public async Task ExecuteAsync_WithItemHavingImdbId_SavesImageAndPersistsUpdate()
    {
        var item = CreateMovie(ImdbId);
        var (task, providerManager, _) = CreateTask([item]);
        providerManager
            .Setup(provider => provider.SaveImage(item, DefaultPosterUrl, ImageType.Primary, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var progress = new RecordingProgress();

        await task.ExecuteAsync(progress, CancellationToken.None);

        providerManager.Verify(
            provider => provider.SaveImage(item, DefaultPosterUrl, ImageType.Primary, null, It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal([ItemUpdateType.ImageUpdate], item.UpdateReasons);
        Assert.Equal([100D], progress.Values);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoItems_ReportsCompleteWithoutSaving()
    {
        var (task, providerManager, libraryManager) = CreateTask([]);
        var progress = new RecordingProgress();

        await task.ExecuteAsync(progress, CancellationToken.None);

        Assert.Equal([100D], progress.Values);
        providerManager.VerifyNoOtherCalls();
        libraryManager.Verify(
            manager => manager.GetItemList(It.Is<InternalItemsQuery>(query => IsBetterPostersQuery(query))),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSaveFails_ContinuesWithLaterItems()
    {
        var failingItem = CreateMovie(ImdbId);
        var successfulItem = CreateMovie("tt0068646");
        var expectedSuccessfulUrl = "https://btttr.cc/poster/imdb/poster-default/tt0068646.jpg";
        var (task, providerManager, _) = CreateTask([failingItem, successfulItem]);
        providerManager
            .Setup(provider => provider.SaveImage(failingItem, DefaultPosterUrl, ImageType.Primary, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));
        providerManager
            .Setup(provider => provider.SaveImage(successfulItem, expectedSuccessfulUrl, ImageType.Primary, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var progress = new RecordingProgress();

        await task.ExecuteAsync(progress, CancellationToken.None);

        Assert.Empty(failingItem.UpdateReasons);
        Assert.Equal([ItemUpdateType.ImageUpdate], successfulItem.UpdateReasons);
        Assert.Equal([50D, 100D], progress.Values);
    }

    [Fact]
    public async Task ExecuteAsync_WithItems_ReportsFinalProgressComplete()
    {
        var firstItem = CreateMovie(ImdbId);
        var secondItem = CreateMovie("tt0068646");
        var (task, providerManager, _) = CreateTask([firstItem, secondItem]);
        providerManager
            .Setup(provider => provider.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), ImageType.Primary, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var progress = new RecordingProgress();

        await task.ExecuteAsync(progress, CancellationToken.None);

        Assert.Equal(2, progress.Values.Count);
        Assert.Equal(100D, progress.Values.Last());
    }

    private static (BetterPostersUpdateTask Task, Mock<IProviderManager> ProviderManager, Mock<ILibraryManager> LibraryManager) CreateTask(IReadOnlyList<BaseItem> items)
    {
        var libraryManager = new Mock<ILibraryManager>(MockBehavior.Strict);
        libraryManager
            .Setup(manager => manager.GetItemList(It.Is<InternalItemsQuery>(query => IsBetterPostersQuery(query))))
            .Returns(items);

        var providerManager = new Mock<IProviderManager>(MockBehavior.Strict);

        return (
            new BetterPostersUpdateTask(
                libraryManager.Object,
                providerManager.Object,
                NullLogger<BetterPostersUpdateTask>.Instance),
            providerManager,
            libraryManager);
    }

    private static TrackingMovie CreateMovie(string imdbId)
    {
        var item = new TrackingMovie();
        item.SetProviderId(MetadataProvider.Imdb, imdbId);
        return item;
    }

    private static bool IsBetterPostersQuery(InternalItemsQuery query)
    {
        return query.HasImdbId == true
            && query.Recursive
            && query.IncludeItemTypes is not null
            && query.IncludeItemTypes.SequenceEqual([BaseItemKind.Movie, BaseItemKind.Series]);
    }

    private sealed class RecordingProgress : IProgress<double>
    {
        public List<double> Values { get; } = [];

        public void Report(double value)
        {
            Values.Add(value);
        }
    }

    private sealed class TrackingMovie : Movie
    {
        public List<ItemUpdateType> UpdateReasons { get; } = [];

        public override Task UpdateToRepositoryAsync(ItemUpdateType updateReason, CancellationToken cancellationToken)
        {
            UpdateReasons.Add(updateReason);
            return Task.CompletedTask;
        }
    }
}
