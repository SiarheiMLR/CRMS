using CRMS.Business.Models;
using CRMS.ViewModels.Admin;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using CRMS; // если нет — добавь using

namespace CRMS.Views.Admin
{
    public partial class UserListPage : Page
    {
        public UserListPage(List<ADUserDto> users)
        {
            InitializeComponent();
            var userService = App.ServiceProvider.GetRequiredService<CRMS.Business.Services.UserService.IUserService>();
            DataContext = new UserListPageViewModel(userService, users);
        }
    }
}

