namespace THS_Scraper
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            panel1 = new Panel();
            btn_logout = new Button();
            btn_login = new Button();
            textBox1 = new TextBox();
            chkbx_capture = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.BackColor = Color.FromArgb(19, 19, 20);
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.ForeColor = SystemColors.Control;
            webView21.Location = new Point(0, 0);
            webView21.Name = "webView21";
            webView21.Size = new Size(982, 455);
            webView21.TabIndex = 0;
            webView21.ZoomFactor = 1D;
            // 
            // panel1
            // 
            panel1.Controls.Add(chkbx_capture);
            panel1.Controls.Add(btn_logout);
            panel1.Controls.Add(btn_login);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 751);
            panel1.Name = "panel1";
            panel1.Size = new Size(982, 48);
            panel1.TabIndex = 2;
            // 
            // btn_logout
            // 
            btn_logout.Location = new Point(118, 6);
            btn_logout.Name = "btn_logout";
            btn_logout.Size = new Size(100, 23);
            btn_logout.TabIndex = 0;
            btn_logout.Text = "Try Logout";
            btn_logout.UseVisualStyleBackColor = true;
            btn_logout.Click += btn_logout_Click;
            // 
            // btn_login
            // 
            btn_login.Location = new Point(12, 6);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(100, 23);
            btn_login.TabIndex = 0;
            btn_login.Text = "Try Login";
            btn_login.UseVisualStyleBackColor = true;
            btn_login.Click += btn_login_Click;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.FromArgb(19, 19, 20);
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Dock = DockStyle.Bottom;
            textBox1.ForeColor = SystemColors.Window;
            textBox1.Location = new Point(0, 455);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(982, 296);
            textBox1.TabIndex = 3;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // chkbx_capture
            // 
            chkbx_capture.AutoSize = true;
            chkbx_capture.ForeColor = SystemColors.ButtonFace;
            chkbx_capture.Location = new Point(224, 10);
            chkbx_capture.Name = "chkbx_capture";
            chkbx_capture.Size = new Size(113, 19);
            chkbx_capture.TabIndex = 2;
            chkbx_capture.Text = "Capture Enabled";
            chkbx_capture.UseVisualStyleBackColor = true;
            chkbx_capture.CheckedChanged += chkbx_capture_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(19, 19, 20);
            ClientSize = new Size(982, 799);
            Controls.Add(webView21);
            Controls.Add(textBox1);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private Panel panel1;
        private Button btn_login;
        private Button btn_logout;
        private TextBox textBox1;
        private CheckBox chkbx_capture;
    }
}
