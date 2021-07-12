using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Diagnostics;
using System.Threading;

namespace FactoryChallenge
{
    public partial class FactoryChallenge : Form
    {
        public class Driver
        {
            public ChromeDriver driver;
            public bool stop = false;
            public bool stopComplite = true;
        }
        protected ChromeDriverService _driverService = null;
        protected ChromeOptions _options = null;
        protected List<Driver> _driverList = new List<Driver>();
        

        public FactoryChallenge()
        {
            InitializeComponent();

            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;

            _options = new ChromeOptions();
            _options.AddArgument("disable-gpu");
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            var _driver = new ChromeDriver(_driverService, _options);
            _driverList.Add(new Driver() { driver = _driver });

            _driver.Navigate().GoToUrl("https://nid.naver.com/nidlogin.login?url=https://section.cafe.naver.com/");
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            foreach (var item in _driverList)
            {
                new Thread(() =>
                {
                    int i = 0;
                    ChromeDriver tempDriver = item.driver;
                    WebDriverWait wait = new WebDriverWait(tempDriver, TimeSpan.FromSeconds(10));
                    tempDriver.Navigate().GoToUrl("https://cafe.naver.com/factory1");

                    tempDriver.FindElementByClassName("cafe-menu-list");
                    var menu = tempDriver.FindElementByXPath("//*[@id=\"menuLink1\"]");
                    menu.Click();
                    item.stopComplite = false;
                    while (item.stop == false)
                    {
                        if (i > 100)
                        {
                            i = 0;
                        }
                        menu = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"cafe-info-data\"]/div[3]/a")));
                        menu.Click();
                        if (tempDriver.WindowHandles.Count > 1)
                        {
                            tempDriver.SwitchTo().Window(tempDriver.WindowHandles[1]);
                            Thread.Sleep(500);
                            if (tempDriver.Title.IndexOf("글쓰기") == -1)
                            {
                                tempDriver.Close();
                                tempDriver.SwitchTo().Window(tempDriver.WindowHandles.First());
                            }
                            else
                            {
                                menu = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"app\"]/div/div/section/div/div[2]/div[1]/div[1]/div[2]/div/textarea")));
                                menu.Click();
                                menu.SendKeys(i.ToString());
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("se-content-guide")));
                                menu.SendKeys(OpenQA.Selenium.Keys.Tab + i.ToString());
                                menu = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"app\"]/div/div/section/div/div[1]/div/a/span")));
                                menu.Click();
                                Thread.Sleep(500);
                                var alert = SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent().Invoke(tempDriver);
                                if (alert != null)
                                {
                                    alert.Accept();
                                    alert = SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent().Invoke(tempDriver);
                                    alert.Accept();
                                    tempDriver.Close();
                                    tempDriver.SwitchTo().Window(tempDriver.WindowHandles.First());
                                }
                                else
                                {
                                    tempDriver.Close();
                                    tempDriver.SwitchTo().Window(tempDriver.WindowHandles.First());
                                    i++;
                                }
                            }
                        }
                    }
                    item.stopComplite = true;
                })
                { IsBackground = true }.Start();
                Thread.Sleep(50);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Application.DoEvents();
        }

        private void FactoryChallenge_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var item in _driverList)
            {
                item.stop = true;
                new Thread(()=> 
                {
                    while (true)
                    {
                        if (item.stopComplite == true)
                        {
                            item.driver.Quit();
                            break;
                        }
                        Thread.Sleep(50);
                    }
                }).Start();
                
            }
        }
    }
}
