using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Timers;

namespace Zapper.Blazor.Components;

public partial class Trackpad : ComponentBase, IDisposable
{
    [Parameter] public EventCallback<(double deltaX, double deltaY)> OnMouseMove { get; set; }
    [Parameter] public EventCallback OnMouseClick { get; set; }
    [Parameter] public EventCallback OnMouseRightClick { get; set; }
    [Parameter] public double Sensitivity { get; set; } = 1.0;

    private bool _isTracking = false;
    private double _lastX = 0;
    private double _lastY = 0;
    private double _currentX = 0;
    private double _currentY = 0;
    private System.Timers.Timer? _movementTimer;
    private double _accumulatedX = 0;
    private double _accumulatedY = 0;
    private DateTime _lastMoveTime = DateTime.Now;

    protected override void OnInitialized()
    {
        _movementTimer = new System.Timers.Timer(50);
        _movementTimer.Elapsed += OnMovementTimer;
        _movementTimer.AutoReset = true;
    }

    private void OnMouseDown(MouseEventArgs e)
    {
        _isTracking = true;
        _lastX = e.OffsetX;
        _lastY = e.OffsetY;
        _currentX = e.OffsetX;
        _currentY = e.OffsetY;
        _accumulatedX = 0;
        _accumulatedY = 0;
        _movementTimer?.Start();
        StateHasChanged();
    }

    private void OnMouseUp(MouseEventArgs e)
    {
        _isTracking = false;
        _movementTimer?.Stop();
        StateHasChanged();
    }

    private void HandleMouseMove(MouseEventArgs e)
    {
        if (_isTracking)
        {
            var deltaX = (e.OffsetX - _lastX) * Sensitivity;
            var deltaY = (e.OffsetY - _lastY) * Sensitivity;

            _accumulatedX += deltaX;
            _accumulatedY += deltaY;

            _lastX = e.OffsetX;
            _lastY = e.OffsetY;
            _currentX = e.OffsetX;
            _currentY = e.OffsetY;
            _lastMoveTime = DateTime.Now;

            StateHasChanged();
        }
    }

    private void OnMouseLeave(MouseEventArgs e)
    {
        _isTracking = false;
        _movementTimer?.Stop();
        StateHasChanged();
    }

    private void OnTouchStart(TouchEventArgs e)
    {
        if (e.Touches.Length <= 0)
            return;

        var touch = e.Touches[0];
        _isTracking = true;
        _lastX = touch.ClientX;
        _lastY = touch.ClientY;
        _currentX = touch.ClientX;
        _currentY = touch.ClientY;
        _accumulatedX = 0;
        _accumulatedY = 0;
        _movementTimer?.Start();
        StateHasChanged();
    }

    private void OnTouchEnd(TouchEventArgs e)
    {
        _isTracking = false;
        _movementTimer?.Stop();
        StateHasChanged();
    }

    private void OnTouchMove(TouchEventArgs e)
    {
        if (!_isTracking || e.Touches.Length <= 0)
            return;

        var touch = e.Touches[0];
        var deltaX = (touch.ClientX - _lastX) * Sensitivity;
        var deltaY = (touch.ClientY - _lastY) * Sensitivity;

        _accumulatedX += deltaX;
        _accumulatedY += deltaY;

        _lastX = touch.ClientX;
        _lastY = touch.ClientY;
        _currentX = touch.ClientX;
        _currentY = touch.ClientY;
        _lastMoveTime = DateTime.Now;

        StateHasChanged();
    }

    private void OnTouchCancel(TouchEventArgs e)
    {
        _isTracking = false;
        _movementTimer?.Stop();
        StateHasChanged();
    }

    private async void OnMovementTimer(object? sender, ElapsedEventArgs e)
    {
        if (!_isTracking || (_accumulatedX == 0 && _accumulatedY == 0))
            return;

        if (OnMouseMove.HasDelegate)
        {
            await InvokeAsync(async () =>
            {
                await OnMouseMove.InvokeAsync((_accumulatedX, _accumulatedY));
                _accumulatedX = 0;
                _accumulatedY = 0;
            });
        }
    }

    private async Task OnClick(MouseEventArgs e)
    {
        var timeSinceLastMove = DateTime.Now - _lastMoveTime;
        if (timeSinceLastMove.TotalMilliseconds > 100)
        {
            await OnLeftClick();
        }
    }

    private async Task OnLeftClick()
    {
        if (OnMouseClick.HasDelegate)
        {
            await OnMouseClick.InvokeAsync();
        }
    }

    private async Task OnRightClick()
    {
        if (OnMouseRightClick.HasDelegate)
        {
            await OnMouseRightClick.InvokeAsync();
        }
    }

    public void Dispose()
    {
        _movementTimer?.Stop();
        _movementTimer?.Dispose();
    }
}