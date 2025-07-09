using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 窗体和控件综合设计
{
    public partial class FormRichText : Form
    {
        public bool isChanged = false;
        public bool isSaved;
        private ContextMenuStrip childContextMenu;

        public FormRichText()
        {
            InitializeComponent();
            InitializeChildContextMenu();

            // 订阅FormClosing事件
            this.FormClosing += FormRichText_FormClosing;
        }

        // 初始化子窗体右键菜单（修改后的版本）
        private void InitializeChildContextMenu()
        {
            childContextMenu = new ContextMenuStrip();

            // 添加菜单项并设置快捷键
            AddContextMenuItem("复制", () => richTextBox1.Copy(), Keys.Control | Keys.C);
            AddContextMenuItem("粘贴", () => richTextBox1.Paste(), Keys.Control | Keys.V);
            AddContextMenuItem("剪切", () => richTextBox1.Cut(), Keys.Control | Keys.X);
            AddContextMenuItem("撤销", () => richTextBox1.Undo(), Keys.Control | Keys.Z);

            // 订阅富文本框的鼠标事件
            richTextBox1.MouseDown += RichTextBox1_MouseDown;
        }

        private void AddContextMenuItem(string text, Action action, Keys? shortcutKeys = null)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Click += (s, e) => action();

            // 设置快捷键（如果提供）
            if (shortcutKeys.HasValue)
            {
                item.ShortcutKeys = shortcutKeys.Value;
                item.ShowShortcutKeys = true; // 显示快捷键文本
            }

            childContextMenu.Items.Add(item);
        }

        // 富文本框右键事件
        private void RichTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                childContextMenu.Show(richTextBox1, e.Location);
            }
        }

        // 处理FormClosing事件
        private void FormRichText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isChanged)
            {
                DialogResult result = MessageBox.Show("是否保存文档 " + this.Text + "？",
                    "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFileIfNeeded();
                    if (isChanged) // 如果保存失败，阻止关闭
                    {
                        e.Cancel = true;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // 取消关闭操作
                }
            }
        }

        // 保存文件的方法
        private void SaveFileIfNeeded()
        {
            if (!isSaved)
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "文本文件|*.txt|RTF文件|*.rtf|所有文件|*.*";
                    saveDialog.Title = "另存为";
                    saveDialog.DefaultExt = "txt";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            richTextBox1.SaveFile(saveDialog.FileName,
                                Path.GetExtension(saveDialog.FileName).ToLower() == ".rtf" ?
                                RichTextBoxStreamType.RichText : RichTextBoxStreamType.PlainText);
                            this.Text = Path.GetFileName(saveDialog.FileName);
                            isSaved = true;
                            isChanged = false;
                            MessageBox.Show("保存成功", "提示",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("保存失败：" + ex.Message, "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    richTextBox1.SaveFile(this.Text, RichTextBoxStreamType.PlainText);
                    isChanged = false;
                    MessageBox.Show("保存成功", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败：" + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}