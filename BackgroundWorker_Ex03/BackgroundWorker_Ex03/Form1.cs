﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Core;
using System.Threading;
using System.Globalization;

namespace BackgroundWorker_Ex03
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitializeMultiDelegate();
        }

        private void InitializeMultiDelegate()
        {
            multiBackgroundWorker1.DoWork += new Core.MultiDoWorkerEventHandler(multiBackgroundWorker1_DoWork);
            multiBackgroundWorker1.ProgressChanged += new Core.MultiProgressChangedEventHandler(multiBackgroundWorker1_ProgressChanged);
            multiBackgroundWorker1.RunWorkerCompleted += new Core.MultiRunWorkerCompletedEventHandler(multiBackgroundWorker1_RunWorkerCompleted);

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            int testNumber = rand.Next(50, 100);

            Guid guid = Guid.NewGuid();
            AddToListView(guid.ToString());

            this.multiBackgroundWorker1.RunWorkerAsync(guid.ToString(), testNumber);
        }

        private void AddToListView(string taskId)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = taskId.ToString();
            lvi.SubItems.Add("Not started");
            lvi.SubItems.Add("--");
            lvi.SubItems.Add("--");
            lvi.Tag = taskId;

            listView1.Items.Add(lvi);
        }

        void multiBackgroundWorker1_DoWork(object sender, Core.MultiDoWorkEventArgs e)
        {
            //e.ta
            string taskId = e.TaskID.ToString();
            e.Result = Compute(taskId, (MultiBackgroundWorker)sender, e);
        }

        private object Compute(string taskId, MultiBackgroundWorker multiBackgroundWorker, MultiDoWorkEventArgs e)
        {
            long result = 0;
            int n = 200;
            for (int i = 1; i <= n; i++)
            {
                //应该判断任务是否已被移除或取消
                if (multiBackgroundWorker.WhetherTaskCancelledOrNot(taskId))
                {
                    e.Cancel = true;
                    break;
                }
                result += i;
                Thread.Sleep(100);
                int progressPercentage = (int)((float)i / (float)n * 100);
                multiBackgroundWorker.ReportProgress(taskId, progressPercentage, Thread.CurrentThread.ManagedThreadId);
            }

            return result;
        }

        void multiBackgroundWorker1_ProgressChanged(object sender, Core.MultiProgressChangedEventArgs e)
        {
            UpdateListView((string)e.TaskID, e.ProgressPercentage, e.UserState);
        }

        private void UpdateListView(string taskId, int progressPercentage, object threadId)
        {
            listView1.BeginUpdate();
            foreach (ListViewItem lvi in listView1.Items)
            {
                if (lvi.Tag != null && lvi.Tag.ToString() == taskId)
                {
                    lvi.SubItems[1].Text = progressPercentage.ToString(CultureInfo.CurrentCulture.NumberFormat) + "%";
                    lvi.SubItems[2].Text = threadId.ToString();

                    break;
                }
            }
            listView1.EndUpdate();

        }


        void multiBackgroundWorker1_RunWorkerCompleted(object sender, Core.MultiRunWorkerCompletedEventArgs e)
        {
            //3步
            //1.判断是否出现错误
            //2.判断是否被取消
            //3.已成功.
            string taskId = e.TaskId.ToString();
            ListViewItem lvi = null;
            if (e.Error != null)
            {
                lvi = UpdateListView(taskId, "Error");
            }
            else if (e.Cancelled)
            {
                lvi = UpdateListView(taskId, "Cancelled");
            }
            else
            {
                lvi = UpdateListView(taskId, e.Result.ToString());
            }
            lvi.Tag = null;
        }

        private ListViewItem UpdateListView(string taskId, string result)
        {
            listView1.BeginUpdate();
            ListViewItem lvi = null;
            foreach (ListViewItem item in this.listView1.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == taskId)
                {
                    item.SubItems[3].Text = result;

                    lvi = item;
                    break;
                }
            }


            listView1.EndUpdate();

            return lvi;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (item != null && item.Tag != null)
                {
                    multiBackgroundWorker1.CancelAsync(item.Tag);
                }
                item.Selected = false;
            }
        }



    }
}
