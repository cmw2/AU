// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This sample happens to use .NET Core, but you can use whichever .NET version makes sense for your project.
// Everything we're demonstrating would also work in .NET Framework 4.5+ with no modifications.
using System;
using System.IO;
// This sample happens to use MSTest, but you can use whichever test framework you like.
// Everything we're demonstrating would also work with xUnit, NUnit, or any other test framework.
using Microsoft.VisualStudio.TestTools.UnitTesting;
// If you're using Selenium already, you're probably already using these.
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
// These are the important new libraries we're demonstrating.
// You'll probably need to add new NuGet package references for these.
using Selenium.Axe;
using FluentAssertions;

namespace AccCheckTests
{
    [TestClass]
    public class AccTest
    {

        private static IWebDriver _webDriver;
        private static readonly string _chromePath;
        private static readonly string _beforePage;
        private static readonly string _afterPage;

        static AccTest()
        {
            // should get these from config system, reading directly from env variables for simplicity at the moment
            _chromePath = Environment.GetEnvironmentVariable("ChromeWebDriver");

            _beforePage = Environment.GetEnvironmentVariable("BeforePage");
            if (String.IsNullOrEmpty(_beforePage))
            {
                _beforePage = "https://cmwaccinssite.z20.web.core.windows.net/before.html";
            }

            _afterPage = Environment.GetEnvironmentVariable("AfterPage");
            if (String.IsNullOrEmpty(_afterPage))
            {
                _afterPage = "https://cmwaccinssite.z20.web.core.windows.net/after.html";
            }

        }

        [TestMethod]
        public void TestAccessibilityOfBeforePage()
        {
            _webDriver.Navigate().GoToUrl(_beforePage);

            new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10))
                .Until(d => d.FindElement(By.Id("main")));

            CheckAccessibility();
        }

        [TestMethod]
        public void TestAccessibilityOfAfterPage()
        {
            _webDriver.Navigate().GoToUrl(_afterPage);

            new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10))
                .Until(d => d.FindElement(By.TagName("main")));

            CheckAccessibility();
        }

        private void CheckAccessibility()
        {
            AxeResult axeResult = new AxeBuilder(_webDriver).Analyze();
            axeResult.Violations.Should().BeEmpty();
        }


        [ClassInitialize]
        public static void StartBrowser(TestContext testContext)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");

            if (String.IsNullOrEmpty(_chromePath))
            {
                _webDriver = new ChromeDriver(options);
            }
            else
            {
                _webDriver = new ChromeDriver(_chromePath, options);
            }

            // You *must* set this timeout to use Selenium.Axe. It defaults to "0 seconds", which isn't enough time for
            // Axe to scan the page. The exact amount of time will depend on the complexity of the page you're testing.
            _webDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(20);
        }

        // It's important to remember to clean up the browser and webdriver; otherwise, the browser process will remain
        // alive after your tests are done, which can cause confusing errors during later builds and/or test sessions.
        [ClassCleanup]
        public static void StopBrowser()
        {
            _webDriver?.Quit();
        }

    }
}

