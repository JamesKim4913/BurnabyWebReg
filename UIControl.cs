using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class UIControl
{


    // 크로스스레드 에러 방지

    // 상태 true/false
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

    // 텍스트박스나 레이블
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

    // 상태표시줄
    // 사용예제 uc.updateStatusBar(toolStripStatusLabel1, ex.Message); // 상태표시줄
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



    /*
    //크로스스레드방지
    // 쓰레드에서 작업할때 화면 UI에 접근할때 사용함
    // 예를들어 리스트뷰에 추가하거나 텍스트박스에 값 변경할때 등에 사용함
    this.Invoke(new MethodInvoker(delegate ()
    {
        // 리스트뷰에 추가  
        // 이곳에 작업추가                                
    }));  // invoke
    */


}

