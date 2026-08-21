#nullable enable
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

public class ShakeEventArgs : EventArgs
{
    public int CurrentCount { get; }
    public int TargetLimit { get; }

    public ShakeEventArgs(int currentCount, int targetLimit)
    {
        CurrentCount = currentCount;
        TargetLimit = targetLimit;
    }
}

public class ShakeChallenge
{
    private int _shakeCount;
    private int _targetLimit;
    private double _gForceThreshold; // Speicher für die dynamische Empfindlichkeit
    private bool _isAlarmActive;

    public event EventHandler<ShakeEventArgs>? ShakeCountChanged;
    public event EventHandler? ChallengeCompleted;

    // Start erwartet nun das Limit und optional den gForce-Schwellenwert (Standard: 1.5)
    public void Start(int targetLimit, double gForceThreshold = 1.5)
    {
        _targetLimit = targetLimit;
        _gForceThreshold = gForceThreshold;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _isAlarmActive = true;
            _shakeCount = 0;
            
            ShakeCountChanged?.Invoke(this, new ShakeEventArgs(_shakeCount, _targetLimit));

            if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        });
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading;
        double gForce = Math.Sqrt(data.Acceleration.X * data.Acceleration.X + 
                                  data.Acceleration.Y * data.Acceleration.Y + 
                                  data.Acceleration.Z * data.Acceleration.Z);

        // Nutzt den dynamisch gesetzten Schwellenwert
        if (gForce > _gForceThreshold) 
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!_isAlarmActive) return;

                _shakeCount++;
                
                ShakeCountChanged?.Invoke(this, new ShakeEventArgs(_shakeCount, _targetLimit));

                if (_shakeCount >= _targetLimit)
                {
                    Stop();
                    _isAlarmActive = false;
                    ChallengeCompleted?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }

    public void Stop()
    {
        if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
        {
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.Stop();
        }
    }
}
