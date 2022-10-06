namespace NovelParserBLL.FileGenerators
{
    internal interface IFileGenerator<T> where T : GenerationParams
    {
        public Task Generate(T generationParams);
    }
}