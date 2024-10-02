using System.ComponentModel;

namespace Game2048
{
    public partial class MainPage : ContentPage
    {
        private Dictionary<int, Color> tilesValuesColors = new Dictionary<int, Color>()
        {
            { 2, new Color(238, 228, 218) },
            { 4, new Color(237, 224, 200) },
            { 8, new Color(242, 177, 121) },
            { 16, new Color(245, 149, 99) },
            { 32, new Color(246, 124, 95) },
            { 64, new Color(246, 94, 59) },
            { 128, new Color(237, 207, 114) },
            { 256, new Color(237, 204, 97) },
            { 512, new Color(237, 200, 80) },
            { 1024, new Color(237, 197, 63) },
            { 2048, new Color(237, 194, 46) }
        };
        

        private Random random = new Random();
        private ScoreViewModel viewModel;
        public string Score = "0";

        public MainPage()
        {
            InitializeComponent();
            Spawn_Random_Tile();

            BackgroundColor = new Color(251, 247, 238);
            viewModel = new ScoreViewModel();
            BindingContext = viewModel;
        }

        private void Spawn_Random_Tile()
        {
            var elements = GameGrid.Children.ToList();
            int columns = GameGrid.ColumnDefinitions.Count;
            int rows = GameGrid.RowDefinitions.Count;

            var freeCells = new List<(int row, int col)>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var isOccupied = elements.Any(x =>
                        Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

                    if (!isOccupied)
                    {
                        freeCells.Add((row, col));
                    }
                }
            }

            if (freeCells.Count > 0)
            {
                var randomIndex = random.Next(freeCells.Count);
                var selectedCell = freeCells[randomIndex];

                int value = random.Next(0, 100);
                int textValue;

                if (value < 70)
                {
                    textValue = 2;
                } else if (value >= 70 && value < 90)
                {
                    textValue = 4;
                } else
                {
                    textValue = 8;
                }

                var label = new Label
                {
                    BackgroundColor = tilesValuesColors[textValue],
                    WidthRequest = 95,
                    HeightRequest = 95,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Text = textValue.ToString(),
                    FontSize = 24,
                };

                GameGrid.Children.Add(label);
                Grid.SetRow(label, selectedCell.row);
                Grid.SetColumn(label, selectedCell.col);
            }

        }

        private void Button_Pressed_Clear(object sender, EventArgs e)
        {
            GameGrid.Children.Clear();
        }

        private void Button_Pressed_Up(object sender, EventArgs e)
        {
            var elements = GameGrid.Children.ToList();
            int columns = GameGrid.ColumnDefinitions.Count;
            int rows = GameGrid.RowDefinitions.Count;

            for (int col = 0; col < columns; col++)
            {
                int minAvailableRow = 0;
                var anyItems = elements.Any(x =>
                    Grid.GetColumn((BindableObject)x) == col);
                
                if (anyItems)
                {
                    for (int row = 0; row < rows; row++)
                    {
                        var element = elements.Find(x =>
                            Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

                        if (element != null)
                        {
                            var stackedOnElement = elements.Find(x =>
                                Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == minAvailableRow -1);

                            if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
                            {
                                ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
                                ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
                                viewModel.Score = (Int32.Parse(viewModel.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
                                GameGrid.Children.Remove(element);
                            } else 
                            {
                                Grid.SetRow((BindableObject)element, minAvailableRow++);
                            }
                        }
                    }
                }
            }

            Spawn_Random_Tile();
        }

        private void Button_Pressed_Left(object sender, EventArgs e)
        {
            var elements = GameGrid.Children.ToList();
            int columns = GameGrid.ColumnDefinitions.Count;
            int rows = GameGrid.RowDefinitions.Count;

            for (int row = 0; row < rows; row++)
            {
                int minAvailableCol = 0;
                var anyItems = elements.Any(x =>
                    Grid.GetRow((BindableObject)x) == row);

                if (anyItems)
                {
                    for (int col = 0; col < columns; col++)
                    {
                        var element = elements.Find(x =>
                            Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

                        if (element != null)
                        {
                            var stackedOnElement = elements.Find(x =>
                                Grid.GetColumn((BindableObject)x) == minAvailableCol - 1 && Grid.GetRow((BindableObject)x) == row);

                            if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
                            {
                                ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
                                ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
                                viewModel.Score = (Int32.Parse(viewModel.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
                                GameGrid.Children.Remove(element);
                            } else
                            {
                                Grid.SetColumn((BindableObject)element, minAvailableCol++);
                            }
                        }
                    }
                }
            }

            Spawn_Random_Tile();
        }

        private void Button_Pressed_Down(object sender, EventArgs e)
        {
            var elements = GameGrid.Children.ToList();
            int columns = GameGrid.ColumnDefinitions.Count;
            int rows = GameGrid.RowDefinitions.Count;

            for (int col = 0; col < columns; col++)
            {
                int maxAvailableRow = rows - 1;
                var anyItems = elements.Any(x =>
                    Grid.GetColumn((BindableObject)x) == col);

                if (anyItems)
                {
                    for (int row = rows - 1; row >= 0; row--)
                    {
                        var element = elements.Find(x =>
                            Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

                        if (element != null)
                        {
                            var stackedOnElement = elements.Find(x =>
                                Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == maxAvailableRow + 1);

                            if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
                            {
                                ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
                                viewModel.Score = (Int32.Parse(viewModel.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
                                GameGrid.Children.Remove(element);
                            } else
                            {
                                Grid.SetRow((BindableObject)element, maxAvailableRow--);
                            }
                        }
                    }
                }
            }

            Spawn_Random_Tile();
        }

        private void Button_Pressed_Right(object sender, EventArgs e)
        {
            var elements = GameGrid.Children.ToList();
            int columns = GameGrid.ColumnDefinitions.Count;
            int rows = GameGrid.RowDefinitions.Count;

            for (int row = 0; row < rows; row++)
            {
                int maxAvailableCol = columns - 1;
                var anyItems = elements.Any(x =>
                    Grid.GetRow((BindableObject)x) == row);

                if (anyItems)
                {
                    for (int col = columns - 1; col >= 0; col--)
                    {
                        var element = elements.Find(x =>
                            Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

                        if (element != null)
                        {
                            var stackedOnElement = elements.Find(x =>
                                Grid.GetColumn((BindableObject)x) == maxAvailableCol + 1 && Grid.GetRow((BindableObject)x) == row);

                            if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
                            {
                                ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
                                ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
                                viewModel.Score = (Int32.Parse(viewModel.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
                                GameGrid.Children.Remove(element);
                            } else
                            {
                                Grid.SetColumn((BindableObject)element, maxAvailableCol--);
                            }
                        }
                    }
                }
            }

            Spawn_Random_Tile();
        }
    }

    public class ScoreViewModel : INotifyPropertyChanged
    {
        private string score;

        public string Score
        {
            get => score;
            set
            {
                score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ScoreViewModel()
        {
            Score = "0";
        }
    }
}
