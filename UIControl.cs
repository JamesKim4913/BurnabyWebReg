using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class UIControl
{


    // Cross-thread error prevention

    // status true/false
    private delegate void ControlStateChange(Control varControl, bool varState);
    public void changeControlStatus(Control varControl, bool varState)
    {
        if (varControl.InvokeRequired)
        {
            varControl.BeginInvoke(new ControlStateChange(changeControlStatus), new object[] { varControl, varState });
        }
        else
        {
            varControl.Enabled = varState;
        }
    }

    // Text box or label
    private delegate void ControlTextChange(Control varControl, string varText);
    public void changeControlText(Control varControl, string varText)
    {
        if (varControl.InvokeRequired)
        {
            varControl.BeginInvoke(new ControlTextChange(changeControlText), new object[] { varControl, varText });
        }
        else
        {
            varControl.Text = varText;
        }
    }

    // Status bar
    // uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
    private delegate void ControlStatusBarChange(ToolStripStatusLabel label, string text);
    public void updateStatusBar(ToolStripStatusLabel label, string text)
    {
        if (label.GetCurrentParent().InvokeRequired)
        {
            label.GetCurrentParent().Invoke(new ControlStatusBarChange(updateStatusBar), label, text);
        }
        else
        {
            label.Text = text;
            label.Invalidate();
        }
    }




}

