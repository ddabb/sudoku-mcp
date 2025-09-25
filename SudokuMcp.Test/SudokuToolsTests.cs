using Xunit;
using System;
using System.IO;

namespace SudokuMcp.Test
{
    /// <summary>
    /// 数独工具测试类
    /// </summary>
    public class SudokuToolsTests
    {
        private readonly SudokuTools sudokuTools = new SudokuTools();

        /// <summary>
        /// 运行所有测试
        /// </summary>
        private void RunAllTests()
        {
            Console.WriteLine("开始运行数独工具测试...");

            TestGenerateSudoku();
            TestSolveSudoku();
            TestValidateSudoku();
            TestGetSudokuVisual();
            TestGetSudokuHint();
            TestEvaluateSudokuDifficulty();

            Console.WriteLine("所有测试完成！");
        }
        [Fact]
        /// <summary>
        /// 测试生成数独功能
        /// </summary>
        public void TestGenerateSudoku()
        {
            Console.WriteLine("测试生成数独功能...");

            // 测试不同难度级别
            for (int difficulty = 1; difficulty <= 5; difficulty++)
            {
                string puzzle = sudokuTools.GenerateSudoku(difficulty);
                if (puzzle == null || puzzle.Length != 81)
                {
                    throw new Exception($"生成的数独无效：难度级别 {difficulty}");
                }

                // 验证生成的数独是否有效
                bool isValid = sudokuTools.ValidateSudoku(puzzle);
                if (!isValid)
                {
                    throw new Exception($"生成的数独验证失败：难度级别 {difficulty}");
                }
            }

            Console.WriteLine("生成数独测试通过！");
        }
        [Fact]
        /// <summary>
        /// 测试求解数独功能
        /// </summary>
        public void TestSolveSudoku()
        {
            Console.WriteLine("测试求解数独功能...");

            // 创建一个有效的数独谜题
            string puzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
            string solution = sudokuTools.SolveSudoku(puzzle);

            if (solution == null || solution.Length != 81)
            {
                throw new Exception("求解数独失败");
            }
            Assert.NotNull(solution);
            Assert.Equal(81, solution?.Length);
            // 验证解决方案是否有效
            bool isValid = sudokuTools.ValidateSudoku(solution);
            if (!isValid)
            {
                throw new Exception("求解结果验证失败");
            }

            // 验证无解的数独
            string unsolvablePuzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080071";
            string unsolvableResult = sudokuTools.SolveSudoku(unsolvablePuzzle);
            if (!unsolvableResult.StartsWith("错误"))
            {
                throw new Exception("无解数独测试失败");
            }

            Console.WriteLine("求解数独测试通过！");
        }
        [Fact]
        /// <summary>
        /// 测试验证数独功能
        /// </summary>
        public void TestValidateSudoku()
        {
            Console.WriteLine("测试验证数独功能...");

            // 有效的数独
            string validPuzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
            bool isValid = sudokuTools.ValidateSudoku(validPuzzle);
            if (!isValid)
            {
                throw new Exception("有效数独验证失败");
            }

            // 无效的数独 - 行重复
            string invalidRowPuzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080070";
            invalidRowPuzzle = invalidRowPuzzle.Substring(0, 0) + "5" + invalidRowPuzzle.Substring(1);
            isValid = sudokuTools.ValidateSudoku(invalidRowPuzzle);
            if (isValid)
            {
                throw new Exception("行重复数独验证失败");
            }

            // 无效的数独 - 列重复
            string invalidColPuzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080070";
            invalidColPuzzle = invalidColPuzzle.Substring(0, 9) + "5" + invalidColPuzzle.Substring(10);
            isValid = sudokuTools.ValidateSudoku(invalidColPuzzle);
            if (isValid)
            {
                throw new Exception("列重复数独验证失败");
            }

            // 无效的数独 - 3x3方格重复
            string invalidBoxPuzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080070";
            invalidBoxPuzzle = invalidBoxPuzzle.Substring(0, 20) + "5" + invalidBoxPuzzle.Substring(21);
            isValid = sudokuTools.ValidateSudoku(invalidBoxPuzzle);
            if (isValid)
            {
                throw new Exception("3x3方格重复数独验证失败");
            }

            Console.WriteLine("验证数独测试通过！");
        }
        [Fact]
        /// <summary>
        /// 测试数独可视化功能
        /// </summary>
        public void TestGetSudokuVisual()
        {
            Console.WriteLine("测试数独可视化功能...");

            string puzzle = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
            string visual = sudokuTools.GetSudokuVisual(puzzle);

            if (visual == null || !visual.Contains("┌───────┬───────┬───────┐"))
            {
                throw new Exception("数独可视化失败");
            }

            Console.WriteLine("数独可视化测试通过！");
        }
        [Fact]
        /// <summary>
        /// 测试获取数独提示功能
        /// </summary>
        public void TestGetSudokuHint()
        {
            Console.WriteLine("测试获取数独提示功能...");

            // 生成一个数独谜题
            string puzzle = sudokuTools.GenerateSudoku(3);
            string hint = sudokuTools.GetSudokuHint(puzzle, 3);

            if (hint == null || !hint.Contains("提示："))
            {
                throw new Exception("获取数独提示失败");
            }

            // 测试已完成的数独
            string solution = sudokuTools.SolveSudoku(puzzle);
            string completedHint = sudokuTools.GetSudokuHint(solution, 1);

            if (!completedHint.Contains("数独已完成"))
            {
                throw new Exception("已完成数独提示测试失败");
            }

            Console.WriteLine("获取数独提示测试通过！");
        }
        [Fact]
        /// <summary>
        /// 测试评估数独难度功能
        /// </summary>
        public void TestEvaluateSudokuDifficulty()
        {
            Console.WriteLine("测试评估数独难度功能...");

            // 创建不同难度的数独
            string easyPuzzle = sudokuTools.GenerateSudoku(1);
            string mediumPuzzle = sudokuTools.GenerateSudoku(3);
            string hardPuzzle = sudokuTools.GenerateSudoku(5);

            string easyEvaluation = sudokuTools.EvaluateSudokuDifficulty(easyPuzzle);
            string mediumEvaluation = sudokuTools.EvaluateSudokuDifficulty(mediumPuzzle);
            string hardEvaluation = sudokuTools.EvaluateSudokuDifficulty(hardPuzzle);

            if (easyEvaluation == null || mediumEvaluation == null || hardEvaluation == null)
            {
                throw new Exception("评估数独难度失败");
            }

            Console.WriteLine("评估数独难度测试通过！");
        }
    }
}