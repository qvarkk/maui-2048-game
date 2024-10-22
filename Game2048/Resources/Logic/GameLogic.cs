namespace Game2048.Resources.Logic
{
    internal class GameLogic
    {
        enum Direction { Up, Down, Left, Right }

        private int Score;
        private int Rows;
        private int Columns;
        private Grid GameGrid;
        private Random random = new Random();
        private List<Tile> tiles = new List<Tile>();

        public GameLogic(int rows, int columns, Grid gameGrid)
        {
            Rows = rows;
            Columns = columns;
            GameGrid = gameGrid;
        }

        private void CancelAnims()
        {
            tiles.ForEach(tile => { tile.CancelAnimations(); });
        }

        private List<(int row, int column)> GetFreeCells()
        {
            return Enumerable.Range(0, Rows)
                .SelectMany(row => Enumerable.Range(0, Columns), (row, column) => new { row, column })
                .Where(rc => !tiles.Any(x => x.row == rc.row && x.column == rc.column))
                .Select(rc => (rc.row, rc.column))
                .ToList();
        }

        public bool HasAvailableMoves()
        {
            if (GetFreeCells().Count > 0)
            {
                return true;
            }

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    if (row < 3 && tiles.Find(x => x.row == row && x.column == column)?.Value == tiles.Find(x => x.row == row + 1 && x.column == column)?.Value)
                    {
                        return true;
                    }
                    if (column < 3 && tiles.Find(x => x.row == row && x.column == column)?.Value == tiles.Find(x => x.row == row && x.column == column + 1)?.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AddTileToGrid(Tile tile)
        {
            tiles.Add(tile);

            GameGrid.Children.Add(tile);
            Grid.SetRow(tile, tile.row);
            Grid.SetColumn(tile, tile.column);
        }

        public async void AddRandomTile(bool forceTwo = false)
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

        public void ResetGameField()
        {
            tiles.Clear();
            GameGrid.Children.Clear();
            AddRandomTile(true);
            AddRandomTile(true);
        }

        // это чудо разбито на 2 функции дабы в самой игре можно было
        // мувать без эвейта, все сразу, если этого не делать придется
        // либо везде эвейт писать и тут свои проблемсы либо без эвейта
        // но тогда эвейты визуала не дают логике работать как надо
        private void MoveTileInGrid(Tile tile)
        {
            var _tile = tiles.Find(x => x == tile);

            if (_tile != null)
            {
                Grid.SetRow(tile, tile.row);
                Grid.SetColumn(tile, tile.column);
            }
        }

        private void MoveTileInList(int row, int column, Tile tile)
        {
            var _tile = tiles.Find(x => x == tile);

            if (_tile != null)
            {
                _tile.row = row;
                _tile.column = column;
            }
        }

        private void RemoveTileFromList(Tile tile)
        {
            var _tile = tiles.Find(x => x == tile);

            if (_tile != null)
            {
                tiles.Remove(tile);
            }
        }

        private void RemoveTileFromGrid(Tile tile)
        {
            var _tile = tiles.Find(x => x == tile);

            if (_tile != null)
            {
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

        private async Task MoveTileToMerge(Tile tile, int row, int column, Direction dir)
        {
            RemoveTileFromList(tile);

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
                    x = 1;
                    break;
            }

            tile.ZIndex = 1;

            tile.ScaleTo(0, 200); // тут эвейт ваще никак нельзя иначе оно летит конкретно чзх

            await tile.TranslateTo((column - Grid.GetColumn(tile) + x) * tile.Width,
                                   (row - Grid.GetRow(tile) + y) * tile.Height,
                                   300, Easing.CubicInOut);

            RemoveTileFromGrid(tile);
        }

        private async Task MoveTile(Tile tile, int row, int column)
        {
            MoveTileInList(row, column, tile);

            await tile.TranslateTo((column - Grid.GetColumn(tile)) * tile.Width,
                                   (row - Grid.GetRow(tile)) * tile.Height,
                                   300, Easing.CubicInOut);

            MoveTileInGrid(tile);

            tile.TranslationX = 0;
            tile.TranslationY = 0;
        }

        private async Task AnimateTileMerge(Tile tile)
        {
            await tile.ScaleTo(1.1, 250, Easing.CubicInOut);
            await tile.ScaleTo(1, 100, Easing.CubicInOut);
            tile.Scale = 1; // если анимка канселится то размер остается 1
        }

        public (int score, bool moveDone) MoveDown()
        {
            CancelAnims();
            int score = 0;
            bool moveDone = false;

            for (int column = 0; column < Columns; column++)
            {
                int cappedRow = Rows - 1;

                var columnTiles = tiles.Where(x => x.column == column)
                              .OrderByDescending(x => x.row)
                              .ToList();

                if (!columnTiles.Any())
                    continue;

                Tile? lastMovedTile = null;

                foreach (var tile in columnTiles)
                {
                    if (lastMovedTile != null && lastMovedTile.Value == tile.Value && lastMovedTile.row == cappedRow + 1)
                    {
                        MoveTileToMerge(tile, cappedRow, column, Direction.Down);
                        AnimateTileMerge(lastMovedTile);
                        lastMovedTile.UpdateValue(tile.Value * 2);
                        lastMovedTile = tile;
                        moveDone = true;
                        score += tile.Value * 2;
                        continue;
                    }

                    if (tile.row != cappedRow)
                    {
                        MoveTile(tile, cappedRow, column);
                        moveDone = true;
                    }

                    lastMovedTile = tile;
                    cappedRow--;
                }
            }

            return (score, moveDone);
        }

        public (int score, bool moveDone) MoveUp()
        {
            CancelAnims();
            int score = 0;
            bool moveDone = false;

            for (int column = 0; column < Columns; column++)
            {
                int cappedRow = 0;

                var columnTiles = tiles.Where(x => x.column == column)
                              .OrderBy(x => x.row)
                              .ToList();

                if (!columnTiles.Any())
                    continue;

                Tile? lastMovedTile = null;

                foreach (var tile in columnTiles)
                {
                    if (lastMovedTile != null && lastMovedTile.Value == tile.Value && lastMovedTile.row == cappedRow - 1)
                    {
                        MoveTileToMerge(tile, cappedRow, column, Direction.Up);
                        AnimateTileMerge(lastMovedTile);
                        lastMovedTile.UpdateValue(tile.Value * 2);
                        lastMovedTile = tile;
                        moveDone = true;
                        score += tile.Value * 2;
                        continue;
                    }

                    if (tile.row != cappedRow)
                    {
                        MoveTile(tile, cappedRow, column);
                        moveDone = true;
                    }

                    lastMovedTile = tile;
                    cappedRow++;
                }
            }

            return (score, moveDone);
        }

        public (int score, bool moveDone) MoveLeft()
        {
            CancelAnims();
            int score = 0;
            bool moveDone = false;

            for (int row = 0; row < Rows; row++)
            {
                int cappedColumn = 0;

                var rowTiles = tiles.Where(x => x.row == row)
                              .OrderBy(x => x.column)
                              .ToList();

                if (!rowTiles.Any())
                    continue;

                Tile? lastMovedTile = null;

                foreach (var tile in rowTiles)
                {
                    if (lastMovedTile != null && lastMovedTile.Value == tile.Value && lastMovedTile.column == cappedColumn - 1)
                    {
                        MoveTileToMerge(tile, row, cappedColumn, Direction.Left);
                        AnimateTileMerge(lastMovedTile);
                        lastMovedTile.UpdateValue(tile.Value * 2);
                        lastMovedTile = tile;
                        moveDone = true;
                        score += tile.Value * 2;
                        continue;
                    }

                    if (tile.column != cappedColumn)
                    {
                        MoveTile(tile, row, cappedColumn);
                        moveDone = true;
                    }

                    lastMovedTile = tile;
                    cappedColumn++;
                }
            }

            return (score, moveDone);
        }

        public (int score, bool moveDone) MoveRight()
        {
            CancelAnims();
            int score = 0;
            bool moveDone = false;

            for (int row = 0; row < Rows; row++)
            {
                int cappedColumn = Columns - 1;

                var rowTiles = tiles.Where(x => x.row == row)
                              .OrderByDescending(x => x.column)
                              .ToList();

                if (!rowTiles.Any())
                    continue;

                Tile? lastMovedTile = null;

                foreach (var tile in rowTiles)
                {
                    if (lastMovedTile != null && lastMovedTile.Value == tile.Value && lastMovedTile.column == cappedColumn + 1)
                    {
                        MoveTileToMerge(tile, row, cappedColumn, Direction.Right);
                        AnimateTileMerge(lastMovedTile);
                        lastMovedTile.UpdateValue(tile.Value * 2);
                        lastMovedTile = tile;
                        moveDone = true;
                        score += tile.Value * 2;
                        continue;
                    }

                    if (tile.column != cappedColumn)
                    {
                        MoveTile(tile, row, cappedColumn);
                        moveDone = true;
                    }

                    lastMovedTile = tile;
                    cappedColumn--;
                }
            }

            return (score, moveDone);
        }
    }
}
