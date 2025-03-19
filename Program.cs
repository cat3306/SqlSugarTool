using SqlSugar;
using CommandLine;

Parser.Default.ParseArguments<Options>(args)
             .WithParsed(Tool.Run);
