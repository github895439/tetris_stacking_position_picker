using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenCvSharp;
using OpenCvSharp.Extensions;

using System.IO;
using System.Diagnostics;
using System.Threading;

namespace tetris_stacking_position_picker
{
    public partial class Form1 : Form
    {
        private readonly String HINT_POSITION =
                Properties.Resources.HINT_POSITION_1 +
                Environment.NewLine +
                Properties.Resources.HINT_POSITION_2 +
                Environment.NewLine +
                Properties.Resources.HINT_POSITION_3 +
                Environment.NewLine +
                Properties.Resources.HINT_POSITION_4;
        private readonly String HINT_COLOR =
                Properties.Resources.HINT_COLOR_1 +
                Environment.NewLine +
                Properties.Resources.HINT_COLOR_2 +
                Environment.NewLine +
                Properties.Resources.HINT_COLOR_3 +
                Environment.NewLine +
                Properties.Resources.HINT_COLOR_4;
        private const int FIELD_SIZE_MAX_X = 10;
        private const int FIELD_SIZE_MAX_Y = 20;
        private const int FIELD_VALUE_EMPTY = 0;
        private const int FIELD_VALUE_EXIST = 1;
        private const int FIELD_VALUE_OLD = 2;
        private const int FIELD_VALUE_NEW = 3;
        private const int NEXT_SIZE_MAX_X = 4;
        private const int NEXT_SIZE_MAX_Y = 2;
        private const int PROGRESS_PICK_GAME_START = 1;
        private const int PROGRESS_PICK_GAME_END = 2;
        private const int PROGRESS_PICK_NEXT = 3;
        private const int PROGRESS_PICK_STACK = 4;
        private const int PROGRESS_PICK_ERROR = 999;
        private const int TETRA = 4;

        private bool isDebug = false;
        private bool isSleep = false;

        private String mp4FilePath;
        private VideoCapture capture = null;
        private bool isCancelPick = false;
        private StringBuilder logLine = new StringBuilder();
        private String logFilenameBase;
        private int logFilenameIndex;

        //ワーカー渡し
        private String progressMessage;
        private int[][] progressField;
        private int[] workerFieldPositionX;
        private int[] workerFieldPositionY;

        //計算値
        private int squreSize;
        private int squreHalfSize;

