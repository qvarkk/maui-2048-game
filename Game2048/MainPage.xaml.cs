using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

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
        private ScoreViewModel score = new ScoreViewModel();

        public List<Tile> Tiles = new List<Tile>();

        public MainPage()
        {
            InitializeComponent();

            Binding scoreBinding = new Binding { Source = score, Path = "Score" };
            scoreLabel.SetBinding(Label.TextProperty, scoreBinding);

            BackgroundColor = new Color(251, 247, 238);
        }

        private async Task<Tile> PlaceTile(int value, int row, int col)
        {
            Tile tile = new Tile(value);
            await tile.ScaleTo(1.2, 0);

            GameGrid.Children.Add(tile);
            Grid.SetRow(tile, row);
            Grid.SetColumn(tile, col);

            await tile.ScaleTo(1, 100);

            return tile;
        }

        public async Task MoveTile(Tile tile, int newRow, int newCol)
        {
            await tile.TranslateTo((newCol - Grid.GetColumn(tile)) * tile.Width,
                                   (newRow - Grid.GetRow(tile)) * tile.Height,
                                   300, Easing.CubicInOut);

            Grid.SetRow(tile, newRow);
            Grid.SetColumn(tile, newCol);

            tile.TranslationX = 0;
            tile.TranslationY = 0;
        }

        public async Task AnimateTileMerge(Tile tile)
        {
            await tile.ScaleTo(1.2, 250, Easing.CubicInOut);
            await tile.ScaleTo(1, 100, Easing.CubicInOut);
        }

        private async void TestButton_1(object sender, EventArgs e)
        {
            Tile tile = await PlaceTile(2, 0, 0);
        }

        private async void TestButton(object sender, EventArgs e)
        {
            await MoveTile((Tile)GameGrid.Children.ToList()[0], 3, 0);
            await AnimateTileMerge((Tile)GameGrid.Children.ToList()[0]);
        }

        private async void AddRandomTile(bool forceTwo = false)
        {
            var elements = GameGrid.Children.ToList();
            int columns = GameGrid.ColumnDefinitions.Count;
            int rows = GameGrid.RowDefinitions.Count;

            var freeCells = Enumerable.Range(0, rows)
                .SelectMany(row => Enumerable.Range(0, columns), (row, col) => new { row, col })
                .Where(rc => !elements.Any(x => Grid.GetColumn((BindableObject)x) == rc.col && Grid.GetRow((BindableObject)x) == rc.row))
                .Select(rc => (rc.row, rc.col))
                .ToList();

            if (freeCells.Count > 0)
            {
                var randomIndex = random.Next(freeCells.Count);
                var selectedCell = freeCells[randomIndex];

                int value;
                var randValue = random.Next(0, 100);

                if (randValue < 75 || forceTwo)
                {
                    value = 2;
                } else
                {
                    value = 4;
                }

                await PlaceTile(value, selectedCell.row, selectedCell.col);
            }
        }

        private void ResetGameField(object sender, EventArgs e)
        {
            GameGrid.Children.Clear();
            score.Score = "0";
            AddRandomTile(true);
        }

        private void ConnectTiles()
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
                                Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == minAvailableRow - 1);

                            if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
                            {
                                ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
                                ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
                                score.Score = (Int32.Parse(score.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
                                GameGrid.Children.Remove(element);
                            } else
                            {
                                Grid.SetRow((BindableObject)element, minAvailableRow++);
                            }
                        }
                    }
                }
            }

            AddRandomTile();
        }

        //    private void Button_Pressed_Left(object sender, EventArgs e)
        //    {
        //        var elements = GameGrid.Children.ToList();
        //        int columns = GameGrid.ColumnDefinitions.Count;
        //        int rows = GameGrid.RowDefinitions.Count;

        //        for (int row = 0; row < rows; row++)
        //        {
        //            int minAvailableCol = 0;
        //            var anyItems = elements.Any(x =>
        //                Grid.GetRow((BindableObject)x) == row);

        //            if (anyItems)
        //            {
        //                for (int col = 0; col < columns; col++)
        //                {
        //                    var element = elements.Find(x =>
        //                        Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

        //                    if (element != null)
        //                    {
        //                        var stackedOnElement = elements.Find(x =>
        //                            Grid.GetColumn((BindableObject)x) == minAvailableCol - 1 && Grid.GetRow((BindableObject)x) == row);

        //                        if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
        //                        {
        //                            ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
        //                            ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
        //                            score.Score = (Int32.Parse(score.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
        //                            GameGrid.Children.Remove(element);
        //                        } else
        //                        {
        //                            Grid.SetColumn((BindableObject)element, minAvailableCol++);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        Spawn_Random_Tile();
        //    }

        //    private void Button_Pressed_Down(object sender, EventArgs e)
        //    {
        //        var elements = GameGrid.Children.ToList();
        //        int columns = GameGrid.ColumnDefinitions.Count;
        //        int rows = GameGrid.RowDefinitions.Count;

        //        for (int col = 0; col < columns; col++)
        //        {
        //            int maxAvailableRow = rows - 1;
        //            var anyItems = elements.Any(x =>
        //                Grid.GetColumn((BindableObject)x) == col);

        //            if (anyItems)
        //            {
        //                for (int row = rows - 1; row >= 0; row--)
        //                {
        //                    var element = elements.Find(x =>
        //                        Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

        //                    if (element != null)
        //                    {
        //                        var stackedOnElement = elements.Find(x =>
        //                            Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == maxAvailableRow + 1);

        //                        if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
        //                        {
        //                            ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
        //                            ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
        //                            score.Score = (Int32.Parse(score.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
        //                            GameGrid.Children.Remove(element);
        //                        } else
        //                        {
        //                            Grid.SetRow((BindableObject)element, maxAvailableRow--);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        Spawn_Random_Tile();
        //    }

        //    private void Button_Pressed_Right(object sender, EventArgs e)
        //    {
        //        var elements = GameGrid.Children.ToList();
        //        int columns = GameGrid.ColumnDefinitions.Count;
        //        int rows = GameGrid.RowDefinitions.Count;

        //        for (int row = 0; row < rows; row++)
        //        {
        //            int maxAvailableCol = columns - 1;
        //            var anyItems = elements.Any(x =>
        //                Grid.GetRow((BindableObject)x) == row);

        //            if (anyItems)
        //            {
        //                for (int col = columns - 1; col >= 0; col--)
        //                {
        //                    var element = elements.Find(x =>
        //                        Grid.GetColumn((BindableObject)x) == col && Grid.GetRow((BindableObject)x) == row);

        //                    if (element != null)
        //                    {
        //                        var stackedOnElement = elements.Find(x =>
        //                            Grid.GetColumn((BindableObject)x) == maxAvailableCol + 1 && Grid.GetRow((BindableObject)x) == row);

        //                        if (stackedOnElement != null && ((Label)stackedOnElement).Text == ((Label)element).Text)
        //                        {
        //                            ((Label)stackedOnElement).Text = (Int32.Parse(((Label)stackedOnElement).Text) * 2).ToString();
        //                            ((Label)stackedOnElement).BackgroundColor = tilesValuesColors[Int32.Parse(((Label)stackedOnElement).Text)];
        //                            score.Score = (Int32.Parse(score.Score) + (Int32.Parse(((Label)stackedOnElement).Text))).ToString();
        //                            GameGrid.Children.Remove(element);
        //                        } else
        //                        {
        //                            Grid.SetColumn((BindableObject)element, maxAvailableCol--);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        Spawn_Random_Tile();
        //    }
        //}

        public class ScoreViewModel : INotifyPropertyChanged
        {
            private string score = "0";
            public event PropertyChangedEventHandler PropertyChanged;

            public string Score
            {
                get => score;
                set
                {
                    if (score != value)
                    {
                        score = value;
                        OnPropertyChanged(nameof(Score));
                    }
                }
            }

            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class Tile : Frame
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

            public Label ValueLabel { get; set; }
            public int Value { get; set; }

            public Tile(int value)
            {
                CornerRadius = 10;
                BackgroundColor = tilesValuesColors[value];
                WidthRequest = 95;
                HeightRequest = 95;
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;

                ValueLabel = new Label
                {
                    Text = value.ToString(),
                    FontSize = 24,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                };

                Value = value;
                Content = ValueLabel;
            }

            public void UpdateValue(int newValue)
            {
                Value = newValue;
                ValueLabel.Text = newValue.ToString();
                BackgroundColor = tilesValuesColors[newValue];
            }
        }
    }
}
