using System.Threading;
using System.Windows;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public abstract class ResourceNodeViewModelBase<TParent> : TreeViewItemViewModel<TParent>, ICanRefreshNode
        where TParent : TreeViewItemViewModel
    {
        private RelayCommand _refreshCommand;
        private RelayCommand _copySelfLinkToClipboardCommand;
        private RelayCommand _copyResourceToClipboardCommand;
        private RelayCommand _copyAltLinkToClipboardCommand;

        protected ResourceNodeViewModelBase(ICosmosResource resource, TParent parent, bool lazyLoadChildren)
            : base(parent, lazyLoadChildren)
        {
            Resource = resource;
        }

        public string Name => Resource.Id;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async () =>
                        {
                            Children.Clear();
                            await LoadChildren(new CancellationToken()).ConfigureAwait(false);
                            //await DispatcherHelper.RunAsync(async () =>
                            //{
                            //    Children.Clear();
                            //    await LoadChildren().ConfigureAwait(false);
                            //});
                        }));
            }
        }

        public RelayCommand CopySelfLinkToClipboardCommand
        {
            get
            {
                return _copySelfLinkToClipboardCommand
                    ?? (_copySelfLinkToClipboardCommand = new RelayCommand(
                        () => Clipboard.SetText(Resource.SelfLink)
                        ));
            }
        }

        //public RelayCommand CopyAltLinkToClipboardCommand
        //{
        //    get
        //    {
        //        return _copyAltLinkToClipboardCommand
        //            ?? (_copyAltLinkToClipboardCommand = new RelayCommand(
        //                () => Clipboard.SetText(Resource.AltLink)
        //                ));
        //    }
        //}

        //public RelayCommand CopyResourceToClipboardCommand
        //{
        //    get
        //    {
        //        return _copyResourceToClipboardCommand
        //            ?? (_copyResourceToClipboardCommand = new RelayCommand(
        //                () =>
        //                {
        //                    using (var stream = new MemoryStream())
        //                    {
        //                        Resource.SaveTo(stream, SerializationFormattingPolicy.Indented);
        //                        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        //                        Clipboard.SetText(json);
        //                    }
        //                }
        //                ));
        //    }
        //}

        protected ICosmosResource Resource { get; set; }

        //protected IDocumentDbService DbService => SimpleIoc.Default.GetInstance<IDocumentDbService>();

        //protected IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();

        //protected IUIServices UIServices => SimpleIoc.Default.GetInstance<IUIServices>();

    }
}
