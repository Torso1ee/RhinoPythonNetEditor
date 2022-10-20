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
                            ResolveSupport = new CompletionItemCapabilityResolveSupportOptions { Properties = new string[] { "sortText", "filterText", "insertText", "textEdit" } },
                            TagSupport = new CompletionItemTagSupportCapabilityOptions
                            {
                                ValueSet = new[] { CompletionItemTag.Deprecated }
                            },
                            CommitCharactersSupport = true
                        }
                    }
                );
            }
        );
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            await Client.Initialize(cancellationTokenSource.Token);
            IsInitialized = true;
        }

        public static async Task<int> RequestCompletionAsync(string path, (int, int) posution)
        {
            var items = await Client.TextDocument.RequestCompletion(new CompletionParams
            {
                TextDocument = path,
                Position = posution,
            });
            var results = new List<CompletionItem>();
            foreach (var item in items)
            {
                var result = await Client.TextDocument.ResolveCompletion(item);
                results.Add(result);
            }
            return results.Count();
        }

        public static void DidOpen(string path)
        {
            Client.DidOpenTextDocument(new DidOpenTextDocumentParams { TextDocument = new TextDocumentItem { Uri = path } });
        }
    }
}

