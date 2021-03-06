﻿using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using Selenium.PageObject.Element;
using Selenium.Utils;

namespace Selenium.PageObject
{
    class PageObject<T> where T : IPageElement
    {
        protected static readonly TimeSpan DEFAULT_TIMEOUT = Properties.Selenium.Default.DefaultTimeout;
        protected static readonly TimeSpan ALERT_TIMEOUT = Properties.Selenium.Default.AlertTimeout;
        protected static readonly string URL = Properties.Selenium.Default.Url;
        protected readonly IWebDriver SESSION;

        public PageObject(IWebDriver session)
        {
            this.SESSION = session;
        }

        private IWebElement GetWebElement(T element, params string[] placeholders)
        {
            return new WebDriverWait(SESSION, DEFAULT_TIMEOUT).Until(
                ExpectedConditions.ElementToBeClickable(element.GetBy(placeholders)));
        }

        private void SendKeys(IWebElement element, string text)
        {
            element.Clear();
            element.SendKeys(text);
        }

        private void AdjustCheckboxState(T element, bool state, params string[] placeholders)
        {
            IWebElement checkbox = GetWebElement(element, placeholders);
            if (checkbox.Selected != state)
            {
                checkbox.Click();
            }
        }

        private void SetSelectedOption(T element, String visibleText, params string[] placeholders)
        {
            SelectElement selectBox = new SelectElement(GetWebElement(element, placeholders));
            IList<IWebElement> options = selectBox.Options;
            List<string> optionsTexts = new List<string>();

            bool valueAvailable = false;
            foreach (IWebElement option in options)
            {
                optionsTexts.Add(option.Text);
                if (option.Text.Equals(visibleText)) {
                    valueAvailable = true;
                    break;
                }
            }

            if (!valueAvailable) {
                throw new SeleniumTestException("There is no option with visible text equal to {0} in drop down. Visible options: {1}.",
                    visibleText, optionsTexts.ToString());
            }

            selectBox.SelectByText(visibleText);
        }

        private void WaitForPageToLoad(IWebElement controlElement)
        {
            new WebDriverWait(SESSION, DEFAULT_TIMEOUT).Until(ExpectedConditions.StalenessOf(controlElement));
        }

        private IAlert WaitGetAlert()
        {
            IAlert alert = null;
            WebDriverWait wait = new WebDriverWait(SESSION, ALERT_TIMEOUT);

            try
            {
                alert = wait.Until(d =>
                {
                    try
                    {
                        return SESSION.SwitchTo().Alert();
                    }
                    catch (NoAlertPresentException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                alert = null;
            }

            return alert;
        }

        protected void WaitForElementToBeVisible(T element, params string[] placeholders)
        {
            new WebDriverWait(SESSION, DEFAULT_TIMEOUT).Until(
                ExpectedConditions.ElementIsVisible(element.GetBy(placeholders)));
        }

        protected void WaitForElementToBeNotVisible(T element, params string[] placeholders)
        {
            new WebDriverWait(SESSION, DEFAULT_TIMEOUT).Until(
                ExpectedConditions.InvisibilityOfElementLocated(element.GetBy(placeholders)));
        }

        protected bool IsEnabled(T element, params string[] placeholders)
        {
            return GetWebElement(element, placeholders).Enabled;
        }

        protected bool IsVisible(T element, params string[] placeholders)
        {
            return GetWebElement(element, placeholders).Displayed;
        }

        protected string GetText(T element, params string[] placeholders)
        {   
            return GetWebElement(element, placeholders).Text;
        }

        protected void SetSelectedOption(T element, IDropDownValue value, params string[] placeholders)
        {
            SetSelectedOption(element, value.Value, placeholders);
        }

        protected string GetSelectedOption(T element, params string[] placeholders)
        {
            return new SelectElement(GetWebElement(element, placeholders)).SelectedOption.Text;
        }

        public void Click(T element, params string[] placeholders)
        {
            GetWebElement(element, placeholders).Click();
        }

        public void ClickAndWaitForPageToLoad(T element, params string[] placeholders)
        {
            IWebElement controlElement = GetWebElement(element, placeholders);
            Click(element);
            WaitForPageToLoad(controlElement);
        }

        public void SendKeys(T element, string text, params string[] placeholders)
        {
            SendKeys(GetWebElement(element, placeholders), text);
        }

        public void PressTabKey(T element, params string[] placeholders)
        {
            GetWebElement(element, placeholders).SendKeys(Keys.Tab);
        }

        public void Clear(T element, params string[] placeholders)
        {
            GetWebElement(element, placeholders).Clear();
        }

        public void TickCheckbox(T element, params string[] placeholders)
        {
            AdjustCheckboxState(element, true, placeholders);
        }

        public void UntickCheckbox(T element, params string[] placeholders)
        {
            AdjustCheckboxState(element, false, placeholders);
        }

        public void ShouldHave(T element, params string[] placeholders)
        {
            GetWebElement(element, placeholders);
        }

        public void ShouldHave(T element, IPlaceholdersProvider placeholdersProvider)
        {
            ShouldHave(element, placeholdersProvider.Placeholders);
        }

        public void ShouldHaveEnabled(T element, params string[] placeholders)
        {
            if (!IsEnabled(element, placeholders)) {
                throw new SeleniumTestException("Element located by {0} = '{1}' is not enabled.", element.Type, element.Expression);
            }
        }

        public void CloseAlert(string alertMessage)
        {
            IAlert alert = WaitGetAlert();
            if (alert.Text.Equals(alertMessage)) {
                alert.Accept();
            } else {
               throw new SeleniumTestException("Expected alert with '{0}' message but got '{1}' message.", alertMessage, alert.Text);
            }
        }

        public void NoAlertPresent()
        {
            IAlert alert = WaitGetAlert();
            if (alert == null)
            {
                return;
            }
            throw new SeleniumTestException("Alert with '{0}' message appeared.", alert.Text);
        }
    }
}
