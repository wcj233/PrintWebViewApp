using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WebviewApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        async Task<List<FrameworkElement>> GetWebPages(Windows.UI.Xaml.Controls.WebView webView, Windows.Foundation.Size page)
        {
            // ask the content its width
            var _WidthString = await webView.InvokeScriptAsync("eval",
                        new[] { "document.body.scrollWidth.toString()" });
            int _ContentWidth;
            if (!int.TryParse(_WidthString, out _ContentWidth))
                throw new Exception(string.Format("failure/width:{0}", _WidthString));
            webView.Width = _ContentWidth;

            // ask the content its height
            var _HeightString = await webView.InvokeScriptAsync("eval",
                        new[] { "document.body.scrollHeight.toString()" });
            int _ContentHeight;
            if (!int.TryParse(_HeightString, out _ContentHeight))
                throw new Exception(string.Format("failure/height:{0}", _HeightString));
            webView.Height = _ContentHeight;

            // how many pages will there be?
            var _Scale = page.Width / _ContentWidth;
            var _ScaledHeight = (_ContentHeight * _Scale);
            var _PageCount = (double)_ScaledHeight / page.Height;
            _PageCount = _PageCount + ((_PageCount > (int)_PageCount) ? 1 : 0);

            // create the pages
            var _Pages = new List<FrameworkElement>();
            for (int i = 0; i < (int)_PageCount; i++)
            {
                var _TranslateY = -page.Height * i;
                var _Page = new Windows.UI.Xaml.Shapes.Rectangle
                {
                    Height = page.Height,
                    Width = page.Width,
                    Margin = new Windows.UI.Xaml.Thickness(5),
                    Tag = new Windows.UI.Xaml.Media.TranslateTransform { Y = _TranslateY },
                };
                _Page.Loaded += async (s, e) =>
                {
                    var _Rectangle = s as Windows.UI.Xaml.Shapes.Rectangle;
                    var _Brush = await GetWebViewBrush(webView);
                    _Brush.Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill;
                    _Brush.AlignmentY = Windows.UI.Xaml.Media.AlignmentY.Top;
                    _Brush.Transform = _Rectangle.Tag as Windows.UI.Xaml.Media.TranslateTransform;
                    _Rectangle.Fill = _Brush;
                };

                var stack = new StackPanel();
                stack.Orientation = Orientation.Vertical;
                stack.Children.Add(_Page);

                _Pages.Add(stack);
            }
            return _Pages;
        }

        async Task<WebViewBrush> GetWebViewBrush(Windows.UI.Xaml.Controls.WebView webView)
        {
            // resize width to content
            var _OriginalWidth = webView.Width;
            var _WidthString = await webView.InvokeScriptAsync("eval",
                    new[] { "document.body.scrollWidth.toString()" });
            int _ContentWidth;
            if (!int.TryParse(_WidthString, out _ContentWidth))
                throw new Exception(string.Format("failure/width:{0}", _WidthString));
            webView.Width = _ContentWidth;

            // resize height to content
            var _OriginalHeight = webView.Height;
            var _HeightString = await webView.InvokeScriptAsync("eval",
                    new[] { "document.body.scrollHeight.toString()" });
            int _ContentHeight;
            if (!int.TryParse(_HeightString, out _ContentHeight))
                throw new Exception(string.Format("failure/height:{0}", _HeightString));
            webView.Height = _ContentHeight;

            // create brush
            var _OriginalVisibilty = webView.Visibility;
            webView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var _Brush = new WebViewBrush
            {
                SourceName = webView.Name,
                Stretch = Windows.UI.Xaml.Media.Stretch.Uniform
            };
            _Brush.Redraw();

            // reset, return
            webView.Width = _OriginalWidth;
            webView.Height = _OriginalHeight;
            webView.Visibility = _OriginalVisibilty;
            return _Brush;
        }

        private IEnumerable<FrameworkElement> allPages { get; set; }

       
        private PrintHelper _printHelper;

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
        
            _printHelper = new PrintHelper(Container);
            _printHelper.OnPreviewPagesCreated += _printHelper_OnPreviewPagesCreated;
            _printHelper.OnPrintSucceeded += _printHelper_OnPrintSucceeded;
            allPages = await GetWebPages(wvTest, new Windows.Foundation.Size(750d, 950d));

            for (int i = 0; i < allPages.ToList().Count; i++)
            {
                _printHelper.AddFrameworkElementToPrint(allPages.ToList()[i]);
            }
         
            await _printHelper.ShowPrintUIAsync("print sample");
        }

        private void _printHelper_OnPrintSucceeded()
        {
            
        }

        private void _printHelper_OnPreviewPagesCreated(List<FrameworkElement> obj)
        {
            foreach (FrameworkElement el in obj)
            {
                el.UpdateLayout();
            }
        }
    }
}
