using Manganese.Text;
using Microsoft.VisualBasic;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using System.Reactive.Linq;

// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
namespace MiraiEat
{
    class TextHeadle
    {
        public async Task<string[]?> GetWeekDayMenu()
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://api.shanwer.top/Dishes/weekdayDishes.txt");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                string[] WeekDayMenu = responseBody.Split("\r\n");
                return WeekDayMenu;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }
        public async Task<string[]?> GetWeekEndMenu()
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://api.shanwer.top/Dishes/weekendDishes.txt");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                string[] WeekDayMenu = responseBody.Split("\r\n");
                return WeekDayMenu;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }
        public async Task<string?> GetRandomMenu()
        {
            //HttpClient client = new HttpClient();
            //HttpResponseMessage response = await client.GetAsync("http://api.shanwer.top/Dishes/weekendDishes.txt");
            var date = DateTime.Now.DayOfWeek;
            if (date == DayOfWeek.Saturday || date == DayOfWeek.Sunday) 
            {
                var rand = new Random();
                var Menu = await GetWeekEndMenu();
                if(Menu != null)
                {
                    var num = rand.NextInt64(0, Menu.Length-2);
                    return Menu[num];
                }
            }
            else
            {
                var rand = new Random();
                var Menu = await GetWeekDayMenu();
                if (Menu != null)
                {
                    var num = rand.NextInt64(0, Menu.Length-2);
                    return Menu[num];
                }
            }
            return null;
        }
    }
    class MainApp
    {

        private static async Task Main(string[] args)
        {
            TextHeadle text = new TextHeadle();

            var bot = new MiraiBot
            {
                Address = "localhost:2253",
                //QQ = "2173254315",
                VerifyKey = "Duya25446"
            };

            await bot.LaunchAsync();
            Console.WriteLine("Loading completed!");
            bot.MessageReceived.OfType<FriendMessageReceiver>().Subscribe(async x =>
                {
                    Console.WriteLine($"收到了来自好友{x.FriendId}发送的消息：{x.MessageChain.GetPlainMessage()}");
                    if (x.MessageChain.GetPlainMessage().Contains("等下吃啥"))
                    {
                        var foud = await text.GetRandomMenu();
                        await x.SendMessageAsync($"推荐吃{foud}");
                    }
                });
            bot.MessageReceived
                .OfType<GroupMessageReceiver>()
                .Subscribe(async x =>
                {
                    Console.WriteLine($"收到了来自群{x.GroupId}由{x.Sender.Id}发送的消息：{x.MessageChain.GetPlainMessage()}");
                    if (x.MessageChain.GetPlainMessage().Contains("等下吃啥"))
                    {
                        var foud = await text.GetRandomMenu();
                        var messageChain = new MessageChainBuilder().At($"{x.Sender.Id}").Plain($" 推荐吃{foud}").Build();
                        await x.SendMessageAsync(messageChain);
                    }
                });
            while (true) ;
        }
    }
}
