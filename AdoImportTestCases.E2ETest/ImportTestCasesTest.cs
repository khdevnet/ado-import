namespace ImportTestCases.Test;

public class ImportTestCasesTest
{
    public const string OrganizationUrl = "https://dev.azure.com/vswebdeveloper";
    public const string Token = "use valid token";
    public const string ProjectName = "test";
    private const int TestPlanId = 5;
    private const string TestSuitePath = "auto\\api1";
    private const string TestCasesPath = "./testcases.json";

    [Fact]
    public async Task Program_Main()
    {
        var args = new[] {
            "--organizationUrl",  OrganizationUrl,
            "--patToken",  Token,
            "--projectName",  ProjectName,
            "--testPlanId",  TestPlanId.ToString(),
            "--testSuitePath",  TestSuitePath,
            "--importFilePath",  TestCasesPath,
        };

        Program.Main(args);
    }
}