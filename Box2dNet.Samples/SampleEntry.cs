namespace Box2dNet.Samples
{
    internal record struct SampleEntry(string category, string name, Func<SampleContext, Sample> createFcn);
}
