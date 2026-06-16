using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BetterPosters.Configuration;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.BaseItemManager;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace BetterPosters.ScheduledTasks;

/// <summary>
/// Replaces movie and series primary images with configured Better Posters images.
/// </summary>
public class BetterPostersUpdateTask : IScheduledTask, IConfigurableScheduledTask
{
    private const int UpdateItemResultUpdated = 0;
    private const int UpdateItemResultSkipped = 1;
    private const int UpdateItemResultFailed = 2;

    private readonly ILibraryManager _libraryManager;
    private readonly IProviderManager _providerManager;
    private readonly IBaseItemManager _baseItemManager;
    private readonly ILogger<BetterPostersUpdateTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetterPostersUpdateTask"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="providerManager">The provider manager.</param>
    /// <param name="baseItemManager">The base item manager.</param>
    /// <param name="logger">The logger.</param>
    public BetterPostersUpdateTask(
        ILibraryManager libraryManager,
        IProviderManager providerManager,
        IBaseItemManager baseItemManager,
        ILogger<BetterPostersUpdateTask> logger)
    {
        _libraryManager = libraryManager;
        _providerManager = providerManager;
        _baseItemManager = baseItemManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Update Better Posters";

    /// <inheritdoc />
    public string Key => "BetterPostersUpdate";

    /// <inheritdoc />
    public string Description => "Replace movie and show primary images with configured Better Posters images.";

    /// <inheritdoc />
    public string Category => Plugin.PluginName;

    /// <inheritdoc />
    public bool IsHidden => false;

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsLogged => true;

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var configuration = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var items = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series],
            Recursive = true,
            HasImdbId = true
        });

        if (items.Count == 0)
        {
            _logger.LogInformation("No movies or shows with IMDb IDs were found for Better Posters update");
            progress.Report(100);
            return;
        }

        var updatedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        _logger.LogInformation("Updating Better Posters images for {ItemCount} items", items.Count);

        for (var index = 0; index < items.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await UpdateItem(items[index], configuration, cancellationToken).ConfigureAwait(false);
            switch (result)
            {
                case UpdateItemResultUpdated:
                    updatedCount++;
                    break;
                case UpdateItemResultSkipped:
                    skippedCount++;
                    break;
                case UpdateItemResultFailed:
                    failedCount++;
                    break;
            }

            progress.Report((index + 1) * 100D / items.Count);
        }

        _logger.LogInformation(
            "Better Posters update finished: {UpdatedCount} updated, {SkippedCount} skipped, {FailedCount} failed",
            updatedCount,
            skippedCount,
            failedCount);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return [];
    }

    private async Task<int> UpdateItem(BaseItem item, PluginConfiguration configuration, CancellationToken cancellationToken)
    {
        var imdbId = item.GetProviderId(MetadataProvider.Imdb);
        if (string.IsNullOrWhiteSpace(imdbId))
        {
            return UpdateItemResultSkipped;
        }

        var libraryOptions = _libraryManager.GetLibraryOptions(item);
        var typeOptions = libraryOptions.GetTypeOptions(item.GetType().Name);
        if (!_baseItemManager.IsImageFetcherEnabled(item, typeOptions, Plugin.PluginName))
        {
            return UpdateItemResultSkipped;
        }

        var url = BetterPosterUrlBuilder.Build(imdbId, configuration);
        try
        {
            await _providerManager.SaveImage(item, url, ImageType.Primary, null, cancellationToken).ConfigureAwait(false);
            await item.UpdateToRepositoryAsync(ItemUpdateType.ImageUpdate, cancellationToken).ConfigureAwait(false);
            return UpdateItemResultUpdated;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Better Posters image for {ItemName} ({ItemId})", item.Name, item.Id);
            return UpdateItemResultFailed;
        }
    }
}
