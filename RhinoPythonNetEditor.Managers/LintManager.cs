using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OmniSharp.Extensions.LanguageServer.Client;
using System.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using CompletionItem = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItem;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using RhinoPythonNetEditor.DataModels.Business;

namespace RhinoPythonNetEditor.Managers
{
    public class LintManager
    {
        private LintManager() { }
        private static LintManager instance;
        public static LintManager Instance
        {
            get
            {
                if (instance == null) instance = new LintManager();
                return instance;
            }
        }
        private Process LSP { get; set; }

        private LanguageClient Client { get; set; }

        public bool IsInitialized { get; set; }
        private void StartLSP()
        {
            var path = Path.GetDirectoryName(typeof(LintManager).Assembly.Location);
            var lspPath = $@"{path}\python_env\Scripts\pylsp.exe";
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = $@"{path}\python_env\python.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.Arguments = lspPath;
            LSP = new Process
            {
                StartInfo = info
            };
            LSP.Start();
        }

        public async Task InitialzeClientAsync()
        {
            StartLSP();
            await Task.Delay(500);
            Client = LanguageClient.Create(
            options =>
            {
                options.WithInput(LSP.StandardOutput.BaseStream)
                   .WithOutput(LSP.StandardInput.BaseStream)
                   .WithCapability(
                    new CompletionCapability
                    {
                        CompletionItem = new CompletionItemCapabilityOptions
                        {
                            DeprecatedSupport = true,
                            DocumentationFormat = new Container<MarkupKind>(MarkupKind.Markdown, MarkupKind.PlainText),
                            PreselectSupport = true,
                            SnippetSupport = true,
                            InsertReplaceSupport = true,
                            ResolveAdditionalTextEditsSupport = true,
                            InsertTextModeSupport = new CompletionItemInsertTextModeSupportCapabilityOptions { ValueSet = new[] { InsertTextMode.AsIs } },
                            TagSupport = new CompletionItemTagSupportCapabilityOptions
                            {
                                ValueSet = new[] { CompletionItemTag.Deprecated }
                            },
                            CommitCharactersSupport = true
                        }
                    },
                    new SignatureHelpCapability
                    {
                        ContextSupport = true,
                        SignatureInformation = new SignatureInformationCapabilityOptions
                        {
                            ActiveParameterSupport = true,
                            DocumentationFormat = new Container<MarkupKind>(MarkupKind.Markdown, MarkupKind.PlainText),
                            ParameterInformation = new SignatureParameterInformationCapabilityOptions
                            {
                                LabelOffsetSupport = true,
                            }
                        }
                    },
                    new PublishDiagnosticsCapability
                    {
                        CodeDescriptionSupport = true,
                        DataSupport = true,
                        RelatedInformation = true,
                        VersionSupport = true,
                    }
                );
            }
        );
            Client.Register(RegisterDiagnostic);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            await Client.Initialize(cancellationTokenSource.Token);
            IsInitialized = true;
        }

        public async Task<IEnumerable<CompletionItem>> RequestCompletionAsync(string path, (int, int) posution)
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            IEnumerable<CompletionItem> items = null;
            try
            {
                items = await Client.TextDocument.RequestCompletion(new CompletionParams
                {
                    TextDocument = path,
                    Position = posution,
                }).AsTask().ConfigureAwait(false);
            }
            catch
            {
                items = null;
            }
            finally
            {
                source.Dispose();
            }
            return items;
        }

        public void DidSave(string path)
        {
            Client.DidSaveTextDocument(new DidSaveTextDocumentParams { TextDocument = new TextDocumentIdentifier { Uri = path } });
        }

        public void DidChange(string path, string content)
        {
            Client.DidChangeTextDocument(new DidChangeTextDocumentParams { TextDocument = new VersionedTextDocumentIdentifier { Uri = path }, ContentChanges = new[] { new TextDocumentContentChangeEvent { Text = content } } });
        }

        public void DidOpen(string path)
        {
            Client.DidOpenTextDocument(new DidOpenTextDocumentParams { TextDocument = new TextDocumentItem { Uri = path } });
        }

        public void DidClose(string path)
        {
            Client.DidCloseTextDocument(new DidCloseTextDocumentParams { TextDocument = new TextDocumentItem { Uri = path } });
        }

        public async Task<CompletionItem> ResolveCompletionItemAsync(CompletionItem item)
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            CompletionItem result = null;
            try
            {
                result = await Client.ResolveCompletion(item, source.Token);
            }
            catch
            {
                result = null;
            }
            finally
            {
                source.Dispose();
            }
            return result;
        }

        public async Task<SignatureHelp> RequestSignatureAsync(string path, (int, int) posution)
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            SignatureHelp result = null;
            try
            {
                result = await Client.TextDocument.RequestSignatureHelp(new SignatureHelpParams
                {
                    TextDocument = path,
                    Position = posution
                }, source.Token);
            }
            catch { result = null; }
            finally
            {
                source.Dispose();
            }
            return result;
        }

        public void RegisterDiagnostic(ILanguageClientRegistry register)
        {
            PublishDiagnosticsExtensions.OnPublishDiagnostics(register, DiagnosticPublished);
        }

        private void DiagnosticPublished(PublishDiagnosticsParams p)
        {
            OnDiagnosticPublished?.Invoke(this, new DiagnosticPublishedEventArgs { PublishDiagnostics = p.Diagnostics.Select(d => DiagnosticToSyntaxInfo(d)).ToList(), File = Path.GetFileNameWithoutExtension(p.Uri.Path) });

        }

        private SyntaxInfo DiagnosticToSyntaxInfo(Diagnostic diagnostic)
        {
            var info = new SyntaxInfo();
            info.Servity = (Servity)Enum.Parse(typeof(Servity), diagnostic.Severity.ToString());
            info.Message = diagnostic.Message;
            info.Source = diagnostic.Source;
            info.Range = $"[Line {diagnostic.Range.Start.Line + 1}, Column {diagnostic.Range.Start.Character}]";
            info.Start = (diagnostic.Range.Start.Line, diagnostic.Range.Start.Character);
            info.End = (diagnostic.Range.End.Line, diagnostic.Range.End.Character);
            return info;
        }

        public event EventHandler<DiagnosticPublishedEventArgs> OnDiagnosticPublished = delegate { };
    }

    public class DiagnosticPublishedEventArgs : EventArgs
    {
        public string File { get; set; }
        public List<SyntaxInfo> PublishDiagnostics { get; set; }
    }
}

