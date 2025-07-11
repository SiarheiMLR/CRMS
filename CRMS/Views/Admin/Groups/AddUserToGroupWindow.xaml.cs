using System.Windows;
using CRMS.Domain.Entities;
using CRMS.ViewModels.Admin.Groups;

namespace CRMS.Views.Admin.Groups
{
    public partial class AddUserToGroupWindow : Window
    {
        private readonly AddUserToGroupViewModel _viewModel;

        public AddUserToGroupWindow(AddUserToGroupViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        public void SetGroup(Group group)
        {
            _viewModel.SetGroup(group);
        }
    }
}

