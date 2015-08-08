using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PathKeeper
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		//переменные для хранения списка файлов и определения времени 
		StringBuilder FileList = new StringBuilder();
		TimeSpan ts;
		Stopwatch stw;

		//обработка глобального массива всех файлов
		StringReader sr;

		//выход
		private void выходToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		//рекурсивная функция для генерирования списка всех файлов на компьютере
		internal int GetDirect(string dir)
		{
			try
			{
				//генерируем список директория и файлов из передаваемого в функцию пути
				string[] SecondFLD = Directory.GetDirectories(dir);
				string[] SecondFL = Directory.GetFiles(dir);

				//добавляем в глобальный список файлы
				foreach (string FL in SecondFL)
				{
					FileList.AppendLine(FL);
				}

				//переходим к следующей директории
				foreach (string FLD in SecondFLD)
				{
					//добавляем папку
					FileList.AppendLine(FLD);
					GetDirect(FLD);
				}
			}
			catch (System.Exception ex)
			{

			}

			return 0;
		}

		//потом для обхода всех директория и генерации нового списка файлов
		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				//списко дисков
				string[] ALLDrive = Directory.GetLogicalDrives();
				
				foreach (string Drive in ALLDrive)
				{
					//список файлов в корне диска
					string[] FL = Directory.GetFiles(Drive);
					
					foreach (string files in FL)
					{
						FileList.AppendLine(files);
					}

					//список директорий в корне диска
					string[] FLD = Directory.GetDirectories(Drive);
					foreach (string folder in FLD)
					{
						//добавляем папку
						FileList.AppendLine(folder);
						GetDirect(folder);
					}

				}
			}
			catch (System.Exception ex)
			{
				
			}


			//сохраняем список в файл

			if(File.Exists("path.txt"))
			{
				File.Delete("path.txt");
				File.WriteAllText("path.txt", FileList.ToString());
			}
			else
			{
				//создаем новый
				File.WriteAllText("path.txt", FileList.ToString());
			}

			//время
			ts = stw.Elapsed;
			toolStripStatusLabel1.Text = "Завершено за " + ts.ToString();
			toolStripProgressBar1.Visible = false;
			Sync.Enabled = false;

		}

		//кнопка синхронизации
		private void Sync_Click(object sender, EventArgs e)
		{
			toolStripProgressBar1.MarqueeAnimationSpeed = 10;
			toolStripProgressBar1.Visible = true;
			toolStripStatusLabel1.Text = "Идет обработка";

			stw = Stopwatch.StartNew();
			FileList.Remove(0, FileList.Length);
			Sync.Enabled = false;
			backgroundWorker1.RunWorkerAsync();

		}

		
		private void Form1_Load(object sender, EventArgs e)
		{		
			CheckForIllegalCrossThreadCalls = false;
			//загрузка данных
			backgroundWorker2.RunWorkerAsync();
		}
		

		//при загрузке формы загружаем в память список файлов
		private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
		{

			toolStripProgressBar1.MarqueeAnimationSpeed = 10;
			toolStripStatusLabel1.Text = "Идет обработка";

			try
			{
				//читаем из файла
				FileList.Append(File.ReadAllText("path.txt", Encoding.UTF8));
				toolStripStatusLabel1.Text = "Завершено";
			}
			catch (System.Exception ex)
			{
				toolStripStatusLabel1.Text = "Список файлов не обнаружен, сделайте синхронизацию";
			}

			toolStripProgressBar1.Visible = false;
			Sync.Enabled = true;


		}


		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			//при заполении поля поиск, очищаем итембокс
			listBox1.Items.Clear();
			listBox2.Items.Clear();

			//если больше 3 символов тогда начинаем поиск
			if (textBox1.Text.Length > 3)
			{
				toolStripProgressBar1.MarqueeAnimationSpeed = 10;
				toolStripProgressBar1.Visible = true;
				toolStripStatusLabel1.Text = "Идет обработка";

				//если уже происходит поиск а пользователь продолжает набирать
				//слово для поиска тогда, останавливаем  поиск и начинаем снова
				try
				{					
					backgroundWorker3.CancelAsync();
					backgroundWorker3.RunWorkerAsync();
				}
				catch (System.Exception ex)
				{
					backgroundWorker3.CancelAsync();
				}

			}

		}

		private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
		{
			//добавляем для обработки глобальный массив файлов
			sr = new StringReader(FileList.ToString());

			while (true)
			{
				try
				{	//читаем построчно глобальный массив
					string tmp = sr.ReadLine();

					//выделяем последнее вхождение в строку слеша 
					int h = tmp.LastIndexOf(@"\");

					//вырезаем имя файла или папки
					string FileName = tmp.Substring(h+1);

					//если введенное для поиска слово содержиться в имени тогда добавляем его в результаты
					if (FileName.IndexOf(textBox1.Text, StringComparison.OrdinalIgnoreCase) != -1)
					{
						//тогда добавляем его результаты поиска
						//имя
						listBox1.Items.Add(FileName);
						//в зеркале полный путь
						listBox2.Items.Add(tmp);
					}

				}
				catch (System.Exception ex)
				{
					break;
				}
				
			}

			toolStripProgressBar1.Visible = false;
			toolStripStatusLabel1.Text = "Завершено";
		}

		private void listBox1_DoubleClick(object sender, EventArgs e)
		{
			//при двойном клике по файлу открываем его в закрепленной к нему программе
			int i = listBox1.SelectedIndex;
			try
			{
				System.Diagnostics.Process.Start(listBox2.Items[i].ToString());
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			
		}

		private void button1_Click(object sender, EventArgs e)
		{
			int SelectedItem = listBox1.SelectedIndex;
			//глючное место
			//я не знаю как увидеть различие между файлои и папкой
			//сначала получаем инфу об объекте как о файле, если эксепшен
			//тогда обрабатываем его как директорию, и то не всегда информация доступна
			try
			{	
				//файл
				FileInfo SelectedFileInfo = new FileInfo(listBox2.Items[SelectedItem].ToString());
				string infoFile = "Имя файла:  " + SelectedFileInfo.Name +
					"\r\nРазмер файла:  " + Convert.ToString(SelectedFileInfo.Length / 1024) + "Кб" +
					"\r\nСоздан:  " + SelectedFileInfo.CreationTime.ToString() +
					"\r\nАтрибуты:  " + SelectedFileInfo.Attributes.ToString();
				MessageBox.Show(infoFile, "Информация o файлe", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

			}
			catch (System.Exception ex)
			{	
				// :) папка
				DirectoryInfo SelectedDirInfo = new DirectoryInfo(listBox2.Items[SelectedItem].ToString());
				string infoDir = "Имя каталога:  " + SelectedDirInfo.Name +
					"\r\nРодительский каталог:  " + SelectedDirInfo.Parent +
					"\r\nСоздан:  " + SelectedDirInfo.CreationTime.ToString() +
					"\r\nАтрибуты:  " + SelectedDirInfo.Attributes.ToString();
				MessageBox.Show(infoDir, "Информация о каталоге", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				
			}


		}

		private void button2_Click(object sender, EventArgs e)
		{

			try
			{
				//выделяем путь если выбран файл
				int i = listBox1.SelectedIndex;

				string path = listBox2.Items[i].ToString();

				if (path.Length - path.LastIndexOf(".") < 5)
				{
					path = path.Remove(path.LastIndexOf(@"\"));
				}

				System.Diagnostics.Process.Start(path);
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		
	}
}
