using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSS.Business.MainModule;
using OSS.Data.Models;
using OSS.Data.Util;
using OSS.Win.Common;

namespace OSS.Win
{
    public partial class Form1 : Form
    {
        private readonly T_ProductApkService _productApkService;

        static int _sleepQuery; //查询间隔指程序每一次处理完一定数据（每查询数）后，休眠多少毫秒再去查询本地数据库，数据变化不多时不要设得太小，设置范围最小1000（1000毫秒=1秒）
        static bool _inSettingConfig = false; //导入配置信息标志 - 导入过就不必再次导入

        static int _remindQueryCount;//每一次从数据库查询的未更新服务的数量
        static int _remainStandbyCount;//提交服务列表小于剩余多少数量时，需要再次查询数据库取得新增队列记录
        static int _reminTaskNum;//提醒服务工作线程数量

        private static bool _noticeFlag = false;//提醒服务运行标志
        private static DateTime? _noticeDateTime = null;//提醒服务时间
        static ProducerConsumer<string> _producerConsumer = null;
        static object _noticeLock = new object();
        const int _getQueryRetryTimes = 9;//获取数据库队列失败连续重试次数
        static int _sleepPost;//服务提交间隔指每次向服务器提交后再次提交的等待时间，设置范围必须大于等于100毫秒, 可以根据服务器情况快慢调整


        public Form1()
        {
            InitializeComponent();
            _productApkService = new T_ProductApkService();
        }

        public void Form1_load()
        {

        }

        #region 写日志
        private void AddMsg(string msg)
        {
            richtxt_Info.AppendText(msg + "\n");
        }

        private void Logging(string msg, string stackTrace = "")
        {
            new Task(() =>
            {
                msg = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg;
                Invoke(new Action(() => AddMsg(msg)));
                LogFileHelper.InsertMsg(msg + (string.IsNullOrWhiteSpace(stackTrace) ? "" : stackTrace));
            }).Start();
        }
        #endregion

        #region 导入配置信息
        private void AppConfigInput()
        {
            if (!_inSettingConfig)
            {
                try
                {
                    _sleepQuery = int.Parse(ConfigurationManager.AppSettings["SleepQuery"].ToString());
                    _sleepPost = int.Parse(ConfigurationManager.AppSettings["SleepPost"].ToString());

                    _remainStandbyCount = int.Parse(ConfigurationManager.AppSettings["RemindRemainStandbyCount"]);
                    _remindQueryCount = int.Parse(ConfigurationManager.AppSettings["RemindQueryCount"]);
                    _reminTaskNum = int.Parse(ConfigurationManager.AppSettings["RemindTaskNum"]);

                    _inSettingConfig = true;
                    Logging("导入配置信息成功!");
                }
                catch
                {
                    Logging("导入配置信息出错!");
                    throw new InvalidOperationException("导入配置信息出错!");
                }
            }
        }
        #endregion

        #region 格式化状态信息、按钮状态
        private void SetlblState(Label lblName, string text, Color color)
        {
            lblName.Text = text;
            lblName.ForeColor = color;
        }

