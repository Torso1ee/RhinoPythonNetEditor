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

namespace RhinoPythonNetEditor.Managers
{
    public class LintManager
    {
        private static Process LSP { get; set; }

        private static LanguageClient Client { get; set; }

        public static bool IsInitialized { get; set; }
        private static void StartLSP()
        {
            var path = Path.GetDirectoryName(typeof(LintManager).Assembly.Location);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = $@"{path}\python_env\Scripts\pylsp.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            LSP = new Process
            {
                StartInfo = info
            };
            LSP.Start();
        }

        public static async Task InitialzeClientAsync()
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
                    }
                );
            }
        );
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            await Client.Initialize(cancellationTokenSource.Token);
            IsInitialized = true;
        }

        public static async Task<IEnumerable<CompletionItem>> RequestCompletionAsync(string path, (int, int) posution)
        {
            var items = await Client.TextDocument.RequestCompletion(new CompletionParams
            {
                TextDocument = path,
                Position = posution,
            });
            return items;
        }

        public static void DidOpen(string path)
        {
            Client.DidOpenTextDocument(new DidOpenTextDocumentParams { TextDocument = new TextDocumentItem { Uri = path } });
        }

        public static void DidClose(string path)
        {
            Client.DidCloseTextDocument(new DidCloseTextDocumentParams { TextDocument = new TextDocumentItem { Uri = path } });
        }

        public static CompletionItem ResolveCompletionItem(CompletionItem item)
        {
            var task = Client.ResolveCompletion(item);
            task.Wait();
            return task.Result;
        }

        public static SignatureHelp RequestSignature(string path, (int, int) posution)
        {
            var task =  Client.TextDocument.RequestSignatureHelp(new SignatureHelpParams
            {
                TextDocument = path,
                Position = posution
            });
            task.Wait();
            return task.Result;
        }

    }
}

