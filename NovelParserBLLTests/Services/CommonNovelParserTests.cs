using Microsoft.VisualStudio.TestTools.UnitTesting;
using NovelParserBLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelParserBLL.Services.Tests
{
    [TestClass()]
    public class CommonNovelParserTests
    {
        [TestMethod()]
        public async void ParseCommonInfoTest_ranobelib()
        {
            var testLink = "https://ranobelib.me/the-beginning-after-the-end-novel?bid=9823&section=chapters&ui=79422";
            var commonNovelParser = new CommonNovelParser(new NovelCacheService(), null);

            var source = new CancellationTokenSource();

            var novel = await commonNovelParser.ParseCommonInfo(testLink, source.Token);

            Assert.IsNotNull(novel);
        }
    }
}