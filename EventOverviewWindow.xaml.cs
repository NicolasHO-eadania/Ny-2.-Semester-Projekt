using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _2.semEksamenProjekt
{
    public partial class EventOverviewWindow : Window
    {
        double timeHeight = 60; // pixels pr time vertikalt
        double dayWidth = 150;  // pixels pr dag horisontalt
        int days = 7;           // dage på skemaet
        int startHour = 8;      // hvornår skemaet starter
        int endHour = 16;       // hvornår skemaet slutter

        EventOverview overview = new EventOverview();

        // repository håndterer alle database funktionerne
        EventRepository repository = new EventRepository();

        // holder styr på hvilken uge der vises
        DateTime currentMonday;

        public EventOverviewWindow()
        {
            InitializeComponent();
        }

        // laver skemaet
        public void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // størrelse på canvas
            int totalHours = endHour - startHour;
            EventsCanvas.Width  = days * dayWidth;
            EventsCanvas.Height = totalHours * timeHeight;

            // start med den nuværende uge
            currentMonday = GetMonday(DateTime.Today);

            LoadFromDatabase();

            DrawTimeLabels();

            UpdateWeekAndDates();

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
        private void UpdateWeekAndDates()
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

        // rydder canvas og tegner events for den nuværende uge
        public void RedrawSchedule()
        {
            EventsCanvas.Children.Clear();

            DrawGrid();

            UpdateWeekAndDates();

            // tegn kun events der ligger i den nuværende uge
            foreach (Event ev in overview.AllEvents)
            {
                if (ev.start >= currentMonday && ev.start < currentMonday.AddDays(7))
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
                    Text              = $"{hour:D2}:00",
                    Width             = 55,
                    Height            = timeHeight,
                    TextAlignment     = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding           = new Thickness(0, 20, 5, 0),
                    FontSize          = 11
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
                    X1              = 0,
                    Y1              = i * timeHeight,
                    X2              = days * dayWidth,
                    Y2              = i * timeHeight,
                    Stroke          = Brushes.LightGray,
                    StrokeThickness = 1
                };
                EventsCanvas.Children.Add(line);
            }

            // en lodret linje for hver dag
            for (int i = 0; i <= days; i++)
            {
                Line line = new Line
                {
                    X1              = i * dayWidth,
                    Y1              = 0,
                    X2              = i * dayWidth,
                    Y2              = totalHours * timeHeight,
                    Stroke          = Brushes.Gray,
                    StrokeThickness = 1
                };
                EventsCanvas.Children.Add(line);
            }
        }

        public void DrawEvent(Event ev)
        {
            // beregner kolonneindeks ud fra currentMonday
            // så events altid lander i den rigtige kolonne uanset hvilken uge der vises
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
                Width           = dayWidth - 6,
                Height          = blockHeight,
                BorderBrush     = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            StackPanel content = new StackPanel { Margin = new Thickness(5, 3, 5, 3) };

            // titel
            content.Children.Add(new TextBlock
            {
                Text         = ev.title,
                FontWeight   = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                FontSize     = 12
            });

            // tidspunkt
            content.Children.Add(new TextBlock
            {
                Text     = $"{ev.start:HH:mm} – {ev.end:HH:mm}",
                FontSize = 11
            });

            // lokale
            if (ev.rooms != null && ev.rooms.Count > 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text     = ev.rooms[0],
                    FontSize = 10
                });
            }

            // undervisere
            if (ev.teachers != null && ev.teachers.Count > 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text     = ev.teachers[0].username,
                    FontSize = 10
                });
            }

            block.Child = content;

            Canvas.SetLeft(block, dayIndex * dayWidth + 3);
            Canvas.SetTop(block,  topPos + 2);

            EventsCanvas.Children.Add(block);
        }

        // skifter til flows vinduet
        void GoToFlows_Click(object sender, RoutedEventArgs e)
        {
            new FlowOverviewWindow().Show();
            Close();
        }

        // fik hjælp fra claude----------------------------------------------------------------------------------------------
        public DateTime GetMonday(DateTime date)
        {
            int daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.AddDays(-daysSinceMonday).Date;
        }

        // fik hjælp fra claude ----------------------------------------------------------------------------------------------
        public int GetDayIndex(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday    => 0,
                DayOfWeek.Tuesday   => 1,
                DayOfWeek.Wednesday => 2,
                DayOfWeek.Thursday  => 3,
                DayOfWeek.Friday    => 4,
                DayOfWeek.Saturday  => 5,
                DayOfWeek.Sunday    => 6,
                _                   => -1
            };
        }
    }
}
