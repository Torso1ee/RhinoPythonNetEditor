using ICSharpCode.AvalonEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RhinoPythonNetEditor.ViewModel.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.Interface;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Windows.Data;

namespace RhinoPythonNetEditor.ViewModel
{
    public class TextEditorViewModel : ObservableRecipient
    {
        private TextDocument document = new TextDocument();

        public TextDocument Document
        {
            get { return document; }
        }

        public ViewModelLocator Locator { get; set; }

        public TextEditorViewModel(ViewModelLocator locator)
        {
            Locator = locator;
            IsActive = true;
        }

        public void SetCode(string text)
        {
            Document.Text = text;
        }

        public async Task<bool> CheckCode()
        {
            if (Locator.ComponentHost is IScriptComponent sc)
            {
                if (sc.GetCode() != Document.Text)
                {
                    var result = await Locator.Messenger.Send(new ConfirmDialogRequestMessage { Message = $"Code has changed compared to last version .Do you want to exit without applying changes?", Title = "Warning" });
                    return (bool)result;
                }
            }
            return true;
        }

        private bool isSearch;

        public bool IsSearch
        {
            get { return isSearch; }
            set { SetProperty(ref isSearch, value); }
        }


        private bool showReplace;

        public bool ShowReplace
        {
            get { return showReplace; }
            set { SetProperty(ref showReplace, value); }
        }

        private bool selectionOnly;

        public bool SelectionOnly
        {
            get { return selectionOnly; }
            set { SetProperty(ref selectionOnly, value); }
        }

        private bool clarifyCase;

        public bool ClarifyCase
        {
            get { return clarifyCase; }
            set { SetProperty(ref clarifyCase, value); }
        }

        private bool allMatch;

        public bool AllMatch
        {
            get { return allMatch; }
            set { SetProperty(ref allMatch, value); }
        }

        private string result;

        public string Result
        {
            get { return result; }
            set { SetProperty(ref result, value); }
        }


        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set { SetProperty(ref searchText, value); }
        }

        private string replaceText="";

        public string ReplaceText
        {
            get { return replaceText; }
            set { SetProperty(ref replaceText, value); }
        }

        private bool useRe;

        public bool UseRe
        {
            get { return useRe; }
            set { SetProperty(ref useRe, value); }
        }
        public ICommand Close => new RelayCommand(() =>
        {
            IsSearch = false;
            Messenger.Send(new ClearSearchMessage());
        });

        private WeakReferenceMessenger Messenger => Locator?.Messenger;

        public void Search(object sender, TextChangedEventArgs e)
        {
            var tb = e.Source as TextBox;
            SearchInternal(tb);
        }

        private int currentIndex;

        public int CurrentIndex
        {
            get { return currentIndex; }
            set { SetProperty(ref currentIndex, value); }
        }

        public ICommand Up => new RelayCommand(() =>
        {
            CurrentIndex--;
            Result = $"Total {CurrentCount} items, At item {CurrentIndex + 1}";
            OnPropertyChanged(nameof(Up));
            OnPropertyChanged(nameof(Down));
            Messenger.Send(new ScrollToMessage(CurrentIndex));
        }, () => CurrentIndex >= 1 && CurrentCount > 0);

        public ICommand Down => new RelayCommand(() =>
        {
            CurrentIndex++;
            Result = $"Total {CurrentCount} items, At item {CurrentIndex + 1}";
            OnPropertyChanged(nameof(Up));
            OnPropertyChanged(nameof(Down));
            Messenger.Send(new ScrollToMessage(CurrentIndex));
        }, () => CurrentIndex <= CurrentCount - 2 && CurrentCount > 0);

        private int currentCount;

        public int CurrentCount
        {
            get { return currentCount; }
            set { SetProperty(ref currentCount, value); }
        }

        public ICommand Replace => new RelayCommand(() =>
        {
            Messenger.Send(new ReplaceRequestMessage { Index = CurrentIndex, IsAll = false, ReplaceText = ReplaceText });
        });

        public ICommand ReplaceAll => new RelayCommand(() =>
        {
            Messenger.Send(new ReplaceRequestMessage { Index = -1, IsAll = true, ReplaceText = ReplaceText });
        });

        public void SearchInternal(TextBox tb)
        {
            BindingExpression bindingExpression = BindingOperations.GetBindingExpression(tb, TextBox.TextProperty);

            BindingExpressionBase bindingExpressionBase =
                BindingOperations.GetBindingExpressionBase(tb, TextBox.TextProperty);
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                ValidationError validationError =
                    new ValidationError(new ExceptionValidationRule(), bindingExpression, "The input is invalid.", new ArgumentException());
                Validation.MarkInvalid(bindingExpressionBase, validationError);
                return;
            }
            else
            {
                Validation.ClearInvalid(bindingExpression);
            }
            var m = Messenger.Send(new SearchRequestMessage { AllMatch = AllMatch, ClarifyCase = ClarifyCase, UseRe = UseRe, SearchText = tb.Text });
            if (m.Response.Item1)
            {
                CurrentCount = m.Response.Item2;
                CurrentIndex = m.Response.Item3;
                if (CurrentCount == 0) Result = "No results.";
                else Result = $"Total {CurrentCount} items, At item {CurrentIndex + 1}";
            }
            else
            {
                CurrentCount = 0;
                CurrentIndex = -1;
                if (CurrentCount == 0) Result = "No results.";
                ValidationError validationError =
                     new ValidationError(new ExceptionValidationRule(), bindingExpression, "The regex is invalid.", new ArgumentException());
                Validation.MarkInvalid(bindingExpressionBase, validationError);
            }
            OnPropertyChanged(nameof(Up));
            OnPropertyChanged(nameof(Down));
        }
    }
}
