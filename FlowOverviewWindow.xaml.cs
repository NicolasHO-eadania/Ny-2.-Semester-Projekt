using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _2.semEksamenProjekt
{
    public partial class FlowOverviewWindow : Window
    {
        FlowRepository flowRepository = new FlowRepository();
        FlowOverview overview = new FlowOverview();

        public FlowOverviewWindow()
        {
            InitializeComponent();
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            overview.AllFlows = flowRepository.GetAllFlows();
            DrawFlowCards();
        }

        // skifter til skema vinduet
        void GoToSkema_Click(object sender, RoutedEventArgs e)
        {
            new EventOverviewWindow().Show();
            Close();
        }

        // tegner et kort for hvert flow i wrappanelet
        void DrawFlowCards()
        {
            FlowsPanel.Children.Clear();

            foreach (Flow flow in overview.AllFlows)
            {
                FlowsPanel.Children.Add(CreateFlowCard(flow));
            }
        }

        // opretter et enkelt flow kort
        Border CreateFlowCard(Flow flow)
        {
            // ydre ramme
            Border card = new Border
            {
                Width           = 220,
                Height          = 120,
                BorderBrush     = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(6)
            };

            // vandret layout: billede til venstre, titel til højre
            StackPanel content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(8)
            };

            // billedpladsholder
            Border imagePlaceholder = new Border
            {
                Width           = 80,
                Height          = 80,
                BorderBrush     = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(0, 0, 10, 0)
            };

            // titel
            TextBlock title = new TextBlock
            {
                Text                = flow.Title,
                FontWeight          = FontWeights.Bold,
                FontSize            = 13,
                TextWrapping        = TextWrapping.Wrap,
                VerticalAlignment   = VerticalAlignment.Center,
                Width               = 100
            };

            content.Children.Add(imagePlaceholder);
            content.Children.Add(title);
            card.Child = content;

            // åbn FlowWindow når man klikker på kortet
            card.MouseUp += (sender, e) =>
            {
                new FlowWindow(flow).Show();
                Close();
            };

            card.Cursor = System.Windows.Input.Cursors.Hand;

            return card;
        }
    }
}
