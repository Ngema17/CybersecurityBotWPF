using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CybersecurityBotWPF.Models;
using CybersecurityBotWPF.Services;

namespace CybersecurityBotWPF
{
    public partial class MainWindow : Window
    {
        private readonly ResponseService _responseService;
        private readonly AudioPlayer _audioPlayer;
        private readonly SentimentAnalyzer _sentimentAnalyzer;
        private UserProfile _user;
        private bool _waitingForName;

        public MainWindow()
        {
            InitializeComponent();
            _responseService = new ResponseService();
            _audioPlayer = new AudioPlayer();
            _sentimentAnalyzer = new SentimentAnalyzer();
            _user = new UserProfile();
            _waitingForName = true;

            Loaded += MainWindow_Loaded;
            UserInputBox.KeyDown += UserInputBox_KeyDown;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await AddBotMessage("Welcome to the Cybersecurity Awareness Bot!", "#64C8FF");
            await AddBotMessage("I am here to help you stay safe online.", "#96FF96");

            AudioPlayer.PlayGreeting("greeting.wav");

            await AddBotMessage("Please enter your name:", "#FFD700");
            UserInputBox.Focus();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ProcessUserInput();
        }

        private async void UserInputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                await ProcessUserInput();
            }
        }

        private async Task ProcessUserInput()
        {
            string input = UserInputBox.Text.Trim();
            UserInputBox.Clear();

            if (string.IsNullOrWhiteSpace(input))
            {
                await AddBotMessage("Please enter something before sending.", "#FF6666");
                return;
            }

            // Handle name input
            if (_waitingForName)
            {
                _user.Name = FormatName(input);
                _waitingForName = false;
                await AddUserMessage(input);
                await AddBotMessage($"Hello, {_user.Name}! Welcome!", "#FF66CC");
                await AddBotMessage("You can ask me about:", "#FFFFFF");
                await AddBotMessage("• Password safety", "#C8C864");
                await AddBotMessage("• Phishing attacks", "#C8C864");
                await AddBotMessage("• Online scams", "#C8C864");
                await AddBotMessage("• Privacy protection", "#C8C864");
                await AddBotMessage("• Safe browsing", "#C8C864");
                await AddDivider();
                await AddBotMessage("Type 'exit' to close the chatbot.", "#C89650");
                await AddDivider();
                return;
            }

            // Handle exit - using OrdinalIgnoreCase to fix CA1862 warning
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                await AddUserMessage(input);
                await AddBotMessage($"Goodbye, {_user.Name}. Stay safe online!", "#66FF66");
                await Task.Delay(1500);
                Application.Current.Shutdown();
                return;
            }

            await AddUserMessage(input);

            // Update status
            StatusText.Text = "● Bot is typing...";
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA500"));

            // Process response
            var sentiment = _sentimentAnalyzer.AnalyzeSentiment(input);
            string response = _responseService.GetResponse(input, _user, sentiment);
            await TypeMessage(response);

            // Reset status
            StatusText.Text = "● Ready";
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64C864"));
        }

        private async Task TypeMessage(string message)
        {
            SendButton.IsEnabled = false;
            UserInputBox.IsEnabled = false;

            var messagePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)  // Fixed: all 4 parameters
            };

            var avatar = new TextBlock
            {
                Text = "🤖 ",
                FontSize = 16,
                Foreground = Brushes.White
            };

            var messageBlock = new TextBlock
            {
                Text = "",
                FontSize = 13,
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 500
            };

            messagePanel.Children.Add(avatar);
            messagePanel.Children.Add(messageBlock);

            await Dispatcher.InvokeAsync(() => ChatPanel.Children.Add(messagePanel));

            // Typing effect
            for (int i = 0; i <= message.Length; i++)
            {
                messageBlock.Text = message.Substring(0, i);
                await Task.Delay(8);
                ScrollToBottom();
            }

            await Dispatcher.InvokeAsync(() => ChatPanel.Children.Add(new TextBlock { Height = 5 }));
            ScrollToBottom();

            SendButton.IsEnabled = true;
            UserInputBox.IsEnabled = true;
            UserInputBox.Focus();
        }

        private async Task AddBotMessage(string message, string colorHex)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var converter = new BrushConverter();
                var messagePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)  // Fixed: all 4 parameters
                };

                var avatar = new TextBlock
                {
                    Text = "🤖 ",
                    FontSize = 16,
                    Foreground = Brushes.White
                };

                var messageBlock = new TextBlock
                {
                    Text = message,
                    FontSize = 13,
                    Foreground = (Brush)converter.ConvertFromString(colorHex),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 500
                };

                messagePanel.Children.Add(avatar);
                messagePanel.Children.Add(messageBlock);
                ChatPanel.Children.Add(messagePanel);
                ChatPanel.Children.Add(new TextBlock { Height = 5 });
                ScrollToBottom();
            });
            await Task.Delay(50);
        }

        private async Task AddUserMessage(string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var messagePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5),  // Fixed: all 4 parameters
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var messageBlock = new TextBlock
                {
                    Text = message,
                    FontSize = 13,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64C8FF")),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 400,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28334A")),
                    Padding = new Thickness(10, 5, 10, 5)  // Fixed: all 4 parameters
                };

                var avatar = new TextBlock
                {
                    Text = " 👤",
                    FontSize = 16,
                    Foreground = Brushes.White
                };

                messagePanel.Children.Add(messageBlock);
                messagePanel.Children.Add(avatar);
                ChatPanel.Children.Add(messagePanel);
                ChatPanel.Children.Add(new TextBlock { Height = 5 });
                ScrollToBottom();
            });
            await Task.Delay(50);
        }

        private async Task AddDivider()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                ChatPanel.Children.Add(new TextBlock
                {
                    Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
                    Foreground = Brushes.Gray,
                    FontSize = 10,
                    Margin = new Thickness(0, 5, 0, 5)  // Fixed: all 4 parameters
                });
                ScrollToBottom();
            });
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToBottom();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private static string FormatName(string name)  // Made static to fix CA1822
        {
            name = name.Trim();
            if (string.IsNullOrEmpty(name)) return "User";
            // Fixed: Simplified substring (IDE0057)
            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}