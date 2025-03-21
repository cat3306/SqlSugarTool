using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using System.Text.Json;
public class Tool
{

    public struct ConfigItem
    {
        [JsonPropertyName("connstr")]
        public string ConnStr { get; set; }

        [JsonPropertyName("dbtype")]
        public int DbType { get; set; }
    }
    public static void Run(Options options)
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (options.ShowConfig)
        {
            var str = File.ReadAllText($"{homeDir}/{options.Config}");
            Console.WriteLine(str);
            Console.WriteLine($"path:{homeDir}/{options.Config}");
            return;
        }

        var config = new ConfigItem { };
        try
        {
            var list = JsonSerializer.Deserialize<Dictionary<string, ConfigItem>>(File.ReadAllText($"{homeDir}/{options.Config}"));
            config = list?.Where(it => it.Key == options.Alias).FirstOrDefault(new KeyValuePair<string, ConfigItem>("default", config)).Value ?? config;
        }
        catch (Exception ex)
        {
            Console.WriteLine("err:" + ex.Message);
            return;
        }
        if (config.ConnStr.IsNullOrEmpty())
        {
            Console.WriteLine($"未找到相应的配置别名{options.Alias}，检查配置文件和参数是否正确。");
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
        try
        {
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
        catch (Exception ex)
        {
            Console.WriteLine("err:" + ex.Message);
        }

    }
}