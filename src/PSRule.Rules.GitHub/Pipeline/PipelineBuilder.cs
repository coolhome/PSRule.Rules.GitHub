﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using PSRule.Rules.GitHub.Configuration;
using PSRule.Rules.GitHub.Pipeline.Output;

namespace PSRule.Rules.GitHub.Pipeline
{
    internal delegate bool ShouldProcess(string target, string action);

    /// <summary>
    /// A helper class for building a pipeline from PowerShell.
    /// </summary>
    public static class PipelineBuilder
    {
        public static IExportPipelineBuilder Export(PSRuleOption option)
        {
            return new ExportPipelineBuilder(option);
        }
    }

    public interface IPipelineBuilder
    {
        void UseCommandRuntime(PSCmdlet commandRuntime);

        void UseExecutionContext(EngineIntrinsics executionContext);

        IPipelineBuilder Configure(PSRuleOption option);

        IPipeline Build();
    }

    public interface IPipeline
    {
        void Begin();

        void Process(PSObject sourceObject);

        void End();
    }

    internal abstract class PipelineBuilderBase : IPipelineBuilder
    {
        private readonly PSPipelineWriter _Output;

        protected readonly PSRuleOption Option;

        protected PSCmdlet CmdletContext;
        protected EngineIntrinsics ExecutionContext;

        protected PipelineBuilderBase()
        {
            Option = new PSRuleOption();
            _Output = new PSPipelineWriter(Option);
        }

        public virtual void UseCommandRuntime(PSCmdlet commandRuntime)
        {
            CmdletContext = commandRuntime;
            _Output.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            ExecutionContext = executionContext;
            _Output.UseExecutionContext(executionContext);
        }

        public virtual IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Output = new OutputOption(option.Output);
            return this;
        }

        public abstract IPipeline Build();

        protected PipelineContext PrepareContext()
        {
            return new PipelineContext(Option);
        }

        protected virtual PipelineWriter PrepareWriter()
        {
            var writer = new PSPipelineWriter(Option);
            writer.UseCommandRuntime(CmdletContext);
            writer.UseExecutionContext(ExecutionContext);
            return writer;
        }

        protected virtual PipelineWriter GetOutput()
        {
            return _Output;
        }
    }

    internal abstract class PipelineBase : IDisposable, IPipeline
    {
        protected readonly PipelineContext Context;
        protected readonly PipelineWriter Writer;

        // Track whether Dispose has been called.
        private bool _Disposed = false;


        protected PipelineBase(PipelineContext context, PipelineWriter writer)
        {
            Context = context;
            Writer = writer;
        }

        #region IPipeline

        public virtual void Begin()
        {
            // Do nothing
        }

        public virtual void Process(PSObject sourceObject)
        {
            // Do nothing
        }

        public virtual void End()
        {
            Writer.End();
        }

        #endregion IPipeline

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    // Do nothing
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
