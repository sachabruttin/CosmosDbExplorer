﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for ContainerPropertyPage.xaml
    /// </summary>
    public partial class ContainerPropertyPage : Page
    {
        public ContainerPropertyPage(ContainerPropertyViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
