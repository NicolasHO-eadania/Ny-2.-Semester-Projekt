using System;
using System.Collections.Generic;
using System.Windows;

namespace _2.semEksamenProjekt
{
    public partial class NewEventWindow : Window
    {
        EventRepository repository;
        EventOverviewWindow overviewWindow;

        public NewEventWindow(EventRepository repository, EventOverviewWindow overviewWindow)
        {
            InitializeComponent();
            this.repository = repository;
            this.overviewWindow = overviewWindow;
        }

        // gemmer event og lukker vinduet
        void Save_Click(object sender, RoutedEventArgs e)
        {
            // tjek at de påkrævede felter er udfyldt
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

            // forsøg at parse tidspunkterne
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

            // lav start og slut til DateTime
            DateTime start = DateInput.SelectedDate.Value.Add(startTime);
            DateTime end   = DateInput.SelectedDate.Value.Add(endTime);

            if (end <= start)
            {
                MessageBox.Show("Sluttidspunkt skal være efter starttidspunkt.");
                return;
            }

            // lav event objektet
            Event ev = new Event
            {
                title       = TitleInput.Text,
                description = DescriptionInput.Text,
                start       = start,
                end         = end,
                city        = CityInput.Text
            };

            // lokale
            if (!string.IsNullOrWhiteSpace(RoomInput.Text))
            {
                ev.rooms = new List<string> { RoomInput.Text };
            }

            // underviser
            if (!string.IsNullOrWhiteSpace(TeacherInput.Text))
            {
                ev.teachers = new List<User> { new User { username = TeacherInput.Text } };
            }

            // hold
            if (!string.IsNullOrWhiteSpace(TeamInput.Text))
            {
                ev.teams = new List<Team> { new Team { teamName = TeamInput.Text } };
            }

            // tags: splittes på komma og trimmes for mellemrum
            if (!string.IsNullOrWhiteSpace(TagsInput.Text))
            {
                ev.tags = new List<string>();
                foreach (string tag in TagsInput.Text.Split(','))
                {
                    ev.tags.Add(tag.Trim());
                }
            }

            // gem i databasen
            repository.NewEvent(ev);

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
