﻿using System.IO;
using System.Windows;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public abstract class ResourceNodeViewModelBase : TreeViewItemViewModel, ICanRefreshNode
    {
        private RelayCommand _refreshCommand;
        private RelayCommand _copySelfLinkToClipboardCommand;
        private RelayCommand _copyResourceToClipboardCommand;
        private RelayCommand _copyAltLinkToClipboardCommand;

        protected ResourceNodeViewModelBase(Resource resource, TreeViewItemViewModel parent, bool lazyLoadChildren)
            : base(parent, parent.MessengerInstance, lazyLoadChildren)
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
                        async x =>
                        {
                            await DispatcherHelper.RunAsync(async () =>
                            {
                                Children.Clear();
                                await LoadChildren();
                            });
                        }));
            }
        }

        public RelayCommand CopySelfLinkToClipboardCommand
        {
            get
            {
                return _copySelfLinkToClipboardCommand
                    ?? (_copySelfLinkToClipboardCommand = new RelayCommand(
                        x => Clipboard.SetText(Resource.SelfLink)
                        ));
            }
        }

        public RelayCommand CopyAltLinkToClipboardCommand
        {
            get
            {
                return _copyAltLinkToClipboardCommand
                    ?? (_copyAltLinkToClipboardCommand = new RelayCommand(
                        x => Clipboard.SetText(Resource.AltLink)
                        ));
            }
        }

        public RelayCommand CopyResourceToClipboardCommand
        {
            get
            {
                return _copyResourceToClipboardCommand
                    ?? (_copyResourceToClipboardCommand = new RelayCommand(
                        x =>
                        {
                            using (var stream = new MemoryStream())
                            {
                                Resource.SaveTo(stream, SerializationFormattingPolicy.Indented);
                                var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                                Clipboard.SetText(json);
                            }
                        }
                        ));
            }
        }

        protected Resource Resource { get; set; }

        protected IDocumentDbService DbService => SimpleIoc.Default.GetInstance<IDocumentDbService>();

        protected IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();
    }
}
