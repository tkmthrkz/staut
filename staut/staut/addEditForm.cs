﻿//入力データのバリデーションチェック
//バリデーションエラー時、決定ボタンの無効化
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

using staut_ClassLibrary;

namespace staut
{
	public partial class addEditForm : Form
	{
		private int setTitleId = 0; //新規作成または編集している当該セットのID、新規作成の場合は0
		private int numberofProgram = 0;  //当該セットに追加されている起動プログラム数
		private const int PROG_NUM = 10; //同時に起動できるファイルの数
		private TextBox[] prognameTextBoxes = new TextBox[PROG_NUM]; //プログラム名入力テキストボックスの集まり
		private TextBox[] pathTextBoxes = new TextBox[PROG_NUM]; //パス入力テキストボックスの集まり
		private bool isAdd = true;
		private bool isValid = true; //すべての入力データに対しバリデーションチェック通過済みか

		//セットを新規作成する場合のインスタンス処理
		public addEditForm() 
		{
			InitializeComponent();
			CreateComponentCluster();
			isAdd = true;
			isValid = true;
			setTitleId = 0;
		}

		//既存セットを編集するときのインスタンス処理
		public addEditForm (IQueryable<SetTitle> setTitles, IQueryable<StartupProg> startupProgs) 
		{
			InitializeComponent();
			Console.WriteLine($"setTitle_numData = {setTitles.Count()}");
			Console.WriteLine($"startupProgs_numData = {startupProgs.Count()}");
			foreach(var setTitle in setTitles) //setTitlesの中身は１つのみ
			{
				setTitleId = setTitle.SetTitleId;
				settitleTextBox.Text = setTitle.TitleName;
			}
			foreach(var startupProg in startupProgs)
			{
				string program_name = startupProg.StartupProgName;
				string program_path = startupProg.StartupProgPath;
				CreateComponentCluster(program_name, program_path);
			}

			//削除ボタン作成
			string DELETEBUTTON_NAME = $"deleteButton";
			const string DELETEBUTTON_TEXT = "削除";
			int DELETEBUTTON_TAG = -1;
			int[] DELETEBUTTON_LOCATE ={
				31,
				404
			};
			int[] DELETEBUTTON_SIZE ={
				112,
				34
			};

			//削除ボタン作成
			Button deleteButton = CreateTools.createButton(DELETEBUTTON_NAME, DELETEBUTTON_TEXT, DELETEBUTTON_TAG, DELETEBUTTON_LOCATE, DELETEBUTTON_SIZE, this);
			deleteButton.Click += deleteButton_Click;
			deleteButton.DialogResult = DialogResult.OK;

			isAdd = false;
			isValid = true;
		}

		private void addEditForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Console.WriteLine($"CloseReason: {e.CloseReason}");
			//バリエーションエラー状態で決定ボタンがクリックされたとき
			if (!isValid && (e.CloseReason == CloseReason.None))
			{
				MessageBox.Show("不適切なデータが含まれています。（赤欄）", "入力値エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.Cancel = true;
			}
		}

		private void deleteButton_Click(object sender, EventArgs e)
		{
			using(var db = new SetTitleDbContext())
			{
				var datas = from setTitle in db.SetTitles
							where setTitle.SetTitleId == setTitleId
							select setTitle;
				var data = datas.First();
				db.Remove(data);
				db.SaveChanges();
			}
		}

