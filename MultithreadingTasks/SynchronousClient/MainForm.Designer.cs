namespace SynchronousClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sendButton = new System.Windows.Forms.Button();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.userIdTextBox = new System.Windows.Forms.TextBox();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.namedPipeRadioButton = new System.Windows.Forms.RadioButton();
            this.socketRadioButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(197, 33);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 0;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // messageTextBox
            // 
            this.messageTextBox.Location = new System.Drawing.Point(12, 35);
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.Size = new System.Drawing.Size(179, 20);
            this.messageTextBox.TabIndex = 1;
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(12, 88);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(260, 203);
            this.logTextBox.TabIndex = 2;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(116, 59);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 3;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // userIdTextBox
            // 
            this.userIdTextBox.Location = new System.Drawing.Point(12, 61);
            this.userIdTextBox.Name = "userIdTextBox";
            this.userIdTextBox.Size = new System.Drawing.Size(98, 20);
            this.userIdTextBox.TabIndex = 4;
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(197, 59);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectButton.TabIndex = 5;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // namedPipeRadioButton
            // 
            this.namedPipeRadioButton.AutoSize = true;
            this.namedPipeRadioButton.Checked = true;
            this.namedPipeRadioButton.Location = new System.Drawing.Point(12, 12);
            this.namedPipeRadioButton.Name = "namedPipeRadioButton";
            this.namedPipeRadioButton.Size = new System.Drawing.Size(82, 17);
            this.namedPipeRadioButton.TabIndex = 6;
            this.namedPipeRadioButton.TabStop = true;
            this.namedPipeRadioButton.Text = "Named pipe";
            this.namedPipeRadioButton.UseVisualStyleBackColor = true;
            // 
            // socketRadioButton
            // 
            this.socketRadioButton.AutoSize = true;
            this.socketRadioButton.Location = new System.Drawing.Point(100, 12);
            this.socketRadioButton.Name = "socketRadioButton";
            this.socketRadioButton.Size = new System.Drawing.Size(59, 17);
            this.socketRadioButton.TabIndex = 7;
            this.socketRadioButton.TabStop = true;
            this.socketRadioButton.Text = "Socket";
            this.socketRadioButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 303);
            this.Controls.Add(this.socketRadioButton);
            this.Controls.Add(this.namedPipeRadioButton);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.userIdTextBox);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.messageTextBox);
            this.Controls.Add(this.sendButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox messageTextBox;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox userIdTextBox;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.RadioButton namedPipeRadioButton;
        private System.Windows.Forms.RadioButton socketRadioButton;
    }
}

