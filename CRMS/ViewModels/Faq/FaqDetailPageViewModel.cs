using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMS.Business.Services.AuthService;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using CRMS.Services;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace CRMS.ViewModels.Faq
{
    public partial class FaqDetailPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly User _currentUser;

        [ObservableProperty]
        private FaqItem item;

        public SnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue();

        public FaqDetailPageViewModel(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = authService.CurrentUser;
        }

        public async Task LoadAsync(int faqId)
        {
            Item = await _unitOfWork.FaqItemsRepository.GetByIdAsync(faqId);
        }

        [RelayCommand]
        private async Task VoteUpAsync()
        {
            if (await _unitOfWork.FaqItemsRepository.HasUserVotedAsync(Item.Id, _currentUser.Id))
            {
                SnackbarMessageQueue.Enqueue("Вы уже голосовали за этот вопрос");
                return;
            }

            await _unitOfWork.FaqVotesRepository.AddAsync(new FaqVote
            {
                FaqItemId = Item.Id,
                UserId = _currentUser.Id,
                IsPositive = true
            });

            Item.PositiveVotes++;
            await _unitOfWork.SaveChangesAsync();
            SnackbarMessageQueue.Enqueue("Спасибо за ваш голос!");
        }

        [RelayCommand]
        private async Task VoteDownAsync()
        {
            if (await _unitOfWork.FaqItemsRepository.HasUserVotedAsync(Item.Id, _currentUser.Id))
            {
                SnackbarMessageQueue.Enqueue("Вы уже голосовали за этот вопрос");
                return;
            }

            await _unitOfWork.FaqVotesRepository.AddAsync(new FaqVote
            {
                FaqItemId = Item.Id,
                UserId = _currentUser.Id,
                IsPositive = false
            });

            Item.NegativeVotes++;
            await _unitOfWork.SaveChangesAsync();
            SnackbarMessageQueue.Enqueue("Спасибо за обратную связь!");
        }
    }
}
