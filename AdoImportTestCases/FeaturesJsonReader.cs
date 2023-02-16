using Newtonsoft.Json;

namespace AdoImportTestCases;

public class FeaturesJsonReader
{
    public FeatureResult[] Read(string path)
    {
        var fileContent = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<FeatureResult[]>(fileContent);
    }
}
