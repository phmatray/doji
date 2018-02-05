namespace Doji.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGitHubService
    {
        Task<List<GitHubRelease>> GetPublishedReleases();
    }
}