using CommunityToolkit.Mvvm.Messaging;
using Grasshopper.Kernel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            Services = ConfigureServices();
            ConfigureFinished.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ConfigureFinished = delegate { };
        public IServiceProvider Services { get; set; }

        public GH_Component ComponentHost { get; set; }
        public WeakReferenceMessenger Messenger => Services.GetService<WeakReferenceMessenger>();
        public MenuBarViewModel MenuBarViewModel => Services.GetService<MenuBarViewModel>();

        public TerminalViewModel TerminalViewModel => Services.GetService<TerminalViewModel>();

        public TextEditorViewModel TextEditorViewModel => Services.GetService<TextEditorViewModel>();

        public DebugViewModel DebugViewModel => Services.GetService<DebugViewModel>();


        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            var messenger = new WeakReferenceMessenger();
            services.AddSingleton(messenger);
            services.AddSingleton(new MenuBarViewModel(this));
            services.AddSingleton(new TerminalViewModel(this));
            services.AddSingleton(new DebugViewModel(this));
            services.AddSingleton(new TextEditorViewModel(this));
            return services.BuildServiceProvider();
        }
    }
}
