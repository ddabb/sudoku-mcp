using System.ComponentModel;
using ModelContextProtocol.Server;
using System.Text;

/// <summary>
/// 数独工具类，提供数独生成、求解等功能
/// </summary>
public class SudokuTools
{
    private const int SIZE = 9;
    private const int EMPTY = 0;
    private static readonly Random random = new Random();

    [McpServerTool]
    [Description("生成一个有效的数独谜题，可指定难度级别")]
    public string GenerateSudoku(
        [Description("难度级别 (1-5)，1为简单，2为中等，3为标准，4为困难，5为专家")] int difficulty = 3)
    {
        // 限制难度范围
        difficulty = Math.Clamp(difficulty, 1, 5);

        // 生成完整的解决方案
        int[,] grid = new int[SIZE, SIZE];
        FillGrid(grid);

        // 根据难度移除数字
        int cellsToRemove = difficulty switch
        {
            1 => 30, // 简单
            2 => 40, // 中等
            3 => 45, // 标准
            4 => 50, // 困难
            5 => 55, // 专家
            _ => 45  // 默认标准
        };

        // 改进：使用唯一解验证的移除逻辑
        RemoveCellsToEnsureUniqueSolution(grid, cellsToRemove);

        // 转换为字符串格式
        return ConvertToString(grid);
    }

    [McpServerTool]
    [Description("求解数独谜题，返回完整解答")]
    public string SolveSudoku(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string puzzle)
    {
        // 验证输入
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            return "错误：输入必须是81个字符的字符串，使用0表示空格";
        }

