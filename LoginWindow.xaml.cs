using System.Windows;

namespace _2.semEksamenProjekt
{
    public partial class LoginWindow : Window
    {
        IRepositoryFactory factory;
        UserRepository userRepository;

        public LoginWindow()
        {
            factory = new RepositoryFactory();
            userRepository = factory.CreateUserRepository();
            InitializeComponent();
        }

        void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            User user = userRepository.GetUserByCredentials(username, password);

            if (user == null)
            {
                MessageBox.Show("Forkert brugernavn eller adgangskode.", "Log ind fejlede");
                return;
            }

            // gem brugeren i session
            Session.Instance.CurrentUser = user;

            // indlæs brugerens hold
            Session.Instance.UserTeams = factory.CreateTeamRepository().GetTeamsByUsername(user.username);

            // åbn skema og luk login vinduet
            new EventOverviewWindow().Show();
            Close();
        }

        // gør at man kann trykke enter for at logge ind
        void PasswordInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Login_Click(sender, e);
            }
        }
    }
}
