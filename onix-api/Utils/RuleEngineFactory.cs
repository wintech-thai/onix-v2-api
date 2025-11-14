using RulesEngine.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class RuleEngineFactory
{
    public static RulesEngine.RulesEngine CreateEngineFromYaml(string yamlText)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        // แปลงจาก YAML → Workflow[]
        var workflows = deserializer.Deserialize<List<Workflow>>(yamlText).ToArray();

        // สร้าง RulesEngine instance
        return new RulesEngine.RulesEngine(workflows, null);
    }
}
