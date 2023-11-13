// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using YarnAcknowledgementsGenerator;

var projectRoot = @"C:\Users\Gradyn Wursten\code\JournalyMobile";
var outputFile = "out.json";

string GenerateDisclaimer() 
{
    ProcessStartInfo startInfo = new ProcessStartInfo
    {
        FileName = @"C:\Program Files\nodejs\yarn.CMD",
        Arguments = "licenses generate-disclaimer",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using (Process process = Process.Start(startInfo))
    {
        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}

Directory.SetCurrentDirectory(projectRoot);

var disclaimer = GenerateDisclaimer();

var output = new List<OutputObj>();

var toProcess = disclaimer.Split("\n-----\n\n").Where(x => x.StartsWith("The following software may be included in this product")).ToArray();;

Console.WriteLine($"Going to process {toProcess.Length} records");

foreach (var record in toProcess)
{
    output.Add(new OutputObj
    {
        Name = string.Join(", ", new Regex(@"(?<=The following software may be included in this product: )(.*?)(?=[.]\sA copy)").Matches(record.Split("\n")[0]).Select(x => x.Value).ToArray()),
        SourceLinks = new Regex(@"https(.*?)(?=\.git)").Matches(record.Split("\n")[0]).Select(x => x.Value).Distinct().ToArray(),
        License = string.Join("\n", record.Split("\n").Skip(2))
    });
}

using (StreamWriter outputFileRef = new StreamWriter(outputFile))
{
    outputFileRef.Write(JsonSerializer.Serialize(output));
}