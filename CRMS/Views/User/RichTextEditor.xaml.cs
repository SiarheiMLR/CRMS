using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using CRMS.ViewModels;
using System.Windows.Documents;

namespace CRMS.Views.User
{
    public partial class RichTextEditor : UserControl
    {
        public static FlowDocument DefaultDocument { get; } = new FlowDocument();

        public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(
            "Document",
            typeof(FlowDocument),
            typeof(RichTextEditor),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDocumentChanged));

        public FlowDocument Document
        {
            get => (FlowDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        private static void OnDocumentChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextEditor editor)
            {
                editor.Editor.Document = e.NewValue as FlowDocument ?? new FlowDocument();
            }
        }

        public RichTextEditor()
        {
            InitializeComponent();
            Editor.TextChanged += (s, e) =>
            {
                // Обновляем свойство при изменении документа
                Document = Editor.Document;
            };
            this.Loaded += RichTextEditor_Loaded;
        }

        private void RichTextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RichTextEditorViewModel viewModel)
            {
                viewModel.Editor = Editor; // Привязка RichTextBox к VM
            }
        }
    }
}
