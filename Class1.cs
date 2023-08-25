﻿using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace ExponentialSmoothing
{
    [PluginName("ExponentialSmoothingFilter")]
    public class ExponentialSmoothingPlugin : IPositionedPipelineElement<IDeviceReport>
    {
        [Property("Max Pressure"), ToolTip("The maximum pressure value.")]
        public float max_pressure { set; get; }

        [Property("Smoothing Factor"), ToolTip("Controls the amount of smoothing applied.")]
        public float smoothingFactor { set; get; }

        private ITabletReport? lastReport;

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                if (lastReport != null)
                {
                    float emaWeightX = CalculateEMAWeight(report.Pressure);
                    float emaWeightY = CalculateEMAWeight(report.Pressure);

                    float smoothedX = SmoothCursor(emaWeightX, report.Position.X, lastReport.Position.X);
                    float smoothedY = SmoothCursor(emaWeightY, report.Position.Y, lastReport.Position.Y);


                    report.Position = new Vector2(smoothedX, smoothedY);

                    Console.WriteLine($"Smoothed X: {smoothedX}, Smoothed Y: {smoothedY}");
                    Console.WriteLine($"EMA Weight X: {emaWeightX}, EMA Weight Y: {emaWeightY}");
                }

                lastReport = report;
            }

            Emit?.Invoke(value);
        }

        public PipelinePosition Position => PipelinePosition.PostTransform;

        private float CalculateEMAWeight(float pressure)
        {
            float normalizedPressure = pressure / max_pressure;

            float clampedNormalizedPressure = Math.Min(normalizedPressure, 1.0f);

            float emaWeight = (1 - clampedNormalizedPressure) + smoothingFactor * clampedNormalizedPressure;
            return emaWeight;
        }
        
        private float SmoothCursor(float weight, float currentPosition, float lastPosition)
        {
            float smoothedValue = weight * currentPosition + (1 - weight) * lastPosition;
            return smoothedValue;
        }


    }
}
