using Game2048.Resources.Logic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Game2048UnitTests
{
    public class GameLogicTests
    {
        GameLogic gameLogic;
        Type gameLogicType = typeof(GameLogic);
        FieldInfo gameGridField, tilesField;
        MethodInfo addTileToGridMethod;

        public GameLogicTests()
        {
            Grid gameGrid = new Grid();
            object[] gameLogicParameters = { 4, 4, gameGrid };

            var _gameLogic = Activator.CreateInstance(gameLogicType, gameLogicParameters);
            var _tilesField = gameLogicType.GetField("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
            var _gameGridField = gameLogicType.GetField("GameGrid", BindingFlags.NonPublic | BindingFlags.Instance);
            var _addTileToGridMethod = gameLogicType.GetMethod("AddTileToGrid", BindingFlags.NonPublic | BindingFlags.Instance);

            if (_gameLogic == null)
                throw new InvalidOperationException("GameLogic instance was not found");
            if (_tilesField == null)
                throw new InvalidOperationException("tiles field was not found");
            if (_gameGridField == null)
                throw new InvalidOperationException("GameGrid field was not found");
            if (_addTileToGridMethod == null)
                throw new InvalidOperationException("AddTileToGrid method was not found");

            gameLogic = (GameLogic)_gameLogic;
            tilesField = _tilesField;
            gameGridField = _gameGridField;
            addTileToGridMethod = _addTileToGridMethod;
        }

        private void ClearGridAndTiles()
        {
            ((Grid)gameGridField.GetValue(gameLogic)!).Children.Clear();
            ((List<Tile>)tilesField.GetValue(gameLogic)!).Clear();
        }

        private void FillGridAndTiles(List<(int value, int row, int column)> tilesData)
        {
            tilesData.ForEach(tileData =>
            {
                addTileToGridMethod.Invoke(gameLogic, new object[] { new Tile(tileData.value, tileData.row, tileData.column) });
            });
        }

        private bool CheckTilesField(List<(int value, int row, int column)> tilesData)
        {
            var tiles = (List<Tile>)tilesField.GetValue(gameLogic)!;

            if (tiles == null || tiles.Count != tilesData.Count)
                return false;

            foreach (var tile in tiles) {
                var tileData = (tile.Value, tile.row, tile.column);

                if (!tilesData.Contains(tileData))
                    return false;
            }

            return true;
        }

        [Theory]
        [MemberData(nameof(MovementMethodsTestData))]
        private void MovementMethodsTests(List<(int value, int row, int column)> initialTilesData,
                                    List<(int value, int row, int column)> finalTilesData,
                                    string methodName, int expectedScore, bool expectedMoveDone)
        {
            ClearGridAndTiles();
            FillGridAndTiles(initialTilesData);

            MethodInfo? moveMethod = gameLogicType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (moveMethod == null)
                throw new InvalidOperationException($"{methodName} method was not found");

            var result = moveMethod.Invoke(gameLogic, null);
            Assert.NotNull(result);

            var (resultScore, resultMoveDone) = ((int score, bool moveDone))result;
            Assert.Equal(expectedScore, resultScore);
            Assert.Equal(expectedMoveDone, resultMoveDone);

            Assert.True(CheckTilesField(finalTilesData));
        }

        [Theory]
        [MemberData(nameof(HasAvailableMovesTestData))]
        private void HasAvailableMovesTests(List<(int value, int row, int column)> tilesData,
                                             bool expectedResult)
        {
            ClearGridAndTiles();
            FillGridAndTiles(tilesData);

            MethodInfo? hasAvailableMovesMethod = gameLogicType.GetMethod("HasAvailableMoves", BindingFlags.Public | BindingFlags.Instance);
            if (hasAvailableMovesMethod == null)
                throw new InvalidOperationException($"HasAvailableMoves method was not found");

            var result = hasAvailableMovesMethod.Invoke(gameLogic, null);
            Assert.NotNull(result);

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> MovementMethodsTestData()
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

        public static IEnumerable<object[]> HasAvailableMovesTestData()
        {
            /*

            Test #1. No possible moves

            (1 is 16)

            | 2 4 8 1 |
            | 1 2 4 8 |
            | 8 1 2 4 |
            | 4 8 1 2 |

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 0, 1), (8, 0, 2), (16, 0, 3),
                                                             (16, 1, 0), (2, 1, 1), (4, 1, 2), (8, 1, 3),
                                                             (8, 2, 0), (16, 2, 1), (2, 2, 2), (4, 2, 3),
                                                             (4, 3, 0), (8, 3, 1), (16, 3, 2), (2, 3, 3) },
                false
            };
            /*

            Test #2. One possible merge

            (1 is 16)

            | 2 4 8 1 | 16
            | 1 2 4 1 | 16
            | 8 1 2 4 |
            | 4 8 1 2 |

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 0, 1), (8, 0, 2), (16, 0, 3),
                                                             (16, 1, 0), (2, 1, 1), (4, 1, 2), (16, 1, 3),
                                                             (8, 2, 0), (16, 2, 1), (2, 2, 2), (4, 2, 3),
                                                             (4, 3, 0), (8, 3, 1), (16, 3, 2), (2, 3, 3) },
                true
            };
            /*

            Test #3. Many possible merges

            | 2 4 8 8 |
            | 2 2 4 4 |
            | 8 8 4 4 |
            | 4 8 2 2 |

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (4, 0, 1), (8, 0, 2), (8, 0, 3),
                                                             (2, 1, 0), (2, 1, 1), (4, 1, 2), (4, 1, 3),
                                                             (8, 2, 0), (8, 2, 1), (4, 2, 2), (4, 2, 3),
                                                             (4, 3, 0), (8, 3, 1), (2, 3, 2), (2, 3, 3) },
                true
            };
            /*

            Test #4. Possible move but no merge

            | 2 . . . |
            | . . . . |
            | . 2 . . |
            | . . . . |

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> { (2, 0, 0), (2, 2, 1) },
                true
            };
            /*

            Test #5. Empty field

            | . . . . |
            | . . . . |
            | . . . . |
            | . . . . |

            */
            yield return new object[]
            {
                new List<(int value, int row, int column)> {  },
                true
            };
        }
    }
}