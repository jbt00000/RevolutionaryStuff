using System;
using System.IO;
using System.Threading.Tasks;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Diagnostics;
using RevolutionaryStuff.Core.Streams;
using RevolutionaryStuff.Functions.Merger.Core;
using RevolutionaryStuff.Functions.Mergers;
using RevolutionaryStuff.Mergers;
using RevolutionaryStuff.Mergers.Pdf;

namespace RevolutionaryStuff.Functions.Merger.PdfMerger
{
    //https://github.com/nebosite/azure-functions-rocketscience/blob/master/src/AFRocketScienceShared/Tools/AssemblyHelper.cs
    public static class Function1
    {
        private static readonly PdfMergeOutputSettings DefaultOutputSettings = new PdfMergeOutputSettings {
            ContentType = RevolutionaryStuff.Core.MimeType.Application.Pdf.PrimaryContentType,
            ContentDispositionFilename = "the.pdf",
        };

        [FunctionName("MergePdf_1")]
        public static async Task<IActionResult> MergePdf1(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using (new LogRegion(log))
            {
                var cc = await MergeCallContext<PdfMergeInputs>.CreateAsync(req, log, PdfMergeInputs.CreateFromJson);
                var pdfReader = new PdfReader(cc.TemplateStream);
                var st = new MemoryStream();
                var pdfWriter = new PdfWriter(new IndestructibleStream(st));
                var doc = new PdfDocument(pdfReader, pdfWriter);
                var form = iText.Forms.PdfAcroForm.GetAcroForm(doc, false);
                if (form == null) throw new MergerException(MergerExceptionCodes.CouldNotLoadTemplate);
                var outputSettings = cc.Inputs.OutputSettings ?? DefaultOutputSettings;
                if (outputSettings.NeedAppearances!=null)
                {
                    form.SetNeedAppearances(outputSettings.NeedAppearances.Value);
                }
                foreach (var kvp in form.GetFormFields())
                {
                    var field = kvp.Value;
                    var fieldName = kvp.Key;
                    if (!cc.TryGetFieldDataByKey(fieldName, out var fd))
                    {
                        log.LogInformation("Not variables to merge with {fieldName}", fieldName);
                        continue;
                    }
                    try
                    {
                        var val = fd.FieldVal;
                        string sval;
                        if (val is bool && field is PdfButtonFormField)
                        {
                            var buttonField = (PdfButtonFormField)field;
                            var bval = (bool)val;
                            var states = buttonField.GetAppearanceStates();
                            if (states != null && states.Length > 0)
                            {
                                if (states.Length == 1)
                                {
                                    sval = bval ? states[0] : "Off";
                                }
                                else
                                {
                                    sval = bval ? states[1] : states[0];
                                }
                            }
                            else
                            {
                                throw new Exception($"field states do not match bool");
                            }
                        }
                        else
                        {
                            sval = cc.ToString(fd) ?? "";
                        }
                        field.SetValue(sval);
                        log.LogDebug("field={fieldName} set", field.GetFieldName());
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Problem merging {fieldByName}", kvp.Key);
                    }
                }
                if (outputSettings.ContentType == RevolutionaryStuff.Core.MimeType.Application.Pdf.PrimaryContentType)
                {
                }
                else
                {
                    throw new MergerException(MergerExceptionCodes.OutputTypeNotSupported);
                }
                doc.Close();
                st.Flush();
                st.Position = 0;
                log.LogDebug("saved");
                return cc.CreateFileStreamResult(req, st, outputSettings);
            }
        }
    }
}
