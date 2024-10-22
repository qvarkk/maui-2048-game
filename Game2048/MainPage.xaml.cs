using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Game2048
{
    public partial class MainPage : ContentPage
    {
        enum Direction { Up, Down, Left, Right }

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

        private int Rows;
        private int Columns;
        private Random random = new Random();
        private ScoreViewModel score = new ScoreViewModel();
        private List<Tile> tiles = new List<Tile>();

        public MainPage()
        {
            InitializeComponent();

            Rows = GameGrid.RowDefinitions.Count;
            Columns = GameGrid.ColumnDefinitions.Count;

            Binding scoreBinding = new Binding { Source = score, Path = "Score" };
            scoreLabel.SetBinding(Label.TextProperty, scoreBinding);

            BackgroundColor = new Color(251, 247, 238);
        }

        private void AddTileToGrid(Tile tile)
        {
            tiles.Add(tile);

            GameGrid.Children.Add(tile);
            Grid.SetRow(tile, tile.row);
            Grid.SetColumn(tile, tile.column);
        }

        private void MoveTileInGrid(int row, int column, Tile tile)
        {
            var _tile = tiles.Find(x => x == tile);

            if (_tile != null)
            {
                _tile.row = row;
                _tile.column = column;

                Grid.SetRow(tile, tile.row);
                Grid.SetColumn(tile, tile.column);
            }
        }

        private async Task MoveTileToMerge(Tile tile, int row, int column, Direction dir)
        {
            int x = 0;
            int y = 0;

            switch (dir)
            {
                case Direction.Up:
                    y = -1;
                    break;
                case Direction.Down:
                    y = 1;
                    break;
                case Direction.Left:
                    x = -1;
                    break;
                case Direction.Right:
                    y = -1;
                    break;
            }


            tile.ZIndex = 1;
            tile.ScaleTo(0, 300);
            await tile.TranslateTo((column - Grid.GetColumn(tile) + x) * tile.Width,
                                   (row - Grid.GetRow(tile) + y) * tile.Height,
                                   300, Easing.CubicInOut);

            RemoveTile(tile);
        }

        private void RemoveTile(Tile tile)
        {
            var _tile = tiles.Find(x => x == tile);

            if (_tile != null)
            {
                tiles.Remove(tile);
                GameGrid.Remove(tile);
            }
        }

        private async Task<Tile> PlaceTile(int value, int row, int column)
        {
            Tile tile = new Tile(value, row, column);
            await tile.ScaleTo(1.2, 0);

            AddTileToGrid(tile);

            await tile.ScaleTo(1, 100);

            return tile;
        }

        public async Task MoveTile(Tile tile, int row, int column)
        {
            await tile.TranslateTo((column - Grid.GetColumn(tile)) * tile.Width,
                                   (row - Grid.GetRow(tile)) * tile.Height,
                                   300, Easing.CubicInOut);

            MoveTileInGrid(row, column, tile);

            tile.TranslationX = 0;
            tile.TranslationY = 0;
        }

        public async Task AnimateTileMerge(Tile tile)
        {
            await tile.ScaleTo(1.2, 250, Easing.CubicInOut);
            await tile.ScaleTo(1, 100, Easing.CubicInOut);
        }

        public List<(int row, int column)> GetFreeCells()
        {
            return Enumerable.Range(0, Rows)
                .SelectMany(row => Enumerable.Range(0, Columns), (row, column) => new { row, column })
                .Where(rc => !tiles.Any(x => x.row == rc.row && x.column == rc.column))
                .Select(rc => (rc.row, rc.column))
                .ToList();
        }

        private async void AddRandomTile(bool forceTwo = false)
        {
            var freeCells = GetFreeCells();

            if (freeCells.Count > 0)
            {
                var randomCell = freeCells[random.Next(freeCells.Count)];

                int value;
                var randValue = random.Next(0, 100);

                if (randValue < 75 || forceTwo)
                {
                    value = 2;
                } else
                {
                    value = 4;
                }

                await PlaceTile(value, randomCell.row, randomCell.column);
            }
        }

        private void ResetGameField(object sender, EventArgs e)
        {
            GameGrid.Children.Clear();
            score.Score = "0";
            AddRandomTile(true);
        }

        private void MoveDown()
        {
            for (int column = 0; column < Columns; column++)
            {
                int cappedRow = Rows - 1;

                var columnTiles = tiles.Where(x => x.column == column)
                              .OrderByDescending(x => x.row)
                              .ToList();

                if (!columnTiles.Any())
                    continue;

                Tile lastMovedTile = null;

                foreach (var tile in columnTiles)
                {
                    if (lastMovedTile != null && lastMovedTile.Value == tile.Value && lastMovedTile.row == cappedRow + 1)
                    {
                        lastMovedTile.UpdateValue(tile.Value * 2);
                        AnimateTileMerge(lastMovedTile);
                        MoveTileToMerge(tile, cappedRow, column, Direction.Down);
                        continue;
                    }

                    if (tile.row != cappedRow)
                    {
                        MoveTile(tile, cappedRow, column);
                    }

                    lastMovedTile = tile;
                    cappedRow--;
                }
            }
        }

        private void MoveUp()
        {
            for (int column = 0; column < Columns; column++)
            {
                int cappedRow = 0;

                var columnTiles = tiles.Where(x => x.column == column)
                              .OrderBy(x => x.row)
                              .ToList();

                if (!columnTiles.Any())
                    continue;

                Tile lastMovedTile = null;

                foreach (var tile in columnTiles)
                {
                    if (lastMovedTile != null && lastMovedTile.Value == tile.Value && lastMovedTile.row == cappedRow - 1)
                    {
                        lastMovedTile.UpdateValue(tile.Value * 2);
                        AnimateTileMerge(lastMovedTile);
                        MoveTileToMerge(tile, cappedRow, column, Direction.Up);
                        continue;
                    }

                    if (tile.row != cappedRow)
                    {
                        MoveTile(tile, cappedRow, column);
                    }

                    lastMovedTile = tile;
                    cappedRow++;
                }
            }
        }

        public void TestButton(object sender, EventArgs e)
        {
            AddRandomTile();
        }

        public void TestButton1(object sender, EventArgs e)
        {
            MoveUp();
        }

        public void TestButton2(object sender, EventArgs e)
        {
            MoveDown();
        }

        public class Tile : Frame
        {
            public int row { get; set; }
            public int column { get; set; }

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

            public Tile(int value, int row, int column)
            {
                CornerRadius = 10;
                BackgroundColor = tilesValuesColors[value];
                WidthRequest = 95;
                HeightRequest = 95;
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
                ZIndex = 2;

                ValueLabel = new Label
                {
                    Text = value.ToString(),
                    FontSize = 24,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                };

                Value = value;
                Content = ValueLabel;

                this.row = row;
                this.column = column;
            }

            public void UpdateValue(int newValue)
            {
                Value = newValue;
                ValueLabel.Text = newValue.ToString();
                BackgroundColor = tilesValuesColors[newValue];
            }
        }

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
    }
}