        private void SetbtnState(Button btnName, string text, bool enable)
        {
            btnName.Text = text;
            btnName.Enabled = enable;
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                AppConfigInput();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(@"启动失败：" + ex.Message, @"错误");
                this.Close();
            }
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (!_noticeFlag)
            {
                _noticeFlag = true;
                _noticeDateTime = DateTime.Now;
                Logging("启动[OSS更新]服务");
                SetbtnState(btn_Start, "停止[OSS更新]服务", true);
                SetlblState(lbl_text, "[OSS更新]扫描服务运行中(" + _noticeDateTime + ")", Color.Green);

                //重置提醒扫描状态：正在扫描--> 等到扫描
                var resetStandbyTask = new Task<bool>(() =>
                {
                    var isReset = false;
                    try
                    {
                        isReset = ResetAllRemindScan();
                    }
                    catch
                    {
                    }
                    return isReset;
                });
                resetStandbyTask.Start();
                resetStandbyTask.Wait();
                var isSuccess = resetStandbyTask.Result;
                if (isSuccess)
                {
                    Logging("重置\"扫描中\"的[OSS更新]扫描服务为等待扫描成功！");
                }
                else
                {
                    Logging("[OSS更新]扫描服务启动失败：重置\"扫描中\"的[OSS更新]扫描服务为等待扫描失败！");
                    _noticeFlag = false;
                    SetbtnState(btn_Start, "启动[OSS更新]扫描服务", true);
                    SetlblState(lbl_text, "[OSS更新]扫描服务停止", Color.Red);
                    return;
                }

                //初始化相关变量、工作线程
                //等到重置“发送中”任务结束后，才能执行下面任务：
                //获取[未提醒]列表数据,为[发起提醒]生产者提供生产方法
                var getRetryTimes = 0;
                Func<List<string>> ProduceFunc = () =>
                {
                    //休眠获取[提醒]列表数据，主要为了间断查询。
                    Thread.Sleep(_sleepQuery);
                    var unList = new List<string>();
                    try
                    {
                        unList = GetTaskList();
                        if (unList.Count > 0)
                        {
                            Logging("获取[OSS更新]队列成功！");
                        }
                        getRetryTimes = 0;
                    }
                    catch (Exception ex)
                    {
                        getRetryTimes = getRetryTimes + 1;
                        if (getRetryTimes >= _getQueryRetryTimes)
                        {
                            _noticeFlag = false;
                            Logging("[OSS更新]服务意外停止：获取[未更新]队列连续失败" + getRetryTimes + "次！|| 错误原因：" +(ex.InnerException == null ? ex.Message : ex.InnerException.ToString()),(string.IsNullOrWhiteSpace(ex.StackTrace) ? "" : ex.StackTrace));
                            ChangeAllServerState();
                            if (_producerConsumer != null)
                            {
                                _producerConsumer.Stop();
                            }
                        }
                        else
                        {
                            Logging("获取[未更新]队列连续失败" + getRetryTimes + "次！|| 错误原因：" +(ex.InnerException == null ? ex.Message : ex.InnerException.ToString()),(string.IsNullOrWhiteSpace(ex.StackTrace) ? "" : ex.StackTrace));
                        }
                    }
                    return unList;
                };

                int consumeRetryTimes = 0;
                //为[未提醒]消费者提供消费方法
                Action<string> notNoticeAction = item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        long durTime = -1;
                        var durTimeUnit = "毫秒";
                        var durTimer = new Stopwatch();
                        durTimer.Start();
                        try
                        {
                            //执行活动提交服务
                            var result = PostRemind(item);
                            durTimer.Stop();
                            durTime = durTimer.ElapsedMilliseconds;
                            if (durTime >= 1000)
                            {
                                durTimeUnit = "秒";
                                durTime = durTime / 1000;
                            }
                            lock (_noticeLock)
                            {
                                consumeRetryTimes = 0;
                            }
                            Logging("[未更新]提交成功." + "(数据ID:" + item + "，运行结果:" + (result ? "提醒成功" : "提醒失败") + "，耗时:" +durTime.ToString() + durTimeUnit + ")");
                        }
                        catch (Exception ex)
                        {
                            lock (_noticeLock)
                            {
                                consumeRetryTimes++;
                                if (consumeRetryTimes >= _getQueryRetryTimes)
                                {
                                    _noticeFlag = false;
                                    Logging("[未更新]服务意外停止:提交提醒服务连续失败" + consumeRetryTimes + "次![其中包含ID：" + item +"]  || 错误原因：" +(ex.InnerException == null ? ex.Message : ex.InnerException.ToString()),(string.IsNullOrWhiteSpace(ex.StackTrace) ? "" : ex.StackTrace));
                                    ChangeAllServerState();
                                    if (_producerConsumer != null)
                                    {
                                        _producerConsumer.Stop();
                                    }
                                }
                                else
                                {
                                    Logging("[未更新]提交提醒服务连续失败" + consumeRetryTimes + "次![其中包含商家ID:" + item +"] || 错误原因：" +(ex.InnerException == null ? ex.Message : ex.InnerException.ToString()),(string.IsNullOrWhiteSpace(ex.StackTrace) ? "" : ex.StackTrace));
                                }
                            }
                        }
                        Thread.Sleep(_sleepPost);
                    }
                };
                //构造生产消费模式，初始化如生产方法，消费方法，指定消费者数量
                _producerConsumer = new ProducerConsumer<string>(ProduceFunc,notNoticeAction, _remainStandbyCount,_reminTaskNum);
                _producerConsumer.Start();
            }
            else //停止
            {
                _noticeFlag = false;
                if (_producerConsumer != null)
                {
                    _producerConsumer.Stop();
                }
                Logging("手动停止[未更新]扫描服务");

                SetbtnState(btn_Start, "启动[OSS更新]扫描服务", true);
                SetlblState(lbl_text, "[OSS更新]扫描服务停止", Color.Red);
            }
        }

        //重置提醒扫描状态：正在扫描--> 等到扫描
        private bool ResetAllRemindScan()
        {
            try
            {
                return _productApkService.ResetNotScanningToWait();
            }
            catch
            {
                throw new InvalidOperationException("重置[OSS更新]队列扫描状态为“等待扫描”失败！");
            }
        }

        //取得远程队列
        private List<string> GetTaskList()
        {
            try
            {
                return _productApkService.GetNotNoticeList(_remindQueryCount);
            }
            catch
            {
                throw new InvalidOperationException("获取[OSS更新]队列连续失败");
            }
        }

        private void ChangeAllServerState()
        {
            new Task(() => this.Invoke(new Action(() =>
            {
                SetbtnState(btn_Start, "启动[OSS更新]扫描服务", true);
                SetlblState(lbl_text, "[OSS更新]扫描服务停止", Color.Red);
            }))).Start();
        }

        //提交提醒执行
        private bool PostRemind(string corpId)
        {
            try
            {
                _productApkService.PostNotNotice(corpId);
                return true;
            }
            catch (Exception)
            {
                throw new Exception("提交[OSS更新]服务失败！");
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;
            switch (m.Msg)
            {
                case (WM_SYSCOMMAND):
                    if ((int)m.WParam == SC_MINIMIZE)
                    {
                        this.Hide();
                        notifyIcon1.Visible = true;
                        notifyIcon1.ShowBalloonTip(1000);
                        this.ShowInTaskbar = false;
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// 窗体关闭时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_noticeFlag) //需要判断所有服务运行标志
            {
                MessageBox.Show(@"请先停止所有服务再退出！", @"警告");
                e.Cancel = true;
            }
            else
            {
                this.notifyIcon1.Visible = false;
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 双击状态栏图标显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!this.Visible)
            {
                this.Show();
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }
    }
}
