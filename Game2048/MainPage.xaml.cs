using Game2048.Resources.Logic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Game2048
{
    public partial class MainPage : ContentPage
    {
        private ScoreViewModel score = new ScoreViewModel();
        private GameLogic gameLogic;

        public MainPage()
        {
            InitializeComponent();

            int rows = GameGrid.RowDefinitions.Count;
            int columns = GameGrid.ColumnDefinitions.Count;

            gameLogic = new GameLogic(rows, columns, GameGrid);

            Binding scoreBinding = new Binding { Source = score, Path = "Score" };
            scoreLabel.SetBinding(Label.TextProperty, scoreBinding);

            BackgroundColor = new Color(251, 247, 238);
        }

        public void OnObjectLoaded(object sender, EventArgs e)
        {
            gameLogic.ResetGameField();
        }

        public void RestartButton(object sender, EventArgs e)
        {
            score.Score = 0;
            gameLogic.ResetGameField();
        }

        private async void ValidateMove((int score, bool moveDone) arg)
        {
            if (arg.moveDone)
            {
                gameLogic.AddRandomTile();
            }
            score.Score += arg.score;

            if (!gameLogic.HasAvailableMoves())
            {
                bool toReset = await DisplayAlert("Game Over!", $"Score: {score.Score}", "Restart", "Ok :(");
                if (toReset)
                    gameLogic.ResetGameField();
            }
        }

        public void MoveDownButton(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveDown());
        }

        public void MoveUpButton(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveUp());
        }

        public void MoveLeftButton(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveLeft());
        }


        public void MoveRightButton(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveRight());
        }

        public class ScoreViewModel : INotifyPropertyChanged
        {
            private string score = "0";
            public event PropertyChangedEventHandler PropertyChanged;

            public int Score
            {
                get => Int32.Parse(score);
                set
                {
                    if (Int32.Parse(score) != value)
                    {
                        score = value.ToString();
                        OnPropertyChanged(nameof(Score));
                    }
                }
            }

            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
