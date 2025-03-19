using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using System.Text.Json;
public class Tool
{
    public struct Config
    {
        [JsonPropertyName("connstr")]
        public string ConnStr { get; set; }

        [JsonPropertyName("dbtype")]
        public int DbType { get; set; }
    }
    public static void Run(Options options)
    {
        var config = new Config();
        try
        {
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(options.Config));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        SqlSugarScope sqlSugarScope = new SqlSugarScope(new ConnectionConfig()
        {
            DbType = DbType.MySql,
            ConnectionString = config.ConnStr,
            IsAutoCloseConnection = true,
        }
   );

        if (!options.TableName.IsNullOrEmpty() && !sqlSugarScope.DbMaintenance.IsAnyTable(options.TableName, false))
        {
            Console.WriteLine($"表 {options.TableName} 不存在。");
            return;
        }

        Func<string, string> formatFunc = it =>
         {
             var lets = it.Split('_');
             return string.Join("", lets.Select(l => l[..1].ToUpper() + l[1..]));
         };

        var db = sqlSugarScope.DbFirst.StringNullable();
        if (options.TableName.IsNullOrEmpty())
        {
            db.FormatPropertyName(formatFunc).IsCreateAttribute().FormatClassName(formatFunc).CreateClassFile(options.OutDir);
        }
        else
        {

            db.Where(options.TableName).FormatPropertyName(formatFunc).IsCreateAttribute().FormatClassName(formatFunc).CreateClassFile(options.OutDir);
        }
    }
}