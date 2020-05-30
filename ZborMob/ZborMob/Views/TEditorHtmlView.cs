using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TEditor;
using TEditor.Abstractions;
using Xamarin.Forms;

namespace ZborMob.Views
{
    public class TEditorHtmlView : Grid
    {
        //create bindable property, html
        private string _html;
        public string Html
        {
            get { return _html; }
            set
            {
                _html = value;
                OnPropertyChanged();
            }
        }
        private bool _isVisibleEdit;
        public bool IsVisibleEdit
        {
            get { return _isVisibleEdit; }
            set
            {
                _isVisibleEdit = value;
                OnPropertyChanged();
            }
        }
        public event EventHandler<EventArgs> SpremiEvent;

        public WebView _displayWebView { get; set; }

        private Button EditButton;
        public TEditorHtmlView()
        {
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            _displayWebView = new WebView() {HeightRequest = 200 };
            EditButton = new Button
            {
                Text = "Uredi",
                HeightRequest = 40,
                Command = new Command(async (obj) =>
                {
                    await ShowTEditor();
                }),
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#1C6EBC"),
                HorizontalOptions = LayoutOptions.End,
                IsVisible = false
            };
            this.Children.Add(EditButton, 1, 0) ;

            SetRow(_displayWebView, 1);
            SetColumnSpan(_displayWebView, 2);
            this.Children.Add(_displayWebView);

        }

        async Task ShowTEditor()
        {
            TEditorResponse response = await CrossTEditor.Current.ShowTEditor(Html);
            if (response.IsSave)
            {
                if (response.HTML != null)
                {
                    _displayWebView.Source = new HtmlWebViewSource() { Html = response.HTML };
                    Html = response.HTML;
                    SpremiEvent.Invoke(this, new EventArgs());
                }
            }
            
        }
        public static readonly BindableProperty HtmlProperty = BindableProperty.Create(
                                                         propertyName: "Html",
                                                         returnType: typeof(string),
                                                         declaringType: typeof(TEditorHtmlView),
                                                         defaultValue: "",
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         propertyChanged: SrcPropertyChanged);
        private static void SrcPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null) newValue = "";
            var control = (TEditorHtmlView)bindable;
            control.Html = newValue.ToString();
           
            control._displayWebView.Source = new HtmlWebViewSource() { Html = newValue.ToString() };
        }
        public static readonly BindableProperty IsVisibleEditProperty = BindableProperty.Create(
                                                       propertyName: "IsVisibleEdit",
                                                       returnType: typeof(bool),
                                                       declaringType: typeof(TEditorHtmlView),
                                                       defaultValue: false,
                                                       defaultBindingMode: BindingMode.TwoWay,
                                                       propertyChanged: VisibilityPropertyChanged);
        private static void VisibilityPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (TEditorHtmlView)bindable;
            bool b = false;
            if (newValue != null)
                b = Boolean.Parse(newValue.ToString());
            control.IsVisibleEdit = b;
            control.EditButton.IsVisible = b;

            //control._displayWebView.Source = new HtmlWebViewSource() { Html = newValue.ToString() };
        }
    }
}
