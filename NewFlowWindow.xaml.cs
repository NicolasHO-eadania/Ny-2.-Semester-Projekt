using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _2.semEksamenProjekt
{
    public partial class NewFlowWindow : Window
    {
        IRepositoryFactory factory = new RepositoryFactory();
        FlowRepository flowRepository;
        FlowOverviewWindow overviewWindow;
        Flow existingFlow;

        List<User> allUsers;
        List<Team> allTeams;
        string selectedColor;

        (string hex, Brush brush)[] colors = new[]
        {
            ("#ADD8E6", (Brush)new SolidColorBrush(Color.FromRgb(173, 216, 230))), // lyseblå
            ("#90EE90", (Brush)new SolidColorBrush(Color.FromRgb(144, 238, 144))), // lysegrøn
            ("#FFD9B3", (Brush)new SolidColorBrush(Color.FromRgb(255, 218, 185))), // lyseorange
            ("#DDA0DD", (Brush)new SolidColorBrush(Color.FromRgb(221, 160, 221))), // lyselilla
            ("#FFFF99", (Brush)new SolidColorBrush(Color.FromRgb(255, 255, 153))), // lysegul
        };

        // constructor til nyt flow
        public NewFlowWindow(FlowRepository flowRepository, FlowOverviewWindow overviewWindow)
        {
            InitializeComponent();
            this.flowRepository = flowRepository;
            this.overviewWindow = overviewWindow;
        }

        // constructor til redigering
        public NewFlowWindow(FlowRepository flowRepository, FlowOverviewWindow overviewWindow, Flow flow)
        {
            InitializeComponent();
            this.flowRepository = flowRepository;
            this.overviewWindow = overviewWindow;
            this.existingFlow = flow;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDropdowns();
            SelectColor("#ADD8E6");

            if (existingFlow != null)
            {
                PrefillFields();
            }
        }

        void LoadDropdowns()
        {
            allTeams = factory.CreateTeamRepository().GetAllTeams();
            TeamInput.Items.Add("");
            foreach (Team team in allTeams)
            {
                TeamInput.Items.Add(team.teamName);
            }
            TeamInput.SelectedIndex = 0;

            allUsers = factory.CreateUserRepository().GetUsersByRole("Underviser");
            foreach (User user in allUsers)
            {
                TeachersList.Items.Add(user.username);
            }

            foreach (string tag in factory.CreateTagRepository().GetFlowTags())
            {
                TagsList.Items.Add(tag);
            }
        }

        void ColorBox_Click(object sender, MouseButtonEventArgs e)
        {
            SelectColor((string)((Border)sender).Tag);
        }

        void SelectColor(string hex)
        {
            selectedColor = hex;

            foreach (Border box in ColorPicker.Children)
            {
                box.BorderBrush = (string)box.Tag == hex ? Brushes.Black : Brushes.Transparent;
            }
        }

        void PrefillFields()
        {
            Title = "Rediger flow";

            TitleInput.Text = existingFlow.Title;

            if (existingFlow.team != null)
            {
                TeamInput.SelectedItem = existingFlow.team.teamName;
            }

            if (existingFlow.teachers != null)
            {
                foreach (User teacher in existingFlow.teachers)
                {
                    int i = TeachersList.Items.IndexOf(teacher.username);
                    if (i >= 0)
                    {
                        TeachersList.SelectedItems.Add(TeachersList.Items[i]);
                    }
                }
            }

            if (existingFlow.tags != null)
            {
                foreach (string tag in existingFlow.tags)
                {
                    int i = TagsList.Items.IndexOf(tag);
                    if (i >= 0)
                    {
                        TagsList.SelectedItems.Add(TagsList.Items[i]);
                    }
                }
            }

            if (existingFlow.color != null)
            {
                SelectColor(existingFlow.color);
            }
        }

        void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleInput.Text))
            {
                MessageBox.Show("Titel er påkrævet.");
                return;
            }

            Flow flow = new Flow
            {
                Title = TitleInput.Text,
                color = selectedColor
            };

            if (TeamInput.SelectedIndex > 0)
            {
                flow.team = new Team { teamName = TeamInput.SelectedItem.ToString() };
            }

            flow.teachers = new List<User>();
            foreach (string username in TeachersList.SelectedItems)
            {
                flow.teachers.Add(new User { username = username });
            }

            flow.tags = new List<string>();
            foreach (string tag in TagsList.SelectedItems)
            {
                flow.tags.Add(tag);
            }

            if (existingFlow != null)
            {
                flow.id = existingFlow.id;
                flowRepository.UpdateFlow(flow);
            }
            else
            {
                // tilføj brugeren automatisk til underviserlisten
                if (!flow.teachers.Exists(t => t.username == Session.Instance.CurrentUser.username))
                {
                    flow.teachers.Add(Session.Instance.CurrentUser);
                }

                flowRepository.NewFlow(flow);
            }

            overviewWindow.Reload();
            Close();
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
