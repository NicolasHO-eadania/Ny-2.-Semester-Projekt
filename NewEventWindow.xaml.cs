using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace _2.semEksamenProjekt
{
    public partial class NewEventWindow : Window
    {
        IRepositoryFactory factory = new RepositoryFactory();
        EventRepository repository;
        EventOverviewWindow overviewWindow;
        Event existingEvent; // null hvis det er et nyt event

        List<User> allUsers;
        List<Team> allTeams;
        List<Flow> allFlows;

        // constructor til nyt event
        public NewEventWindow(EventRepository repository, EventOverviewWindow overviewWindow)
        {
            InitializeComponent();
            this.repository = repository;
            this.overviewWindow = overviewWindow;
        }

        // constructor til redigering - udfylder felterne med eksisterende data
        public NewEventWindow(EventRepository repository, EventOverviewWindow overviewWindow, Event ev)
        {
            InitializeComponent();
            this.repository     = repository;
            this.overviewWindow = overviewWindow;
            this.existingEvent  = ev;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDropdowns();

            if (existingEvent != null)
            {
                PrefillFields();
            }
        }

        // fylder lister og combobox med data fra databasen
        void LoadDropdowns()
        {
            // undervisere
            allUsers = factory.CreateUserRepository().GetUsersByRole("Underviser");
            foreach (User user in allUsers)
            {
                TeachersList.Items.Add(user.username);
            }

            // hold
            allTeams = factory.CreateTeamRepository().GetAllTeams();
            foreach (Team team in allTeams)
            {
                TeamsList.Items.Add(team.teamName);
            }

            // flows
            allFlows = factory.CreateFlowRepository().GetAllFlows();
            FlowInput.Items.Add("");
            foreach (Flow flow in allFlows)
            {
                FlowInput.Items.Add(flow.Title);
            }

            foreach (string tag in factory.CreateTagRepository().GetEventTags())
            {
                TagsList.Items.Add(tag);
            }

            FlowInput.SelectedIndex = 0;
        }

        // udfylder felterne når man redigerer et eksisterende event
        void PrefillFields()
        {
            Title = "Rediger event";

            TitleInput.Text = existingEvent.title;
            DescriptionInput.Text = existingEvent.description;
            DateInput.SelectedDate = existingEvent.start.Date;
            StartTimeInput.Text = existingEvent.start.ToString("HH:mm");
            EndTimeInput.Text = existingEvent.end.ToString("HH:mm");
            CityInput.Text = existingEvent.city;

            if (existingEvent.rooms != null && existingEvent.rooms.Count > 0)
            {
                RoomInput.Text = existingEvent.rooms[0];
            }

            // marker eksisterende undervisere i listen
            if (existingEvent.teachers != null)
            {
                foreach (User teacher in existingEvent.teachers)
                {
                    int index = TeachersList.Items.IndexOf(teacher.username);
                    if (index >= 0)
                    {
                        TeachersList.SelectedItems.Add(TeachersList.Items[index]);
                    }
                }
            }

            // marker eksisterende hold i listen
            if (existingEvent.teams != null)
            {
                foreach (Team team in existingEvent.teams)
                {
                    int index = TeamsList.Items.IndexOf(team.teamName);
                    if (index >= 0)
                    {
                        TeamsList.SelectedItems.Add(TeamsList.Items[index]);
                    }
                }
            }

            // vælg det tilknyttede flow i comboboxen
            if (existingEvent.flowId != null)
            {
                Flow flow = allFlows.Find(f => f.id == existingEvent.flowId);
                if (flow != null)
                {
                    FlowInput.SelectedItem = flow.Title;
                }
            }

            if (existingEvent.tags != null)
            {
                foreach (string tag in existingEvent.tags)
                {
                    int i = TagsList.Items.IndexOf(tag);
                    if (i >= 0) TagsList.SelectedItems.Add(TagsList.Items[i]);
                }
            }
        }

        // gemmer event og lukker vinduet
        void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleInput.Text))
            {
                MessageBox.Show("Titel er påkrævet.");
                return;
            }

            if (DateInput.SelectedDate == null)
            {
                MessageBox.Show("Dato er påkrævet.");
                return;
            }

            if (!TimeSpan.TryParse(StartTimeInput.Text, out TimeSpan startTime))
            {
                MessageBox.Show("Ugyldigt starttidspunkt. Skriv fx 08:00.");
                return;
            }

            if (!TimeSpan.TryParse(EndTimeInput.Text, out TimeSpan endTime))
            {
                MessageBox.Show("Ugyldigt sluttidspunkt. Skriv fx 10:00.");
                return;
            }

            DateTime start = DateInput.SelectedDate.Value.Add(startTime);
            DateTime end   = DateInput.SelectedDate.Value.Add(endTime);

            if (end <= start)
            {
                MessageBox.Show("Sluttidspunkt skal være efter starttidspunkt.");
                return;
            }

            Event ev = new Event
            {
                title = TitleInput.Text,
                description = DescriptionInput.Text,
                start = start,
                end = end,
                city = CityInput.Text
            };

            // lokale
            if (!string.IsNullOrWhiteSpace(RoomInput.Text))
            {
                ev.rooms = new List<string> { RoomInput.Text };
            }

            // undervisere
            ev.teachers = new List<User>();
            foreach (string username in TeachersList.SelectedItems)
            {
                ev.teachers.Add(new User { username = username });
            }

            // hold 
            ev.teams = new List<Team>();
            foreach (string teamName in TeamsList.SelectedItems)
            {
                ev.teams.Add(new Team { teamName = teamName });
            }

            // flow 
            if (FlowInput.SelectedIndex > 0)
            {
                string selectedTitle = FlowInput.SelectedItem.ToString();
                Flow selectedFlow = allFlows.Find(f => f.Title == selectedTitle);
                if (selectedFlow != null)
                {
                    ev.flowId = selectedFlow.id;
                }
            }

            // tags
            ev.tags = new List<string>();
            foreach (string tag in TagsList.SelectedItems)
            {
                ev.tags.Add(tag);
            }

            if (existingEvent != null)
            {
                ev.id = existingEvent.id;
                repository.UpdateEvent(ev);
            }
            else
            {
                repository.NewEvent(ev);
            }

            overviewWindow.LoadFromDatabase();
            overviewWindow.RedrawSchedule();

            Close();
        }

        // lukker uden at gemme
        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
