using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Integrals
{
    class StrToExpression
    {
        public double Calculate(string formula, double x,double y)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameter = new CompilerParameters();
            parameter.ReferencedAssemblies.Add("System.dll");
            parameter.GenerateExecutable = false;       //<--不生成exe
            parameter.GenerateInMemory = true;          //<--直接在内存运行
            CompilerResults result = provider.CompileAssemblyFromSource(parameter,GenerateCodeBlocks(formula));
            //动态编译
            if (result.Errors.Count > 0)
            {
                MessageBox.Show("error");
            }
            //编译成功
            double calculated = Convert.ToDouble(result.CompiledAssembly.GetType("demo.calculation").GetMethod("dowork").Invoke(null, new object[] { x, y }));
            //这里通过反射调
            return calculated;
        }

        string GenerateCodeBlocks(string formula)
        {
            string code =
                "using System;" +
                "namespace demo" +
                "{" +
                   "public static class calculation" +
                   "{" +
                   "public static double dowork(double x, double y)" +
                   "{ return " + formula +
                   ";}}}"; //这里是将你的formula和代码片段拼接成完整的程序准备编译的过程。
            return code;
        }
    }
}
