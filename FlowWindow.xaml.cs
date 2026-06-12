using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _2.semEksamenProjekt
{
    public partial class FlowWindow : Window
    {
        Flow flow;
        IRepositoryFactory factory = new RepositoryFactory();
        SubFlowRepository subFlowRepository;
        List<SubFlow> allSubFlows;

        public FlowWindow(Flow flow)
        {
            factory = new RepositoryFactory();
            subFlowRepository = factory.CreateSubFlowRepository();
            this.flow = flow;
            InitializeComponent();
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        // genindlæs subflows fra databasen
        public async void Reload()
        {
            FlowTitle.Text = flow.Title;

            // skjul tilføj knap hvis brugeren ikke kan redigere dette flow
            AddSubFlowButton.Visibility = Session.Instance.CanEditSubFlows(flow) ? Visibility.Visible : Visibility.Collapsed;

            // hent subflows fra databasen i baggrunden
            allSubFlows = await Task.Run(() => subFlowRepository.GetSubFlowsByFlowId(flow.id));

            // opdater UI når data er hentet
            SubFlowTree.Items.Clear();
            ContentHeading.Text = "";
            ContentText.Text = "";

            foreach (SubFlow subFlow in allSubFlows.Where(sf => sf.parentId == null))
            {
                SubFlowTree.Items.Add(CreateTreeItem(subFlow));
            }

        }

        // opretter et TreeViewItem med højreklik menu og tilføjer children rekursivt
        TreeViewItem CreateTreeItem(SubFlow subFlow)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = subFlow.heading,
                Tag = subFlow
            };

            // højreklik menu kun hvis brugeren kan redigere dette flow
            if (Session.Instance.CanEditSubFlows(flow))
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem editItem = new MenuItem { Header = "Rediger" };
                editItem.Click += (s, args) =>
                {
                    new NewSubFlowWindow(subFlowRepository, this, flow.id, subFlow).Show();
                };

                MenuItem deleteItem = new MenuItem { Header = "Slet" };
                deleteItem.Click += (s, args) =>
                {
                    if (MessageBox.Show($"Vil du slette '{subFlow.heading}'?", "Slet subflow",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        subFlowRepository.DeleteSubFlow(subFlow.id);
                        Reload();
                    }
                };

                contextMenu.Items.Add(editItem);
                contextMenu.Items.Add(deleteItem);
                item.ContextMenu = contextMenu;
            }

            // tilføj children rekursivt
            foreach (SubFlow child in allSubFlows.Where(sf => sf.parentId == subFlow.id))
            {
                item.Items.Add(CreateTreeItem(child));
            }

            return item;
        }

        // viser indholdet af det valgte subflow
        void SubFlowTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SubFlowTree.SelectedItem is TreeViewItem selectedItem)
            {
                SubFlow subFlow = (SubFlow)selectedItem.Tag;
                ContentHeading.Text = subFlow.heading;
                ContentText.Text = subFlow.text ?? "";
            }
        }

        // åbner vindue til at tilføje nyt subflow
        void NewSubFlow_Click(object sender, RoutedEventArgs e)
        {
            new NewSubFlowWindow(subFlowRepository, this, flow.id).Show();
        }

        void GoToSkema_Click(object sender, RoutedEventArgs e)
        {
            new EventOverviewWindow().Show();
            Close();
        }

        void GoToFlows_Click(object sender, RoutedEventArgs e)
        {
            new FlowOverviewWindow().Show();
            Close();
        }

        void GoBack_Click(object sender, RoutedEventArgs e)
        {
            new FlowOverviewWindow().Show();
            Close();
        }
    }
}
