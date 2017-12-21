using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
/// <summary>
/// 技术:
///     1.动态多语言
///     2.异步cmd回显
/// </summary>
namespace ToolkitProject
{
    // 1.定义委托  
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] myParmArray = {
            @"/A",            @"/M",            @"/P",
            @"/S",            @"/E",            @"/B",
            @"/W",            @"/C",            @"/I",
            @"/Q",            @"/F",            @"/L",
            @"/G",            @"/H",            @"/R",
            @"/T",            @"/U",            @"/K",
            @"/N",            @"/O",            @"/X",
            @"/Y",            @"/-Y",           @"/Z",
            @"/B",            @"/J",            @"/D"
        };
        // 2.定义委托事件  
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Init();
            ShowChkboxArray();
        }
        private void Init()
        {
            //3.将相应函数注册到委托事件中  
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
            ReadErrOutput += new DelReadErrOutput(ReadErrOutputAction);
        }
        private static int chknum = 27;
        CheckBox[] chks = new CheckBox[chknum];

        private void ShowChkboxArray()
        {
            for (int i = 0; i < chknum; i++)
            {
                chks[i] = new CheckBox()
                {
                    Margin = new Thickness(10 + 310 * (i % 3), 10 + 18 * (i / 3), 0, 0),
                    Name = "chk" + i.ToString()
                };
                chks[i].SetResourceReference(ContentControl.ContentProperty, "cmd" + i.ToString());
                chks[i].Checked += new System.Windows.RoutedEventHandler(Btchk_Click); //统一的事件处理
                ListGrid1.Children.Add(chks[i]); //在窗体上呈现控件
            }
        }

        private void Btchk_Click(object sender, System.EventArgs e)
        {
            //MessageBox.Show(((CheckBox)sender).Content + " was clicked !"); //通过sender判断激发事件的控件
            //int i = int.Parse(((CheckBox)sender).Name.Substring(3));
           
        }

        private void CtlBt_PATH1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openDialog1 = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "source",
                SelectedPath = "c:\\"
            };
            if (openDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CtlTb_PATH1.Text = openDialog1.SelectedPath; 
            }
        }

        private void CtlBt_PATH2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openDialog2 = new System.Windows.Forms.FolderBrowserDialog() {
                Description = "distinct",
                SelectedPath = "c:\\"
            };
            if (openDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                string str = CtlTb_PATH1.Text.Substring(CtlTb_PATH1.Text.LastIndexOf("\\")).Trim();
                CtlTb_PATH2.Text = openDialog2.SelectedPath + str+"\\";
            }
        }

        private string langName = "en_US";
        private void BTLangCN_Click(object sender, RoutedEventArgs e)
        {
            langName = "cn";
            BtnChangeLang_Click(langName);
        }

        private void BTLangEN_Click(object sender, RoutedEventArgs e)
        {
            langName = "en";
            BtnChangeLang_Click(langName);
        }

        private void BTLangDE_Click(object sender, RoutedEventArgs e)
        {
            langName = "de";
            BtnChangeLang_Click(langName);
        }

        private void BTLangKR_Click(object sender, RoutedEventArgs e)
        {
            langName = "kr";
            BtnChangeLang_Click(langName);
        }

        private void BTLangFR_Click(object sender, RoutedEventArgs e)
        {
            langName = "fr";
            BtnChangeLang_Click(langName);
        }

        private void BTLangJP_Click(object sender, RoutedEventArgs e)
        {
            langName = "jp";
            BtnChangeLang_Click(langName);
        }
        private void BtnChangeLang_Click(string strlang)
        {
            ResourceDictionary langRd = null;
            try
            {
                //根据名字载入语言文件
                langRd = Application.LoadComponent(new Uri(@"lang\" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }            
            if (langRd != null)
            {
                //如果已使用其他语言,先清空
                if (this.Resources.MergedDictionaries.Count > 0)
                {
                    this.Resources.MergedDictionaries.Clear();
                }
                this.Resources.MergedDictionaries.Add(langRd);
            }
        }

        //To start
        Process CmdProcess = new Process();
        private void BTStart_Click(object sender, RoutedEventArgs e)
        {
           string str1 = this.FindResource("start").ToString();            
            if(BTStart.Content.ToString().Trim()== str1)
            {
                BTStart.SetResourceReference(ContentControl.ContentProperty, "stop");
                if (CmdProcess == null)
                {
                    CmdProcess = new Process();
                }

                string str = "xcopy " + CtlTb_PATH1.Text + @"  " + CtlTb_PATH2.Text +@"  " + GetParam();
                RealAction(@"cmd.exe", " /k "+ str);
            }
            else
            {
                BTStart.SetResourceReference(ContentControl.ContentProperty, "start");
                CmdProcess.Kill();
                CmdProcess = null;
            }
        }
        //获得点击参数
        private string GetParam()
        {
            string str = "";
            if (CtlRadBt_bt1.IsChecked == true)
            {
                for (int i = 0; i < chknum; i++)
                {
                    if (chks[i].IsChecked == true)
                    {
                        str = str + myParmArray[i];
                    }
                }
                if (string.IsNullOrEmpty(str))
                    str =  @"/E/S/Y/H/L";                
            }
            else if (CtlRadBt_bt2.IsChecked == true)
                str = @"/E/S/Y/H/L";
            else if (CtlRadBt_bt3.IsChecked == true)
                str =  @"/E/S/Y";
            return str;
        }
        /// <summary>
        /// 进程重定向
        /// </summary>
        /// <param name="StartFileName"></param>
        /// <param name="StartFileArg"></param>
        private void RealAction(string StartFileName, string StartFileArg)
        {
            CmdProcess.StartInfo.FileName = StartFileName;      // 命令  
            CmdProcess.StartInfo.Arguments = StartFileArg;      // 参数  

            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口  
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入  
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出  
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
                                                                //CmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;  

            CmdProcess.OutputDataReceived += new DataReceivedEventHandler(P_OutputDataReceived);
            CmdProcess.ErrorDataReceived += new DataReceivedEventHandler(P_ErrorDataReceived);

            CmdProcess.EnableRaisingEvents = true;                      // 启用Exited事件  
            CmdProcess.Exited += new EventHandler(CmdProcess_Exited);   // 注册进程结束事件  

            CmdProcess.Start();
            CmdProcess.BeginOutputReadLine();
            CmdProcess.BeginErrorReadLine();            
        }
        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // 4. 异步调用，需要invoke  
                this.Dispatcher.Invoke(ReadStdOutput, new object[] { e.Data });
            }
        }

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.Dispatcher.Invoke(ReadErrOutput, new object[] { e.Data });
            }
        }
        private int listCount = 8;
        private void ReadStdOutputAction(string result)
        {            
            CmdListBox.Items.Insert(0, result);
            if(CmdListBox.Items.Count> listCount)
                CmdListBox.Items.RemoveAt(listCount);
        }

        private void ReadErrOutputAction(string result)
        {         
            CmdListBox.Items.Insert(0, result);
            if (CmdListBox.Items.Count > listCount)
                CmdListBox.Items.RemoveAt(listCount);
        }
        
        private void CmdProcess_Exited(object sender, EventArgs e)
        {
            // 执行结束后触发  
            string str1 = this.FindResource("workfinish").ToString();
            MessageBox.Show(str1); 
        }
    }
}
