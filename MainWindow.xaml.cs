using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace CyberWareASM
{
    public partial class MainWindow : Window
    {
        // ──────────────────────────────────────────────────────────────
        //  FIELDS
        //  MainWindow owns both MemoryStore and ChatBot.
        //  MemoryStore is created first so we can populate it before
        //  passing it into ChatBot — and so we can read it back later
        //  (e.g. to update the sidebar FavouriteTopic label).
        // ──────────────────────────────────────────────────────────────
        private MemoryStore _memory = new MemoryStore();
        private ChatBot _chatBot = null!;
        private AudioPlayer _audio = new AudioPlayer();

        // ──────────────────────────────────────────────────────────────
        //  CONSTRUCTOR
        // ──────────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();
            // Focus the first text box as soon as the window is ready
            Loaded += (_, _) => TxtFirstName.Focus();
        }

        // ──────────────────────────────────────────────────────────────
        //  NAME SCREEN — SUBMIT BUTTON
        // ──────────────────────────────────────────────────────────────
        private void BtnSubmitName_Click(object sender, RoutedEventArgs e)
        {
            string firstName = TxtFirstName.Text.Trim();
            string surname = TxtSurname.Text.Trim();

            // ── Validation: both fields must be filled ────────────────
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(surname))
            {
                TxtValidation.Visibility = Visibility.Visible;
                return;
            }
            TxtValidation.Visibility = Visibility.Collapsed;

            // ── Populate MemoryStore ──────────────────────────────────
            _memory.FirstName = firstName;
            _memory.LastName = surname;

            // ── Initialise ChatBot with the shared MemoryStore ────────
            // ChatBot(MemoryStore memory) — matches the constructor signature
            _chatBot = new ChatBot(_memory);

            // ── Update sidebar user info label ────────────────────────
            TxtSidebarName.Text = _memory.FullName;   // FirstName + " " + LastName

            // ── Transition: hide name screen, reveal chat screen ──────
            NameScreen.Visibility = Visibility.Collapsed;
            ChatScreen.Visibility = Visibility.Visible;

            // ── Play Ahlumile.wav from bin/Debug/net10.0 ─────────────
            // AudioPlayer.PlayGreeting() calls PlaySound("Ahlumile.wav")
            // which resolves the path via AppDomain.CurrentDomain.BaseDirectory
            _audio.PlayGreeting();

            // ── Show greeting message in the chat window ──────────────
            AppendBotMessage(_chatBot.GetGreeting());

            TxtUserInput.Focus();
        }

        // ──────────────────────────────────────────────────────────────
        //  SEND BUTTON CLICK
        // ──────────────────────────────────────────────────────────────
        private void BtnSend_Click(object sender, RoutedEventArgs e) => SendMessage();

        // ──────────────────────────────────────────────────────────────
        //  ENTER KEY IN INPUT BOX
        // ──────────────────────────────────────────────────────────────
        private void TxtUserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        // ──────────────────────────────────────────────────────────────
        //  END CONVERSATION
        //  Shows the farewell message, disables input, then closes the
        //  app after a short pause so the user can read the message.
        // ──────────────────────────────────────────────────────────────
        private void BtnEndConvo_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage($"🔒  Session terminated. Stay safe out there, {_memory.FirstName}. Goodbye!");
            TxtUserInput.IsEnabled = false;
            BtnSend.IsEnabled = false;
            BtnEndConvo.IsEnabled = false;

            // Wait 2.5 seconds so the user can read the farewell, then exit
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                Application.Current.Shutdown();
            };
            timer.Start();
        }

        // ──────────────────────────────────────────────────────────────
        //  SIDEBAR TOPIC BUTTONS
        //  Each button's Tag holds the raw keyword/topic string that
        //  ChatBot.ProcessInput() / KeywordResponder / TopicStore expects.
        // ──────────────────────────────────────────────────────────────
        private void TopicBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_chatBot == null || sender is not Button btn) return;

            // Use the Tag value so ProcessInput receives the exact keyword
            string query = btn.Tag?.ToString() ?? btn.Content?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query)) return;

            AppendUserMessage(btn.Content?.ToString() ?? query);
            string response = _chatBot.ProcessInput(query);
            AppendBotMessage(response);
            SyncSidebarTopic();
        }

        // ──────────────────────────────────────────────────────────────
        //  SIDEBAR QUICK-ASK BUTTONS
        // ──────────────────────────────────────────────────────────────
        private void QuickBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_chatBot == null || sender is not Button btn) return;

            string query = btn.Tag?.ToString() ?? btn.Content?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query)) return;

            AppendUserMessage(btn.Content?.ToString() ?? query);
            string response = _chatBot.ProcessInput(query);
            AppendBotMessage(response);
            SyncSidebarTopic();
        }

        // ──────────────────────────────────────────────────────────────
        //  CORE: SEND A MESSAGE
        // ──────────────────────────────────────────────────────────────
        private void SendMessage()
        {
            if (_chatBot == null) return;

            string userText = TxtUserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userText)) return;

            AppendUserMessage(userText);
            TxtUserInput.Clear();

            // ChatBot.ProcessInput(string) — matches signature in ChatBot.cs
            string botResponse = _chatBot.ProcessInput(userText);
            AppendBotMessage(botResponse);

            SyncSidebarTopic();
        }

        // ──────────────────────────────────────────────────────────────
        //  APPEND BOT MESSAGE BUBBLE  (left-aligned, robot avatar)
        // ──────────────────────────────────────────────────────────────
        private void AppendBotMessage(string text)
        {
            var row = new Grid { Margin = new Thickness(0, 6, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // robot avatar
            var avatar = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Margin = new Thickness(0, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new LinearGradientBrush(
                    Color.FromRgb(59, 130, 246),
                    Color.FromRgb(168, 85, 247),
                    new Point(0, 0), new Point(1, 1))
            };
            avatar.Child = new TextBlock
            {
                Text = "🤖",
                FontSize = 17,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(avatar, 0);

            // bubble
            var bubble = new Border
            {
                CornerRadius = new CornerRadius(4, 16, 16, 16),
                Padding = new Thickness(16, 10, 16, 10),
                Background = new SolidColorBrush(Color.FromRgb(18, 18, 46)),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 0,
                    Color = Color.FromRgb(59, 130, 246),
                    Opacity = 0.14
                }
            };
            bubble.Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };
            Grid.SetColumn(bubble, 1);

            row.Children.Add(avatar);
            row.Children.Add(bubble);
            ChatMessagesPanel.Children.Add(row);
            ScrollToBottom();
        }

        // ──────────────────────────────────────────────────────────────
        //  APPEND USER MESSAGE BUBBLE  (right-aligned, person avatar)
        // ──────────────────────────────────────────────────────────────
        private void AppendUserMessage(string text)
        {
            var row = new Grid { Margin = new Thickness(0, 6, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // bubble (right side)
            var bubble = new Border
            {
                CornerRadius = new CornerRadius(16, 4, 16, 16),
                Padding = new Thickness(16, 10, 16, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new LinearGradientBrush(
                    Color.FromRgb(30, 27, 75),
                    Color.FromRgb(45, 27, 105),
                    new Point(0, 0), new Point(1, 1)),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 0,
                    Color = Color.FromRgb(168, 85, 247),
                    Opacity = 0.20
                }
            };
            bubble.Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };
            Grid.SetColumn(bubble, 1);

            // person avatar
            var avatar = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Margin = new Thickness(10, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new LinearGradientBrush(
                    Color.FromRgb(236, 72, 153),
                    Color.FromRgb(168, 85, 247),
                    new Point(0, 0), new Point(1, 1))
            };
            avatar.Child = new TextBlock
            {
                Text = "👤",
                FontSize = 17,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(avatar, 2);

            row.Children.Add(bubble);
            row.Children.Add(avatar);
            ChatMessagesPanel.Children.Add(row);
            ScrollToBottom();
        }

        // ──────────────────────────────────────────────────────────────
        //  HELPERS
        // ──────────────────────────────────────────────────────────────

        /// <summary>Scrolls the chat panel to the latest message.</summary>
        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                new Action(() => ChatScrollViewer.ScrollToBottom()));
        }

        /// <summary>
        /// Reads FavouriteTopic directly from the shared MemoryStore and
        /// updates the sidebar label.  No need to go through ChatBot because
        /// MainWindow owns the same MemoryStore instance.
        /// </summary>
        private void SyncSidebarTopic()
        {
            // _memory is the exact same object ChatBot holds internally,
            // so any write ChatBot does to FavouriteTopic is visible here.
            TxtSidebarTopic.Text = string.IsNullOrWhiteSpace(_memory.FavouriteTopic)
                ? "Not set yet"
                : _memory.FavouriteTopic;
        }
    }
}