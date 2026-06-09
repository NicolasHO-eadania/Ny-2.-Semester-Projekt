using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _2.semEksamenProjekt
{
    public partial class FlowWindow : Window
    {
        Flow flow;
        SubFlowRepository subFlowRepository = new SubFlowRepository();
        List<SubFlow> allSubFlows;

        public FlowWindow(Flow flow)
        {
            InitializeComponent();
            this.flow = flow;
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            FlowTitle.Text = flow.Title;

            // hent alle subflows for dette flow
            allSubFlows = subFlowRepository.GetSubFlowsByFlowId(flow.id);

            // byg træstrukturen fra topniveau subflows (ParentId == null)
            foreach (SubFlow subFlow in allSubFlows.Where(sf => sf.parentId == null))
            {
                SubFlowTree.Items.Add(CreateTreeItem(subFlow));
            }
        }

        // opretter et TreeViewItem for et subflow og tilføjer børn rekursivt
        TreeViewItem CreateTreeItem(SubFlow subFlow)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = subFlow.heading,
                Tag    = subFlow  // gem subflow objektet så vi kan hente det ved klik
            };

            // find og tilføj alle børn rekursivt
            foreach (SubFlow child in allSubFlows.Where(sf => sf.parentId == subFlow.id))
            {
                item.Items.Add(CreateTreeItem(child));
            }

            return item;
        }

        // viser indholdet af det valgte subflow i højre side
        void SubFlowTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SubFlowTree.SelectedItem is TreeViewItem selectedItem)
            {
                SubFlow subFlow = (SubFlow)selectedItem.Tag;
                ContentHeading.Text = subFlow.heading;
                ContentText.Text    = subFlow.text ?? "";
            }
        }

        // navigation
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
