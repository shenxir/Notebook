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
    public partial class formBack : Form
    {
        int documentCount = 1;                          //文档计数
        private ContextMenuStrip contextMenuStrip;
        private Control mdiClient;
        public FormRichText formRichText;
        public formBack()
        {
            InitializeComponent();
            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            contextMenuStrip = new ContextMenuStrip();

            // 添加复制菜单项
            ToolStripMenuItem copyMenuItem = new ToolStripMenuItem("复制");
            copyMenuItem.Click += (sender, e) => PerformEditAction(form => form.richTextBox1.Copy());
            contextMenuStrip.Items.Add(copyMenuItem);

            // 添加粘贴菜单项
            ToolStripMenuItem pasteMenuItem = new ToolStripMenuItem("粘贴");
            pasteMenuItem.Click += (sender, e) => PerformEditAction(form => form.richTextBox1.Paste());
            contextMenuStrip.Items.Add(pasteMenuItem);

            // 添加剪切菜单项
            ToolStripMenuItem cutMenuItem = new ToolStripMenuItem("剪切");
            cutMenuItem.Click += (sender, e) => PerformEditAction(form => form.richTextBox1.Cut());
            contextMenuStrip.Items.Add(cutMenuItem);

            // 添加撤销菜单项
            ToolStripMenuItem undoMenuItem = new ToolStripMenuItem("撤销");
            undoMenuItem.Click += (sender, e) => PerformEditAction(form => form.richTextBox1.Undo());
            contextMenuStrip.Items.Add(undoMenuItem);

            // 订阅鼠标按下事件
            this.MouseDown += FormBack_MouseDown;
        }

        private void FormBack_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip.Show(this, e.Location);
            }
        }
        private void MdiClient_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip.Show(mdiClient, e.Location);
            }
        }
        private void formBack_Load(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                if (control is MdiClient)
                {
                    mdiClient = control;
                    mdiClient.MouseDown += MdiClient_MouseDown;
                    break;
                }
            }
            // 设置菜单项快捷键
            设置菜单项快捷键(文件FToolStripMenuItem, Keys.Control | Keys.F);
            设置菜单项快捷键(编辑EToolStripMenuItem, Keys.Control | Keys.E);
            设置菜单项快捷键(窗口ToolStripMenuItem, Keys.Control | Keys.W);
            设置菜单项快捷键(保存ToolStripMenuItem, Keys.Control | Keys.S);
            设置菜单项快捷键(复制ToolStripMenuItem, Keys.Control | Keys.C);
            设置菜单项快捷键(剪切ToolStripMenuItem, Keys.Control | Keys.X);
            设置菜单项快捷键(粘贴ToolStripMenuItem, Keys.Control | Keys.V);
        
        }

        // 文件菜单点击事件（可留空，仅作为菜单分组）
        private void 文件FToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void 设置菜单项快捷键(ToolStripMenuItem menuItem, Keys shortcutKeys)
        {
            menuItem.ShortcutKeys = shortcutKeys;
            menuItem.ShowShortcutKeys = true; // 显示快捷键文本
        }
        // 新建文件
        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 创建新的子窗体（假设FormRichText为文本编辑子窗体）
            FormRichText newForm = new FormRichText();
            newForm.MdiParent = this;  // 设置为当前MDI窗体的子窗体
            newForm.Text = "新建文档";
            newForm.Show();
        }

        // 打开文件
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "文本文件|*.txt|RTF文件|*.rtf|所有文件|*.*";
                openDialog.Title = "打开文件";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FormRichText newForm = new FormRichText();
                        newForm.MdiParent = this;
                        newForm.Text = Path.GetFileName(openDialog.FileName);
                        newForm.Show();

                        // 根据文件类型加载内容
                        if (Path.GetExtension(openDialog.FileName).ToLower() == ".rtf")
                        {
                            newForm.richTextBox1.LoadFile(openDialog.FileName, RichTextBoxStreamType.RichText);
                        }
                        else
                        {
                            newForm.richTextBox1.LoadFile(openDialog.FileName, RichTextBoxStreamType.PlainText);
                        }

                        newForm.isSaved = true;
                        newForm.isChanged = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("打开文件失败：" + ex.Message, "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 保存文件
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormRichText activeForm = (FormRichText)this.ActiveMdiChild;
            if (activeForm == null)
            {
                MessageBox.Show("没有可保存的文档", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 如果文档未保存过，调用另存为
            if (!activeForm.isSaved)
            {
                SaveAsFile(activeForm);
            }
            else
            {
                // 已保存过，直接保存
                try
                {
                    activeForm.richTextBox1.SaveFile(activeForm.Text,
                        RichTextBoxStreamType.PlainText);
                    activeForm.isChanged = false;
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

        // 退出程序
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 检查所有子窗体是否需要保存
            foreach (Form childForm in this.MdiChildren)
            {
                FormRichText richForm = (FormRichText)childForm;
                if (richForm.isChanged)
                {
                    DialogResult result = MessageBox.Show("是否保存文档 " + richForm.Text + "？",
                        "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveAsFile(richForm);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return; // 取消退出
                    }
                }
            }

            this.Close(); // 关闭主窗体
        }

        // 另存为辅助方法
        private void SaveAsFile(FormRichText form)
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
                        form.richTextBox1.SaveFile(saveDialog.FileName,
                            Path.GetExtension(saveDialog.FileName).ToLower() == ".rtf" ?
                            RichTextBoxStreamType.RichText : RichTextBoxStreamType.PlainText);
                        form.Text = Path.GetFileName(saveDialog.FileName);
                        form.isSaved = true;
                        form.isChanged = false;
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

        // 窗口菜单点击事件（可留空）
        private void 窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        // 水平平铺子窗体
        private void 水平平铺ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        // 垂直平铺子窗体
        private void 垂直平铺ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        // 层叠子窗体
        private void 重叠ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        // 编辑菜单点击事件（可留空）
        private void 编辑EToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        // 撤销操作
        private void 取消ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Undo());
        }

        // 剪切文本
        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Cut());
        }

        // 复制文本
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Copy());
        }

        // 撤销操作（与取消ToolStripMenuItem功能重复，可合并）
        private void 撤销ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Undo());
        }

        // 粘贴文本
        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Paste());
        }

        // 按钮-复制（与菜单功能重复，调用同一逻辑）
        private void btnCopy_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Copy());
        }

        // 按钮-剪切
        private void btnCut_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Cut());
        }

        // 按钮-打开
        private void btnOpen_Click(object sender, EventArgs e)
        {
            打开ToolStripMenuItem_Click(sender, e); // 调用菜单的打开逻辑
        }

        // 按钮-粘贴
        private void btnPaste_Click(object sender, EventArgs e)
        {
            PerformEditAction(form => form.richTextBox1.Paste());
        }

        // 按钮-保存
        private void btnSave_Click(object sender, EventArgs e)
        {
            保存ToolStripMenuItem_Click(sender, e); // 调用菜单的保存逻辑
        }

        // 编辑操作辅助方法
        private void PerformEditAction(Action<FormRichText> action)
        {
            FormRichText activeForm = (FormRichText)this.ActiveMdiChild;
            if (activeForm == null)
            {
                MessageBox.Show("没有活动的编辑窗口", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                action(activeForm);
                activeForm.isChanged = true; // 操作后标记文档已修改
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}