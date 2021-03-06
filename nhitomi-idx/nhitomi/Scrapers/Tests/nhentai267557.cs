using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace nhitomi.Scrapers.Tests
{
    public class nhentai267557 : ScraperTest<nhentaiBook>
    {
        readonly nhentaiScraper _scraper;

        public nhentai267557(nhentaiScraper scraper, ILogger<nhentai267557> logger) : base(logger)
        {
            _scraper = scraper;
        }

        protected override Task<nhentaiBook> GetAsync(CancellationToken cancellationToken = default) => _scraper.GetAsync(267557, cancellationToken);

        protected override void Match(nhentaiBook other)
        {
            Match("id", 267557, other.Id);
            Match("mediaId", 1389990, other.MediaId);
            Match("uploadDate", 1553975507, other.UploadDate);

            Match("title.english", "(C91) [Purin Kai Yoghurt (Chiri)] CxMxK Note IX", other.Title.English);
            Match("title.japanese", "(C91) [プリン海ヨーグルト (ちり)] CxMxK Note IX", other.Title.Japanese);

            Match("pages", 16, other.Images.Pages.Length);
            Match("pages.ext", new string('p', 16), string.Concat(other.Images.Pages.Select(p => p.Type)));

            Match("tags", new[]
            {
                "chiri",
                "japanese",
                "purin kai yoghurt",
                "full censorship",
                "bondage",
                "lolicon",
                "full color",
                "stockings",
                "fox girl",
                "catgirl",
                "doujinshi",
                "kemonomimi",
                "original"
            }, other.Tags.Select(t => t.Name));
        }
    }
}