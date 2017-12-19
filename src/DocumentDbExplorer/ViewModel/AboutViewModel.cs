using System;
using System.Diagnostics;
using System.Reflection;
using GalaSoft.MvvmLight;

namespace DocumentDbExplorer.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly Assembly _assembly;
        private readonly FileVersionInfo _fvi;

        public AboutViewModel()
        {
            _assembly = Assembly.GetEntryAssembly();
            _fvi = FileVersionInfo.GetVersionInfo(_assembly.Location);
        }

        public string Version => _fvi.FileVersion;

        public string Title => ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute), false))?.Title ?? "error retriving assembly title";
    }
}
