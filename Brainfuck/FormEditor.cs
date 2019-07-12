using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brainfuck
{
    public partial class FormEditor : Form
    {
        private const int memorySize = 1000;
        private int pointerPosition;
        private int[] memory;
        private int positionInCode;
        private int inputPosition;
        private bool stopping;
        private string code = "";
        private string output;
        private bool wrap = true;
        public FormEditor()
        {
            InitializeComponent();
            for (int i = 0; i < memorySize * 2 - 1; i++)
                listBoxMemory.Items.Add("");
            Reset();
            Update();
        }
        private void DoStep()
        {
            if (positionInCode >= code.Length)
                throw new BrainfuckException("We are at the end");
            char command = code[positionInCode];
            switch (command)
            {
                case '<':
                    if (pointerPosition <= 0)
                    {
                        if (!wrap)
                            throw new BrainfuckException("Negative memory limit reached");
                        pointerPosition = memorySize * 2 - 2;
                    }
                    else
                        pointerPosition--;
                    break;
                case '>':
                    if (pointerPosition >= memorySize * 2 - 2)
                    {
                        if (!wrap)
                            throw new BrainfuckException("Positive memory limit reached");
                        pointerPosition = 0;
                    }
                    else
                        pointerPosition++;
                    break;
                case '+':
                    if (memory[pointerPosition] == 255)
                        memory[pointerPosition] = 0;
                    else
                        memory[pointerPosition]++;
                    break;
                case '-':
                    if (memory[pointerPosition] == 0)
                        memory[pointerPosition] = 255;
                    else
                        memory[pointerPosition]--;
                    break;
                case '.':
                    output += (char)memory[pointerPosition];
                    break;
                case ',':
                    if (inputPosition >= textBoxInput.Text.Length)
                        memory[pointerPosition] = 0;
                    else
                        memory[pointerPosition] = textBoxInput.Text[inputPosition];
                    inputPosition++;
                    break;
                case '[':
                    if (memory[pointerPosition] == 0)
                    {
                        int depth = 1;
                        while (depth > 0)
                        {
                            positionInCode++;
                            if (code[positionInCode] == '[')
                                depth++;
                            if (code[positionInCode] == ']')
                                depth--;
                        }
                    }
                    break;
                case ']':
                    if (memory[pointerPosition] != 0)
                    {
                        int depth = 1;
                        while (depth > 0)
                        {
                            positionInCode--;
                            if (code[positionInCode] == '[')
                                depth--;
                            if (code[positionInCode] == ']')
                                depth++;
                        }
                    }
                    break;
            }
            do
            {
                positionInCode++;
                if (positionInCode >= code.Length)
                    break;
            }
            while (!"<>.,+-[]$".Contains(code[positionInCode]));
        }
        private void ButtonRunStep_Click(object sender, EventArgs e)
        {
            try
            {
                DoStep();
            }
            catch (BrainfuckException exception)
            {
                MessageBox.Show(exception.Message);
            }
            UpdateUI();
        }
        private void UpdateUI()
        {
            int cursorPosition = richTextBoxCode.SelectionStart;
            for (int i = 0; i < memorySize * 2 - 1; i++)
            {
                listBoxMemory.Items[i] = (i - memorySize + 1) + ":\t" + memory[i];
            }
            listBoxMemory.SelectedIndex = pointerPosition;
            richTextBoxCode.SelectAll();
            richTextBoxCode.SelectionColor = Color.Blue;
            int startPosition = 0;
            int endPosition = 0;
            while (true)
            {
                while (endPosition < code.Length)
                {
                    if (!"<>.,+-[]".Contains(code[endPosition]))
                        break;
                    endPosition++;
                }
                if (endPosition >= code.Length)
                    break;
                startPosition = endPosition;
                while (endPosition < code.Length)
                {
                    if ("<>.,+-[]".Contains(code[endPosition]))
                        break;
                    endPosition++;
                }
                if (endPosition >= code.Length)
                    endPosition = code.Length;
                richTextBoxCode.Select(startPosition, endPosition - startPosition);
                richTextBoxCode.SelectionColor = Color.DarkGreen;
                startPosition = endPosition;
            }
            richTextBoxCode.SelectionStart = positionInCode;
            richTextBoxCode.SelectionLength = 1;
            richTextBoxCode.SelectionColor = Color.Red;
            richTextBoxCode.SelectionLength = 0;
            richTextBoxCode.SelectionStart = cursorPosition;
            labelOutput.Text = output;
        }
        private void Reset()
        {
            pointerPosition = memorySize - 1;
            positionInCode = 0;
            inputPosition = 0;
            memory = new int[memorySize * 2 - 1];
            UpdateUI();
            output = "";
            labelOutput.Text = "";
        }
        private void ButtonReset_Click(object sender, EventArgs e)
        {
            Reset();
        }
        private void ButtonRun_Click(object sender, EventArgs e)
        {
            buttonStop.Enabled = true;
            buttonRunStep.Enabled = false;
            buttonRun.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }
        private void RichTextBoxCode_TextChanged(object sender, EventArgs e)
        {
            code = richTextBoxCode.Text;
            UpdateUI();
        }
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                DoStep();
                if (positionInCode >= code.Length || stopping)
                    break;
            }
            while (code[positionInCode] != '$');
        }
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            stopping = true;
        }
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is BrainfuckException)
            {
                UpdateUI();
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Error != null)
                throw e.Error;
            UpdateUI();
            stopping = false;
            buttonStop.Enabled = false;
            buttonRunStep.Enabled = true;
            buttonRun.Enabled = true;
        }
        private void CheckBoxWrap_CheckedChanged(object sender, EventArgs e)
        {
            wrap = checkBoxWrap.Checked;
        }
    }
}