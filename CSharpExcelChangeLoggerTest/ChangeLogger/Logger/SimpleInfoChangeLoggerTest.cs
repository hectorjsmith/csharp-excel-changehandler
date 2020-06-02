﻿using CSharpExcelChangeHandler.ChangeHandling.Handler;
using CSharpExcelChangeHandler.ChangeHandling.Logger;
using CSharpExcelChangeHandlerTest.Mock;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpExcelChangeHandlerTest.ChangeLogger.Logger
{
    class SimpleInfoChangeLoggerTest
    {
        [Test]
        public void Given_SimpleChangeLogger_When_HandleChangeMethodCalled_Then_ChangeIsLogged()
        {
            TestLogger logger = new TestLogger();
            
            IChangeHandler highlighter = new SimpleInfoChangeLogger(logger);
            highlighter.HandleChange(new SimpleMockMemoryComparison(), new SimpleMockSheet(), new SimpleMockRange());

            Assert.AreEqual(1, logger.InfoMessageCount, "One info message should be logged for a change");
        }
    }
}
