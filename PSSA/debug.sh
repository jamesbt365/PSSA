pkill -f OpenTabletDriver.Daemon
pkill -f OpenTabletDriver.UX.Gtk

mkdir -p ~/.config/OpenTabletDriver/Plugins/PSSA/
cp bin/Debug/net6.0/PSSA.dll ~/.config/OpenTabletDriver/Plugins/PSSA/PSSA.dll

otd-gui > /dev/null 2>&1 & otd-daemon
