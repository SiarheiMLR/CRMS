using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Domain.Entities;
using System.Windows;

namespace CRMS.ViewModels
{
    public partial class TicketEditViewModel : ObservableObject
    {
        private readonly Ticket _originalTicket;

        [ObservableProperty]
        private string _subject;

        [ObservableProperty]
        private string _content;

        [ObservableProperty]
        private string _windowTitle;

        public TicketEditViewModel(Ticket ticket = null)
        {
            _originalTicket = ticket ?? new Ticket();

            // Инициализация полей
            Subject = _originalTicket.Subject;
            Content = _originalTicket.Content;
            WindowTitle = ticket?.Id == 0 ? "Новый тикет" : "Редактирование тикета";
        }

        [RelayCommand]
        private void Save()
        {
            if (Validate())
            {
                // Обновление исходного объекта
                _originalTicket.Subject = Subject;
                _originalTicket.Content = Content;

                // Закрытие окна с положительным результатом
                CloseWindow(true);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseWindow(false);
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Subject))
            {
                MessageBox.Show("Тема тикета обязательна для заполнения",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                MessageBox.Show("Описание тикета не может быть пустым",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public Ticket GetTicket()
        {
            return _originalTicket;
        }
        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }
    }
}