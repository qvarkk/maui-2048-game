using Game2048.Resources.Logic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Game2048UnitTests
{
    public class GameLogicTests
    {
        GameLogic gameLogic;
        FieldInfo gameGridField, tilesField;

        public GameLogicTests()
        {
            Grid gameGrid = new Grid();
            object[] gameLogicParameters =
            {
                4,
                4,
                gameGrid
            };

            Type type = typeof(GameLogic);
            var _gameLogic = Activator.CreateInstance(type, gameLogicParameters);

            var _tilesField = type.GetField("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
            var _gameGridField = type.GetField("GameGrid", BindingFlags.NonPublic | BindingFlags.Instance);

            if (_gameLogic == null)
                throw new InvalidOperationException("GameLogic instance was not found");
            if (_tilesField == null)
                throw new InvalidOperationException("tiles field was not found");
            if (_gameGridField == null)
                throw new InvalidOperationException("GameGrid field was not found");

            gameLogic = (GameLogic)_gameLogic;
            tilesField = _tilesField;
            gameGridField = _gameGridField;
        }

        private void ClearGridAndTiles()
        {
            ((Grid)gameGridField.GetValue(gameLogic)!).Children.Clear();
            ((List<Tile>)tilesField.GetValue(gameLogic)!).Clear();
        }

        private bool CheckTilesField(List<(int value, int row, int column)> tilesInformation)
        {
            var tiles = (List<Tile>)tilesField.GetValue(gameLogic)!;

            if (tiles == null || tiles.Count != tilesInformation.Count)
                return false;

            foreach (var tile in tiles) {
                var tileData = (tile.Value, tile.row, tile.column);

                if (!tilesInformation.Contains(tileData))
                    return false;
            }

            return true;
        }

        [Theory]
        [MemberData(nameof(MovementData))]
        private void TestMoveMethods(List<(int value, int row, int column)> initialTilesInformation,
                                    List<(int value, int row, int column)> finalTilesInformation,
                                    string methodName, int score, bool moveDone)
        {
            // Clear tiles list and grid to ensure game field is empty
            ClearGridAndTiles();

            // Get AddTileToGrid method to generate tiles from initialTilesInformation
            MethodInfo? addTileToGridMethod = typeof(GameLogic).GetMethod("AddTileToGrid", BindingFlags.NonPublic | BindingFlags.Instance);
            if (addTileToGridMethod == null)
                throw new InvalidOperationException("AddTileToGrid method was not found");

            // Fill GameGrid and tiles fields
            initialTilesInformation.ForEach(tileInfo =>
            {
                addTileToGridMethod.Invoke(gameLogic, new object[] { new Tile(tileInfo.value, tileInfo.row, tileInfo.column) });
            });

            // Get movement method corresponding to methodName
            MethodInfo? moveMethod = typeof(GameLogic).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (moveMethod == null)
                throw new InvalidOperationException($"{methodName} method was not found");

            // Invoke movement method and check result for null
            var result = moveMethod.Invoke(gameLogic, null);
            Assert.NotNull(result);

            // Assert tiles field to be as it expected in finalTilesInformation
            Assert.True(CheckTilesField(finalTilesInformation));
            var (resultScore, resultMoveDone) = ((int score, bool moveDone))result;

            // Assert returned values
            Assert.Equal(score, resultScore);
            if (moveDone)
                Assert.True(moveDone);
            else 
                Assert.False(moveDone);
        }

        public static IEnumerable<object[]> MovementData()
        {
            /*
 
            Test #1. Merge two tiles

            | . . . . |    | . . . . |
            | . . . . | => | . . . . |
            | 2 . . . | => | . . . . |
            | 2 . . . |    | 4 . . . |

            Score += 4
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 2, 0), (2, 3, 0) },
                new List<(int value, int row, int column)> { (4, 3, 0) },
                "MoveDown", 4, true
            };
            /*
             
            Test #2. No possible move

            | . . . . |    | . . . . |
            | . . . . | => | . . . . |
            | 2 . . . | => | 2 . . . |
            | 4 . . . |    | 4 . . . |
            
            Score += 0
            MoveDone = false

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 2, 0), (4, 3, 0) },
                new List<(int value, int row, int column)> { (2, 2, 0), (4, 3, 0) },
                "MoveDown", 0, false
            };
            /*

            Test #3. Merge two tiles two times

            | 2 . . . |    | . . . . |
            | 2 . . . | => | . . . . |
            | 2 . . . | => | 4 . . . |
            | 2 . . . |    | 4 . . . |

            Score += 8
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (2, 1, 0), (2, 2, 0), (2, 3, 0) },
                new List<(int value, int row, int column)> { (4, 2, 0), (4, 3, 0) },
                "MoveDown", 8, true
            };

            /*
             
            Test #1. Merge two tiles

            | . . . . |    | 4 . . . |
            | . . . . | => | . . . . |
            | 2 . . . | => | . . . . |
            | 2 . . . |    | . . . . |
            
            Score += 4
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 2, 0), (2, 3, 0) },
                new List<(int value, int row, int column)> { (4, 0, 0) },
                "MoveUp", 4, true
            };
            /*
             
            Test #2. No possible move

            | 2 . . . |    | 2 . . . |
            | 4 . . . | => | 4 . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |
            
            Score += 0
            MoveDone = false

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 1, 0) },
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 1, 0) },
                "MoveUp", 0, false
            };
            /*
             
            Test #3. Merge two tiles two times
            
            | 2 . . . |    | 4 . . . |
            | 2 . . . | => | 4 . . . |
            | 2 . . . | => | . . . . |
            | 2 . . . |    | . . . . |

            Score += 8
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (2, 1, 0), (2, 2, 0), (2, 3, 0) },
                new List<(int value, int row, int column)> { (4, 0, 0), (4, 1, 0) },
                "MoveUp", 8, true
            };

            /*
             
            Test #1. Merge two tiles

            | 2 2 . . |    | 4 . . . |
            | . . . . | => | . . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |
            
            Score += 4
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (2, 0, 1) },
                new List<(int value, int row, int column)> { (4, 0, 0) },
                "MoveLeft", 4, true
            };
            /*
             
            Test #2. No possible move

            | 2 4 . . |    | 2 4 . . |
            | . . . . | => | . . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |
            
            Score += 0
            MoveDone = false

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 0, 1) },
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 0, 1) },
                "MoveLeft", 0, false
            };
            /*
             
            Test #3. Merge two tiles two times
            
            | 2 2 2 2 |    | 4 4 . . |
            | . . . . | => | . . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |

            Score += 8
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (2, 0, 1), (2, 0, 2), (2, 0, 3) },
                new List<(int value, int row, int column)> { (4, 0, 0), (4, 0, 1) },
                "MoveLeft", 8, true
            };

            /*
             
            Test #1. Merge two tiles

            | . . 2 2 |    | . . . 4 |
            | . . . . | => | . . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |
            
            Score += 4
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 2), (2, 0, 3) },
                new List<(int value, int row, int column)> { (4, 0, 3) },
                "MoveRight", 4, true
            };
            /*
             
            Test #2. No possible move

            | . . 2 4 |    | . . 2 4 |
            | . . . . | => | . . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |
            
            Score += 0
            MoveDone = false

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 2), (4, 0, 3) },
                new List<(int value, int row, int column)> { (2, 0, 2), (4, 0, 3) },
                "MoveRight", 0, false
            };
            /*
             
            Test #3. Merge two tiles two times
            
            | 2 2 2 2 |    | . . 4 4 |
            | . . . . | => | . . . . |
            | . . . . | => | . . . . |
            | . . . . |    | . . . . |

            Score += 8
            MoveDone = true

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (2, 0, 1), (2, 0, 2), (2, 0, 3) },
                new List<(int value, int row, int column)> { (4, 0, 2), (4, 0, 3) },
                "MoveRight", 8, true
            };
        }
    }
}