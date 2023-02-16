using System.Text;

namespace AdoImportTestCases;

public class StepsBuilder
{
    StringBuilder builder;
    int counter;

    public StepsBuilder()
    {
        builder = new StringBuilder();
        builder.Append("<steps id=\"0\" last=\"1\">");
        counter = 1;
    }

    public StepsBuilder Add(string stepContent, string stepExpected = "")
    {
        counter++;
        var step = CreateStep(counter, stepContent, stepExpected);
        builder.Append(step);
        return this;
    }

    public string Build()
        => "<steps id=\"0\" last=\"1\">" +
              builder.ToString() +
           "</steps>";

    private static string CreateStep(int stepNumber, string stepContent, string stepExpected)
        => "<step id=\"" + stepNumber + "\" type=\"ValidateStep\">" +
                "<parameterizedString isformatted=\"true\">" +
                    stepContent +
                "</parameterizedString>" +
                "<parameterizedString isformatted=\"true\">" +
                    stepExpected +
                "</parameterizedString>" +
                "<description/>" +
            "</step>";
}