		//「参照ボタン」
		private void progRefeButton_Click(object sender, EventArgs e)
		{
			Button button = sender as Button;
			OpenFileDialog dialog = new OpenFileDialog();
			//ユーザのプロファイルディレクトを取得
			string initialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			dialog.InitialDirectory = initialDirectory;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				int tag = Convert.ToInt32(button.Tag);
				pathTextBoxes[tag].Text = dialog.FileName;
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
			//パステキストボックスの作成上限数を超えた場合（プログラム追加上限数を超えた場合）
			if (numberofProgram >= PROG_NUM) 
			{
				MessageBox.Show("これ以上プログラムを追加できません。","追加不可", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			CreateComponentCluster();
			return;
		}

		//「決定」ボタン
		private void decideButton_Click(object sender, EventArgs e)
		{
			if (!isValid)
			{
				return;
			}

			if (isAdd)
			{
				Console.WriteLine("Add Data");
				using (var db = new SetTitleDbContext())
				{
					DbAdd(db);
					ShowTables(db);
				}
			}
			else
			{
				Console.WriteLine("Edit Data");
				using (var db = new SetTitleDbContext())
				{
					var datas_setTitle = from setTitle in db.SetTitles
										 where setTitle.SetTitleId == setTitleId
										 select setTitle;
					var data_setTitle = datas_setTitle.First();
					db.Remove(data_setTitle);
					db.SaveChanges();
					DbAdd(db, false);
				}
			}
			return;
		}

		//セットタイトル名の入力チェック
		private void settitleTextBox_Validating(object sender, CancelEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			//空欄の場合
			if (textBox.Text.Trim().Length == 0) 
			{
				textBox.BackColor = Color.Red;
				isValid = false;
			}
			else
			{
				textBox.BackColor = Color.White;
				isValid = true;
			}
			return;
		}

		private void prognameTextBox_Validating(object sender, CancelEventArgs e)
		{

		}

		private void pathTextBox_Validating(object sender, CancelEventArgs e)
		{

		}




		private void CreateComponentCluster(string program_name="", string program_path="")
		{
			//prognameLabel
			string PNLABEL_NAME = $"prognameLabel{numberofProgram}";
			string PNLABEL_TEXT = $"{numberofProgram + 1}. プログラム名";
			const int PNLABEL_OFFSET = 100;
			int[] PNLABEL_LOCATE = {
				204, //X座標
				PNLABEL_OFFSET * numberofProgram + allProgPanel.AutoScrollPosition.Y //Y座標
			};

			//pathLabel
			string PATHLABEL_NAME = $"pathLabel{numberofProgram}";
			const string PATHLABEL_TEXT = "パス";
			const int PATHLABEL_OFFSET = 100;
			int[] PATHLABEL_LOCATE = {
				423, //X座標
				PATHLABEL_OFFSET * numberofProgram + allProgPanel.AutoScrollPosition.Y //Y座標
			};

			//prognameTextBox
			string PNBOX_NAME = $"prognameTextBox{numberofProgram}";
			const int PNBOX_OFFSET = 100; //テキストボックス間の距離（Y）
			const int PNBOX_FIRST_OFFSET = 28; //Panelと最上部のテキストボックスとの距離（Y）
			int[] PNBOX_LOCATE = {
				204, //X座標
				PNBOX_OFFSET * numberofProgram + PNBOX_FIRST_OFFSET + allProgPanel.AutoScrollPosition.Y //Y座標
			};
			int[] PNBOX_SIZE = {
				150, //テキストボックス幅
				31 //テキストボックス高さ
			};

			//pathTextBox
			string PATHBOX_NAME = $"pathTextBox{numberofProgram}";
			const int PATHBOX_OFFSET = 100;
			const int PATHBOX_FIRST_OFFSET = 28; //Panelと最上部のテキストボックスとの距離（Y）
			int[] PATHBOX_LOCATE = {
				423, //X座標
				PATHBOX_OFFSET * numberofProgram + PATHBOX_FIRST_OFFSET + allProgPanel.AutoScrollPosition.Y //Y座標
			};
			int[] PATHBOX_SIZE = {
				150, //テキストボックス幅
				31 //テキストボックス高さ
			};

			//progRefeButton
			string PRBUTTON_NAME = $"progRefeButton{numberofProgram}";
			const string PRBUTTON_TEXT = "参照";
			int PRBUTTON_TAG = numberofProgram;
			const int PRBUTTON_OFFSET = 100;
			const int PRBUTTON_FIRST_OFFSET = 28;
			int[] PRBUTTON_LOCATE ={
				579,
				PRBUTTON_OFFSET * numberofProgram + PRBUTTON_FIRST_OFFSET + allProgPanel.AutoScrollPosition.Y
			};
			int[] PRBUTTON_SIZE ={
				74,
				31
			};

			CreateTools.createLabel(PNLABEL_NAME, PNLABEL_TEXT, PNLABEL_LOCATE, allProgPanel);
			CreateTools.createLabel(PATHLABEL_NAME, PATHLABEL_TEXT, PATHLABEL_LOCATE, allProgPanel);
			TextBox prognameTextBox = CreateTools.createTextBox(PNBOX_NAME, PNBOX_LOCATE, PNBOX_SIZE, allProgPanel);
			prognameTextBox.Text = program_name;
			prognameTextBox.Validating += prognameTextBox_Validating;
			prognameTextBoxes[numberofProgram] = prognameTextBox;
			TextBox pathTextBox = CreateTools.createTextBox(PATHBOX_NAME, PATHBOX_LOCATE, PATHBOX_SIZE, allProgPanel);
			pathTextBox.Text = program_path;
			pathTextBox.Validating += pathTextBox_Validating;
			pathTextBoxes[numberofProgram] = pathTextBox;
			Button button = CreateTools.createButton(PRBUTTON_NAME, PRBUTTON_TEXT, PRBUTTON_TAG, PRBUTTON_LOCATE, PRBUTTON_SIZE, allProgPanel);
			button.Click += progRefeButton_Click;

			numberofProgram++;
			return;
		}

		private void DbAdd(SetTitleDbContext db, bool isAdd=true)
		{
			if (isAdd)
			{
				db.Add(new SetTitle { TitleName = settitleTextBox.Text });
			}
			else
			{
				db.Add(new SetTitle { SetTitleId = setTitleId, TitleName = settitleTextBox.Text });
			}
			db.SaveChanges();

			SetTitle set;
			if (isAdd)
			{
				set = db.SetTitles
						  .OrderBy(s => s.SetTitleId)
	                      .Last();
			}
			else
			{
				var sets = from setTitle in db.SetTitles
					  where setTitle.SetTitleId == setTitleId
					  select setTitle;
				set = sets.First();
			}
			for (int i = 0; i < PROG_NUM; i++)
			{
				try
				{
					set.StartupProgs.Add(new StartupProg
					{
						StartupProgName = prognameTextBoxes[i].Text,
						StartupProgPath = pathTextBoxes[i].Text,
						SetTitleId = set.SetTitleId,
						SetTitle = set
					});
					db.SaveChanges();

				}
				catch (NullReferenceException ne) //起動できる上限数より少ないファイルを設定した場合
				{
					db.SaveChanges();
					break;
				}
				finally
				{
					DbOrganize(db);
				}
			}

		}

		//データベース内のデータを整理する（空白のみのデータを削除）
		private void DbOrganize(SetTitleDbContext db)
		{
			var datas = from startupProg in db.StartupProgs
						select startupProg;
			foreach(var data in datas)
			{
				if(data.StartupProgName.Trim().Length == 0||data.StartupProgPath.Trim().Length == 0)
				{
					db.Remove(data);
					db.SaveChanges();
				}
			}
		}

		private void ShowTables(SetTitleDbContext db)
		{
			var queryAllSetTiles = from setTitle in db.SetTitles
									select setTitle;
			var queryAllStartupProg = from startupProg in db.StartupProgs
								   select startupProg;

			foreach (var data in queryAllSetTiles)
			{
				Console.WriteLine("SetTitleId = " + data.SetTitleId);
				Console.WriteLine("SetTitleName = " + data.TitleName);
				Console.WriteLine("SetTitle_StartupProg_Count = " + data.StartupProgs.Count);
			}
			Console.WriteLine();

			foreach (var data in queryAllStartupProg)
			{
				Console.WriteLine("StartupProgId = " + data.StartupProgId);
				Console.WriteLine("StartupProgName = " + data.StartupProgName);
				Console.WriteLine("StartupProgPath = " + data.StartupProgPath);
				Console.WriteLine("StartupProg_SetTitleId = " + data.SetTitleId);
				Console.WriteLine("StartupProg_SetTitle = " + data.SetTitle.SetTitleId);
			}
			Console.WriteLine();
		}

	}
}
