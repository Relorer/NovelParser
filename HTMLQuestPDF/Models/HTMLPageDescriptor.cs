using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTMLQuestPDF.Models
{
    internal class HTMLPageDescriptor
    {
        private readonly PageSize containerSize;

        private readonly Func<string, string> getImagePath;

        private readonly string html;
    }
}
