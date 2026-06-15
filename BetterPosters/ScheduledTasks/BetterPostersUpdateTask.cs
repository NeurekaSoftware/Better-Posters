using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BetterPosters.Configuration;
using Jellyfin.Data.Enums;
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
    private readonly ILibraryManager _libraryManager;
    private readonly IProviderManager _providerManager;
    private readonly ILogger<BetterPostersUpdateTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetterPostersUpdateTask"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="providerManager">The provider manager.</param>
    /// <param name="logger">The logger.</param>
    public BetterPostersUpdateTask(
        ILibraryManager libraryManager,
        IProviderManager providerManager,
        ILogger<BetterPostersUpdateTask> logger)
    {
        _libraryManager = libraryManager;
        _providerManager = providerManager;
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
            progress.Report(100);
            return;
        }

        for (var index = 0; index < items.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await UpdateItem(items[index], configuration, cancellationToken).ConfigureAwait(false);
            progress.Report((index + 1) * 100D / items.Count);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return [];
    }

    private async Task UpdateItem(BaseItem item, PluginConfiguration configuration, CancellationToken cancellationToken)
    {
        var imdbId = item.GetProviderId(MetadataProvider.Imdb);
        if (string.IsNullOrWhiteSpace(imdbId))
        {
            return;
        }

        var url = BetterPosterUrlBuilder.Build(imdbId, configuration);
        try
        {
            await _providerManager.SaveImage(item, url, ImageType.Primary, null, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Better Posters image for {ItemName} ({ItemId})", item.Name, item.Id);
        }
    }
}
