using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace Integrals
{
    public partial class MainForm : Form
    {

        public static CompilerResults compResult = null;
        double yMin = 0;
        double yMax = 0;
        public MainForm()
        {
            InitializeComponent();
        }



       enum Method 
        {
            蒙特卡洛算法,
            定义求积分法,
            变步长梯形求积分法
        };

        private void MainForm_Load(object sender, EventArgs e)
        {
            zedGraphControl1.GraphPane.XAxis.Title = "x";
            zedGraphControl1.GraphPane.YAxis.Title = "f(x)";
            zedGraphControl1.GraphPane.Title = null;

            string[] method = Enum.GetNames(typeof(Method));
            foreach (string str in method)
            {
                cbMethod.Items.Add(str);
            }
            cbMethod.SelectedIndex = 0;
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            txbResult.Text = "正在计算...";
            lbResult.Text = "正在计算...";
            string expression = txbExpreession.Text;
            double upLimit = 0;
            double lowLimit = 0;
            try
            {
                upLimit = Convert.ToDouble(txbUplimit.Text);
                lowLimit = Convert.ToDouble(txbLowlimit.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("积分区间输入错误！", "Error", MessageBoxButtons.OK);
             }
            Method m = (Method)cbMethod.SelectedIndex;

            compResult = getCompilerResult(expression);
            if (compResult == null)
                return;
            Draw(upLimit, lowLimit);

            double result = calcIntegrals(upLimit, lowLimit,m);
            lbResult.Text = result.ToString();

            string msg = "积分函数：f(x)="+expression+",\r\n"+"积分区间：["+lowLimit+","+upLimit+"],\r\n"+"结果:F(x)="+result+"。\r\n";
            txbResult.Text = msg;


        }
        #region 绘图部分
        private void Draw(double upLimit,double lowLimit)
        {
            zedGraphControl1.GraphPane.CurveList.Clear();

            double[] Xs = new double[1000];
            double[] Ys = new double[1000];
            Xs[0] = lowLimit;
            Ys[0] = f(lowLimit);
            double step = (upLimit - lowLimit) / 1000;
            for (int i = 1; i < 1000; i++)
            {
                Xs[i] = Xs[i - 1] + step;
                Ys[i] = f(Xs[i]);
                yMin = Ys[i] < yMin ? Ys[i] : yMin;
                yMax = Ys[i] > yMax ? Ys[i] : yMax;
            }

            zedGraphControl1.GraphPane.AddCurve("f(x)", Xs, Ys, Color.Blue, SymbolType.None);
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }
        #endregion

        #region 动态编译
        private CompilerResults getCompilerResult(string expression)
        {
            //动态编译函数表达式
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameter = new CompilerParameters();
            parameter.ReferencedAssemblies.Add("System.dll");
            parameter.GenerateExecutable = false;       //<--不生成exe
            parameter.GenerateInMemory = true;          //<--直接在内存运行
            CompilerResults result = provider.CompileAssemblyFromSource(parameter, GenerateCodeBlocks(expression));
            //动态编译
            if (result.Errors.Count > 0)
            {
                MessageBox.Show("函数表达式错误！", "Error", MessageBoxButtons.OK);
                return null;
            }
            return result;
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
        #endregion

        #region 积分求解
        private double f(double x)
        {
            if (compResult == null)
            {
                return 0;
            }
            else
                return Convert.ToDouble(compResult.CompiledAssembly.GetType("demo.calculation").GetMethod("dowork").Invoke(null, new object[] { x, 0 }));
        }

        private double calcIntegrals(double upLimit, double lowLimit, Method method)
        {
            double result = 0;
            switch (method)
            {
                case Method.蒙特卡洛算法:
                    result = methodMTK(upLimit, lowLimit);
                    break;
                case Method.定义求积分法:
                    result = methodDY(upLimit, lowLimit);
                    break;
                case Method.变步长梯形求积分法:
                    result = methodBBCTX(upLimit, lowLimit);
                    break;
                default:
                    break;
            }

            return result;
        }

        private double methodMTK( double upLimit, double lowLimit)
        {
            Random randx = new Random();
            Random randy = new Random();

            double result = 0;
            int N = 1000000;  
            int count = 0;
            for (int i = 0; i < N; i++)
            {
                double x = lowLimit + (upLimit - lowLimit) * randx.NextDouble();
                double y = yMin + (yMax - yMin) * randx.NextDouble(); ;
                if (f(x) > 0 && f(x) >= y && y >= 0)
                    count++;
                if (f(x) < 0 && f(x) <= y && y <= 0)
                    count--;
            }
            result = Math.Abs(((double)count / N) * ((yMax-yMin)*(upLimit-lowLimit)));
            return result;
            
            
        }

        private double methodDY(double upLimit, double lowLimit)
        {
            int N = 1000000;
            double step = (upLimit - lowLimit) / N;
            double result = 0;
            for (int i = 0; i < N; i++)
            {
                result += f(lowLimit+step*i) * step;
            }
            return result;
        }

        private double methodBBCTX(double upLimit, double lowLimit)
        { 
            double e = 0.0000001;
            double s0 = 0;
            double s = 05 * (f(upLimit) + f(lowLimit)) * (upLimit - lowLimit);
            for (int i = 2; Math.Abs(s - s0) > e; i *= 2)
            {
                double h = (upLimit - lowLimit) / i;
                double sum = 0;
                double a = lowLimit;
                for (int j = 0; j < i; j++)
                {
                    sum += 0.5 * (f(a) + f(a + h)) * h;
                    a = a + h;
                }

                s0 = s;
                s = sum;

            }
            return s0;
        }

        #endregion
    }
}
