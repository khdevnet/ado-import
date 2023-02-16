using AdoImportTestCases;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.CommandLine;
using System.CommandLine.Invocation;

public class Program
{
    public static void Main(string[] args)
    {
        var organizationUrl = new Option<string>("--organizationUrl", description: "Azure organization url") { IsRequired = true };
        var patToken = new Option<string>("--patToken", description: "Azure pat token") { IsRequired = true };
        var projectName = new Option<string>("--projectName", description: "Azure project name") { IsRequired = true };
        var testPlanId = new Option<int>("--testPlanId", description: "Azure test planId") { IsRequired = true };
        var testSuitePath = new Option<string>("--testSuitePath", description: "Azure project test suite path") { IsRequired = true };
        var importFilePath = new Option<string>("--importFilePath", description: "Path to test cases list in json") { IsRequired = true };
        var maxParallelism = new Option<int>("--maxParallelism", () => 5, description: "Process test cases parallel") { IsRequired = true };

        var rootCommand = new RootCommand
            {
                organizationUrl,
                patToken,
                projectName,
                testPlanId,
                testSuitePath,
                importFilePath,
                maxParallelism,
            };

        rootCommand.Description = "A app for automatically associate automated tests with test cases cli.";

        rootCommand.Handler = CommandHandler.Create((
            string organizationUrl,
            string patToken,
            string projectName,
            int testPlanId,
            string testSuitePath,
            string importFilePath,
            int maxParallelism) =>
        {
            ImportTestCases(organizationUrl, patToken, projectName, testPlanId, testSuitePath, importFilePath, maxParallelism).GetAwaiter().GetResult();
        });

        rootCommand.Invoke(args);
    }

    private static async Task ImportTestCases(
         string projectUrl,
         string patToken,
         string projectName,
         int testPlanId,
         string testSuitePath,
         string importFilePath,
         int maxParallelism)
    {

        var features = new FeaturesJsonReader().Read(importFilePath);

        var orgUrl = new Uri(projectUrl);
        var cred = new VssBasicCredential(string.Empty, patToken);
        var connection = new VssConnection(orgUrl, cred);

        var workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
        var testPlanClient = connection.GetClient<TestPlanHttpClient>();
        int testSuiteId = testPlanClient.GetSuiteId(projectName, testPlanId, testSuitePath);

        await testPlanClient.RemoveTestCasesFromSuite(projectName, testPlanId, testSuiteId);

        var scenarios = features.SelectMany(f => f.Scenarios);

        var suiteTestCaseCreateUpdate = await workItemClient.GetTestCasesAsync(projectName, scenarios, maxParallelism);

        var res = await testPlanClient.AddTestCasesToSuiteAsync(suiteTestCaseCreateUpdate, projectName, testPlanId, testSuiteId);
    }
}


