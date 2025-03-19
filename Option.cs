using CommandLine;
public class Options
{

    [Option('t', "tablename", HelpText = "表名.", Default = "")]
    public string TableName { get; set; } = null!;
    [Option('o', "outdir", HelpText = "生成目录", Default = "./Models")]
    public string OutDir { get; set; } = null!;

    [Option('c', "config", HelpText = "配置文件", Default = "./config.json")]
    public string Config { get; set; } = null!;
}