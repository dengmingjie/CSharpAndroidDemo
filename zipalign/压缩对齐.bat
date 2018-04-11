@echo off
zipalign -v 4 %1 %~n1_zipaligned%~x1
Pause
Exit