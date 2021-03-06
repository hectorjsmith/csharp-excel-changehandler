﻿using ExcelChangeHandler.Api.Config;
using ExcelChangeHandler.Base;
using ExcelChangeHandler.ChangeHandling.Factory;
using ExcelChangeHandler.ChangeHandling.Handler;
using ExcelChangeHandler.ChangeHandling.Processor;
using ExcelChangeHandler.Excel;
using ExcelChangeHandler.Excel.Cached;
using ExcelChangeHandler.Logging;
using System;

namespace ExcelChangeHandler.Api
{
    public class GenericChangeHandlerApi<TWorksheetType, TRangeType> : IGenericChangeHandlerApi<TWorksheetType, TRangeType>
        where TWorksheetType : IWorksheet where TRangeType : IRange
    {
        private const int DEFAULT_HIGHLIGHT_COLOUR = 65535;

        private readonly ILoggingManager _loggingManager = new LoggingManager();

        private IChangeProcessor<TWorksheetType, TRangeType>? _changeProcessor;
        private IChangeProcessor<TWorksheetType, TRangeType> ChangeProcessor =>
            _changeProcessor ?? (_changeProcessor = NewActiveChangeProcessor());

        private IChangeHandlerFactory<TWorksheetType, TRangeType>? _changeHandlerFactory;
        public IChangeHandlerFactory<TWorksheetType, TRangeType> ChangeHandlerFactory =>
            _changeHandlerFactory ?? (_changeHandlerFactory = NewSimpleChangeHandlerFactory());

        public IConfiguration Configuration { get; } = new Configuration();

        private bool ChangeHandlingEnabled => Configuration.ChangeHandlingEnabled;

        internal GenericChangeHandlerApi()
        {
        }

        public void SetApplicationLogger(ILogger? logger)
        {
            _loggingManager.SetLogger(logger);
        }

        public void ClearAllHandlers()
        {
            ChangeProcessor.ClearAllHandlers();
        }

        public void AddDefaultHandlers()
        {
            AddCustomHandler(ChangeHandlerFactory.NewSimpleChangeHighlighter(DEFAULT_HIGHLIGHT_COLOUR));
        }

        public void AddCustomHandler(IChangeHandler<TWorksheetType, TRangeType> handler)
        {
            ChangeProcessor.AddHandler(handler);
        }

        public void BeforeChange(TWorksheetType sheet, TRangeType range)
        {
            if (ChangeHandlingEnabled)
            {
                ChangeProcessor.BeforeChange(sheet, range);
            }
        }

        public void AfterChange(TWorksheetType sheet, TRangeType range)
        {
            if (ChangeHandlingEnabled)
            {
                ChangeProcessor.AfterChange(sheet, range);
            }
        }

        private IChangeProcessor<TWorksheetType, TRangeType> NewActiveChangeProcessor()
        {
            return new ActiveChangeProcessor<TWorksheetType, TRangeType>(_loggingManager, Configuration);
        }

        private IChangeHandlerFactory<TWorksheetType, TRangeType> NewSimpleChangeHandlerFactory()
        {
            return new SimpleChangeHandlerFactory<TWorksheetType, TRangeType>(_loggingManager);
        }
    }
}
