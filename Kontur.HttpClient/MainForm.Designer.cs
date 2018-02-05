namespace Kontur.HttpClient
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblUrl = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.lblCode = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.pctResult = new System.Windows.Forms.PictureBox();
            this.btnSendRequest = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pctResult)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUrl
            // 
            this.lblUrl.AutoSize = true;
            this.lblUrl.Location = new System.Drawing.Point(12, 16);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(102, 13);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "Введите URL-путь:";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(114, 13);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(235, 20);
            this.txtUrl.TabIndex = 1;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(15, 41);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(93, 37);
            this.btnLoadImage.TabIndex = 2;
            this.btnLoadImage.Text = "Загрузить изображение";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.lblCode.Location = new System.Drawing.Point(12, 81);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(32, 13);
            this.lblCode.TabIndex = 4;
            this.lblCode.Text = "Код: ";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(12, 94);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(62, 13);
            this.lblResult.TabIndex = 5;
            this.lblResult.Text = "Результат:";
            // 
            // pctResult
            // 
            this.pctResult.Location = new System.Drawing.Point(13, 111);
            this.pctResult.Name = "pctResult";
            this.pctResult.Size = new System.Drawing.Size(542, 295);
            this.pctResult.TabIndex = 6;
            this.pctResult.TabStop = false;
            // 
            // btnSendRequest
            // 
            this.btnSendRequest.Location = new System.Drawing.Point(114, 41);
            this.btnSendRequest.Name = "btnSendRequest";
            this.btnSendRequest.Size = new System.Drawing.Size(90, 37);
            this.btnSendRequest.TabIndex = 7;
            this.btnSendRequest.Text = "Отправить запрос";
            this.btnSendRequest.UseVisualStyleBackColor = true;
            this.btnSendRequest.Click += new System.EventHandler(this.btnSendRequest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 418);
            this.Controls.Add(this.btnSendRequest);
            this.Controls.Add(this.pctResult);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblCode);
            this.Controls.Add(this.btnLoadImage);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.lblUrl);
            this.Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)(this.pctResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.PictureBox pctResult;
        private System.Windows.Forms.Button btnSendRequest;
    }
}

