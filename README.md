# PSSA

A plugin that changes its smoothing simply based on how hard you are pressing your pen down.


## Recommended settings
There really isn't that much too it, but the way that EMA works means that weight scales expenially, so you shouldn't have a huge range between min and max pressure, because it will scale worse, when you're in reserved mode you should probably have smooth below minimum pressure enabled. A "good" (its preference) value to play around with for EMA weight is 0.5 and below.

Max pressure can be anywhere above the minimum pressure value, but going too high may make it hard to percieve changes in smoothing during normal usage. Play around with this to see what you like.

### Minimum pressure
This is the first point where the smoothing will begin to change, you should probably set value to around the point where you "naturally" drag, you can find this value in the tablet > tablet debugger on the main interface of OpenTabletDriver.

### Maximum pressure
This is where smoothing stops scaling, beyond this point the smoothing will stay the same. This value shouldn't be too much higher than the minimum pressure, as because the way EMA weight works most of the change happens in the weights closer to 0.

### Minimum smoothing weight
This is the lowest the smoothing will be. In normal operation this is at the minimum pressure, in reversed mode it is at the maximum pressure.

### Maximum smoothing weight
This is the highest the smoothing will be at any given point. When not reversed this will be at the max pressure, and when reversed it will be at the minimum.

### Smooth below minimum pressure
This is the setting that dictates if the smoothing should be used below the minimum pressure, in reversed mode it is **highly recommended** that you turn this on for a better experience as this will use at least the maximum smoothing value at a given moment.

### Reverse smoothing
This is the setting that controls if smoothing should be increased with pressure (normal), or decreases with higher pressure (reversed). Reversing the smoothing method is far more useful for gameplay, but having it not reversed is better for drawing lines.