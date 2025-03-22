using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using System.Text.Json;
using System.Text;
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
        //var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dirTuple = Options.DefaultDir();
        if (options.ShowConfig)
        {
            if (!File.Exists(dirTuple.Item2))
            {
                Console.WriteLine($"配置文件不存在。初始化配置文件。{dirTuple.Item2}");
                if (!Directory.Exists(dirTuple.Item1))
                    Directory.CreateDirectory(dirTuple.Item1);
                try
                {
                    var file = File.Create(dirTuple.Item2);
                    var json = JsonSerializer.Serialize(new Dictionary<string, ConfigItem>() { { "default", new ConfigItem { ConnStr = "server=127.0.0.1;port=3306;Database=test;Uid=root;Pwd=123456", DbType = 1 } } });
                    file.Write(Encoding.UTF8.GetBytes(json));
                    file.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("err:" + ex.Message);
                    return;
                }
            }
            var str = File.ReadAllText(dirTuple.Item2);
            Console.WriteLine(str);
            Console.WriteLine($"path:{Options.DefaultDir().Item2}");
            return;
        }

        var config = new ConfigItem { };
        try
        {
            var list = JsonSerializer.Deserialize<Dictionary<string, ConfigItem>>(File.ReadAllText(Options.DefaultDir().Item2 + options.Config));
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