// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Monaco;
using Monaco.Editor;
using Monaco.Helpers;
using Windows.System;
using Windows.System.Profile;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Doji.Common;
using Doji.Controls;
using Doji.Pages;

namespace Doji
{
    public sealed partial class Shell
    {
        public static Shell Current { get; private set; }

        private bool _isPaneOpen;

        private XamlRenderService _xamlRenderer = new XamlRenderService();
        private bool _lastRenderedProperties = true;
        private ThreadPoolTimer _autocompileTimer;

        private DateTime _timePatternEditedFirst = DateTime.MinValue;
        private DateTime _timePatternEditedLast = DateTime.MinValue;
        private bool _xamlCodeRendererSupported = false;

        public bool DisplayWaitRing
        {
            set { waitRing.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public ObservableCollection<PatternCommand> Commands { get; } = new ObservableCollection<PatternCommand>();

        public Shell()
        {
            InitializeComponent();

            Current = this;
        }

        public void ShowInfoArea()
        {
            InfoAreaGrid.Visibility = Visibility.Visible;
            RootGrid.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
            RootGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            RootGrid.RowDefinitions[1].Height = new GridLength(32);
            Splitter.Visibility = Visibility.Visible;
        }

        public void ShowOnlyHeader(string title)
        {
            Title.Text = title;
            HideInfoArea();
        }

        /// <summary>
        /// Navigates to a Pattern via a deep link.
        /// </summary>
        /// <param name="deepLink">The deep link. Specified as protocol://[collectionName]?pattern=[patternName]</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task NavigateToPatternAsync(string deepLink)
        {
            var parser = DeepLinkParser.Create(deepLink);
            var targetPattern = await Patterns.GetPatternByName(parser["pattern"]);
            if (targetPattern != null)
            {
                NavigateToPattern(targetPattern);
            }
        }

        public void NavigateToPattern(Pattern pattern)
        {
            var pageType = Type.GetType("Doji.PatternPages." + pattern.Type);

            if (pageType != null)
            {
                InfoAreaPivot.Items.Clear();
                NavigationFrame.Navigate(pageType, pattern.Name);
            }
        }

        public void RegisterNewCommand(string name, RoutedEventHandler action)
        {
            Commands.Add(new PatternCommand(name, () =>
            {
                try
                {
                    action.Invoke(this, new RoutedEventArgs());
                }
                catch (Exception ex)
                {
                    ExceptionNotification.Show(ex.Message, 3000);
                }
            }));
        }

        public Task StartSearch(string startingText = "")
        {
            return HamburgerMenu.StartSearch(startingText);
        }

        public async Task RefreshXamlRenderAsync()
        {
            if (HamburgerMenu.CurrentPattern != null)
            {
                var code = string.Empty;
                if (InfoAreaPivot.SelectedItem == PropertiesPivotItem)
                {
                    code = HamburgerMenu.CurrentPattern.BindedXamlCode;
                }
                else
                {
                    code = HamburgerMenu.CurrentPattern.UpdatedXamlCode;
                }

                if (!string.IsNullOrWhiteSpace(code))
                {
                    await UpdateXamlRenderAsync(code);
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationFrame.Navigating += NavigationFrame_Navigating;
            NavigationFrame.Navigated += NavigationFrameOnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Get list of patterns
            var patternCategories = (await Patterns.GetCategoriesAsync()).ToList();

            HamburgerMenu.ItemsSource = patternCategories;

            // Options
            HamburgerMenu.OptionsItemsSource = new[]
            {
                new Option { Glyph = "\xE10F", Name = "About", PageType = typeof(About) }
            };

            HideInfoArea();
            NavigationFrame.Navigate(typeof(About));

            if (!string.IsNullOrWhiteSpace(e?.Parameter?.ToString()))
            {
                var parser = DeepLinkParser.Create(e.Parameter.ToString());
                var targetPattern = await Pattern.FindAsync(parser.Root, parser["pattern"]);
                if (targetPattern != null)
                {
                    NavigateToPattern(targetPattern);
                }
            }
        }

        private async void NavigationFrame_Navigating(object sender, NavigatingCancelEventArgs navigationEventArgs)
        {
            ProcessPatternEditorTime();

            PatternCategory category;
            if (navigationEventArgs.Parameter == null)
            {
                DataContext = null;
                HamburgerMenu.CurrentPattern = null;
                category = navigationEventArgs.Parameter as PatternCategory;

                if (category != null)
                {
                    TrackingManager.TrackPage($"{navigationEventArgs.SourcePageType.Name} - {category.Name}");
                }
                else
                {
                    TrackingManager.TrackPage($"{navigationEventArgs.SourcePageType.Name}");
                }

                HideInfoArea();
            }
            else
            {
                TrackingManager.TrackPage(navigationEventArgs.SourcePageType.Name);
                Commands.Clear();
                ShowInfoArea();

                var patternName = navigationEventArgs.Parameter.ToString();
                HamburgerMenu.CurrentPattern = await Patterns.GetPatternByName(patternName);
                DataContext = HamburgerMenu.CurrentPattern;

                if (HamburgerMenu.CurrentPattern == null)
                {
                    HideInfoArea();
                    return;
                }

                category = await Patterns.GetCategoryByPattern(HamburgerMenu.CurrentPattern);
                await Patterns.PushRecentPattern(HamburgerMenu.CurrentPattern);

                var propertyDesc = HamburgerMenu.CurrentPattern.PropertyDescriptor;

                InfoAreaPivot.Items.Clear();

                if (propertyDesc != null)
                {
                    _xamlRenderer.DataContext = propertyDesc.Expando;
                }

                Title.Text = HamburgerMenu.CurrentPattern.Name;

                if (propertyDesc != null && propertyDesc.Options.Count > 0)
                {
                    InfoAreaPivot.Items.Add(PropertiesPivotItem);
                }

                if (HamburgerMenu.CurrentPattern.HasXAMLCode)
                {
                    if (AnalyticsInfo.VersionInfo.GetDeviceFormFactor() != DeviceFormFactor.Desktop || HamburgerMenu.CurrentPattern.DisableXamlEditorRendering)
                    {
                        // Only makes sense (and works) for now to show Live Xaml on Desktop, so fallback to old system here otherwise.
                        XamlReadOnlyCodeRenderer.XamlSource = HamburgerMenu.CurrentPattern.UpdatedXamlCode;

                        InfoAreaPivot.Items.Add(XamlReadOnlyPivotItem);
                    }
                    else
                    {
                        XamlCodeRenderer.Text = HamburgerMenu.CurrentPattern.UpdatedXamlCode;

                        InfoAreaPivot.Items.Add(XamlPivotItem);

                        _xamlCodeRendererSupported = true;
                    }

                    InfoAreaPivot.SelectedIndex = 0;
                }

                if (HamburgerMenu.CurrentPattern.HasCSharpCode)
                {
                    CSharpCodeRenderer.CSharpSource = await this.HamburgerMenu.CurrentPattern.GetCSharpSourceAsync();
                    InfoAreaPivot.Items.Add(CSharpPivotItem);
                }

                if (HamburgerMenu.CurrentPattern.HasJavaScriptCode)
                {
                    JavaScriptCodeRenderer.CSharpSource = await this.HamburgerMenu.CurrentPattern.GetJavaScriptSourceAsync();
                    InfoAreaPivot.Items.Add(JavaScriptPivotItem);
                }

                if (!string.IsNullOrEmpty(HamburgerMenu.CurrentPattern.CodeUrl))
                {
                    GitHub.NavigateUri = new Uri(HamburgerMenu.CurrentPattern.CodeUrl);
                    GitHub.Visibility = Visibility.Visible;
                }
                else
                {
                    GitHub.Visibility = Visibility.Collapsed;
                }

                if (HamburgerMenu.CurrentPattern.HasDocumentation)
                {
                    var docs = await this.HamburgerMenu.CurrentPattern.GetDocumentationAsync();
                    if (!string.IsNullOrWhiteSpace(docs))
                    {
                        DocumentationTextblock.Text = docs;
                        InfoAreaPivot.Items.Add(DocumentationPivotItem);
                    }
                }

                if (InfoAreaPivot.Items.Count == 0)
                {
                    HideInfoArea();
                }

                HamburgerMenu.Title = $"{category.Name} > {HamburgerMenu.CurrentPattern?.Name}";
                ApplicationView.SetTitle(this, $"{category.Name} > {HamburgerMenu.CurrentPattern?.Name}");
            }
        }

        private void HideInfoArea()
        {
            InfoAreaGrid.Visibility = Visibility.Collapsed;
            RootGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            RootGrid.RowDefinitions[1].Height = GridLength.Auto;
            HamburgerMenu.CurrentPattern = null;
            Commands.Clear();
            Splitter.Visibility = Visibility.Collapsed;
            HamburgerMenu.Title = string.Empty;
            ApplicationView.SetTitle(this, string.Empty);
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandOrCloseProperties();
        }

        private void PivotTitle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ExpandOrCloseProperties();
        }

        private void ExpandOrCloseProperties()
        {
            var states = VisualStateManager.GetVisualStateGroups(HamburgerMenu).FirstOrDefault();
            string currentState = states.CurrentState.Name;

            switch (currentState)
            {
                case "NarrowState":
                case "MediumState":
                    // If pane is open, close it
                    if (_isPaneOpen)
                    {
                        Grid.SetRowSpan(InfoAreaGrid, 1);
                        Grid.SetRow(InfoAreaGrid, 1);
                        _isPaneOpen = false;
                        ExpandButton.Content = "";
                    }
                    else
                    {
                        // pane is closed, so let's open it
                        Grid.SetRowSpan(InfoAreaGrid, 2);
                        Grid.SetRow(InfoAreaGrid, 0);
                        _isPaneOpen = true;
                        ExpandButton.Content = "";

                        // Update Read-Only XAML tab when switching back to show changes to TwoWay Bound Properties
                        if (HamburgerMenu.CurrentPattern?.HasXAMLCode == true && InfoAreaPivot.SelectedItem == XamlReadOnlyPivotItem)
                        {
                            XamlReadOnlyCodeRenderer.XamlSource = HamburgerMenu.CurrentPattern.UpdatedXamlCode;
                        }
                    }

                    break;

                case "WideState":
                    // If pane is open, close it
                    if (_isPaneOpen)
                    {
                        Grid.SetColumnSpan(InfoAreaGrid, 1);
                        Grid.SetColumn(InfoAreaGrid, 1);
                        _isPaneOpen = false;
                        ExpandButton.Content = "";
                    }
                    else
                    {
                        // Pane is closed, so let's open it
                        Grid.SetColumnSpan(InfoAreaGrid, 2);
                        Grid.SetColumn(InfoAreaGrid, 0);
                        _isPaneOpen = true;
                        ExpandButton.Content = "";
                    }

                    break;
            }
        }

        /// <summary>
        /// Called when [back requested] event is fired.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="backRequestedEventArgs">The <see cref="BackRequestedEventArgs"/> instance containing the event data.</param>
        private void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            if (backRequestedEventArgs.Handled)
            {
                return;
            }

            if (NavigationFrame.CanGoBack)
            {
                NavigationFrame.GoBack();
                backRequestedEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// When the frame has navigated this method is called.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="navigationEventArgs">The <see cref="NavigationEventArgs"/> instance containing the event data.</param>
        private void NavigationFrameOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = NavigationFrame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;

            if (_isPaneOpen)
            {
                ExpandOrCloseProperties();
            }

            if (HamburgerMenu.CurrentPattern != null && HamburgerMenu.CurrentPattern.HasXAMLCode)
            {
                this._lastRenderedProperties = true;

                // Called to load the pattern initially as we don't get an Item Pivot Selection Changed with Pattern Loaded yet.
                var t = UpdateXamlRenderAsync(HamburgerMenu.CurrentPattern.BindedXamlCode);
            }
        }

        private void HamburgerMenu_OnOptionsItemClick(object sender, ItemClickEventArgs e)
        {
            var option = e.ClickedItem as Option;
            if (option == null)
            {
                return;
            }

            if (NavigationFrame.CurrentSourcePageType != option.PageType)
            {
                NavigationFrame.Navigate(option.PageType);
            }

            HamburgerMenu.IsPaneOpen = false;

            var expanders = HamburgerMenu.FindDescendants<Expander>();
            foreach (var expander in expanders)
            {
                expander.IsExpanded = false;
            }
        }

        private async void InfoAreaPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InfoAreaPivot.SelectedItem != null)
            {
                if (DataContext is Pattern pattern)
                {
                    TrackingManager.TrackEvent("PropertyGrid", (InfoAreaPivot.SelectedItem as FrameworkElement)?.Name, pattern.Name);
                }
            }

            if (HamburgerMenu.CurrentPattern == null)
            {
                return;
            }

            if (InfoAreaPivot.SelectedItem == PropertiesPivotItem)
            {
                // If we switch to the Properties Panel, we want to use a binded version of the Xaml Code.
                if (HamburgerMenu.CurrentPattern.HasXAMLCode)
                {
                    _lastRenderedProperties = true;

                    var t = UpdateXamlRenderAsync(HamburgerMenu.CurrentPattern.BindedXamlCode);
                }

                return;
            }

            if (HamburgerMenu.CurrentPattern.HasXAMLCode && InfoAreaPivot.SelectedItem == XamlPivotItem && _lastRenderedProperties)
            {
                // Use this flag so we don't re-render the XAML tab if we're switching from tabs other than the properties one.
                _lastRenderedProperties = false;

                // If we switch to the Live Preview, then we want to use the Value based Text
                XamlCodeRenderer.Text = HamburgerMenu.CurrentPattern.UpdatedXamlCode;

                var t = UpdateXamlRenderAsync(HamburgerMenu.CurrentPattern.UpdatedXamlCode);
                await XamlCodeRenderer.RevealPositionAsync(new Position(1, 1));

                XamlCodeRenderer.Focus(FocusState.Programmatic);
                return;
            }

            if (HamburgerMenu.CurrentPattern.HasXAMLCode && InfoAreaPivot.SelectedItem == XamlReadOnlyPivotItem)
            {
                // Update Read-Only XAML tab on non-desktop devices to show changes to Properties
                XamlReadOnlyCodeRenderer.XamlSource = HamburgerMenu.CurrentPattern.UpdatedXamlCode;
            }

            if (HamburgerMenu.CurrentPattern.HasCSharpCode && InfoAreaPivot.SelectedItem == CSharpPivotItem)
            {
                CSharpCodeRenderer.CSharpSource = await HamburgerMenu.CurrentPattern.GetCSharpSourceAsync();

                return;
            }

            if (HamburgerMenu.CurrentPattern.HasJavaScriptCode && InfoAreaPivot.SelectedItem == JavaScriptPivotItem)
            {
                JavaScriptCodeRenderer.JavaScriptSource = await HamburgerMenu.CurrentPattern.GetJavaScriptSourceAsync();

                return;
            }
        }

        private async void DocumentationTextblock_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            TrackingManager.TrackEvent("Link", e.Link);
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void DocumentationTextblock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            e.Image = new BitmapImage(new Uri("ms-appx:///Assets/pixel.png"));
            e.Handled = true;
        }

