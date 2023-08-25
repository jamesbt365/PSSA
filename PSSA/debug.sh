pkill -f OpenTabletDriver.Daemon
pkill -f OpenTabletDriver.UX.Gtk

mkdir -p ~/.config/OpenTabletDriver/Plugins/plugin/
cp bin/Debug/net6.0/plugin.dll ~/.config/OpenTabletDriver/Plugins/plugin/plugin.dll

otd-gui > /dev/null 2>&1 & otd-daemon
