using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _2.semEksamenProjekt
{
    public partial class FlowOverviewWindow : Window
    {
        IRepositoryFactory factory;
        FlowRepository flowRepository;
        FlowOverview overview = new FlowOverview();

        public FlowOverviewWindow()
        {
            factory = new RepositoryFactory();
            flowRepository = factory.CreateFlowRepository();
            InitializeComponent();
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // skjul tilføj knap hvis brugeren ikke kan redigere flows
            if (!Session.Instance.CanEditFlows)
            {
                AddFlowButton.Visibility = Visibility.Collapsed;
            }

            // indlæs tags i filter dropdown
            TagFilter.Items.Add("Alle tags");
            foreach (string tag in factory.CreateTagRepository().GetFlowTags())
            {
                TagFilter.Items.Add(tag);
            }

            TagFilter.SelectedIndex = 0;

            Reload();
        }

        // genindlæs flows fra databasen og tegn kortene igen
        public async void Reload()
        {
            // hent flows fra databasen i baggrunden
            await Task.Run(() =>
            {
                overview.AllFlows = flowRepository.GetAllFlows();
                overview.SortByTitle();
            });

            // opdater UI når data er hentet
            DrawFlowCards();
        }

        // opdaterer flow oversigten når tag filter ændres
        void TagFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            DrawFlowCards();
        }

        void GoToSkema_Click(object sender, RoutedEventArgs e)
        {
            new EventOverviewWindow().Show();
            Close();
        }

        void NewFlow_Click(object sender, RoutedEventArgs e)
        {
            new NewFlowWindow(flowRepository, this).Show();
        }

        void DrawFlowCards()
        {
            FlowsPanel.Children.Clear();

            string selectedTag = TagFilter.SelectedIndex > 0 ? TagFilter.SelectedItem.ToString() : null;

            int i = 0;
            foreach (Flow flow in overview.AllFlows)
            {
                if (Session.Instance.CanSeeFlow(flow) && (selectedTag == null || (flow.tags != null && flow.tags.Contains(selectedTag))))
                {
                    FlowsPanel.Children.Add(CreateFlowCard(flow, i++));
                }
            }
        }

        Border CreateFlowCard(Flow flow, int index = 0)
        {
            // brug gemt farve fra databasen
            Brush background = flow.color != null ? (Brush)new SolidColorBrush((Color)ColorConverter.ConvertFromString(flow.color)) : Brushes.White;

            Border card = new Border
            {
                Width = 250,
                Height = 120,
                Background = background,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(6),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            TextBlock title = new TextBlock
            {
                Text = flow.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            card.Child = title;

            // venstreklik åbner FlowWindow
            card.MouseLeftButtonUp += (s, args) =>
            {
                new FlowWindow(flow).Show();
                Close();
            };

            // højreklik menu kun for admin og underviser
            if (Session.Instance.CanEditFlows)
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem editItem = new MenuItem { Header = "Rediger" };
                editItem.Click += (s, args) =>
                {
                    new NewFlowWindow(flowRepository, this, flow).Show();
                };

                MenuItem deleteItem = new MenuItem { Header = "Slet" };
                deleteItem.Click += (s, args) =>
                {
                    if (MessageBox.Show($"Vil du slette '{flow.Title}'?", "Slet flow",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        flowRepository.DeleteFlow(flow.id);
                        Reload();
                    }
                };

                contextMenu.Items.Add(editItem);
                contextMenu.Items.Add(deleteItem);
                card.ContextMenu = contextMenu;
            }

            return card;
        }
    }
}
