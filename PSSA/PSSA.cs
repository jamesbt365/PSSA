﻿﻿using System;
using System.Dynamic;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace PSSA
{
    [PluginName("PSSA")]
    public class PSSA : IPositionedPipelineElement<IDeviceReport>
    {
        [Property("Min Pressure"), ToolTip("The pressure value where scaling starts."), DefaultPropertyValue(1f)]
        public float MinPressure 
        {
            set => this._MinPressure = Math.Max(value, 1f);
            get => this._MinPressure;
        }

        [Property("Max Pressure"), ToolTip("The pressure value where scaling ends."), DefaultPropertyValue(8191f)]
        public float MaxPressure 
        { 
            set => this._MaxPressure = Math.Max(value, 1f);
            get => this._MaxPressure;
        }
        

        [Property("Minimum smoothing weight"), ToolTip("The least smoothing will be at a given point above the minimum pressure."), DefaultPropertyValue(1.0f)]
        public float MinWeight 
        {
            set => this._MinWeight = Math.Clamp(value, 0f, 1f);
            get => this._MinWeight;
        
        }

        [Property("Maximum smoothing weight"), ToolTip("The most smoothing will be at the maximum pressure."), DefaultPropertyValue(0.5f)]
        public float MaxWeight 
        { 
            set => this._MaxWeight = Math.Clamp(value, 0f, 1f);
            get => this._MaxWeight;
        }

        
        [BooleanProperty("Smooth below minimum pressure", ""), ToolTip("Dictates if smoothing is applied before minimum pressure.")]
        public bool BaseSmoothing { set; get; }

        [BooleanProperty("Reverse Smoothing", ""), ToolTip("Reverse the smoothing behavior.")]
        public bool ReverseSmoothing { set; get; }

        private float _MinPressure;
        private float _MaxPressure;
        private float _MinWeight;
        private float _MaxWeight;

        private ITabletReport? lastReport;

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                if (lastReport != null)
                {
                    if (ReverseSmoothing) {
                        report.Position = SmoothCursor(CalculateEMAWeightReversed(report.Pressure), report.Position, lastReport.Position);
                    }
                    else {
                        report.Position = SmoothCursor(CalculateEMAWeight(report.Pressure), report.Position, lastReport.Position);
                    }

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
            float clampedNormalizedPressure = Math.Clamp((normalizedPressure - startIncreasePressure) / (1.0f - startIncreasePressure), 0.0f, 1.0f);


            if (!BaseSmoothing && normalizedPressure < startIncreasePressure)
            {
                return 1.0f;
            }
            return (1 - clampedNormalizedPressure) * MinWeight + (MaxWeight * clampedNormalizedPressure);
        }

        private float CalculateEMAWeightReversed(float pressure)
        {
            float normalizedPressure = (pressure - MinPressure) / (MaxPressure - MinPressure);
            // No idea if this is right....
            
            float clampedNormalizedPressure = Math.Clamp(normalizedPressure, 0.0f, 1.0f);


            // Base smoothing kinda has to be enabled to be "used correctly"
            // The way ema works makes reversal awkward anyway, I don't know how "useful" this is.
            if (!BaseSmoothing && normalizedPressure < 0)
            {
                return 1.0f;
            }
            return (1 - clampedNormalizedPressure) * MaxWeight + (MinWeight * clampedNormalizedPressure);
        }

        private Vector2 SmoothCursor(float weight, Vector2 currentPosition, Vector2 lastPosition)
        {
            Vector2 smoothedPosition = weight * currentPosition + (1 - weight) * lastPosition;
            return smoothedPosition;
        }



    }
}
