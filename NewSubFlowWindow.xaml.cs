using System.Collections.Generic;
using System.Windows;

namespace _2.semEksamenProjekt
{
    public partial class NewSubFlowWindow : Window
    {
        SubFlowRepository subFlowRepository;
        FlowWindow flowWindow;
        int flowId;
        SubFlow existingSubFlow; // null hvis det er et nyt subflow

        List<SubFlow> allSubFlows;

        // constructor til nyt subflow
        public NewSubFlowWindow(SubFlowRepository subFlowRepository, FlowWindow flowWindow, int flowId)
        {
            InitializeComponent();
            this.subFlowRepository = subFlowRepository;
            this.flowWindow = flowWindow;
            this.flowId = flowId;
        }

        // constructor til redigering
        public NewSubFlowWindow(SubFlowRepository subFlowRepository, FlowWindow flowWindow, int flowId, SubFlow subFlow)
        {
            InitializeComponent();
            this.subFlowRepository = subFlowRepository;
            this.flowWindow = flowWindow;
            this.flowId = flowId;
            this.existingSubFlow = subFlow;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadParentDropdown();

            if (existingSubFlow != null)
            {
                PrefillFields();
            }
        }

        // dropdown med eksisterende subflows
        void LoadParentDropdown()
        {
            allSubFlows = subFlowRepository.GetSubFlowsByFlowId(flowId);

            ParentInput.Items.Add("(ingen – topniveau)");

            foreach (SubFlow sf in allSubFlows)
            {
                // undgå at man kan vælge sig selv som forælder
                if (existingSubFlow == null || sf.id != existingSubFlow.id)
                {
                    ParentInput.Items.Add(sf);
                }
            }

            ParentInput.SelectedIndex = 0;
        }

        void PrefillFields()
        {
            Title = "Rediger subflow";

            HeadingInput.Text = existingSubFlow.heading;
            TextInput.Text = existingSubFlow.text ?? "";

            if (existingSubFlow.parentId != null)
            {
                SubFlow parent = allSubFlows.Find(sf => sf.id == existingSubFlow.parentId);
                if (parent != null)
                {
                    ParentInput.SelectedItem = parent;
                }
            }
        }

        void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HeadingInput.Text))
            {
                MessageBox.Show("Overskrift er påkrævet.");
                return;
            }

            SubFlow sf = new SubFlow
            {
                flowId = flowId,
                heading = HeadingInput.Text,
                text = TextInput.Text
            };

            // sæt parentId hvis en forælder er valgt
            if (ParentInput.SelectedItem is SubFlow selectedParent)
            {
                sf.parentId = selectedParent.id;
            }

            if (existingSubFlow != null)
            {
                sf.id = existingSubFlow.id;
                subFlowRepository.UpdateSubFlow(sf);
            }
            else
            {
                subFlowRepository.NewSubFlow(sf);
            }

            flowWindow.Reload();
            Close();
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
