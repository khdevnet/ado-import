namespace AdoImportTestCases;

public class FeatureResult
{
    public FeatureInfo Info { get; set; } = new FeatureInfo();

    public IEnumerable<ScenarioResult> Scenarios { get; set; } = Enumerable.Empty<ScenarioResult>();
}

public class ScenarioResult
{
    public ScenarioInfo? Info { get; set; }

    public string? AutomatedTestName { get; set; }

    public string? AutomatedTestStorage { get; set; }

    public IEnumerable<StepResult>? Steps { get; set; }
}

public class StepResult
{
    public StepInfo? Info { get; set; }

    public IEnumerable<string>? Comments { get; set; }
}

public class StepInfo
{
    public string? Name { get; set; }

    public string? GroupPrefix { get; set; }

    public int Number { get; set; }

    public int Total { get; set; }
}

public class ScenarioInfo
{
    public string? Name { get; set; }

    public string? Code { get; set; }

    public FeatureInfo? Parent { get; set; }

    public IEnumerable<string>? Labels { get; set; }

    public IEnumerable<string>? Categories { get; set; }
}

public class FeatureInfo
{
    public string? Name { get; set; }

    public IEnumerable<string>? Labels { get; set; }

    public string? Description { get; set; }
}


