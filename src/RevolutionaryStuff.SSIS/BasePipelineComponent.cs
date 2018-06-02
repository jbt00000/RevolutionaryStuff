using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>https://www.simple-talk.com/sql/ssis/developing-a-custom-ssis-source-component/</remarks>
    /// <remarks>https://docs.microsoft.com/en-us/sql/integration-services/extending-packages-custom-objects-data-flow-types/developing-a-custom-transformation-component-with-synchronous-outputs</remarks>
    public abstract class BasePipelineComponent : PipelineComponent
    {
        internal const int AssemblyComponentVersion = 4;
        internal const int EnUsCodePage = 1252;


        protected static class CommonPropertyNames
        {
            public const string IgnoreCase = "IgnoreCase";
            public const string OutputColumnName = "OutputColumnName";
        }

        protected int GetCustomPropertyAsInt(string propertyName, int fallback = 0)
            => Parse.ParseInt32(GetCustomPropertyAsString(propertyName), fallback);

        protected bool GetCustomPropertyAsBool(string propertyName, bool fallback = false)
            => Parse.ParseBool(GetCustomPropertyAsString(propertyName), fallback);

        protected string GetCustomPropertyAsString(string propertyName, string fallback = null)
        {
            try
            {
                var p = ComponentMetaData.CustomPropertyCollection[propertyName];
                if (p != null)
                {
                    return (string)p.Value;
                }
            }
            catch (Exception)
            { }
            return fallback;
        }

        private enum BasePipelineComponentCodes
        {
            Error,
            ColumnMappingSuccess,
            ColumnMappingError,
            InvalidBufferId,
            ComponentClaimsSyncButOutputsIndicateOtherwise,
            TraceRegionStart,
            TraceRegionEnd,
            MethodResult,
        };

        protected const int SampleSize = 25;

        protected IDTSCustomProperty100 CreateCustomProperty(string name, string defaultValue, string description)
        {
            var p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = name;
            p.Description = description;
            p.Value = defaultValue;
            return p;
        }

        private enum BasePipelineInfoMessages
        {
            WaitingForDebuggerAttachment,
        }


        protected int StatusNotifyIncrement = 1000;

        private bool DebuggerAttachmentWaitDone;

        private static bool DebuggerAttachmentWaitDone_s = false;

        protected void DebuggerAttachmentWait()
        {
            lock (this)
            {
                DebuggerAttachmentWaitDone_s = DebuggerAttachmentWaitDone_s && true; //to get rid of compiler warning and worse yet, optimizations...
                if (!DebuggerAttachmentWaitDone)
                {
#if false
                    for (int z = 0; z < 60 && !DebuggerAttachmentWaitDone_s; ++z)
                    {
                        System.Threading.Thread.Sleep(1000);
                        FireInformation(BasePipelineInfoMessages.WaitingForDebuggerAttachment, $"{this.GetType().Name} {z}/60");
                    }
#endif
                }
                DebuggerAttachmentWaitDone = true;
            }
        }

        public override DTSValidationStatus Validate()
        {
            using (var tr = CreateTraceRegion())
            {
                var ret = base.Validate();
                if (ret == DTSValidationStatus.VS_ISVALID)
                {
                    ret = OnValidate();
                }
                return tr.SniffResult(ret);
            }
        }

        protected virtual DTSValidationStatus OnValidate()
        {
            var ret = base.Validate();
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (AllOutputsAreSynchronous)
            {
                int z = 0;
                foreach (IDTSOutput100 output in ComponentMetaData.OutputCollection)
                {
                    if (output.SynchronousInputID < 1)
                    {
                        FireInformation(BasePipelineComponentCodes.ComponentClaimsSyncButOutputsIndicateOtherwise, $"Output [{output.Name}]({z}) is claiming SynchronousInputID={output.SynchronousInputID}");
                        return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                    }
                    ++z;
                }
            }
            return DTSValidationStatus.VS_ISVALID;
        }


        private readonly IDictionary<int, int> ProcessInputIterationByInputId = new Dictionary<int, int>();
        private readonly IDictionary<int, Stopwatch> StopwatchesByInputId = new Dictionary<int, Stopwatch>();

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            base.ProcessInput(inputID, buffer);
            if (buffer != null)
            {
                var sw = StopwatchesByInputId.FindOrCreate(inputID, () => new Stopwatch());
                if (!buffer.EndOfRowset)
                {
                    try
                    {
                        sw.Start();
                        ProcessInputIterationByInputId.Increment(inputID);
                        OnProcessInput(inputID, buffer);
                    }
                    finally
                    {
                        sw.Stop();
                    }
                }
                if (buffer.EndOfRowset)
                {
                    using (CreateTraceRegion($"EndOfRowset for inputID={inputID} iterations={ProcessInputIterationByInputId.Increment(inputID,0)} elapsedTime={sw.Elapsed}", nameof(OnProcessInputEndOfRowset)))
                    {
                        OnProcessInputEndOfRowset(inputID);
                    }
                }
            }
        }

        protected virtual void OnProcessInputEndOfRowset(int inputID)
        { }

        protected abstract void OnProcessInput(int inputID, PipelineBuffer buffer);

        protected readonly bool AllOutputsAreSynchronous;

        protected BasePipelineComponent(bool allOutputsAreSynchronous)
        {
            AllOutputsAreSynchronous = allOutputsAreSynchronous;
        }

        public override IDTSOutputColumn100 InsertOutputColumnAt(
                                         int outputID,
                                         int outputColumnIndex,
                                         string name,
                                         string description)
        {
            throw new Exception(string.Format("Fail to add output column name to {0} ", ComponentMetaData.Name), null);
        }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();

            ComponentMetaData.ContactInfo = "jason@jasonthomas.com";
        }

        protected TraceRegion CreateTraceRegion(string message=null, [CallerMemberName] string caller = null)
            => new TraceRegion(this, message, caller);

        protected class TraceRegion : BaseDisposable
        {
            private static int Id_s = 1;
            private readonly int Id = Interlocked.Increment(ref Id_s);
            private readonly BasePipelineComponent Component;
            private readonly string Message;
            private readonly string Caller;
            private readonly Stopwatch SW = new Stopwatch();

            public TraceRegion(BasePipelineComponent component, string message, string caller)
            {
                Requires.NonNull(component, nameof(component));

                Component = component;
                Message = message;
                Caller = caller;
                Component.FireInformation(BasePipelineComponentCodes.TraceRegionStart, $"{Message} [TRID{Id}] vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv", Caller);
                SW.Start();
            }

            private object Result;
            private bool SniffResultCalled;

            public T SniffResult<T>(T res)
            {
                Requires.SingleCall(ref SniffResultCalled);
                Result = res;
                return res;
            }

            protected override void OnDispose(bool disposing)
            {
                SW.Stop();
                Component.FireInformation(BasePipelineComponentCodes.TraceRegionStart, $"{Message} returned [{Result}] and took {SW.Elapsed} [TRID{Id}] ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", Caller);
                base.OnDispose(disposing);
            }
        }

        protected void FireInformation<TMessageCode>(TMessageCode code, string message, [CallerMemberName] string caller = null) where TMessageCode : struct
        {
            bool fireAgain = true;
            var component = $"{ComponentMetaData.Name}.{caller}";
            var desc = $"{code}: {message}";
            Trace.WriteLine($"Information: {component}({code})=>{desc}");
            ComponentMetaData.FireInformation((int)(object)code, component, desc, "", 0, ref fireAgain);
        }

        protected void FireError<TMessageCode>(TMessageCode code, string message, [CallerMemberName] string caller = null) where TMessageCode : struct
            => FireError(code, new Exception(message), message, caller);

        protected void FireError<TMessageCode>(TMessageCode code, Exception ex, string message=null, [CallerMemberName] string caller = null, bool throwException=true) where TMessageCode : struct
        {
            bool cancel = true;
            var component = $"{ComponentMetaData.Name}.{caller}";
            var desc = $"{code}: {message ?? ex.Message}";
            Trace.WriteLine($"Error: {component}({code})=>{desc}");
            ComponentMetaData.FireError((int)(object)code, $"{component}", $"{desc}", "", 0, out cancel);
            if (throwException)
            {
                throw ex;
            }
        }

        /// <remarks>https://technet.microsoft.com/en-us/library/ms345165(v=sql.110).aspx</remarks>
        protected object GetObject(string colName, PipelineBuffer buffer, ColumnBufferMapping cbm)
        {
            var colDataType = cbm.GetColumnFromColumnName(colName).DataType;
            var n = cbm.GetPositionFromColumnName(colName);
            if (buffer.IsNull(n)) return null;
            switch (colDataType)
            {
                case DataType.DT_BOOL:
                    return buffer.GetBoolean(n);
                case DataType.DT_I1:
                    return buffer.GetSByte(n);
                case DataType.DT_I2:
                    return buffer.GetInt16(n);
                case DataType.DT_I4:
                    return buffer.GetInt32(n);
                case DataType.DT_I8:
                    return buffer.GetInt64(n);
                case DataType.DT_UI1:
                    return buffer.GetByte(n);
                case DataType.DT_UI2:
                    return buffer.GetUInt16(n);
                case DataType.DT_UI4:
                    return buffer.GetUInt32(n);
                case DataType.DT_UI8:
                    return buffer.GetUInt64(n);
                case DataType.DT_R4:
                    return buffer.GetSingle(n);
                case DataType.DT_R8:
                    return buffer.GetDouble(n);
                case DataType.DT_DBDATE:
                    return buffer.GetDate(n);
                case DataType.DT_DATE:
                case DataType.DT_DBTIMESTAMP:
                case DataType.DT_DBTIMESTAMP2:
                case DataType.DT_FILETIME:
                    return buffer.GetDateTime(n);
                case DataType.DT_STR:
                case DataType.DT_WSTR:
                case DataType.DT_NTEXT:
                case DataType.DT_TEXT:
                    return buffer.GetString(n);
                case DataType.DT_NUMERIC:
                case DataType.DT_DECIMAL:
                case DataType.DT_CY:
                    return buffer.GetDecimal(n);
                case DataType.DT_GUID:
                    return buffer.GetGuid(n);
            }
            bool cancel = true;
            ComponentMetaData.FireError(123, "GetObject", string.Format("GetObject(colName={0}, colDataType={1}) is not yet supported", colName, colDataType), "", 0, out cancel);
            return null;
        }

        protected ColumnBufferMapping CreateColumnBufferMapping(IDTSInput100 input, int? overrideBuffer=null)
        {
            //done via ternary operator instead of GetValueOrDefault so as to not dereference output.Buffer unless critical
            var bufferId = overrideBuffer.HasValue ? overrideBuffer.Value : input.Buffer;
            var cbm = new ColumnBufferMapping();
            int cnt = input.InputColumnCollection.Count;
            using (CreateTraceRegion($"Output.ID={input.ID} Output.Name={input.Name} #cols={cnt} for bufferId={bufferId} into cbm={cbm.ID}"))
            {
                if (bufferId < 1)
                {
                    FireError(BasePipelineComponentCodes.InvalidBufferId, $"BufferId({bufferId}) for input=[{input.Name}] really should be > 0");
                }
                else
                {
                    for (int x = 0; x < cnt;)
                    {
                        var column = input.InputColumnCollection[x++];
                        try
                        {
                            var offset = BufferManager.FindColumnByLineageID(bufferId, column.LineageID);
                            cbm.Add(column, offset);
                            FireInformation(BasePipelineComponentCodes.ColumnMappingSuccess, $"{x}/{cnt} column.Name=[{column.Name}] column.LineageID=[{column.LineageID}] => {offset}");
                        }
                        catch (Exception ex)
                        {
                            FireError(BasePipelineComponentCodes.ColumnMappingError, ex, $"{x}/{cnt} column.Name=[{column.Name}] column.LineageID=[{column.LineageID}]", throwException: false);
                            throw;
                        }
                    }
                }
                return cbm;
            }
        }

        protected ColumnBufferMapping CreateColumnBufferMapping(IDTSOutput100 output, int? overrideBuffer = null)
        {
            //done via ternary operator instead of GetValueOrDefault so as to not dereference output.Buffer unless critical
            var bufferId = overrideBuffer.HasValue ? overrideBuffer.Value : output.Buffer;
            var cbm = new ColumnBufferMapping();
            int cnt = output.OutputColumnCollection.Count;
            using (CreateTraceRegion($"Output.ID={output.ID} Output.Name={output.Name} #cols={cnt} for bufferId={bufferId} into cbm={cbm.ID}"))
            {
                if (bufferId < 1)
                {
                    FireError(BasePipelineComponentCodes.InvalidBufferId, $"BufferId({bufferId}) for output=[{output.Name}] really should be > 0");
                }
                for (int x = 0; x < cnt;)
                {
                    var column = output.OutputColumnCollection[x++];
                    try
                    {
                        var offset = BufferManager.FindColumnByLineageID(bufferId, column.LineageID);
                        cbm.Add(column, offset);
                        FireInformation(BasePipelineComponentCodes.ColumnMappingSuccess, $"{x}/{cnt} column.Name=[{column.Name}] column.LineageID=[{column.LineageID}] => {offset}");
                    }
                    catch (Exception ex)
                    {
                        FireError(BasePipelineComponentCodes.ColumnMappingError, ex, $"{x}/{cnt} column.Name=[{column.Name}] column.LineageID=[{column.LineageID}]", throwException: false);
                        throw;
                    }
                }
                return cbm;
            }
        }

        protected class RuntimeData : BaseDisposable
        {
            protected readonly BasePipelineComponent Parent;

            public readonly IList<ColumnBufferMapping> InputColumnBufferMappings;

            public readonly IList<ColumnBufferMapping> OutputColumnBufferMappings;

            protected int GetCustomPropertyAsInt(string propertyName, int fallback = 0)
                => Parent.GetCustomPropertyAsInt(propertyName, fallback);

            protected bool GetCustomPropertyAsBool(string propertyName, bool fallback = false)
                => Parent.GetCustomPropertyAsBool(propertyName, fallback);

            protected string GetCustomPropertyAsString(string propertyName, string fallback = null)
                => Parent.GetCustomPropertyAsString(propertyName, fallback);

            protected IDTSComponentMetaData100 ComponentMetaData
                => Parent.ComponentMetaData;

            private ColumnBufferMapping CreateColumnBufferMapping(IDTSInput100 input, int? overrideBuffer = null)
                => Parent.CreateColumnBufferMapping(input, overrideBuffer);

            private ColumnBufferMapping CreateColumnBufferMapping(IDTSOutput100 output, int? overrideBuffer = null)
                => Parent.CreateColumnBufferMapping(output, overrideBuffer);


            public RuntimeData(BasePipelineComponent parent)
            {
                Requires.NonNull(parent, nameof(parent));
                Parent = parent;

                var cbms = new List<ColumnBufferMapping>();
                for (int z = 0; z < ComponentMetaData.InputCollection.Count; ++z)
                {
                    var cols = ComponentMetaData.InputCollection[z];
                    cbms.Add(Parent.CreateColumnBufferMapping(cols));
                }
                InputColumnBufferMappings = cbms.AsReadOnly();

                cbms = new List<ColumnBufferMapping>();
                for (int z = 0; z < ComponentMetaData.OutputCollection.Count; ++z)
                {
                    var output = ComponentMetaData.OutputCollection[z];
                    int bufferId = Parent.GetBufferId(output);
                    cbms.Add(Parent.CreateColumnBufferMapping(output, bufferId));
                }
                OutputColumnBufferMappings = cbms.AsReadOnly();
            }
        }

        private int GetBufferId(IDTSOutput100 output)
        {
            if (output.SynchronousInputID == 0)
            {
                return output.Buffer;
            }
            foreach (IDTSInput100 input in ComponentMetaData.InputCollection)
            {
                if (input.ID == output.SynchronousInputID)
                {
                    return input.Buffer;
                }
            }
            throw new Exception($"Output is synchronous to {output.SynchronousInputID} but could not find that input");
        }

        protected RuntimeData RD { get; private set; }

        protected virtual RuntimeData ConstructRuntimeData() 
            => new RuntimeData(this);

        public override void PreExecute()
        {
            using (CreateTraceRegion())
            {
                DebuggerAttachmentWait();
                base.PreExecute();
                RD = ConstructRuntimeData();
                Requires.NonNull(RD, nameof(RD));
            }
        }

        public override void PostExecute()
        {
            base.PostExecute();
            Stuff.Dispose(RD);
            RD = null;
        }

        /// <summary>
        /// Upgrade the metadata if it needs it.
        /// Right now all this does is update the version number in the XML.
        /// </summary>
        /// <param name="pipelineVersion">The curreht version of the pipeline.</param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            // Get the attributes for the executable
            var componentAttribute = (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DtsPipelineComponentAttribute), false);
            int binaryVersion = componentAttribute.CurrentVersion;
            using (CreateTraceRegion($"Upgrading {this.GetType().Name} on [{ComponentMetaData.Name}]({ComponentMetaData.ID}) from version {pipelineVersion} to version {binaryVersion}"))
            {
                // Set the SSIS Package's version ID for this component to the binary version...
                ComponentMetaData.Version = binaryVersion;
                OnPerformUpgrade(pipelineVersion, binaryVersion);
            }
        }

        protected virtual void OnPerformUpgrade(int from, int to)
        { }
    }
}
