using System.Windows;
using CRMS.Domain.Entities;
using CRMS.ViewModels.Admin.Groups;
using MahApps.Metro.Controls;

namespace CRMS.Views.Admin.Groups
{
    public partial class AddUserToGroupWindow : MetroWindow
    {
        private readonly AddUserToGroupViewModel _viewModel;

        public AddUserToGroupWindow(AddUserToGroupViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _viewModel.CloseAction = () => this.Close(); // Добавлено закрытие окна
            DataContext = _viewModel;
        }

        public void SetGroup(Group group)
        {
            _viewModel.SetGroupAsync(group);
        }
    }
}

