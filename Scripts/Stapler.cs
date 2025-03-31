using Godot;
using System;

public partial class Stapler : Sprite2D
{
	private bool isDragging = false;
    private Vector2 relativeOffset;
    public override void _Process(double delta)
    {
        // 检测鼠标左键按下开始拖拽
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!isDragging)
            {
                // 获取鼠标位置和窗口位置
                Vector2 mousePos = DisplayServer.MouseGetPosition();
                Vector2 windowPos = GetViewport().GetWindow().Position;
                
                // 计算相对偏移
                relativeOffset = windowPos - mousePos;
                GD.Print("Drag started with offset: " + relativeOffset);
                
                isDragging = true;
            }
        }
        else
        {
            isDragging = false;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && isDragging)
        {
            // 移动窗口位置 = 相对偏移 + 当前鼠标位置
            GetViewport().GetWindow().Position = (Vector2I)(relativeOffset + DisplayServer.MouseGetPosition());
        }
    }
}
