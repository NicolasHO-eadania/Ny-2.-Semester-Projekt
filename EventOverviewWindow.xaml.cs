using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace _2.semEksamenProjekt
{
    public partial class EventOverviewWindow : Window
    {
        double timeHeight = 60; // pixels pr time vertikalt
        double dayWidth = 145; // pixels pr dag horisontalt
        int days = 7; // dage på skemaet
        int startHour = 7; // hvornår skemaet starter
        int endHour = 18; // hvornår skemaet slutter

        EventOverview overview = new EventOverview();
        IRepositoryFactory factory = new RepositoryFactory();
        EventRepository repository;

        // bruges til at holde styr på hvilken uge der vises
        DateTime currentMonday;

        public EventOverviewWindow()
        {
            factory = new RepositoryFactory();
            repository = factory.CreateEventRepository();
            InitializeComponent();
        }

        // laver skemaet
        public async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // størrelse på canvas
            int totalHours = endHour - startHour;
            EventsCanvas.Width  = days * dayWidth;
            EventsCanvas.Height = totalHours * timeHeight;

            // start med den nuværende uge
            currentMonday = GetMonday(DateTime.Today);

            // skjul tilføj knap hvis brugeren ikke er admin
            if (!Session.Instance.CanEditEvents)
                AddEventButton.Visibility = Visibility.Collapsed;

            LoadTagFilter();
            DrawTimeLabels();
            UpdateWeekAndDates();

            // hent events fra databasen i baggrunden
            await Task.Run(() => LoadFromDatabase());

            // opdater UI når data er hentet
            RedrawSchedule();
        }

        // går en uge tilbage
        void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            currentMonday = currentMonday.AddDays(-7);
            RedrawSchedule();
        }

        // går en uge frem
        void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            currentMonday = currentMonday.AddDays(7);
            RedrawSchedule();
        }

        // går tilbage til den nuværende uge
        void CurrentWeek_Click(object sender, RoutedEventArgs e)
        {
            currentMonday = GetMonday(DateTime.Today);
            RedrawSchedule();
        }

        // opdaterer uge og datoer
        void UpdateWeekAndDates()
        {
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(currentMonday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            WeekLabel.Text = $"Uge {weekNumber}";

            // opdater dag-headers med dato
            string[] dayNames = { "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag", "Søndag" };
            TextBlock[] dayLabels = { DayLabel1, DayLabel2, DayLabel3, DayLabel4, DayLabel5, DayLabel6, DayLabel7 };

            for (int i = 0; i < 7; i++)
            {
                DateTime day = currentMonday.AddDays(i);
                dayLabels[i].Text = $"{dayNames[i]}\n{day:dd-MM}";
            }
        }

        // rydder skemaet og tegner events for den nuværende uge
        public void RedrawSchedule()
        {
            EventsCanvas.Children.Clear();

            DrawGrid();

            UpdateWeekAndDates();

            string selectedTag = TagFilter.SelectedIndex > 0 ? TagFilter.SelectedItem.ToString() : null;

            // tegn kun events der er synlige for brugeren, matcher tag filter og er i den nuværende uge
            foreach (Event ev in overview.AllEvents)
            {
                if (ev.start >= currentMonday && ev.start < currentMonday.AddDays(7)
                    && Session.Instance.CanSeeEvent(ev)
                    && MatchesTagFilter(ev.tags, selectedTag))
                    DrawEvent(ev);
            }
        }

        
        // henter alle events fra databasen
        public void LoadFromDatabase()
        {
            overview.AllEvents = repository.GetAllEvents();
        }

        public void DrawTimeLabels()
        {
            for (int hour = startHour; hour <= endHour; hour++)
            {
                TextBlock label = new TextBlock
                {
                    Text = $"{hour:D2}:00",
                    Width = 55,
                    Height = timeHeight,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(0, 20, 5, 0),
                    FontSize = 11
                };
                TimeColumn.Children.Add(label);
            }
        }

        // linjer i skemaet
        public void DrawGrid()
        {
            int totalHours = endHour - startHour;

            // en vandret linje for hver time
            for (int i = 0; i <= totalHours; i++)
            {
                Line line = new Line
                {
                    X1 = 0,
                    Y1 = i * timeHeight,
                    X2 = days * dayWidth,
                    Y2 = i * timeHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                EventsCanvas.Children.Add(line);
            }

            // en lodret linje for hver dag
            for (int i = 0; i <= days; i++)
            {
                Line line = new Line
                {
                    X1 = i * dayWidth,
                    Y1 = 0,
                    X2 = i * dayWidth,
                    Y2 = totalHours * timeHeight,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1
                };
                EventsCanvas.Children.Add(line);
            }
        }

        public void DrawEvent(Event ev)
        {
            // beregner kolonneindeks ud fra currentMonday
            // så events er i den rigtige kolonne uanset hvilken uge der vises
            int dayIndex = (ev.start.Date - currentMonday).Days;

            // crash fix
            if (dayIndex < 0 || dayIndex >= days)
                return;

            // y position baseret på starttidspunkt
            double topPos = (ev.start.Hour - startHour + ev.start.Minute / 60.0) * timeHeight;

            // højde baseret på varighed
            double durationHours = (ev.end - ev.start).TotalHours;
            double blockHeight   = durationHours * timeHeight - 4;

            // opretter event blokken
            Border block = new Border
            {
                Width = dayWidth - 6,
                Height = blockHeight,
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            StackPanel content = new StackPanel { Margin = new Thickness(5, 3, 5, 3) };

            // titel
            content.Children.Add(new TextBlock
            {
                Text = ev.title,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            });

            // tidspunkt
            content.Children.Add(new TextBlock
            {
                Text = $"{ev.start:HH:mm} – {ev.end:HH:mm}",
                FontSize = 11
            });

            // lokale
            if (ev.rooms != null && ev.rooms.Count > 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = ev.rooms[0],
                    FontSize = 10
                });
            }

            // undervisere
            if (ev.teachers != null && ev.teachers.Count > 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = ev.teachers[0].username,
                    FontSize = 10
                });
            }

            block.Child = content;

            // højreklik menu kun for admin
            if (Session.Instance.CanEditEvents)
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem editItem = new MenuItem { Header = "Rediger" };
                editItem.Click += (s, args) =>
                {
                    new NewEventWindow(repository, this, ev).Show();
                };

                MenuItem deleteItem = new MenuItem { Header = "Slet" };
                deleteItem.Click += (s, args) =>
                {
                    if (MessageBox.Show($"Vil du slette '{ev.title}'?", "Slet event",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        repository.DeleteEvent(ev.id);
                        LoadFromDatabase();
                        RedrawSchedule();
                    }
                };

                contextMenu.Items.Add(editItem);
                contextMenu.Items.Add(deleteItem);
                block.ContextMenu = contextMenu;
            }

            // venstreklik åbner det tilknyttede flow
            block.MouseLeftButtonUp += (s, args) =>
            {
                if (ev.flowId != null)
                {
                    Flow flow = factory.CreateFlowRepository().GetFlowById(ev.flowId.Value);
                    if (flow != null)
                    {
                        new FlowWindow(flow).Show();
                        Close();
                    }
                }
                args.Handled = true;
            };

            // byg tooltip tekst fra event data
            string tooltip = $"{ev.title}\n{ev.start:HH:mm} – {ev.end:HH:mm}";

            if (!string.IsNullOrWhiteSpace(ev.description))
            {
                tooltip += $"\n\n{ev.description}";
            }

            if (ev.rooms != null && ev.rooms.Count > 0)
            {
                tooltip += $"\nLokale: {string.Join(", ", ev.rooms)}";
            }

            if (ev.teachers != null && ev.teachers.Count > 0)
            {
                tooltip += $"\nUndervisere: {string.Join(", ", ev.teachers.Select(t => t.username))}";
            }

            if (ev.teams != null && ev.teams.Count > 0)
            {
                tooltip += $"\nHold: {string.Join(", ", ev.teams.Select(t => t.teamName))}";
            }

            block.ToolTip = tooltip;

            ToolTipService.SetInitialShowDelay(block, 0);

            Canvas.SetLeft(block, dayIndex * dayWidth + 3);
            Canvas.SetTop(block, topPos + 2);

            EventsCanvas.Children.Add(block);
        }

        // åbner vindue til at oprette nyt event
        void NewEvent_Click(object sender, RoutedEventArgs e)
        {
            new NewEventWindow(repository, this).Show();
        }

        // skifter til flows vinduet
        void GoToFlows_Click(object sender, RoutedEventArgs e)
        {
            new FlowOverviewWindow().Show();
            Close();
        }

        // indlæser tags i filteret
        void LoadTagFilter()
        {
            TagFilter.Items.Add("Alle tags");
            foreach (string tag in factory.CreateTagRepository().GetEventTags())
            {
                TagFilter.Items.Add(tag);
            }
            TagFilter.SelectedIndex = 0;
        }

        // tegner skemaet om når tag filteret ændres
        void TagFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            RedrawSchedule();
        }

        // returnerer true hvis ingen tags er valgt eller eventet har det valgte tag
        bool MatchesTagFilter(List<string> tags, string selectedTag)
        {
            if (selectedTag == null || selectedTag == "Alle tags")
            {
                return true;
            }
            if (tags == null)
            {
                return false;
            }
            return tags.Contains(selectedTag);
        }

        // beregner datoen for mandagen i den uge en given dato ligger i
        public DateTime GetMonday(DateTime date)
        {
            int daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.AddDays(-daysSinceMonday).Date;
        }

    }
}
