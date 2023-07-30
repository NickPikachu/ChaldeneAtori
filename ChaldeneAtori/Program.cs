using Chaldene.Sessions;
using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ChaldeneAtori
{
    class Program
    {
        static async Task Main()
        {
            // 获取当前程序根目录
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //  创建配置文件
            string configPath = Path.Combine(rootDirectory, "config.yml");
            if (!File.Exists(configPath))
            {
                using StreamWriter writer = new StreamWriter(configPath);
                writer.WriteLine("# config.yml");
                writer.WriteLine("# address: 连接链接");
                writer.WriteLine("# verifyKey: 登录密钥");
                writer.WriteLine("# qq: 登录账号");
            }

            // 读取配置内容并设置局部变量
            using StreamReader reader = new StreamReader(configPath);
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);

            var rootNode = (YamlMappingNode)yamlStream.Documents[0].RootNode;
            var addressNode = (YamlScalarNode)rootNode["address"];
            var verifyKeyNode = (YamlScalarNode)rootNode["verifyKey"];
            var qqNode = (YamlScalarNode)rootNode["qq"];

            string address = addressNode?.Value ?? string.Empty;
            string verifyKey = verifyKeyNode?.Value ?? string.Empty;
            string qq = qqNode?.Value ?? string.Empty;

            // 连接 Mirai
            Console.WriteLine("ChaldeneAtori v1.0.0 Alpha");
            Console.WriteLine($"正在透过 {address} 连接 mirai-api-http 中...");
            Console.WriteLine($"账户 {qq} 连接已完成！");
            var bot = new MiraiBot(address, verifyKey, qq);
            await bot.LaunchAsync();
            Console.WriteLine("已成功连接至Mirai");

            bot.Disconnected += async (sender, args) =>
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("mirai-api-http 连接断开, 正在尝试重连...");
                        await sender.LaunchAsync();
                        break;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            };

            bot.GroupMessageReceived += (sender, args) =>
            {
                Console.WriteLine($"接收到消息: {args.MessageChain.GetPlainMessage()}");
                string plainMessage = args.MessageChain.GetPlainMessage();
                if (plainMessage.StartsWith(".ban"))
                {
                    // 去除获取的消息的前5个字符，即去除 ".ban " 保留QQ账户
                    string numberString = plainMessage[4..]; 
                    if (int.TryParse(numberString, out int number))
                    {
                        string BannedNumber = args.MessageChain.();
                        bot.SendGroupMessageAsync(BannedNumber, $"已全局封禁 {number} 账户！");
                    }
                    else
                    {
                        // 如果提取账户失败，执行下方代码
                        bot.SendGroupMessageAsync(233656, $"未发现");
                    }

            };

        }
    }
}