        //プレイ情報
        private bool isPlaying = false;
        private bool isSkipGame = false;
        private bool isMergeNext = false;
        private bool isAllEmpty;
        private List<String> mino = new List<String>();
        private int[][] fieldOld;
        private int[][] fieldNew;
        private int[][] fieldRtn;
        private bool[][] next;
        private String logFilename;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @".";
                openFileDialog.Filter = "mp4 files (*.mp4)|*.mp4|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    mp4FilePath = openFileDialog.FileName;
                    textBox1.Text = mp4FilePath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(HINT_POSITION);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(HINT_POSITION);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MessageBox.Show(HINT_POSITION);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            MessageBox.Show(HINT_POSITION);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MessageBox.Show(HINT_COLOR);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == String.Empty)
            {
                MessageBox.Show(Properties.Resources.ERROR_INPUT_EMPTY);
                return;
            }

            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show(Properties.Resources.ERROR_NOT_EXIST);
                return;
            }

            if (capture == null)
            {
                capture = new VideoCapture(textBox1.Text);

                if (!capture.IsOpened())
                {
                    capture.Dispose();
                    capture = null;
                    MessageBox.Show(Properties.Resources.ERROR_FAIL_OPEN);
                    return;
                }

                textBox1.Enabled = false;
                button1.Enabled = false;
            }

            TimeSpan ts = new TimeSpan((long)capture.FrameCount * 10000000 / (long)capture.Fps);
            MessageBox.Show(String.Format(
                    Properties.Resources.INFO_CONTENT_1 + Environment.NewLine + Properties.Resources.INFO_CONTENT_2,
                    capture.Fps,
                    Properties.Resources.UNIT_FPS,
                    capture.FrameCount,
                    Properties.Resources.UNIT_FRAME,
                    ts.ToString()));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                if (MessageBox.Show(
                        Properties.Resources.CONFIRM_CLEAR,
                        Properties.Resources.CONFIRM_CLEAR_CAPTION,
                        MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }

                capture.Dispose();
                capture = null;
                textBox1.Enabled = true;
                button1.Enabled = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void changeFormForPick(bool isPicking)
        {
            if (isPicking)
            {
                button5.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                button7.Enabled = false;
                button6.Enabled = false;
                button8.Enabled = true;
                toolStripStatusLabel1.Text = Properties.Resources.STATUS_PICKING;
            }
            else
            {
                button5.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                button7.Enabled = true;
                button6.Enabled = true;
                button8.Enabled = false;
                toolStripStatusLabel1.Text = String.Empty;
            }
        }

        private void calcSettingValue()
        {
            squreSize = Convert.ToInt32(Math.Round((Convert.ToDouble(textBox13.Text) - Convert.ToDouble(textBox11.Text)) / FIELD_SIZE_MAX_Y));
            squreHalfSize = squreSize / 2;
            int fieldWidth = (Convert.ToInt32(textBox14.Text) - squreHalfSize) - (Convert.ToInt32(textBox12.Text) + squreHalfSize);
            workerFieldPositionX = new int[FIELD_SIZE_MAX_X];

            for (int x = 0; x < FIELD_SIZE_MAX_X; x++)
            {
                workerFieldPositionX[x] = Convert.ToInt32(textBox12.Text) + squreHalfSize + x * fieldWidth / (FIELD_SIZE_MAX_X - 1);
            }

            int fieldHeight = (Convert.ToInt32(textBox13.Text) - squreHalfSize) - (Convert.ToInt32(textBox11.Text) + squreHalfSize);
            workerFieldPositionY = new int[FIELD_SIZE_MAX_Y];

            for (int y = 0; y < FIELD_SIZE_MAX_Y; y++)
            {
                workerFieldPositionY[y] = Convert.ToInt32(textBox11.Text) + squreHalfSize + y * fieldHeight / (FIELD_SIZE_MAX_Y - 1);
            }

            fieldOld = new int[FIELD_SIZE_MAX_X][];
            fieldNew = new int[FIELD_SIZE_MAX_X][];
            fieldRtn = new int[FIELD_SIZE_MAX_X][];

            for (int i = 0; i < FIELD_SIZE_MAX_X; i++)
            {
                fieldOld[i] = new int[FIELD_SIZE_MAX_Y];
                fieldNew[i] = new int[FIELD_SIZE_MAX_Y];
                fieldRtn[i] = new int[FIELD_SIZE_MAX_Y];
            }

            next = new bool[NEXT_SIZE_MAX_X][];

            for (int x = 0; x < NEXT_SIZE_MAX_X; x++)
            {
                next[x] = new bool[NEXT_SIZE_MAX_Y];
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (capture == null)
            {
                capture = new VideoCapture(textBox1.Text);

                if (!capture.IsOpened())
                {
                    capture.Dispose();
                    capture = null;
                    MessageBox.Show(Properties.Resources.ERROR_FAIL_OPEN);
                    return;
                }

                textBox1.Enabled = false;
                button1.Enabled = false;
            }

            calcSettingValue();

            isPlaying = false;
            capture.PosFrames = 0;
            logFilenameBase = Path.GetFileName(textBox1.Text) + "_";
            logFilenameIndex = 1;
            backgroundWorker1.RunWorkerAsync();
            changeFormForPick(true);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();

            for (int i = 0; i < 3; i++)
            {
                if (!backgroundWorker1.IsBusy)
                {
                    break;
                }

                Thread.Sleep(1000);
            }
        }

        private void logField(int[][] field, String message)
        {
            TimeSpan ts = new TimeSpan((long)capture.PosFrames * 10000000 / (long)capture.Fps / 10000000 * 10000000);
            String log = String.Format(
                    Properties.Resources.LOG_TIMESTAMP_FORMAT + Properties.Resources.LOG_FIELD_FORMAT,
                    ts.ToString(),
                    capture.PosFrames,
                    message);

            if (!checkBox1.Checked)
            {
                textBox6.AppendText(log + Environment.NewLine);
            }

            File.AppendAllLines(logFilename, new String[] { log });

            for (int i = 0; i < FIELD_SIZE_MAX_Y; i++)
            {
                logLine.Clear();

                for (int j = 0; j < FIELD_SIZE_MAX_X; j++)
                {
                    switch (field[j][i])
                    {
                        case FIELD_VALUE_EMPTY:
                        {
                            logLine.Append("＿");
                            break;
                        }
                        case FIELD_VALUE_EXIST:
                        {
                            logLine.Append("□");
                            break;
                        }
                        case FIELD_VALUE_OLD:
                        {
                            logLine.Append("◆");
                            break;
                        }
                        case FIELD_VALUE_NEW:
                        {
                            logLine.Append("■");
                            break;
                        }
                        default:
                        {
                            logLine.Append("？");
                            break;
                        }
                    }
                }

                if (!checkBox1.Checked)
                {
                    textBox6.AppendText(logLine.ToString() + Environment.NewLine);
                }

                File.AppendAllLines(logFilename, new String[] { logLine.ToString() });
            }
        }

        private void logMessage(String message)
        {
            TimeSpan ts = new TimeSpan((long)capture.PosFrames * 10000000 / (long)capture.Fps / 10000000 * 10000000);
            String log = String.Format(
                    Properties.Resources.LOG_TIMESTAMP_FORMAT + "{2}",
                    ts.ToString(),
                    capture.PosFrames,
                    message);

            if (!checkBox1.Checked)
            {
                textBox6.AppendText(log + Environment.NewLine);
            }

            File.AppendAllLines(logFilename, new String[] { log });
        }

        private String analyzeNext()
        {
            if (next[0][0] && next[1][0] && next[2][0] && next[3][0])
            {
                return "I";
            }

            if (next[1][0] && next[2][0] && next[1][1] && next[2][1])
            {
                return "O";
            }

            if (next[0][0] && next[1][0] && next[2][0] && next[1][1])
            {
                return "T";
            }

            if (next[0][0] && next[1][0] && next[2][0] && next[0][1])
            {
                return "L";
            }

            if (next[0][0] && next[1][0] && next[2][0] && next[2][1])
            {
                return "J";
            }

            if (next[1][0] && next[2][0] && next[0][1] && next[1][1])
            {
                return "S";
            }

            if (next[0][0] && next[1][0] && next[1][1] && next[2][1])
            {
                return "Z";
            }

            return null;
        }

        private String getNext(Bitmap bmp)
        {
            int countTrue = 0;

            for (int y = 0, posY = Convert.ToInt32(textBox3.Text); y < NEXT_SIZE_MAX_Y; y++, posY += squreSize)
            {
                for (int x = 0, posX = Convert.ToInt32(textBox2.Text) - squreSize; x < NEXT_SIZE_MAX_X; x++, posX += squreSize)
                {
                    double brightness1 = bmp.GetPixel(posX, posY).GetBrightness();
                    double brightness2 = bmp.GetPixel(posX, posY - 1).GetBrightness();
                    double brightness = brightness1 > brightness2 ? brightness1 : brightness2;

                    if (brightness > Properties.Settings.Default.EXIST_BLIGHTNESS_NEXT_MIN)
                    {
                        next[x][y] = true;
                        countTrue++;
                    }
                    else
                    {
                        next[x][y] = false;
                    }
                }
            }

            if (countTrue != TETRA)
            {
                return null;
            }

            String rtn = analyzeNext();

            return rtn;
        }

        private void initGameStart()
        {
            mino.Clear();
            isSkipGame = false;

            for (int y = 0; y < FIELD_SIZE_MAX_Y; y++)
            {
                for (int x = 0; x < FIELD_SIZE_MAX_X; x++)
                {
                    fieldOld[x][y] = FIELD_VALUE_EMPTY;
                }
            }
        }

        private bool analyzeGameStart(object sender, Bitmap bmp)
        {
            Color color = bmp.GetPixel(Convert.ToInt32(textBox5.Text), Convert.ToInt32(textBox4.Text));
            float brightness = color.GetBrightness();

            if (brightness > Properties.Settings.Default.GAME_START_AND_END_BLIGHTNESS_MIN)
            {
                if (isDebug)
                {
                    bmp.Save(capture.PosFrames.ToString() + "_start.jpg");
                    isDebug = false;
                }

                logFilename = logFilenameBase + logFilenameIndex.ToString() + ".log";
                File.Delete(logFilename);
                BackgroundWorker worker = sender as BackgroundWorker;
                worker.ReportProgress(PROGRESS_PICK_GAME_START);

                initGameStart();

                String minoNow = getNext(bmp);

                if (minoNow != null)
                {
                    mino.Add(minoNow);
                }
                else
                {
                    progressMessage = Properties.Resources.LOG_ERROR_UNKNOWN_MINO;
                    worker.ReportProgress(PROGRESS_PICK_ERROR);
                    isSkipGame = true;
                }

                return true;
            }

            return false;
        }

        private bool analyzeGameEnd(object sender, Bitmap bmp)
        {
            Color color1 = bmp.GetPixel(Convert.ToInt32(textBox7.Text), Convert.ToInt32(textBox8.Text));
            float brightness1 = color1.GetBrightness();
            Color color2 = bmp.GetPixel(Convert.ToInt32(textBox9.Text), Convert.ToInt32(textBox10.Text));
            float brightness2 = color2.GetBrightness();

            if ((brightness1 > Properties.Settings.Default.GAME_START_AND_END_BLIGHTNESS_MIN) && (brightness2 > Properties.Settings.Default.GAME_START_AND_END_BLIGHTNESS_MIN))
            {
                if (isDebug)
                {
                    bmp.Save(capture.PosFrames.ToString() + "_end.jpg");
                    isDebug = false;
                }

                BackgroundWorker worker = sender as BackgroundWorker;
                worker.ReportProgress(PROGRESS_PICK_GAME_END);
                return false;
            }

            return true;
        }

        private void getField(Bitmap bmp)
        {
            for (int y = 0; y < FIELD_SIZE_MAX_Y; y++)
            {
                for (int x = 0; x < FIELD_SIZE_MAX_X; x++)
                {
                    double brightness1 = bmp.GetPixel(workerFieldPositionX[x], workerFieldPositionY[y]).GetBrightness();
                    double brightness2 = bmp.GetPixel(workerFieldPositionX[x], workerFieldPositionY[y] - 1).GetBrightness();
                    double brightness = brightness1 > brightness2 ? brightness1 : brightness2;

                    if (brightness >= Properties.Settings.Default.EXIST_BLIGHTNESS_FIELD_MIN)
                    {
                        fieldNew[x][y] = FIELD_VALUE_EXIST;
                    }
                    else
                    {
                        fieldNew[x][y] = FIELD_VALUE_EMPTY;
                    }
                }
            }
        }

        private int[][] analyzeStack()
        {
            for (int y = 0; y < FIELD_SIZE_MAX_Y; y++)
            {
                for (int x = 0; x < FIELD_SIZE_MAX_X; x++)
                {
                    fieldRtn[x][y] = FIELD_VALUE_EMPTY;

                    if ((fieldOld[x][y] == FIELD_VALUE_EXIST) && (fieldNew[x][y] == FIELD_VALUE_EXIST))
                    {
                        fieldRtn[x][y] = FIELD_VALUE_EXIST;
                        continue;
                    }

                    if ((fieldOld[x][y] == FIELD_VALUE_EMPTY) && (fieldNew[x][y] == FIELD_VALUE_EXIST))
                    {
                        fieldRtn[x][y] = FIELD_VALUE_NEW;
                        continue;
                    }

                    if ((fieldOld[x][y] == FIELD_VALUE_EXIST) && (fieldNew[x][y] == FIELD_VALUE_EMPTY))
                    {
                        fieldRtn[x][y] = FIELD_VALUE_OLD;
                    }
                }
            }

            return fieldRtn;
        }

        private void storeToFieldOld()
        {
            for (int y = 0; y < FIELD_SIZE_MAX_Y; y++)
            {
                for (int x = 0; x < FIELD_SIZE_MAX_X; x++)
                {
                    if (fieldNew[x][y] == FIELD_VALUE_EMPTY)
                    {
                        fieldOld[x][y] = FIELD_VALUE_EMPTY;
                    }
                    else
                    {
                        fieldOld[x][y] = FIELD_VALUE_EXIST;
                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Mat frameNew = new Mat();
            Mat frameOld;
            Bitmap bmpNew = new Bitmap(1, 1);
            Bitmap bmpOld;
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 0; ((i < capture.FrameCount) && !backgroundWorker1.CancellationPending); i++)
            {
                if (isDebug)
                {
                    worker.CancelAsync();
                }

                frameOld = frameNew;
                bmpOld = bmpNew;
                frameNew = new Mat();
                capture.Read(frameNew);
                bmpNew = frameNew.ToBitmap();

                if (!isPlaying)
                {
                    isPlaying = analyzeGameStart(sender, bmpNew);
                }
                else
                {
                    isPlaying = analyzeGameEnd(sender, bmpNew);

                    if ((!isSkipGame) && isPlaying)
                    {
                        getField(bmpNew);

                        isAllEmpty = true;

                        for (int x = 0; x < FIELD_SIZE_MAX_X; x++)
                        {
                            if (fieldNew[x][FIELD_SIZE_MAX_Y - 1] != FIELD_VALUE_EMPTY)
                            {
                                isAllEmpty = false;
                                break;
                            }
                        }

                        if (!isAllEmpty)
                        {
                            String minoNow = getNext(bmpNew);

                            if (minoNow == null)
                            {
                                if (!isMergeNext)
                                {
                                    if (isDebug)
                                    {
                                        bmpOld.Save((capture.PosFrames - 1).ToString() + "_merge_old.jpg");
                                        bmpNew.Save((capture.PosFrames - 1).ToString() + "_merge_new.jpg");
                                        isDebug = false;
                                    }

                                    getField(bmpOld);

                                    progressField = analyzeStack();

                                    progressMessage = (capture.PosFrames - 1).ToString();
                                    worker.ReportProgress(PROGRESS_PICK_STACK);
                                    isMergeNext = true;

                                    storeToFieldOld();
                                }
                            }
                            else
                            {
                                String minoOld = mino[mino.Count - 1];

                                if (minoNow != minoOld)
                                {
                                    if (mino.Count > 1)
                                    {
                                        if (isDebug)
                                        {
                                            bmpOld.Save((capture.PosFrames - 1).ToString() + "_change_old.jpg");
                                            bmpNew.Save((capture.PosFrames - 1).ToString() + "_change_new.jpg");
                                            isDebug = false;
                                        }

                                        if (isMergeNext)
                                        {
                                            isMergeNext = false;
                                        }
                                        else
                                        {
                                            getField(bmpOld);

                                            progressField = analyzeStack();

                                            progressMessage = (capture.PosFrames - 1).ToString();
                                            worker.ReportProgress(PROGRESS_PICK_STACK);

                                            storeToFieldOld();
                                        }
                                    }

                                    mino.Add(minoNow);
                                }
                            }
                        }
                    }
                }

                bmpOld.Dispose();
                frameOld.Dispose();
                toolStripStatusLabel1.Text =
                        Properties.Resources.STATUS_PICKING +
                        ":" +
                        capture.PosFrames.ToString() +
                        Properties.Resources.UNIT_FRAME +
                        "/" +
                        capture.FrameCount +
                        Properties.Resources.UNIT_FRAME;

                if ((capture.PosFrames > 13600) && isSleep)
                {
                    Thread.Sleep(1000 / 2);
                }
            }

            bmpNew.Dispose();
            frameNew.Dispose();
            isCancelPick = backgroundWorker1.CancellationPending;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isCancelPick)
            {
                MessageBox.Show(Properties.Resources.INFO_STOP_PICK);
            }
            else
            {
                MessageBox.Show(Properties.Resources.INFO_COMPLETE_PICK);
            }

            changeFormForPick(false);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case PROGRESS_PICK_GAME_START:
                    {
                        logMessage(Properties.Resources.LOG_GAME_START);
                        break;
                    }
                case PROGRESS_PICK_GAME_END:
                    {
                        logMessage(Properties.Resources.LOG_GAME_END);
                        logFilenameIndex++;
                        break;
                    }
                case PROGRESS_PICK_NEXT:
                    {
                        logMessage(progressMessage);
                        break;
                    }
                case PROGRESS_PICK_STACK:
                    {
                        logField(progressField, progressMessage);
                        break;
                    }
                case PROGRESS_PICK_ERROR:
                    {
                        logMessage(progressMessage);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
