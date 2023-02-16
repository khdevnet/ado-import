using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace AdoImportTestCases;

public static class TestPlanClientExtensions
{
    public static int GetSuiteId(this TestPlanHttpClient TestPlanClient, string TeamProjectName, int TestPlanId, string SuitePath)
    {
        TestPlan testPlan = TestPlanClient.GetTestPlanByIdAsync(TeamProjectName, TestPlanId).Result;
        if (SuitePath == "") return testPlan.RootSuite.Id;

        List<TestSuite> testPlanSuites = TestPlanClient.GetTestSuitesForPlanAsync(TeamProjectName, TestPlanId, SuiteExpand.Children, asTreeView: true).Result;

        string[] pathArray = SuitePath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        TestSuite suiteMarker = testPlanSuites[0]; //first level is the root suite

        for (int i = 0; i < pathArray.Length; i++)
        {
            suiteMarker = (from ts in suiteMarker.Children where ts.Name == pathArray[i] select ts).FirstOrDefault();

            if (suiteMarker == null) return 0;

            if (i == pathArray.Length - 1) return suiteMarker.Id;
        }

        return 0;
    }

    public static async Task RemoveTestCasesFromSuite(this TestPlanHttpClient testPlanClient, string projectName, int testPlanId, int testSuiteId)
    {
        var existTestCases = await testPlanClient.GetTestCaseListAsync(projectName, testPlanId, testSuiteId);

        if (existTestCases.Any())
        {
            await testPlanClient.RemoveTestCasesFromSuiteAsync(projectName, testPlanId, testSuiteId, string.Join(",", existTestCases.Select(x => x.workItem.Id)));
        }
    }

}
