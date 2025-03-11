using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Godot;

public partial class 订书机 : Control
{
	//Export/
	[Export] public Godot.Button searchButton;
	[Export] public Godot.Button setTopButton;
	[Export] public Godot.Button CancelButton;
	[Export] public Godot.Button DisPlayButton;
	[Export] public Godot.LineEdit searchInput;
	[Export] public Godot.OptionButton DisPlayColumn;
	// 定义回调委托
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	//变量
	public class WindowInfo
	{
		public IntPtr targetWindow;
		public bool isTopMost=false;
	}
	public WindowInfo windowInfo = new WindowInfo();
	//Win32 API常量
	private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_SHOWWINDOW = 0x0040;
	private const int GWL_EXSTYLE = -20;
	private const int WS_EX_TOOLWINDOW = 0x00000080;
	private const int WS_EX_APPWINDOW = 0x00040000;
	private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
	//Win32 API函数
	[DllImport("user32.dll", SetLastError = true)]
	static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
	[DllImport("user32.dll")]
	static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
									int X, int Y, int cx, int cy, uint uFlags);
	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
	public override void _Ready()
	{
		// searchButton.ButtonDown += () => SearchWindow(searchInput.Text);
		setTopButton.ButtonDown += () => SetTopWindow();
		CancelButton.ButtonDown += () => CancelTopWindow();
		DisPlayButton.ButtonDown += () => DisPlayAllWindow();
	}
	public override void _Process(double delta)
	{
		// 如果目标窗口已经设置为置顶状态
		if (windowInfo.isTopMost && windowInfo.targetWindow != IntPtr.Zero)
		{
			// 检查窗口是否仍然处于置顶状态
			int currentStyle = GetWindowLong(windowInfo.targetWindow, GWL_EXSTYLE);
			if ((currentStyle & 0x8) != 0x8) // 0x8 是 WS_EX_TOPMOST
			{
				// 如果窗口不再置顶，重新置顶
				SetTopWindow();
			}
		}
	}
	public override void _Notification(int what)
	{
		switch (what)
		{
			case (int)NotificationWMCloseRequest: // 用户尝试关闭窗口
			case (int)NotificationPredelete:       // 节点即将被删除
				GD.Print("Godot 退出中，取消目标窗口置顶状态...");
				CancelTopWindow();
				break;
		}
	}
	// public void SearchWindow(string windowName)
	// {
	// 	 // 先清空旧句柄
	//     windowInfo.targetWindow = IntPtr.Zero;
		
	//     // 使用模糊查找
	//     List<IntPtr> matchedWindows = new List<IntPtr>();
	//     EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
	//     {
	//         if (!IsWindowVisible(hWnd)) return true;
			
	//         int length = GetWindowTextLength(hWnd);
	//         if (length == 0) return true;
			
	//         StringBuilder sb = new StringBuilder(length + 1);
	//         GetWindowText(hWnd, sb, sb.Capacity);
	//         string title = sb.ToString();

	//         // 模糊匹配
	//         if (title.Contains(windowName))
	//         {
	//             matchedWindows.Add(hWnd);
	//         }
	//         return true;
	//     }, IntPtr.Zero);
	//     // 选择第一个匹配的窗口
	//     if (matchedWindows.Count > 0)
	//     {
	//         windowInfo.targetWindow = matchedWindows[0];
	//         GD.Print($"找到窗口：{GetWindowTitle(windowInfo.targetWindow)}");
	//     }
	//     else
	//     {
	//         GD.PrintErr("未找到匹配窗口");
	//     }
	// }
	// // 新增辅助方法获取窗口标题
	// private string GetWindowTitle(IntPtr hWnd)
	// {
	//     int length = GetWindowTextLength(hWnd);
	//     if (length == 0) return "";
		
	//     StringBuilder sb = new StringBuilder(length + 1);
	//     GetWindowText(hWnd, sb, sb.Capacity);
	//     return sb.ToString();
	// }
	public void SetTopWindow()
	{
		bool result_last = SetWindowPos(
			windowInfo.targetWindow,
			HWND_NOTOPMOST,  // 使用HWND_NOTOPMOST取消置顶
			0, 0, 0, 0,
			SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW
		);
		if (result_last)
		{
			GD.Print("取消上一个窗口的置顶成功！");
			windowInfo.isTopMost = false;
		}
		else
		{
			GD.PrintErr($"取消上一个窗口的置顶失败！错误代码：{Marshal.GetLastWin32Error()}");
		}
		windowInfo.targetWindow = IntPtr.Zero;// 清空目标窗口
		windowInfo.targetWindow = FindWindow(null,DisPlayColumn.GetItemText(DisPlayColumn.Selected)); //选择下拉框中的窗口
		if (windowInfo.targetWindow == IntPtr.Zero)
		{
			GD.Print("请先选择有效窗口");
			return;
		}
		bool result_current = SetWindowPos(
			windowInfo.targetWindow,
			HWND_TOPMOST,
			0, 0, 0, 0,
			SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | 0x0200 /*SWP_FRAMECHANGED*/
		);
		// 双重验证
		if (result_current)
		{
			Thread.Sleep(50); // 等待窗口层级刷新
			int currentStyle = GetWindowLong(windowInfo.targetWindow, GWL_EXSTYLE);
			if ((currentStyle & 0x8) == 0x8) // 验证WS_EX_TOPMOST
			{
				GD.Print("置顶成功");
				windowInfo.isTopMost = true;
			}
			else
			{
				GD.PrintErr("窗口样式未更新");
			}
		}
		else
		{
			GD.PrintErr($"操作失败 (Error 0x{Marshal.GetLastWin32Error():X8})");
		}
	}
	public void CancelTopWindow()
	{
		if (windowInfo.targetWindow == IntPtr.Zero) return;

		bool result = SetWindowPos(
			windowInfo.targetWindow,
			HWND_NOTOPMOST,  // 使用HWND_NOTOPMOST取消置顶
			0, 0, 0, 0,
			SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW
		);

		if (result)
		{
			GD.Print("取消置顶成功！");
			windowInfo.isTopMost = false;
		}
		else
		{
			GD.PrintErr($"取消置顶失败！错误代码：{Marshal.GetLastWin32Error()}");
		}
	}
	public void DisPlayAllWindow()
	{
		DisPlayColumn.Clear();
		List<string> windowTitles = new List<string>();
		EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
		{
			// 检查窗口是否可见
			if (!IsWindowVisible(hWnd))
				return true;
			// 检查排除工具窗口
			int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
			if ((exStyle & WS_EX_TOOLWINDOW) != 0)
				return true;
			// 检查窗口标题长度
			int length = GetWindowTextLength(hWnd);
			if (length == 0)
				return true;
			// 获取窗口标题
			StringBuilder stringBuilder = new StringBuilder(length + 1);
			GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
			string title = stringBuilder.ToString();
			// 添加到结果列表
			if (!string.IsNullOrEmpty(title))
				windowTitles.Add(title);
			return true;
		}, IntPtr.Zero);
		foreach (string title in windowTitles)
		{
			DisPlayColumn.AddItem(title);
			GD.Print(title);
		}
	}
}
