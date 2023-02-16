using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.Text;
using WorkItemTeamFoundation = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using WorkItem = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem;

namespace AdoImportTestCases;

public static class WorkItemClientExtensions
{
    public static async Task<WorkItemTeamFoundation> CreateOrUpdateTestCase(this WorkItemTrackingHttpClient httpClient, string projectName, ScenarioResult scenarioResult)
    {

        WorkItemTeamFoundation workItem = null;
        var testCaseId = await GetTestCaseByTag(httpClient, projectName, scenarioResult.Info.Code);
        var testCaseDoc = CreateTestCasePathDocument(scenarioResult);

        if (!testCaseId.HasValue)
        {
            workItem = await httpClient.CreateWorkItemAsync(testCaseDoc, projectName, "Test Case");
        }
        else
        {
            workItem = await httpClient.UpdateWorkItemAsync(testCaseDoc, testCaseId.Value);
        }

        return workItem;
    }

    public static async Task<IEnumerable<SuiteTestCaseCreateUpdateParameters>> GetTestCasesAsync(this WorkItemTrackingHttpClient httpClient, string projectName, IEnumerable<ScenarioResult> scenarios, int parallel = 5)
    {
        var testCases = new List<SuiteTestCaseCreateUpdateParameters>();
        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = parallel
        };

        await Parallel.ForEachAsync(scenarios, parallelOptions, async (sc, token) =>
        {
            var testCase = await httpClient.CreateOrUpdateTestCase(projectName, sc);
            testCases.Add(CreateTestCaseWorkItem(testCase.Id.Value));
        });

        return testCases;
    }

    private static SuiteTestCaseCreateUpdateParameters CreateTestCaseWorkItem(int id)
        => new SuiteTestCaseCreateUpdateParameters()
        {
            workItem = new WorkItem()
            {
                Id = id
            }
        };

    private static async Task<int?> GetTestCaseByTag(WorkItemTrackingHttpClient httpClient, string projectName, string tag)
    {
        var wiql = new Wiql()
        {
            Query = "Select [Id] " +
        "From WorkItems " +
        "Where [Work Item Type] = 'Test Case' " +
        "And [Tags] Contains '" + tag + "' " +
        "And [System.TeamProject] = '" + projectName + "' ",
        };

        var result = await httpClient.QueryByWiqlAsync(wiql);
        var ids = result.WorkItems.Select(item => item.Id).ToArray();
        return ids.Length > 0 ? ids[0] : null;
    }

    public static JsonPatchDocument CreateTestCasePathDocument(ScenarioResult scenarioResult)
    {
        var title = $"{scenarioResult.Info.Parent.Name}:{scenarioResult.Info.Name}";
        var tagsList = new List<string> { };
        tagsList.AddRange(scenarioResult.Info.Labels);
        tagsList.Add(scenarioResult.Info.Code);
        var tags = string.Join(";", tagsList);
        string steps = GetSteps(scenarioResult.Steps);
        JsonPatchDocument patchDocument = new JsonPatchDocument();

        patchDocument.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = title
            }
        );

        patchDocument.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.Steps",
                Value = steps
            }
        );

        patchDocument.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.AssignedTo",
                Value = ""
            }
        );

        patchDocument.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Tags",
                Value = tags,
            }
        );

        if (scenarioResult.AutomatedTestName is not null)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.AutomatedTestName",
                Value = scenarioResult.AutomatedTestName,
            });
        }
        if (scenarioResult.AutomatedTestStorage is not null)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.AutomatedTestStorage",
                Value = scenarioResult.AutomatedTestStorage
            });
        }

        patchDocument.Add(new JsonPatchOperation
        {
            Operation = Operation.Add,
            Path = "/fields/Microsoft.VSTS.TCM.AutomatedTestId",
            Value = Guid.NewGuid().ToString()
        });

        patchDocument.Add(new JsonPatchOperation
        {
            Operation = Operation.Add,
            Path = "/fields/Microsoft.VSTS.TCM.AutomationStatus",
            Value = "Automated"
        });

        return patchDocument;
    }

    private static string GetSteps(IEnumerable<StepResult> stepResults)
    {
        var stepsBuilder = new StepsBuilder();

        stepResults.ToList().ForEach(step =>
        {
            var stepContent = new StringBuilder();
            stepContent.AppendLine(step.Info.Name);
            step.Comments.ForEach(comment => stepContent.AppendLine(comment));
            stepsBuilder.Add(stepContent.ToString());
        });

        var steps = stepsBuilder.Build();
        return steps;
    }
}
