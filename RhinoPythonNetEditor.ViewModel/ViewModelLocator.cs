using CommunityToolkit.Mvvm.Messaging;
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
        }

        public IServiceProvider Services { get; set; }

        public WeakReferenceMessenger Messenger => Services.GetService<WeakReferenceMessenger>();
        public MenuBarViewModel MenuBarViewModel => Services.GetService<MenuBarViewModel>();

        public TerminalViewModel TerminalViewModel => Services.GetService<TerminalViewModel>();

        public DebugViewModel DebugViewModel => Services.GetService<DebugViewModel>();


        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            var messenger = new WeakReferenceMessenger();
            services.AddSingleton(messenger);
            services.AddSingleton(new MenuBarViewModel(messenger));
            services.AddSingleton(new TerminalViewModel(messenger));
            services.AddSingleton( new DebugViewModel(messenger));
            return services.BuildServiceProvider();
        }
    }
}
