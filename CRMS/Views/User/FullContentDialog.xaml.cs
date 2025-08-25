using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using CRMS.Domain.Entities;

namespace CRMS.Views.User
{
    public partial class FullContentDialog : MetroWindow
    {
        public FullContentDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}