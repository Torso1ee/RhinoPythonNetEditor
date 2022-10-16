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

        public static  async Task<int> RequestCompletionAsync(string path, (int, int) posution)
        {
            var items = await Client.TextDocument.RequestCompletion(new CompletionParams
            {
                TextDocument = path,
                Position = posution,
            });
            return items.Count();
        }



    }

}
