using System.Net.Http.Headers;
using System.Text.Json;
using SAIN.Preset.Shared;
using SAINServerMod.Services;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SAINServerMod.Web.Updates;

[Injectable(InjectionType.Singleton)]
public sealed class GitHubReleaseService(
    ConfigService configService,
    IHttpClientFactory httpClientFactory,
    ISptLogger<GitHubReleaseService> logger
)
{
    private const string ReleasesUrl = "https://api.github.com/repos/ArchangelWTF/SAIN/releases?per_page=10";

    private static readonly Version _currentVersion = new(SAINVersionInfo.SAINVersion);
    private static readonly TimeSpan _cacheLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(8);
    private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    private readonly SemaphoreSlim _fetchLock = new(1, 1);

    private DateTimeOffset _fetchedAt = DateTimeOffset.MinValue;

    public event Action? FeedChanged;

    public ReleaseFeedState State { get; private set; } = ReleaseFeedState.Idle;
    public IReadOnlyList<GitHubRelease> Releases { get; private set; } = [];

    public Version CurrentVersion
    {
        get { return _currentVersion; }
    }

    /// <summary>The newest stable release that runs on this server, which the running version is compared against.</summary>
    public GitHubRelease? LatestStable
    {
        get { return Releases.FirstOrDefault(release => !release.Draft && !release.Prerelease && IsCompatible(release)); }
    }

    // A release with no versioned SPT tag tells us nothing either way, so it stays eligible.
    public static bool IsCompatible(GitHubRelease release)
    {
        List<CompatibilityTag> sptTags =
        [
            .. ReleaseDirectives
                .ParseCompatibility(release.Body)
                .Where(tag => string.Equals(tag.Platform, "SPT", StringComparison.OrdinalIgnoreCase) && tag.Version is not null),
        ];

        return sptTags.Count == 0 || sptTags.Any(MatchesRunningPlatform);
    }

    public bool UpdateAvailable
    {
        get
        {
            if (LatestStable is null || !TryParseVersion(LatestStable.TagName, out var latest))
            {
                return false;
            }

            return latest > _currentVersion;
        }
    }

    private string CachePath
    {
        get { return Path.Combine(configService.ModPath, "cache", "releases.json"); }
    }

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (State == ReleaseFeedState.Loaded && DateTimeOffset.Now - _fetchedAt < _cacheLifetime)
        {
            return;
        }

        await RefreshAsync(false, cancellationToken);
    }

    public async Task RefreshAsync(bool force = true, CancellationToken cancellationToken = default)
    {
        if (!await _fetchLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            if (!force && State == ReleaseFeedState.Loaded && DateTimeOffset.Now - _fetchedAt < _cacheLifetime)
            {
                return;
            }

            if (State != ReleaseFeedState.Loaded)
            {
                SetState(ReleaseFeedState.Loading);

                CachedReleases? cached = await ReadCacheAsync(cancellationToken);

                if (cached is not null)
                {
                    Releases = cached.Releases;
                    _fetchedAt = cached.FetchedAt;
                    SetState(ReleaseFeedState.Loaded);

                    if (DateTimeOffset.Now - cached.FetchedAt < _cacheLifetime && !force)
                    {
                        return;
                    }
                }
            }

            List<GitHubRelease>? fetched = await FetchAsync(cancellationToken);

            if (fetched is null)
            {
                SetState(Releases.Count > 0 ? ReleaseFeedState.Loaded : ReleaseFeedState.Unavailable);
                return;
            }

            Releases = fetched;
            _fetchedAt = DateTimeOffset.Now;
            SetState(ReleaseFeedState.Loaded);

            await WriteCacheAsync(cancellationToken);
        }
        finally
        {
            _fetchLock.Release();
        }
    }

    private async Task<List<GitHubRelease>?> FetchAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpClient client = httpClientFactory.CreateClient();

            client.Timeout = _requestTimeout;

            // GitHub rejects requests without a user agent outright.
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SAIN", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using HttpResponseMessage response = await client.GetAsync(ReleasesUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.Debug($"[SAIN] GitHub returned {(int)response.StatusCode} for the release feed");
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, _serializerOptions, cancellationToken);

            return releases?.Where(release => !release.Draft).ToList();
        }
        catch (Exception ex)
        {
            // Offline installs are the norm rather than the exception, so this never escalates past debug.
            logger.Debug($"[SAIN] Could not reach GitHub for the release feed: {ex.Message}");
            return null;
        }
    }

    private async Task<CachedReleases?> ReadCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(CachePath))
            {
                return null;
            }

            await using var stream = File.OpenRead(CachePath);
            return await JsonSerializer.DeserializeAsync<CachedReleases>(stream, _serializerOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Debug($"[SAIN] Could not read the cached release feed: {ex.Message}");
            return null;
        }
    }

    private async Task WriteCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath)!);

            var payload = new CachedReleases { FetchedAt = _fetchedAt, Releases = [.. Releases] };

            await File.WriteAllTextAsync(CachePath, JsonSerializer.Serialize(payload, _serializerOptions), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Debug($"[SAIN] Could not cache the release feed: {ex.Message}");
        }
    }

    private void SetState(ReleaseFeedState state)
    {
        State = state;
        FeedChanged?.Invoke();
    }

    public static bool TryParseVersion(string tag, out Version version)
    {
        return Version.TryParse(tag.TrimStart('v', 'V'), out version);
    }

    public static bool MatchesRunningPlatform(CompatibilityTag tag)
    {
        if (!string.Equals(tag.Platform, "SPT", StringComparison.OrdinalIgnoreCase) || tag.Version is null)
        {
            return false;
        }

        Version? running = ProgramStatics.SPT_VERSION();

        if (running is null)
        {
            return false;
        }

        if (!Range.TryParse(tag.Version.TrimStart('v', 'V'), out Range supported))
        {
            return false;
        }

        return supported.IsSatisfied(running.ToString());
    }
}
