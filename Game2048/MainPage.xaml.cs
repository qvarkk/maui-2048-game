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

            SwipeGestureRecognizer upSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            SwipeGestureRecognizer downSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            SwipeGestureRecognizer leftSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            SwipeGestureRecognizer rightSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };

            upSwipeGesture.Swiped += OnSwiped;
            downSwipeGesture.Swiped += OnSwiped;
            leftSwipeGesture.Swiped += OnSwiped;
            rightSwipeGesture.Swiped += OnSwiped;

            GameGrid.GestureRecognizers.Add(upSwipeGesture);
            GameGrid.GestureRecognizers.Add(downSwipeGesture);
            GameGrid.GestureRecognizers.Add(leftSwipeGesture);
            GameGrid.GestureRecognizers.Add(rightSwipeGesture);

            BackgroundColor = new Color(251, 247, 238);
        }

        public void OnObjectLoaded(object sender, EventArgs e)
        {
            gameLogic.ResetGameField();
        }

        public void RestartAction(object sender, EventArgs e)
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

        public void MoveDownAction(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveDown());
        }

        public void MoveUpAction(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveUp());
        }

        public void MoveLeftAction(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveLeft());
        }


        public void MoveRightAction(object sender, EventArgs e)
        {
            ValidateMove(gameLogic.MoveRight());
        }

        void OnSwiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Left:
                    MoveLeftAction(sender, e);
                    break;
                case SwipeDirection.Right:
                    MoveRightAction(sender, e);
                    break;
                case SwipeDirection.Up:
                    MoveUpAction(sender, e);
                    break;
                case SwipeDirection.Down:
                    MoveDownAction(sender, e);
                    break;
            }
        }

        public class ScoreViewModel : INotifyPropertyChanged
        {
            private string score = "0";
            public event PropertyChangedEventHandler? PropertyChanged;

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
