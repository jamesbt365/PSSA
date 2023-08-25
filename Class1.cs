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
        [Property("Min Pressure"), ToolTip("The pressure value where scaling starts."), DefaultPropertyValue(0)]
        public float MinPressure { set; get; }
        [Property("Max Pressure"), ToolTip("The pressure value where scaling ends."), DefaultPropertyValue(8191)]
        public float MaxPressure { set; get; }

        [Property("Max smoothing weight"), ToolTip("The most smoothing will be at the maximum pressure."), DefaultPropertyValue(0.5)]
        public float SmoothingFactor { set; get; }

        [Property("Minimum smoothing weight"), ToolTip("The least smoothing will be at a given point above the minimum pressure."), DefaultPropertyValue(1)]
        public float MinWeight { set; get; }
        
        [BooleanProperty("Smooth below minimum pressure", ""), ToolTip("Dictates if smoothing is applied before minimum pressure.")]
        public bool BaseSmoothing { set; get; }

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
                }

                lastReport = report;
            }

            Emit?.Invoke(value);
        }

        public PipelinePosition Position => PipelinePosition.PostTransform;



        private float CalculateEMAWeight(float pressure)
        {
            float normalizedPressure = pressure / MaxPressure;

            float startIncreasePressure = MinPressure / MaxPressure;
            float clampedNormalizedPressure = Math.Max(Math.Min((normalizedPressure - startIncreasePressure) / (1.0f - startIncreasePressure), 1.0f), 0.0f);
            float emaWeight;

            if (!BaseSmoothing && normalizedPressure < startIncreasePressure)
            {
                emaWeight = 1.0f;
            }
            else
            {
                emaWeight = (1 - clampedNormalizedPressure) * MinWeight + (SmoothingFactor * clampedNormalizedPressure);
            }
            
            return emaWeight;
        }

        private float SmoothCursor(float weight, float currentPosition, float lastPosition)
        {
            float smoothedValue = weight * currentPosition + (1 - weight) * lastPosition;
            return smoothedValue;
        }


    }
}