        // 转换为二维数组
        int[,] grid = new int[SIZE, SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * SIZE + j;
                if (index < puzzle.Length && char.IsDigit(puzzle[index]))
                {
                    grid[i, j] = puzzle[index] - '0';
                }
                else
                {
                    grid[i, j] = 0;
                }
            }
        }

        // 首先检查解的数量
        int solutionCount = CountSolutions(puzzle, 3); // 最多检查3个解

        if (solutionCount == 0)
        {
            return "错误：无法求解此数独谜题，可能不是有效的数独";
        }

        // 求解数独获取一个解
        if (SolveSudokuGrid(grid))
        {
            StringBuilder result = new StringBuilder();

            if (solutionCount == 1)
            {
                result.AppendLine("数独求解成功（唯一解）：");
            }
            else
            {
                result.AppendLine($"数独有多个解（检测到至少{solutionCount}个解），以下是一个参考解：");
            }

            result.Append(ConvertToString(grid));
            return result.ToString();
        }
        else
        {
            return "错误：无法求解此数独谜题，可能不是有效的数独";
        }
    }

    [McpServerTool]
    [Description("验证数独谜题是否符合规则（行、列、九宫格内无重复数字）")]
    public bool ValidateSudoku(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string? puzzle)
    {
        // 验证输入
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            return false;
        }

        // 转换为二维数组
        int[,] grid = new int[SIZE, SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * SIZE + j;
                if (index < puzzle.Length && char.IsDigit(puzzle[index]))
                {
                    grid[i, j] = puzzle[index] - '0';
                }
                else
                {
                    grid[i, j] = 0;
                }
            }
        }

        // 验证每一行
        for (int row = 0; row < SIZE; row++)
        {
            bool[] used = new bool[SIZE + 1];
            for (int col = 0; col < SIZE; col++)
            {
                int num = grid[row, col];
                if (num != 0)
                {
                    if (used[num])
                    {
                        return false;
                    }
                    used[num] = true;
                }
            }
        }

        // 验证每一列
        for (int col = 0; col < SIZE; col++)
        {
            bool[] used = new bool[SIZE + 1];
            for (int row = 0; row < SIZE; row++)
            {
                int num = grid[row, col];
                if (num != 0)
                {
                    if (used[num])
                    {
                        return false;
                    }
                    used[num] = true;
                }
            }
        }

        // 验证每个3x3方格
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                bool[] used = new bool[SIZE + 1];
                for (int row = boxRow * 3; row < boxRow * 3 + 3; row++)
                {
                    for (int col = boxCol * 3; col < boxCol * 3 + 3; col++)
                    {
                        int num = grid[row, col];
                        if (num != 0)
                        {
                            if (used[num])
                            {
                                return false;
                            }
                            used[num] = true;
                        }
                    }
                }
            }
        }

        return true;
    }

    [McpServerTool]
    [Description("将数独谜题转换为美观的可视化表格形式")]
    public string GetSudokuVisual(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string puzzle)
    {
        // 验证输入
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            return "错误：输入必须是81个字符的字符串，使用0表示空格";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("┌───────┬───────┬───────┐");

        for (int i = 0; i < SIZE; i++)
        {
            sb.Append("│ ");
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * SIZE + j;
                char c = puzzle[index];
                sb.Append(c == '0' ? '.' : c);
                sb.Append(' ');

                if ((j + 1) % 3 == 0 && j < SIZE - 1)
                {
                    sb.Append("│ ");
                }
            }
            sb.AppendLine("│");

            if ((i + 1) % 3 == 0 && i < SIZE - 1)
            {
                sb.AppendLine("├───────┼───────┼───────┤");
            }
        }

        sb.AppendLine("└───────┴───────┴───────┘");
        return sb.ToString();
    }

    [McpServerTool]
    [Description("为数独谜题提供指定数量的解题提示，显示应填入的数字位置")]
    public string GetSudokuHint(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string puzzle,
        [Description("需要获取的提示数量，默认为1")] int hintCount = 1)
    {
        // 验证输入
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            return "错误：输入必须是81个字符的字符串，使用0表示空格";
        }

        // 转换为二维数组
        int[,] grid = new int[SIZE, SIZE];
        int[,] solution = new int[SIZE, SIZE];

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * SIZE + j;
                if (index < puzzle.Length && char.IsDigit(puzzle[index]))
                {
                    grid[i, j] = puzzle[index] - '0';
                    solution[i, j] = grid[i, j];
                }
                else
                {
                    grid[i, j] = 0;
                    solution[i, j] = 0;
                }
            }
        }

        // 求解数独以获取完整解决方案
        if (!SolveSudokuGrid(solution))
        {
            return "错误：无法求解此数独谜题，可能不是有效的数独";
        }

        // 收集所有空格位置
        List<(int row, int col)> emptyCells = new List<(int row, int col)>();
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (grid[i, j] == 0)
                {
                    emptyCells.Add((i, j));
                }
            }
        }

        // 如果没有空格，返回错误
        if (emptyCells.Count == 0)
        {
            return "数独已完成，不需要提示";
        }

        // 限制提示数量
        hintCount = Math.Min(hintCount, emptyCells.Count);

        // 随机选择空格并填入答案
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("提示：");

        // 随机打乱空格列表
        Shuffle(emptyCells);

        for (int i = 0; i < hintCount; i++)
        {
            var (row, col) = emptyCells[i];
            int value = solution[row, col];
            sb.AppendLine($"位置 ({row + 1},{col + 1}) 应填入数字 {value}");

            // 更新原始谜题
            int index = row * SIZE + col;
            char[] puzzleChars = puzzle.ToCharArray();
            puzzleChars[index] = (char)('0' + value);
            puzzle = new string(puzzleChars);
        }

        sb.AppendLine("\n更新后的数独：");
        sb.Append(GetSudokuVisual(puzzle));

        return sb.ToString();
    }


    [McpServerTool]
    [Description("分析数独谜题的难度级别，基于空格数量评估为简单、中等、困难或专家级别")]
    public string EvaluateSudokuDifficulty(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string puzzle)
    {
        // 验证输入
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            return "错误：输入必须是81个字符的字符串，使用0表示空格";
        }

        // 转换为二维数组
        int[,] grid = new int[SIZE, SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * SIZE + j;
                if (index < puzzle.Length && char.IsDigit(puzzle[index]))
                {
                    grid[i, j] = puzzle[index] - '0';
                }
                else
                {
                    grid[i, j] = 0;
                }
            }
        }

        // 计算空格数量
        int emptyCount = 0;
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (grid[i, j] == 0)
                {
                    emptyCount++;
                }
            }
        }

        // 评估难度
        string difficulty;
        if (emptyCount < 35)
        {
            difficulty = "简单";
        }
        else if (emptyCount < 45)
        {
            difficulty = "中等";
        }
        else if (emptyCount < 50)
        {
            difficulty = "困难";
        }
        else
        {
            difficulty = "专家";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"数独难度评估：");
        sb.AppendLine($"空格数量：{emptyCount}");
        sb.AppendLine($"难度级别：{difficulty}");

        return sb.ToString();
    }

    #region 新增方法：CountSolutions 和改进的生成逻辑

    [McpServerTool]
    [Description("计算数独谜题的解的数量（最多计算 maxCount 个解，用于验证唯一解）")]
    public int CountSolutions(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string puzzle,
        [Description("最大计算解的数量，超过此数将停止计算")] int maxCount = 2)
    {
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            throw new ArgumentException("输入必须是81个字符的字符串，使用0表示空格");
        }

        int[,] grid = new int[SIZE, SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * SIZE + j;
                grid[i, j] = puzzle[index] - '0';
            }
        }

        int count = 0;
        CountSolutionsRecursive(grid, ref count, maxCount);
        return count;
    }

    [McpServerTool]
    [Description("检查数独谜题的解的唯一性，返回详细的解的信息")]
    public string CheckSolutionUniqueness(
        [Description("数独谜题，使用0表示空格，格式为81个数字的字符串，从左到右、从上到下排列")] string puzzle)
    {
        // 验证输入
        if (string.IsNullOrEmpty(puzzle) || puzzle.Length != 81)
        {
            return "错误：输入必须是81个字符的字符串，使用0表示空格";
        }

        // 计算解的数量
        int solutionCount = CountSolutions(puzzle, 3); // 最多检查3个解

        StringBuilder result = new StringBuilder();
        result.AppendLine("数独解的唯一性检查结果：");

        if (solutionCount == 0)
        {
            result.AppendLine("状态：无解");
            result.AppendLine("说明：此数独谜题没有有效解，可能存在冲突或不符合数独规则");
        }
        else if (solutionCount == 1)
        {
            result.AppendLine("状态：唯一解");
            result.AppendLine("说明：此数独谜题有且仅有一个解，是标准的数独谜题");

            // 提供解答
            string solution = SolveSudoku(puzzle);
            if (!solution.StartsWith("错误"))
            {
                result.AppendLine("\n解答：");
                result.Append(solution.Substring(solution.IndexOf('\n') + 1)); // 去掉第一行的状态信息
            }
        }
        else
        {
            result.AppendLine($"状态：多解（检测到至少{solutionCount}个解）");
            result.AppendLine("说明：此数独谜题有多个可能的解，不符合标准数独的唯一解要求");

            // 提供一个参考解
            string solution = SolveSudoku(puzzle);
            if (!solution.StartsWith("错误"))
            {
                result.AppendLine("\n参考解（其中一个可能的解）：");
                // 提取解答部分，去掉状态信息
                string[] lines = solution.Split('\n');
                if (lines.Length > 1)
                {
                    result.Append(lines[lines.Length - 1]); // 获取最后一行的解答
                }
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// 递归计算解的数量
    /// </summary>
    /// <param name="grid">当前网格状态</param>
    /// <param name="count">解的计数器（引用传递）</param>
    /// <param name="maxCount">最大计数，达到此数停止</param>
    private static void CountSolutionsRecursive(int[,] grid, ref int count, int maxCount)
    {
        if (count >= maxCount) return; // 达到最大计数，提前返回

        // 找到第一个空格
        int row = -1, col = -1;
        bool isEmpty = false;
        for (int i = 0; i < SIZE && !isEmpty; i++)
        {
            for (int j = 0; j < SIZE && !isEmpty; j++)
            {
                if (grid[i, j] == EMPTY)
                {
                    row = i;
                    col = j;
                    isEmpty = true;
                    break;
                }
            }
        }

        // 如果没有空格，说明找到一个解
        if (!isEmpty)
        {
            count++;
            return;
        }

        // 尝试填入1-9
        for (int num = 1; num <= SIZE; num++)
        {
            if (IsSafe(grid, row, col, num))
            {
                grid[row, col] = num;
                CountSolutionsRecursive(grid, ref count, maxCount);

                if (count >= maxCount) return; // 提前返回

                grid[row, col] = EMPTY; // 回溯
            }
        }
    }

    /// <summary>
    /// 改进的移除单元格方法，确保生成的谜题有唯一解
    /// </summary>
    /// <param name="grid">完整的解</param>
    /// <param name="count">目标要移除的单元格数量</param>
    private static void RemoveCellsToEnsureUniqueSolution(int[,] grid, int count)
    {
        List<(int, int)> cells = new List<(int, int)>();
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                cells.Add((i, j));
            }
        }
        Shuffle(cells);

        int removed = 0;
        foreach (var (row, col) in cells)
        {
            if (removed >= count) break;

            int backup = grid[row, col];
            grid[row, col] = EMPTY;

            // 创建一个副本用于验证唯一解，避免修改原grid
            int[,] tempGrid = new int[SIZE, SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    tempGrid[i, j] = grid[i, j];
                }
            }

            if (CountSolutionsRecursiveForValidation(tempGrid) != 1)
            {
                // 不唯一，恢复
                grid[row, col] = backup;
            }
            else
            {
                removed++;
            }
        }
    }

    /// <summary>
    /// 专门用于验证唯一性的递归解计数器（不传入maxCount，但内部限制为2）
    /// </summary>
    private static int CountSolutionsRecursiveForValidation(int[,] grid)
    {
        int count = 0;
        CountSolutionsRecursiveInternal(grid, ref count);
        return count;
    }

    private static void CountSolutionsRecursiveInternal(int[,] grid, ref int count)
    {
        if (count > 1) return; // 超过1个解，提前退出

        int row = -1, col = -1;
        bool isEmpty = false;
        for (int i = 0; i < SIZE && !isEmpty; i++)
        {
            for (int j = 0; j < SIZE && !isEmpty; j++)
            {
                if (grid[i, j] == EMPTY)
                {
                    row = i;
                    col = j;
                    isEmpty = true;
                    break;
                }
            }
        }

        if (!isEmpty)
        {
            count++;
            return;
        }

        for (int num = 1; num <= SIZE; num++)
        {
            if (IsSafe(grid, row, col, num))
            {
                grid[row, col] = num;
                CountSolutionsRecursiveInternal(grid, ref count);
                if (count > 1) return; // 提前退出
                grid[row, col] = EMPTY;
            }
        }
    }


    #endregion

    #region 辅助方法

    // 填充整个数独网格
    private static bool FillGrid(int[,] grid)
    {
        // 找到一个空格
        int row = -1, col = -1;
        bool isEmpty = true;

        for (int i = 0; i < SIZE && isEmpty; i++)
        {
            for (int j = 0; j < SIZE && isEmpty; j++)
            {
                if (grid[i, j] == EMPTY)
                {
                    row = i;
                    col = j;
                    isEmpty = false;
                }
            }
        }

        // 如果没有空格，数独已完成
        if (isEmpty)
        {
            return true;
        }

        // 获取1-9的随机排列
        List<int> numbers = Enumerable.Range(1, 9).ToList();
        Shuffle(numbers);

        // 尝试填入数字
        foreach (int num in numbers)
        {
            if (IsSafe(grid, row, col, num))
            {
                grid[row, col] = num;

                if (FillGrid(grid))
                {
                    return true;
                }

                grid[row, col] = EMPTY;
            }
        }

        return false;
    }

    // 检查在指定位置放置数字是否安全
    private static bool IsSafe(int[,] grid, int row, int col, int num)
    {
        // 检查行
        for (int i = 0; i < SIZE; i++)
        {
            if (grid[row, i] == num)
            {
                return false;
            }
        }

        // 检查列
        for (int i = 0; i < SIZE; i++)
        {
            if (grid[i, col] == num)
            {
                return false;
            }
        }

        // 检查3x3方格
        int startRow = row - row % 3;
        int startCol = col - col % 3;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[i + startRow, j + startCol] == num)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // 求解数独
    private static bool SolveSudokuGrid(int[,] grid)
    {
        // 找到一个空格
        int row = -1, col = -1;
        bool isEmpty = true;

        for (int i = 0; i < SIZE && isEmpty; i++)
        {
            for (int j = 0; j < SIZE && isEmpty; j++)
            {
                if (grid[i, j] == EMPTY)
                {
                    row = i;
                    col = j;
                    isEmpty = false;
                }
            }
        }

        // 如果没有空格，数独已完成
        if (isEmpty)
        {
            return true;
        }

        // 尝试填入数字1-9
        for (int num = 1; num <= SIZE; num++)
        {
            if (IsSafe(grid, row, col, num))
            {
                grid[row, col] = num;

                if (SolveSudokuGrid(grid))
                {
                    return true;
                }

                grid[row, col] = EMPTY;
            }
        }

        return false;
    }

    // 将数独网格转换为字符串
    private static string ConvertToString(int[,] grid)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                sb.Append(grid[i, j]);
            }
        }

        return sb.ToString();
    }

    // 随机打乱列表
    private static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    #endregion
}



