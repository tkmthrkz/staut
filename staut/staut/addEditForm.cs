﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace staut
{
	public partial class addEditForm : Form
	{
		private int numberofAPBClicks;  //「プログラムを追加」ボタンが押された回数
		private TextBox[] pathTextBoxes; //パステキストボックスの集まり
		private const int PROG_NUM = 10; //同時に起動できるファイルの数

		public addEditForm()
		{
			InitializeComponent();
			numberofAPBClicks = 0;
			pathTextBoxes = new TextBox[PROG_NUM];
			pathTextBoxes[0] = pathTextBox1; //デフォルトであるパステキストボックスを格納
		}

		private void progRefeButton_Click(object sender, EventArgs e)
		{
			Button button = sender as Button;
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = Environment.CurrentDirectory;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				int tag = Convert.ToInt32(button.Tag);
				pathTextBoxes[tag - 1].Text = dialog.FileName;
			}
			else
			{
				Console.WriteLine("canceled progRefButton Dialog");
			}
			dialog.Dispose();
			return;
		}


		//「プログラムを追加」ボタン
		private void addProgButton_Click(object sender, EventArgs e)
		{
			Console.WriteLine("Clicked addProgramButton in addEditForm");
			numberofAPBClicks++; //クリック回数のカウント

			//パステキストボックスの作成上限数を超えた場合（プログラム追加上限数を超えた場合）
			if (numberofAPBClicks >= PROG_NUM) 
			{
				MessageBox.Show("これ以上プログラムを追加できません。","追加不可", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}


			//prognameLabel
			string PNLABEL_NAME = $"prognameLabel{numberofAPBClicks+1}";
			string PNLABEL_TEXT = $"{numberofAPBClicks+1}. プログラム名";
			const int PNLABEL_OFFSET = 100;
			int[] PNLABEL_LOCATE = {
				prognameLabel1.Location.X, //X座標
				PNLABEL_OFFSET * numberofAPBClicks + allProgPanel.AutoScrollPosition.Y //Y座標
			};

			//pathLabel
			string PATHLABEL_NAME = $"pathLabel{numberofAPBClicks+1}";
			const string PATHLABEL_TEXT = "パス";
			const int PATHLABEL_OFFSET = 100;
			int[] PATHLABEL_LOCATE = {
				pathLabel1.Location.X, //X座標
				PATHLABEL_OFFSET * numberofAPBClicks + allProgPanel.AutoScrollPosition.Y //Y座標
			};

			//prognameTextBox
			string PNBOX_NAME = $"prognameTextBox{numberofAPBClicks+1}";
			const int PNBOX_OFFSET = 100; //テキストボックス間の距離（Y）
			const int PNBOX_FIRST_OFFSET = 28; //Panelと最上部のテキストボックスとの距離（Y）
			int[] PNBOX_LOCATE = { 
				prognameTextBox1.Location.X, //X座標
				PNBOX_OFFSET * numberofAPBClicks + PNBOX_FIRST_OFFSET + allProgPanel.AutoScrollPosition.Y //Y座標
			};
			int[] PNBOX_SIZE = {
				prognameTextBox1.Size.Width, //テキストボックス幅
				prognameTextBox1.Size.Height //テキストボックス高さ
			};

			//pathTextBox
			string PATHBOX_NAME = $"pathTextBox{numberofAPBClicks+1}";
			const int PATHBOX_OFFSET = 100;
			const int PATHBOX_FIRST_OFFSET = 28; //Panelと最上部のテキストボックスとの距離（Y）
			int[] PATHBOX_LOCATE = {
				pathTextBox1.Location.X, //X座標
				PATHBOX_OFFSET * numberofAPBClicks + PATHBOX_FIRST_OFFSET + allProgPanel.AutoScrollPosition.Y //Y座標
			};
			int[] PATHBOX_SIZE = {
				pathTextBox1.Size.Width, //テキストボックス幅
				pathTextBox1.Size.Height //テキストボックス高さ
			};

			//progRefeButton
			string PRBUTTON_NAME = $"progRefeButton{numberofAPBClicks+1}";
			const string PRBUTTON_TEXT = "参照";
			int PRBUTTON_TAG = numberofAPBClicks + 1;
			const int PRBUTTON_OFFSET = 100;
			const int PRBUTTON_FIRST_OFFSET = 28;
			int[] PRBUTTON_LOCATE ={
				progRefeButton1.Location.X,
				PRBUTTON_OFFSET * numberofAPBClicks + PRBUTTON_FIRST_OFFSET + allProgPanel.AutoScrollPosition.Y
			};
			int[] PRBUTTON_SIZE ={
				progRefeButton1.Size.Width,
				progRefeButton1.Size.Height
			};

			createLabel(PNLABEL_NAME, PNLABEL_TEXT, PNLABEL_LOCATE);
			createLabel(PATHLABEL_NAME, PATHLABEL_TEXT, PATHLABEL_LOCATE);
			createTextBox(PNBOX_NAME, PNBOX_LOCATE, PNBOX_SIZE);
			TextBox pathTextBox = createTextBox(PATHBOX_NAME, PATHBOX_LOCATE, PATHBOX_SIZE);
			pathTextBoxes[numberofAPBClicks] = pathTextBox;
			createButton(PRBUTTON_NAME, PRBUTTON_TEXT, PRBUTTON_TAG, PRBUTTON_LOCATE, PRBUTTON_SIZE);
			return;
		}

		private void createLabel(string name, string text, int[] locate)
		{
			Label label = new Label();
			label.Name = name;
			label.Location = new Point(locate[0], locate[1]);
			label.Text = text;
			allProgPanel.Controls.Add(label);
			return;
		}

		private TextBox createTextBox(string name, int[] locate, int[] size)
		{
			TextBox textBox = new TextBox();
			textBox.Name = name;
			textBox.Location = new Point(locate[0], locate[1]);
			textBox.Size = new Size(size[0], size[1]);
			allProgPanel.Controls.Add(textBox);
			return textBox;
		}

		private void createButton(string name, string text, int tag, int[] locate, int[] size)
		{
			Button button = new Button();
			button.Name = name;
			button.Text = text;
			button.Tag = tag;
			button.Location = new Point(locate[0], locate[1]);
			button.Size = new Size(size[0], size[1]);
			button.Click += progRefeButton_Click;
			allProgPanel.Controls.Add(button);
			return;
		}

		//「決定」ボタン
		//データベース
		private void decideButton_Click(object sender, EventArgs e)
		{
			//string DATADIR_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create) + "\\staut"; //本アプリが作成したデータを保存するディレクトリ
			//string DATAFILE_PATH = DATADIR_PATH + "\\data.csv"; //データを記載するファイルのパス
			//string ENCODE = "shift_jis";

			//Console.WriteLine("DATADIR_PATH = " + DATADIR_PATH);
			//Console.WriteLine("DATAFILE_PATH = " + DATAFILE_PATH);

			//Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			//Encoding enc = Encoding.GetEncoding(ENCODE);

			//Directory.CreateDirectory(DATADIR_PATH);
			//StreamWriter writer = new StreamWriter(DATAFILE_PATH, true, enc); //追記で書き込む

			//writer.WriteLine("#" + settitleTextBox.Text);
			//for(int i=0; i < PROG_NUM; i++)
			//{
			//	try
			//	{
			//		writer.Write(pathTextBoxes[i].Text + ",");
			//	}
			//	catch(NullReferenceException ne) //起動できる上限数より少ないファイルを設定した場合
			//	{
			//		break;
			//	}
			//	finally
			//	{
			//		writer.Write("\n");
			//	}
			//}
			//writer.Close();
			//return;
		}
	}
}
