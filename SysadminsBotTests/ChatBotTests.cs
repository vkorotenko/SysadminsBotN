using Microsoft.VisualStudio.TestTools.UnitTesting;
using SysadminsBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysadminsBot.Tests
{
    [TestClass()]
    public class ChatBotTests
    {
        [TestMethod()]
        public void UTF8ToWin1251Test()
        {
            var loc = new LocalDeepSeek();
            var result = loc.Reply("Проверка сети").Result;
        }
    }
}