        private void GitHub_OnClick(object sender, RoutedEventArgs e)
        {
            TrackingManager.TrackEvent("Link", GitHub.NavigateUri.ToString());
        }

        private void HamburgerMenu_PatternPickerItemClick(object sender, ItemClickEventArgs e)
        {
            NavigateToPattern(e.ClickedItem as Pattern);
        }

        private Visibility GreaterThanZero(int value)
        {
            return value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private CssLineStyle _errorStyle = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xD6, 0xD6))
        };

        private CssGlyphStyle _errorIconStyle = new CssGlyphStyle()
        {
            GlyphImage = new Uri("ms-appx-web:///Icons/Error.png")
        };

        private async Task UpdateXamlRenderAsync(string text)
        {
            // Hide any Previous Errors
            XamlCodeRenderer.Decorations.Clear();
            XamlCodeRenderer.Options.GlyphMargin = false;

            // Try and Render Xaml to a UIElement
            UIElement element = null;
            try
            {
                element = _xamlRenderer.Render(text);
            }
            catch (Exception ex)
            {
                ExceptionNotification.Show(ex.Message, 3000);
            }

            if (element != null)
            {
                // Add element to main panel
                var content = NavigationFrame.Content as Page;
                var root = content.FindDescendantByName("XamlRoot");

                if (root is Panel)
                {
                    // If we've defined a 'XamlRoot' element to host us as a panel, use that.
                    (root as Panel).Children.Clear();
                    (root as Panel).Children.Add(element);
                }
                else
                {
                    // Otherwise, just replace the entire page's content
                    content.Content = element;
                }

                // Tell the page we've finished with an update to the XAML contents, after the control has rendered.
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    (content as IXamlRenderListener)?.OnXamlRendered(element as FrameworkElement);
                });
            }
            else if (_xamlRenderer.Errors.Count > 0)
            {
                var error = _xamlRenderer.Errors.First();

                XamlCodeRenderer.Options.GlyphMargin = true;

                var range = new Range(error.StartLine, 1, error.EndLine, await XamlCodeRenderer.GetModel().GetLineMaxColumnAsync(error.EndLine));

                // Highlight Error Line
                XamlCodeRenderer.Decorations.Add(new IModelDeltaDecoration(
                    range,
                    new IModelDecorationOptions() { IsWholeLine = true, ClassName = _errorStyle, HoverMessage = new string[] { error.Message } }));

                // Show Glyph Icon
                XamlCodeRenderer.Decorations.Add(new IModelDeltaDecoration(
                    range,
                    new IModelDecorationOptions() { IsWholeLine = true, GlyphMarginClassName = _errorIconStyle, GlyphMarginHoverMessage = new string[] { error.Message } }));
            }
        }

        private static readonly int[] NonCharacterCodes = new int[]
        {
            // Modifier Keys
            16, 17, 18, 20, 91,

            // Esc / Page Keys / Home / End / Insert
            27, 33, 34, 35, 36, 45,

            // Arrow Keys
            37, 38, 39, 40,

            // Function Keys
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123
        };

        private void XamlCodeRenderer_KeyDown(Monaco.CodeEditor sender, Monaco.Helpers.WebKeyEventArgs args)
        {
            // Handle Shortcuts.
            // Ctrl+Enter or F5 Update // TODO: Do we need this in the app handler too? (Thinking no)
            if ((args.KeyCode == 13 && args.CtrlKey) ||
                 args.KeyCode == 116)
            {
                var t = UpdateXamlRenderAsync(XamlCodeRenderer.Text);

                // Eat key stroke
                args.Handled = true;
            }

            // Ignore as a change to the document if we handle it as a shortcut above or it's a special char.
            if (!args.Handled && Array.IndexOf(NonCharacterCodes, args.KeyCode) == -1)
            {
                // TODO: Mark Dirty here if we want to prevent overwrites.

                // Setup Time for Auto-Compile
                this._autocompileTimer?.Cancel(); // Stop Old Timer

                // Create Compile Timer
                this._autocompileTimer = ThreadPoolTimer.CreateTimer(
                    async (e) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            var t = UpdateXamlRenderAsync(XamlCodeRenderer.Text);

                            if (_timePatternEditedFirst == DateTime.MinValue)
                            {
                                _timePatternEditedFirst = DateTime.Now;
                            }

                            _timePatternEditedLast = DateTime.Now;
                        });
                    }, TimeSpan.FromSeconds(0.5));
            }
        }

        private void XamlCodeRenderer_Loading(object sender, RoutedEventArgs e)
        {
            XamlCodeRenderer.Options.Folding = true;
        }

        private void XamlCodeRenderer_InternalException(CodeEditor sender, Exception args)
        {
            TrackingManager.TrackException(args);

            // If you hit an issue here, please report repro steps along with all the info from the Exception object.
#if DEBUG
            Debugger.Break();
#endif
        }

        private void ProcessPatternEditorTime()
        {
            if (HamburgerMenu.CurrentPattern != null &&
                HamburgerMenu.CurrentPattern.HasXAMLCode &&
                _xamlCodeRendererSupported)
            {
                if (_timePatternEditedFirst != DateTime.MinValue &&
                    _timePatternEditedLast != DateTime.MinValue)
                {
                    int secondsEdditingPattern = (int)Math.Floor((_timePatternEditedLast - _timePatternEditedFirst).TotalSeconds);
                    TrackingManager.TrackEvent("xamleditor", "edited", HamburgerMenu.CurrentPattern.Name, secondsEdditingPattern);
                }
                else
                {
                    TrackingManager.TrackEvent("xamleditor", "not_edited", HamburgerMenu.CurrentPattern.Name);
                }
            }

            _timePatternEditedFirst = _timePatternEditedLast = DateTime.MinValue;
        }
    }
}