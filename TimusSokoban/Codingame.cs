//#define BYTE_ARRAY
//#define PRIORITY_QUEUE
//#define SIMULATE
#define ONLINE_JUDGE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Timus
{
    public class BoardSettings
    {
        private readonly char[] _charMap = "# .$@*+".ToCharArray();
        public BoardSettings() { }
        public BoardSettings(char wall, char space, char target)
        {
            _charMap[0] = wall;
            _charMap[1] = space;
            _charMap[2] = target;
        }
        public char Wall { get => _charMap[0]; }
        public char Space { get => _charMap[1]; }
        public char FreeTarget { get => _charMap[2]; }
        public char Box { get => _charMap[3]; }
        public char Player { get => _charMap[4]; }
        public char TargetWithBox { get => _charMap[5]; }
        public char TargetWithPlayer { get => _charMap[6]; }
    }
    public interface IOHelper
    {
        BoardState ReadBoard(BoardSettings settings);
        BoardState ReadMove(BoardState board, BoardSettings settings);
        void PrintBoard(BoardState board, BoardSettings settings);
        void PrintSolution(string moveSequence);
    }
    public class CodingameIO : IOHelper
    {
        public BoardState ReadBoard(BoardSettings settings)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]);
            Console.Error.WriteLine(width);
            int height = int.Parse(inputs[1]);
            Console.Error.WriteLine(height);
            int boxCount = int.Parse(inputs[2]);
            var lines = new List<string>();
            for (int i = 0; i < height; i++)
            {
                string line = Console.ReadLine();
                Console.Error.WriteLine(line);
                lines.Add(line);
            }

            BoardState board = ReadBoard(lines.ToArray(), settings);
            BoardState.BoxCount = boxCount;
            return board;
        }

        public BoardState ReadMove(BoardState board, BoardSettings settings)
        {
            var inputs = Console.ReadLine().Split(' ');
            int pusherX = int.Parse(inputs[0]);
            int pusherY = int.Parse(inputs[1]);
            var boxes = new List<(int, int)>();
            for (int i = 0; i < BoardState.BoxCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int boxX = int.Parse(inputs[0]);
                int boxY = int.Parse(inputs[1]);
                boxes.Add((boxX, boxY));
            }

            board = FillBoard(board, pusherX, pusherY, boxes);
            return board;
        }

        private static BoardState FillBoard(BoardState board, int px, int py, List<(int, int)> boxes)
        {
            short player = (short)(BoardState.Width * py + px);
            foreach (var b in boxes)
            {
                short index = (short)(BoardState.Width * b.Item2 + b.Item1);
                board[index] |= Cell.Box;
            }

            board.ReprocessAccessible(player);
            board.LastMove = new Move(player, Direction.D);
            return board;
        }

        private BoardState ReadBoard(string[] lines, BoardSettings settings)
        {
            BoardState.Width = lines.Max(l => l.Length);
            BoardState.Height = lines.Length;
            BoardState.Len = BoardState.Width * BoardState.Height;
            var board = new BoardState(BoardState.Len);

            short player = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    short index = (short)(BoardState.Width * i + j);
                    if (lines[i][j] == settings.TargetWithPlayer || lines[i][j] == settings.Player)
                    {
                        player = index;
                    }

                    board[index] = lines[i][j] == settings.TargetWithBox || lines[i][j] == settings.Box
                        ? Cell.Box
                        : lines[i][j] == settings.Wall ? Cell.Wall : Cell.Space;

                    if (lines[i][j] == settings.FreeTarget || lines[i][j] == settings.TargetWithBox || lines[i][j] == settings.TargetWithPlayer)
                    {
                        board[index] |= Cell.Target;
                        BoardState.Targets.Add(index);
                    }
                }
            }

            if (player >= 0)
            {
                board.ReprocessAccessible(player);
                // Dummy move to record start player position.
                board.LastMove = new Move(player, Direction.D);
            }

            return board;
        }

        public void PrintBoard(BoardState board, BoardSettings settings)
        {
            for (int i = 0; i < BoardState.Len; i++)
            {
                if (i / BoardState.Width > 0 && i % BoardState.Width == 0)
                {
                    Console.Error.Write(Environment.NewLine);
                }

                switch (board[i] & ~Cell.Target)
                {
                    case Cell.Wall:
                        Console.Error.Write(settings.Wall);
                        break;
                    case Cell.Accessible:
                    case Cell.Space:
                        bool playerHere = board.LastMove.BoxId == i;
                        Console.Error.Write((board[i] & Cell.Target) != 0
                            ? playerHere ? settings.TargetWithPlayer : settings.FreeTarget
                            : playerHere ? settings.Player : settings.Space);
                        break;
                    case Cell.Box:
                        Console.Error.Write((board[i] & Cell.Target) != 0 ? settings.TargetWithBox : settings.Box);
                        break;
                    default:
                        Console.Error.WriteLine(board[i]);
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void PrintSolution(string moveSequence)
        {
            throw new NotImplementedException();
        }
    }
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> list;
        public int Count { get { return list.Count; } }
        public readonly bool IsDescending;

        public PriorityQueue()
        {
            list = new List<T>();
        }

        public PriorityQueue(bool isdesc)
            : this()
        {
            IsDescending = isdesc;
        }

        public PriorityQueue(int capacity)
            : this(capacity, false)
        { }

        public PriorityQueue(IEnumerable<T> collection)
            : this(collection, false)
        { }

        public PriorityQueue(int capacity, bool isdesc)
        {
            list = new List<T>(capacity);
            IsDescending = isdesc;
        }

        public PriorityQueue(IEnumerable<T> collection, bool isdesc)
            : this()
        {
            IsDescending = isdesc;
            foreach (var item in collection)
                Enqueue(item);
        }

        public void Enqueue(T x)
        {
            list.Add(x);
            int i = Count - 1;

            while (i > 0)
            {
                int p = (i - 1) / 2;
                if ((IsDescending ? -1 : 1) * list[p].CompareTo(x) <= 0) break;

                list[i] = list[p];
                i = p;
            }

            if (Count > 0) list[i] = x;
        }

        public T Dequeue()
        {
            T target = Peek();
            T root = list[Count - 1];
            list.RemoveAt(Count - 1);

            int i = 0;
            while (i * 2 + 1 < Count)
            {
                int a = i * 2 + 1;
                int b = i * 2 + 2;
                int c = b < Count && (IsDescending ? -1 : 1) * list[b].CompareTo(list[a]) < 0 ? b : a;

                if ((IsDescending ? -1 : 1) * list[c].CompareTo(root) >= 0) break;
                list[i] = list[c];
                i = c;
            }

            if (Count > 0) list[i] = root;
            return target;
        }

        public T Peek()
        {
            if (Count == 0) throw new InvalidOperationException("Queue is empty.");
            return list[0];
        }

        public void Clear()
        {
            list.Clear();
        }
    }

    [DebuggerDisplay("{BoxId} - {Direction}")]
    public struct Move : IEquatable<Move>
    {
        public Move(short i, Direction direction)
        {
            BoxId = i;
            Direction = direction;
        }

        public short BoxId { get; }

        public Direction Direction { get; }

        public bool Equals(Move other)
        {
            return BoxId == other.BoxId && Direction == other.Direction;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Move move && Equals(move);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BoxId * 397) ^ (int)Direction;
            }
        }
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public struct BoardState : IEquatable<BoardState>, IComparable<BoardState>
    {
        public const byte ValuableMask = 0x66;
        public const byte AccessibleMask = 0x22;

        public static int CurrentId = 1;

        public static int Width;
        public static int Height;
        public static int Len;
        public static int BoxCount;
        public static List<short> Targets = new List<short>();

        private static int ElementCount
        {
            get
            {
#if BYTE_ARRAY
                return Len / 2 + Len % 2;
#else
                return Len;
#endif
            }
        }
        public int Weight { get; set; }

        public int Id { get; set; }

        public int ParentId { get; set; }

        public short Generation { get; set; }

#if BYTE_ARRAY
        private byte[] State { get; }
#else
        private Cell[] State { get; }
#endif
        public Move LastMove { get; set; }

        public BoardState(BoardState orig)
            : this(ElementCount)
        {
            for (int i = 0; i < ElementCount; i++)
            {
#if BYTE_ARRAY
                State[i] = (byte)(orig.State[i] & ~AccessibleMask);
#else
                this[i] = orig[i] & ~Cell.Accessible;
#endif
            }
            Generation = orig.Generation;
        }

        public BoardState(int elementCount)
        {
#if BYTE_ARRAY
            State = new byte[elementCount];
#else
            State = new Cell[elementCount];
#endif
            Weight = 0;
            ParentId = 0;
            Id = 0;
            Generation = 0;
            LastMove = default(Move);
        }

        public Cell this[int index]
        {
            get
            {
#if BYTE_ARRAY
                return (Cell)((index & 1) == 0
                    ? State[index >> 1] >> 4
                    : State[index >> 1] & 0x0F);
#else
                return State[index];
#endif
            }
            set
            {
#if BYTE_ARRAY
                State[index >> 1] = (byte)((index & 1) == 0
                    ? (byte)value << 4 | (State[index >> 1] & 0x0F)
                    : (byte)value | (State[index >> 1] & 0xF0));
#else
                State[index] = value;
#endif
            }
        }

        public BoardState MakeMove(Move move, bool restoreSolution = false)
        {
            var board = new BoardState(this);

            var b = move.GetFinalBoxPosition();
            board[b] ^= Cell.Box;
            board[move.BoxId] ^= Cell.Box;

            if (!restoreSolution)
            {
                board.ReprocessAccessible(move.BoxId);
                board.ParentId = Id;
                board.Id = CurrentId;
                CurrentId++;
            }

            board.LastMove = move;
            return board;
        }

        public void ReprocessAccessible(int player)
        {
            if (player < 0) return;

            this[player] ^= Cell.Accessible;

            if (this[player + 1] == Cell.Space || this[player + 1] == Cell.Target)
            {
                ReprocessAccessible(player + 1);
            }
            if (this[player - 1] == Cell.Space || this[player - 1] == Cell.Target)
            {
                ReprocessAccessible(player - 1);
            }
            if (this[player + Width] == Cell.Space || this[player + Width] == Cell.Target)
            {
                ReprocessAccessible(player + Width);
            }
            if (this[player - Width] == Cell.Space || this[player - Width] == Cell.Target)
            {
                ReprocessAccessible(player - Width);
            }
        }

        public bool Equals(BoardState other)
        {
            for (int i = 0; i < ElementCount; i++)
            {
#if BYTE_ARRAY
                if ((State[i] & ValuableMask) != (other.State[i] & ValuableMask))
#else
                if ((this[i] & Cell.Valuable) != (other[i] & Cell.Valuable))
#endif
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BoardState state && Equals(state);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < ElementCount; i++)
                {
#if BYTE_ARRAY
                    hash = hash * 397 + (int)(State[i] & ValuableMask);
#else
                    hash = hash * 397 + (int)(this[i] & Cell.Valuable);
#endif
                }

                return hash;
            }
        }

        public int CompareTo(BoardState other)
        {
            return Weight.CompareTo(other.Weight);
        }

        public void CalculateWeight()
        {
#if PRIORITY_QUEUE
            Weight = Generation;
            int boxId = 0;
            for (short i = 0; i < Len; i++)
            {
                if ((this[i] & Cell.Box) != 0)
                {
                    Weight += Targets.Contains(i)
                        ? -100
                        :
                        (Targets[boxId] % Width - i % Width) * (Targets[boxId] % Width - i % Width) +
                        (Targets[boxId] / Width - i / Width) * (Targets[boxId] / Width - i / Width);
                    boxId++;
                }
                //else if ((this[i] & Cell.Accessible) != 0)
                //{
                //    Weight -= 5;
                //}
            }
#endif
        }

        private string DebuggerDisplay
        {
            get
            {
                var sb = new StringBuilder();
                for (int i = 0; i < Len; i++)
                {
                    if (i / Width > 0 && i % Width == 0)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    var target = (this[i] & Cell.Target) != 0;
                    switch (this[i] & ~Cell.Target)
                    {
                        case Cell.Wall:
                            sb.Append('#');
                            break;
                        case Cell.Accessible:
                            sb.Append(target ? '.' : ' ');
                            break;
                        case Cell.Space:
                            sb.Append(target ? 'Z' : 'X');
                            break;
                        case Cell.Box:
                            sb.Append(target ? '*' : '$');
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return sb.ToString();
            }
        }
    }

    public static class SokobanExtensions
    {
        public static bool IsObstacle(this Cell cell)
        {
            return (cell & (Cell.Box | Cell.Wall)) != 0;
        }

        public static bool IsWallOrTarget(this Cell cell)
        {
            return (cell & (Cell.Target | Cell.Wall)) != 0;
        }

        public static bool IsFree(this Cell cell)
        {
            return !IsObstacle(cell);
        }

        public static int GetStartPlayerPosition(this Move move)
        {
            switch (move.Direction)
            {
                case Direction.R:
                    return move.BoxId - 1;
                case Direction.L:
                    return move.BoxId + 1;
                case Direction.U:
                    return move.BoxId + BoardState.Width;
                case Direction.D:
                    return move.BoxId - BoardState.Width;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int GetFinalBoxPosition(this Move move)
        {
            switch (move.Direction)
            {
                case Direction.R:
                    return move.BoxId + 1;
                case Direction.L:
                    return move.BoxId - 1;
                case Direction.U:
                    return move.BoxId - BoardState.Width;
                case Direction.D:
                    return move.BoxId + BoardState.Width;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsWinning(this BoardState board)
        {
            for (int i = 0; i < BoardState.Len; i++)
            {
                if ((board[i] & Cell.Target) != 0 &&
                    (board[i] & Cell.Box) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<Move> FindMoves(this BoardState board)
        {
            for (short i = 0; i < BoardState.Len; i++)
            {
                if ((board[i] & Cell.Box) != 0)
                {
                    if ((board[i - 1] & Cell.Accessible) != 0 &&
                        board[i + 1].IsFree())
                    {
                        yield return new Move(i, Direction.R);
                    }
                    if ((board[i + 1] & Cell.Accessible) != 0 &&
                        board[i - 1].IsFree())
                    {
                        yield return new Move(i, Direction.L);
                    }
                    if ((board[i - BoardState.Width] & Cell.Accessible) != 0 &&
                        board[i + BoardState.Width].IsFree())
                    {
                        yield return new Move(i, Direction.D);
                    }
                    if ((board[i + BoardState.Width] & Cell.Accessible) != 0 &&
                        board[i - BoardState.Width].IsFree())
                    {
                        yield return new Move(i, Direction.U);
                    }
                }
            }
        }

        public static bool IsMoveOk(this BoardState board, int box, Direction direction)
        {
            switch (direction)
            {
                case Direction.R:
                    return board[box + 1].IsFree();
                case Direction.L:
                    return board[box - 1].IsFree();
                case Direction.U:
                    return board[box - BoardState.Width].IsFree();
                case Direction.D:
                    return board[box + BoardState.Width].IsFree();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BoardState MakeMove(this BoardState board, ref short player, char move)
        {
            var newBoard = new BoardState(board);
            short offset = 0;
            switch (Char.ToLower(move))
            {
                case 'r':
                    offset = 1;
                    break;
                case 'l':
                    offset = -1;
                    break;
                case 'u':
                    offset = (short)-BoardState.Width;
                    break;
                case 'd':
                    offset = (short)BoardState.Width;
                    break;
            }
            if ((newBoard[player + offset] & Cell.Box) != 0 && newBoard[player + 2 * offset].IsFree())
            {
                newBoard[player + offset] ^= Cell.Box;
                newBoard[player + 2 * offset] ^= Cell.Box;
            }
            if (newBoard[player + offset].IsFree())
            {
                player += offset;
            }

            return newBoard;
        }


        public static bool IsStatePlayable(this BoardState board)
        {
            int i = board.LastMove.GetFinalBoxPosition();

            // #1: angle pattern
            if (board[i - 1] == Cell.Wall && board[i - BoardState.Width] == Cell.Wall ||
                board[i - 1] == Cell.Wall && board[i + BoardState.Width] == Cell.Wall ||
                board[i + 1] == Cell.Wall && board[i - BoardState.Width] == Cell.Wall ||
                board[i + 1] == Cell.Wall && board[i + BoardState.Width] == Cell.Wall)
            {
                // Just came on target
                return (board[i] & Cell.Target) != 0;
            }

            // #2: square pattern
            if (board[i - 1].IsObstacle())
            {
                if (board[i - 1 - BoardState.Width].IsObstacle() && board[i - BoardState.Width].IsObstacle())
                {
                    if (!((board[i] & Cell.Target) != 0 &&
                        board[i - 1].IsWallOrTarget() &&
                        board[i - 1 - BoardState.Width].IsWallOrTarget() &&
                        board[i - BoardState.Width].IsWallOrTarget()))
                    {
                        return false;
                    }
                }
                if (board[i - 1 + BoardState.Width].IsObstacle() && board[i + BoardState.Width].IsObstacle())
                {
                    if (!((board[i] & Cell.Target) != 0 &&
                        board[i - 1].IsWallOrTarget() &&
                        board[i - 1 + BoardState.Width].IsWallOrTarget() &&
                        board[i + BoardState.Width].IsWallOrTarget()))
                    {
                        return false;
                    }
                }
            }
            if (board[i + 1].IsObstacle())
            {
                if (board[i + 1 - BoardState.Width].IsObstacle() && board[i - BoardState.Width].IsObstacle())
                {
                    if (!((board[i] & Cell.Target) != 0 &&
                        board[i + 1].IsWallOrTarget() &&
                        board[i + 1 - BoardState.Width].IsWallOrTarget() &&
                        board[i - BoardState.Width].IsWallOrTarget()))
                    {
                        return false;
                    }
                }
                if (board[i + 1 + BoardState.Width].IsObstacle() && board[i + BoardState.Width].IsObstacle())
                {
                    if (!((board[i] & Cell.Target) != 0 &&
                        board[i + 1].IsWallOrTarget() &&
                        board[i + 1 + BoardState.Width].IsWallOrTarget() &&
                        board[i + BoardState.Width].IsWallOrTarget()))
                    {
                        return false;
                    }
                }
            }

            // #3: 

            return true;
        }
    }

    [Flags]
    public enum Cell : byte
    {
        Space = 0,
        Wall = 1,
        Accessible = 2,
        Box = 4,
        Target = 8,
        Valuable = Accessible | Box
    }

    public enum Direction : byte
    {
        R,
        L,
        U,
        D
    }

    public class Sokoban
    {
        private static void Main()
        {
#if ONLINE_JUDGE
            //var input = Console.In.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
#else
            var solveTimer = new Stopwatch();
            solveTimer.Start();

            var input = test14.ToList();
#endif

            //var board = ReadBoard(input.ToArray());
            var settings = new BoardSettings('#', '.', '*');
            var defaultSettings = new BoardSettings();
            var io = new CodingameIO();
            var board = io.ReadBoard(settings);
            string moveChain = String.Empty;
            int moveCount = 0;
            // game loop
            while (true)
            {
                board = io.ReadMove(board, settings);
                if (moveCount == 0)
                {
                    io.PrintBoard(board, defaultSettings);
#if !SIMULATE
                    var positions = new HashSet<BoardState>();
                    bool solutionFound = false;
#if PRIORITY_QUEUE
            var solutionQueue = new PriorityQueue<BoardState>();
#else
                    var solutionQueue = new Queue<BoardState>();
#endif
                    solutionQueue.Enqueue(board);

                    BoardState winBoard = board;

                    void Process(BoardState b)
                    {
                        if (b.IsWinning())
                        {
                            solutionFound = true;
                            winBoard = b;
                            return;
                        }
                        if (positions.Add(b))
                        {
                            foreach (var move in b.FindMoves())
                            {
                                var newB = b.MakeMove(move);
                                if (newB.IsStatePlayable())
                                {
                                    newB.Generation++;
                                    newB.CalculateWeight();
                                    solutionQueue.Enqueue(newB);
                                }
                            }
                        }
                    }

                    int maxGen = 0;
                    while (!solutionFound && solutionQueue.Count > 0)
                    {
                        string solutions = $"Queue len: {solutionQueue.Count}";
                        var b = solutionQueue.Dequeue();
                        Process(b);
#if !ONLINE_JUDGE
                if (maxGen < b.Generation)
                {
                    maxGen = b.Generation;
                    Console.WriteLine($"Gen: {b.Generation}, Positions: {positions.Count}, {solutions}");
                }
#endif
                    }

#if !ONLINE_JUDGE
            var solutionRestoreTimer = new Stopwatch();
            solutionRestoreTimer.Start();
#endif

                    var solution = new Stack<Move>();
                    while (!winBoard.Equals(board))
                    {
                        solution.Push(winBoard.LastMove);
                        winBoard = positions.First(p => p.Id == winBoard.ParentId);
                    }

                    var moveChainBuilder = new StringBuilder();
                    while (solution.Count > 0)
                    {
                        var move = solution.Pop();
                        moveChainBuilder.Append(GetPath(winBoard, move));
                        moveChainBuilder.Append(move.Direction);
                        winBoard = winBoard.MakeMove(move, true);
                    }

                    moveChain = moveChainBuilder.ToString().ToUpper();
                    Console.Error.WriteLine(moveChain);
                }
#if !ONLINE_JUDGE
            solutionRestoreTimer.Stop();
            solveTimer.Stop();
            Console.WriteLine();
            Console.WriteLine($"Elapsed time: {solveTimer.ElapsedMilliseconds}, " +
                              $"Solution restore time: {solutionRestoreTimer.ElapsedMilliseconds}");
            Console.WriteLine();
#endif
                //Console.WriteLine(moveChain);
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Console.WriteLine(moveChain[moveCount]);
                moveCount++;
            }
#endif
#if !ONLINE_JUDGE
            int step = 0;
            var simulationBoard = new BoardState(board);
            var player = board.LastMove.BoxId;
            ConsoleKey key = default(ConsoleKey);
            do
            {
#if SIMULATE
                var c = ' ';
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        c = 'l';
                        break;
                    case ConsoleKey.RightArrow:
                        c = 'r';
                        break;
                    case ConsoleKey.UpArrow:
                        c = 'u';
                        break;
                    case ConsoleKey.DownArrow:
                        c = 'd';
                        break;
                    case ConsoleKey.R:
                        simulationBoard = new BoardState(board);
                        player = board.LastMove.BoxId;
                        break;
                }
                simulationBoard = simulationBoard.MakeMove(ref player, c);
#endif

                Console.WriteLine();
                PrintBoard(simulationBoard, player);
#if !SIMULATE
                if (step < moveChain.Length)
                {
                    var c = moveChain[step];
                    simulationBoard = simulationBoard.MakeMove(ref player, c);
                    step++;
                }
#endif

                Console.SetCursorPosition(0, Console.CursorTop - BoardState.Height);
            } while ((key = Console.ReadKey().Key) != ConsoleKey.Escape);
#endif
        }

        private static string GetPath(BoardState board, Move move)
        {
            var startPoint = board.LastMove.BoxId;
            var endPoint = move.GetStartPlayerPosition();
            Queue<int> positions = new Queue<int>();
            positions.Enqueue(startPoint);
            var covered = new Dictionary<int, int>
            {
                [startPoint] = startPoint
            };

            while (positions.Count > 0)
            {
                var p = positions.Dequeue();

                if (p.Equals(endPoint))
                {
                    StringBuilder sb = new StringBuilder();
                    while (!covered[p].Equals(p))
                    {
                        sb.Append(GetPathChar(p, covered[p]));
                        p = covered[p];
                    }
                    return new string(sb.ToString().Reverse().ToArray());
                }

                foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    if (board.IsMoveOk(p, direction))
                    {
                        var newP = GetPosition(p, direction);
                        if (!covered.ContainsKey(newP))
                        {
                            covered[newP] = p;
                            positions.Enqueue(newP);
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static char GetPathChar(int start, int end)
        {
            return
                start - end > 1 ? 'd' :
                start - end == 1 ? 'r' :
                start - end == -1 ? 'l' : 'u';
        }

        private static int GetPosition(int p, Direction direction)
        {
            switch (direction)
            {
                case Direction.R:
                    return p + 1;
                case Direction.L:
                    return p - 1;
                case Direction.U:
                    return p - BoardState.Width;
                case Direction.D:
                    return p + BoardState.Width;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}