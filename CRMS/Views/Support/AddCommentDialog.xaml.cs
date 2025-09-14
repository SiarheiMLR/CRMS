using CRMS.Views.User;
using System;
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

namespace CRMS.Views.Support
{
    /// <summary>
    /// Логика взаимодействия для AddCommentDialog.xaml
    /// </summary>
    public partial class AddCommentDialog : UserControl
    {
        public AddCommentDialog()
        {
            InitializeComponent();
        }

        // Сбрасывает содержимое RichTextEditor на новый пустой FlowDocument
        public void ResetEditor()
        {
            if (CommentEditor != null)
            {
                // Создаем полностью новый документ
                CommentEditor.Document = new FlowDocument();

                // Принудительно обновляем привязку
                var binding = BindingOperations.GetBindingExpression(
                    CommentEditor, RichTextEditor.DocumentProperty);
                binding?.UpdateTarget();
            }
        }
    }
}
