using System;

namespace SudokuMcp.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("开始运行数独工具测试程序...");

                SudokuToolsTests tests = new SudokuToolsTests();
                tests.RunAllTests();

                Console.WriteLine("所有测试成功完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败：{ex.Message}");
                Console.WriteLine($"异常详情：{ex}");
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}