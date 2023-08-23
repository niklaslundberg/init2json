using System.Dynamic;
using System.Text;
using IniParser;
using Newtonsoft.Json;

if (args.Length == 0)
{
    Console.WriteLine("Usage: sourcefile.ini targetfile.json");
    return 1;
}

string sourceFilePath = args[0];

try
{
    if (!File.Exists(sourceFilePath))
    {
        Console.WriteLine($"File '{sourceFilePath}' does not exist");
        return 1;
    }

    var parser = new FileIniDataParser();
    var iniFile = parser.ReadFile(sourceFilePath);

    ExpandoObject topLevel = new();
    foreach (var iniFileSection in iniFile.Sections)
    {
        var asDictionary = (IDictionary<string, object?>)topLevel;
        var section = new ExpandoObject();
        var sectionAsDictionary = (IDictionary<string, object?>)section;

        foreach (var key in iniFileSection.Keys)
        {
            if (int.TryParse(key.Value, out int asInt))
            {
                sectionAsDictionary[key.KeyName] = asInt;
            }
            else if (bool.TryParse(key.Value, out bool asBool))
            {
                sectionAsDictionary[key.KeyName] = asBool;
            }
            else
            {
                sectionAsDictionary[key.KeyName] = key.Value;
            }
        }

        asDictionary[iniFileSection.SectionName] = sectionAsDictionary;
    }

    string asJson = JsonConvert.SerializeObject(topLevel, Formatting.Indented);

    Console.WriteLine(asJson);

    if (args.Length > 1)
    {
        await File.WriteAllTextAsync(args[1], asJson, Encoding.UTF8);
    }

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);

    return 1;
}