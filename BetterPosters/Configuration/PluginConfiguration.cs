using MediaBrowser.Model.Plugins;

namespace BetterPosters.Configuration;

/// <summary>
/// Rating sources supported by btttr.cc.
/// </summary>
public enum PosterRatingSource
{
    /// <summary>
    /// Use the btttr.cc default average rating.
    /// </summary>
    Average,

    /// <summary>
    /// IMDb rating.
    /// </summary>
    Imdb,

    /// <summary>
    /// TMDB rating.
    /// </summary>
    Tmdb,

    /// <summary>
    /// Rotten Tomatoes rating.
    /// </summary>
    RottenTomatoes,

    /// <summary>
    /// Metacritic rating.
    /// </summary>
    Metacritic,

    /// <summary>
    /// Trakt rating.
    /// </summary>
    Trakt,

    /// <summary>
    /// Letterboxd rating.
    /// </summary>
    Letterboxd,

    /// <summary>
    /// Roger Ebert rating.
    /// </summary>
    RogerEbert
}

/// <summary>
/// Poster languages supported by btttr.cc.
/// </summary>
public enum PosterLanguage
{
    /// <summary>
    /// English posters.
    /// </summary>
    English,

    /// <summary>
    /// Spanish posters.
    /// </summary>
    Spanish,

    /// <summary>
    /// French posters.
    /// </summary>
    French,

    /// <summary>
    /// German posters.
    /// </summary>
    German,

    /// <summary>
    /// Brazilian Portuguese posters.
    /// </summary>
    PortugueseBrazil,

    /// <summary>
    /// Portugal Portuguese posters.
    /// </summary>
    PortuguesePortugal,

    /// <summary>
    /// Italian posters.
    /// </summary>
    Italian,

    /// <summary>
    /// Dutch posters.
    /// </summary>
    Dutch,

    /// <summary>
    /// Polish posters.
    /// </summary>
    Polish,

    /// <summary>
    /// Russian posters.
    /// </summary>
    Russian,

    /// <summary>
    /// Turkish posters.
    /// </summary>
    Turkish,

    /// <summary>
    /// Arabic posters.
    /// </summary>
    Arabic,

    /// <summary>
    /// Japanese posters.
    /// </summary>
    Japanese,

    /// <summary>
    /// Korean posters.
    /// </summary>
    Korean,

    /// <summary>
    /// Chinese posters.
    /// </summary>
    Chinese,

    /// <summary>
    /// Hindi posters.
    /// </summary>
    Hindi,

    /// <summary>
    /// Swedish posters.
    /// </summary>
    Swedish,

    /// <summary>
    /// Czech posters.
    /// </summary>
    Czech
}

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        EnableTrendTags = true;
        EnableQualityTags = false;
        EnableGenre = true;
        EnableRating = true;
        RatingSource = PosterRatingSource.Average;
        EnableAgeRating = false;
        Language = PosterLanguage.English;
    }

    /// <summary>
    /// Gets or sets a value indicating whether trend tags are enabled.
    /// </summary>
    public bool EnableTrendTags { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether quality tags are enabled.
    /// </summary>
    public bool EnableQualityTags { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the genre label is enabled.
    /// </summary>
    public bool EnableGenre { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rating label is enabled.
    /// </summary>
    public bool EnableRating { get; set; }

    /// <summary>
    /// Gets or sets the rating source.
    /// </summary>
    public PosterRatingSource RatingSource { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the age rating label is enabled.
    /// </summary>
    public bool EnableAgeRating { get; set; }

    /// <summary>
    /// Gets or sets the poster language.
    /// </summary>
    public PosterLanguage Language { get; set; }
}
