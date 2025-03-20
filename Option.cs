using CommandLine;
public class Options
{

    [Option('t', "tablename", HelpText = "表名", Default = "")]
    public string TableName { get; set; } = null!;
    [Option('o', "outdir", HelpText = "生成目录", Default = "./Models")]
    public string OutDir { get; set; } = null!;

    [Option('c', "config", HelpText = "配置文件 格式{\"connstr\":\"server=127.0.0.1;port=3306;Database=test;Uid=root;Pwd=123456\",\"dbtype\":1}。", Default = ".SqlSugarTool/config.json")]
    public string Config { get; set; } = null!;

    [Option('s', "showconfig", HelpText = "显示配置文件内容", Default = false)]
    public bool ShowConfig { get; set; }

    [Option('a', "alias", HelpText = "配置别名", Default = "default")]
    public string Alias { get; set; } = null!;
}